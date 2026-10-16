using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace QuanLyBanHangSupermarket
{
    public static class Money
    {
        public static decimal LineTotal(int quantity, decimal price, decimal discount)
        {
            if (quantity <= 0 || quantity > 1000000) throw new ArgumentException("Số lượng phải từ 1 đến 1.000.000.");
            if (price <= 0 || price > 1100000000m) throw new ArgumentException("Đơn giá phải lớn hơn 0 và không quá 1,1 tỷ đồng.");
            if (discount < 0 || discount > 100) throw new ArgumentException("Giảm giá phải từ 0 đến 100%.");
            return Math.Round(quantity * price * (1m - discount / 100m), 2, MidpointRounding.AwayFromZero);
        }
        public static decimal SalePrice(decimal purchasePrice)
        { return Math.Round(purchasePrice * 1.10m, 2, MidpointRounding.AwayFromZero); }
        public static string NewId(string prefix) { return prefix + DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_" + Guid.NewGuid().ToString("N").Substring(0, 8); }
        public static DataTable NewCart()
        {
            var dt = new DataTable();
            dt.Columns.Add("MaVach", typeof(string)); dt.Columns.Add("TenHang", typeof(string));
            dt.Columns.Add("SoLuong", typeof(int)); dt.Columns.Add("DonGia", typeof(decimal));
            dt.Columns.Add("GiamGia", typeof(decimal)); dt.Columns.Add("ThanhTien", typeof(decimal));
            dt.Columns.Add("GiaVon", typeof(decimal));
            dt.PrimaryKey = new[] { dt.Columns["MaVach"] };
            return dt;
        }
        public static decimal Total(DataTable cart)
        { return cart.Rows.Cast<DataRow>().Sum(r => Convert.ToDecimal(r["ThanhTien"])); }
    }
    public static class Session
    {
        public static string Username { get; internal set; }
        public static string EmployeeId { get; internal set; }
        public static string Role { get; internal set; }
        public static bool IsAdmin { get { return Role == "Admin"; } }
        public static void Require(params string[] roles)
        { if (!roles.Contains(Role)) throw new UnauthorizedAccessException("Tài khoản của bạn không có quyền thực hiện thao tác này."); }
    }
    public static class Passwords
    {
        public const int Iterations = 120000;
        public static string Hash(string password, byte[] salt)
        {
            using (var derive = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
                return Convert.ToBase64String(derive.GetBytes(32));
        }
        public static byte[] Salt()
        { var salt = new byte[16]; using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(salt); return salt; }
        public static bool Equal(string a, string b)
        {
            var x = Encoding.UTF8.GetBytes(a ?? ""); var y = Encoding.UTF8.GetBytes(b ?? "");
            int diff = x.Length ^ y.Length;
            for (int i = 0; i < Math.Max(x.Length, y.Length); i++) diff |= (i < x.Length ? x[i] : 0) ^ (i < y.Length ? y[i] : 0);
            return diff == 0;
        }
    }
    public static class Accounts
    {
        public static bool Authenticate(string username, string password)
        {
            var rows = DatabaseHelper.GetDataToTable("SELECT * FROM TaiKhoan WHERE TenDangNhap=@u AND HoatDong=1", DatabaseHelper.Param("@u", username));
            if (rows.Rows.Count != 1) return false;
            var r = rows.Rows[0];
            if (!Passwords.Equal(Passwords.Hash(password, Convert.FromBase64String(r["Salt"].ToString())), r["PasswordHash"].ToString())) return false;
            Session.Username = r["TenDangNhap"].ToString(); Session.Role = r["VaiTro"].ToString(); Session.EmployeeId = r["MaNV"].ToString();
            return true;
        }
        public static void Create(string username, string password, string role, string employee, bool first)
        {
            if (username.Length < 3 || username.Length > 40 || username.Any(c => !(char.IsLetterOrDigit(c) || c == '_' || c == '.')))
                throw new ArgumentException("Tên đăng nhập dài 3–40 ký tự, chỉ gồm chữ, số, dấu chấm và gạch dưới.");
            if (password.Length < 8 || password.Length > 128) throw new ArgumentException("Mật khẩu cần từ 8 đến 128 ký tự.");
            if (!new[] { "Admin", "ThuNgan", "ThuKho" }.Contains(role)) throw new ArgumentException("Vai trò không hợp lệ.");
            if (!first) Session.Require("Admin");
            if (role != "Admin" && string.IsNullOrWhiteSpace(employee)) throw new ArgumentException("Thu ngân và thủ kho phải gắn với một nhân viên.");
            var salt = Passwords.Salt();
            DatabaseHelper.InTransaction((conn, tx) =>
            {
                if (first && Convert.ToInt32(DatabaseHelper.Scalar(conn, tx, "SELECT COUNT(*) FROM TaiKhoan WITH(UPDLOCK,HOLDLOCK)")) != 0)
                    throw new InvalidOperationException("Đã có tài khoản. Vui lòng đăng nhập bằng tài khoản hiện tại.");
                if (first && role != "Admin") throw new ArgumentException("Tài khoản đầu tiên phải là quản trị viên.");
                return DatabaseHelper.Execute(conn, tx, "INSERT TaiKhoan(TenDangNhap,PasswordHash,Salt,VaiTro,MaNV,HoatDong) VALUES(@u,@h,@s,@r,@e,1)",
                    DatabaseHelper.Param("@u", username), DatabaseHelper.Param("@h", Passwords.Hash(password, salt)), DatabaseHelper.Param("@s", Convert.ToBase64String(salt)),
                    DatabaseHelper.Param("@r", role), DatabaseHelper.Param("@e", string.IsNullOrWhiteSpace(employee) ? null : employee));
            });
        }
        public static void ChangePassword(string oldPassword, string newPassword)
        {
            var row = DatabaseHelper.GetDataToTable("SELECT PasswordHash,Salt FROM TaiKhoan WHERE TenDangNhap=@u AND HoatDong=1", DatabaseHelper.Param("@u", Session.Username));
            if (row.Rows.Count != 1 || !Passwords.Equal(Passwords.Hash(oldPassword, Convert.FromBase64String(row.Rows[0]["Salt"].ToString())), row.Rows[0]["PasswordHash"].ToString()))
                throw new ArgumentException("Mật khẩu hiện tại không đúng.");
            if (newPassword.Length < 8 || newPassword.Length > 128) throw new ArgumentException("Mật khẩu mới cần từ 8 đến 128 ký tự.");
            var salt = Passwords.Salt();
            DatabaseHelper.ExecuteNonQuery("UPDATE TaiKhoan SET PasswordHash=@h,Salt=@s WHERE TenDangNhap=@u", DatabaseHelper.Param("@h", Passwords.Hash(newPassword, salt)),
                DatabaseHelper.Param("@s", Convert.ToBase64String(salt)), DatabaseHelper.Param("@u", Session.Username));
        }
    }
    public static class Sales
    {
        public static string Save(bool sale, string employee, string partner, DateTime date, DataTable cart)
        {
            Session.Require(sale ? new[] { "Admin", "ThuNgan" } : new[] { "Admin", "ThuKho" });
            if (string.IsNullOrEmpty(employee) || string.IsNullOrEmpty(partner)) throw new ArgumentException("Chọn nhân viên và khách hàng/nhà cung cấp.");
            if (!Session.IsAdmin && employee != Session.EmployeeId) throw new UnauthorizedAccessException("Chỉ được lập hóa đơn dưới tên nhân viên của tài khoản.");
            if (cart.Rows.Count == 0) throw new ArgumentException("Thêm ít nhất một mặt hàng vào hóa đơn.");
            if (date.Date > DateTime.Today) throw new ArgumentException("Ngày hóa đơn không được ở tương lai.");
            string id = Money.NewId(sale ? "HDB_" : "HDN_");
            return DatabaseHelper.InTransaction((conn, tx) =>
            {
                decimal total = 0;
                foreach (DataRow r in cart.Rows)
                {
                    if(!sale && (decimal)r["DonGia"]>1000000000m)throw new ArgumentException("Giá nhập không được quá 1 tỷ đồng.");
                    r["ThanhTien"] = Money.LineTotal((int)r["SoLuong"], (decimal)r["DonGia"], (decimal)r["GiamGia"]); total += (decimal)r["ThanhTien"];
                }
                if(total>9999999999999999.99m)throw new ArgumentException("Tổng tiền vượt giới hạn lưu trữ của hóa đơn.");
                DatabaseHelper.Execute(conn, tx, sale
                    ? "INSERT HoaDonBan(SoHDB,MaNV,NgayBan,MaKhach,TongTien) VALUES(@id,@e,@d,@p,@t)"
                    : "INSERT HoaDonNhap(SoHDN,MaNV,NgayNhap,MaNCC,TongTien) VALUES(@id,@e,@d,@p,@t)",
                    DatabaseHelper.Param("@id", id), DatabaseHelper.Param("@e", employee), DatabaseHelper.Param("@d", date), DatabaseHelper.Param("@p", partner), DatabaseHelper.Param("@t", total));
                // Lock in a consistent order for concurrent multi-product invoices.
                foreach (DataRow r in cart.Rows.Cast<DataRow>().OrderBy(r => r["MaVach"].ToString(), StringComparer.Ordinal))
                {
                    string code = r["MaVach"].ToString(); int quantity = (int)r["SoLuong"]; decimal price = (decimal)r["DonGia"];
                    decimal cost;
                    using (var cmd = new SqlCommand("SELECT TenHangHoa,DonGiaBan,DonGiaNhap FROM DMHangHoa WITH(UPDLOCK,HOLDLOCK) WHERE MaVach=@c", conn, tx))
                    {
                        cmd.Parameters.Add(DatabaseHelper.Param("@c", code));
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read()) throw new InvalidOperationException("Hàng hóa không còn tồn tại: " + code);
                            r["TenHang"] = reader["TenHangHoa"].ToString(); cost = Convert.ToDecimal(reader["DonGiaNhap"]);
                            if (sale && price != Convert.ToDecimal(reader["DonGiaBan"])) throw new InvalidOperationException("Giá sản phẩm " + code + " đã thay đổi. Bỏ dòng này và thêm lại để lấy giá mới.");
                        }
                    }
                    int changed = DatabaseHelper.Execute(conn, tx, sale
                        ? "UPDATE DMHangHoa SET SoLuong=SoLuong-@q WHERE MaVach=@c AND SoLuong>=@q"
                        : "UPDATE DMHangHoa SET SoLuong=SoLuong+@q,DonGiaNhap=@price,DonGiaBan=@sell WHERE MaVach=@c AND SoLuong<=2147483647-@q",
                        DatabaseHelper.Param("@q", quantity), DatabaseHelper.Param("@c", code), DatabaseHelper.Param("@price", price), DatabaseHelper.Param("@sell", Money.SalePrice(price)));
                    if (changed != 1) throw new InvalidOperationException(sale ? "Không đủ tồn kho cho mã " + code + ". Hóa đơn chưa được lưu." : "Số lượng tồn kho vượt giới hạn cho mã " + code);
                    r["GiaVon"] = cost;
                    DatabaseHelper.Execute(conn, tx, sale
                        ? "INSERT ChiTietHDB(SoHDB,MaVach,TenHang,SoLuong,DonGia,GiaVon,GiamGia,ThanhTien) VALUES(@id,@c,@n,@q,@price,@cost,@discount,@t)"
                        : "INSERT ChiTietHDN(SoHDN,MaVach,TenHang,SoLuong,DonGia,GiamGia,ThanhTien) VALUES(@id,@c,@n,@q,@price,@discount,@t)",
                        DatabaseHelper.Param("@id", id), DatabaseHelper.Param("@c", code), DatabaseHelper.Param("@n", r["TenHang"]), DatabaseHelper.Param("@q", quantity),
                        DatabaseHelper.Param("@price", price), DatabaseHelper.Param("@cost", cost), DatabaseHelper.Param("@discount", r["GiamGia"]), DatabaseHelper.Param("@t", r["ThanhTien"]));
                    RecordStock(conn, tx, code, sale ? -quantity : quantity, sale ? "Bán hàng" : "Nhập hàng", id);
                }
                return id;
            });
        }
        public static void RecordStock(SqlConnection conn, SqlTransaction tx, string product, int quantity, string type, string reference)
        {
            DatabaseHelper.Execute(conn, tx, "INSERT LichSuKho(MaVach,ThayDoi,TonSau,Loai,ThamChieu,NguoiThucHien) SELECT MaVach,@q,SoLuong,@l,@r,@u FROM DMHangHoa WHERE MaVach=@c",
                DatabaseHelper.Param("@q", quantity), DatabaseHelper.Param("@l", type), DatabaseHelper.Param("@r", reference), DatabaseHelper.Param("@u", Session.Username), DatabaseHelper.Param("@c", product));
        }
        public static string CountStock(DataTable rows, string note)
        {
            Session.Require("Admin", "ThuKho");
            if (string.IsNullOrWhiteSpace(note)) throw new ArgumentException("Nhập lý do/ghi chú kiểm kê.");
            if (rows.Rows.Count == 0) throw new ArgumentException("Chưa có hàng hóa để kiểm kê.");
            string id = Money.NewId("KK_");
            return DatabaseHelper.InTransaction((conn, tx) =>
            {
                DatabaseHelper.Execute(conn, tx, "INSERT KiemKe(MaKiemKe,GhiChu,NguoiThucHien) VALUES(@id,@n,@u)", DatabaseHelper.Param("@id", id), DatabaseHelper.Param("@n", note), DatabaseHelper.Param("@u", Session.Username));
                foreach (DataRow r in rows.Rows.Cast<DataRow>().OrderBy(r => r["MaVach"].ToString(), StringComparer.Ordinal))
                {
                    int before = Convert.ToInt32(r["SoLuong"]), actual = Convert.ToInt32(r["ThucTe"]);
                    if (actual < 0 || actual > 1000000) throw new ArgumentException("Tồn thực tế phải từ 0 đến 1.000.000.");
                    int changed = DatabaseHelper.Execute(conn, tx, "UPDATE DMHangHoa SET SoLuong=@a WHERE MaVach=@c AND SoLuong=@b", DatabaseHelper.Param("@a", actual), DatabaseHelper.Param("@b", before), DatabaseHelper.Param("@c", r["MaVach"]));
                    if (changed != 1) throw new InvalidOperationException("Tồn kho đã thay đổi từ khi tải danh sách. Nhấn Tải lại và kiểm tra lại số lượng.");
                    DatabaseHelper.Execute(conn, tx, "INSERT ChiTietKiemKe(MaKiemKe,MaVach,TonHeThong,TonThucTe) VALUES(@id,@c,@b,@a)", DatabaseHelper.Param("@id", id), DatabaseHelper.Param("@c", r["MaVach"]), DatabaseHelper.Param("@b", before), DatabaseHelper.Param("@a", actual));
                    RecordStock(conn, tx, r["MaVach"].ToString(), actual - before, "Kiểm kê", id);
                }
                return id;
            });
        }
    }
}

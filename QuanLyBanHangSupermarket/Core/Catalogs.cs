using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace QuanLyBanHangSupermarket
{
    public class FieldSpec
    {
        public string Name, Label, Kind, LookupTable, LookupKey, LookupLabel;
        public int Length;
        public bool Required, Computed;
        public FieldSpec(string name, string label, int length = 100, bool required = true, string kind = "Text")
        { Name = name; Label = label; Length = length; Required = required; Kind = kind; }
        public static FieldSpec Lookup(string name, string label, string table, string key, string display, bool required = true)
        { return new FieldSpec(name, label, 20, required, "Lookup") { LookupTable = table, LookupKey = key, LookupLabel = display }; }
    }
    public class CatalogSpec
    {
        public string Table, Key;
        public string Title { get; set; }
        public FieldSpec[] Fields;
        public string[] WriteRoles;
        public CatalogSpec(string table, string title, string key, params FieldSpec[] fields)
        { Table = table; Title = title; Key = key; Fields = fields; WriteRoles = new[] { "Admin" }; }
    }
    public static class Catalogs
    {
        static CatalogSpec Pair(string table, string title, string key, string name, int length)
        { return new CatalogSpec(table, title, key, new FieldSpec(key, "Mã", 20), new FieldSpec(name, "Tên", length)); }
        public static readonly CatalogSpec[] All = {
            Pair("NhomHang","Nhóm hàng","MaNhom","TenNhom",100),
            new CatalogSpec("TheLoai","Thể loại","MaLoai",new FieldSpec("MaLoai","Mã loại",20),new FieldSpec("TenLoai","Tên loại",100),FieldSpec.Lookup("MaNhom","Nhóm hàng","NhomHang","MaNhom","TenNhom")),
            Pair("DonViTinh","Đơn vị tính","MaDonVi","TenDonVi",50),
            Pair("ChatLieu","Chất liệu","MaChatLieu","TenChatLieu",100),
            Pair("CongDung","Công dụng","MaCongDung","TenCongDung",100),
            Pair("NoiSanXuat","Nơi sản xuất","MaNoiSX","TenNoiSX",100),
            Pair("CaLam","Ca làm","MaCa","TenCa",50),
            Pair("CongViec","Công việc","MaCV","TenCV",100),
            new CatalogSpec("KhachHang","Khách hàng","MaKhach",new FieldSpec("MaKhach","Mã khách",20),new FieldSpec("TenKhach","Tên khách",100),new FieldSpec("DiaChi","Địa chỉ",200,false),new FieldSpec("DienThoai","Điện thoại",20,false)) { WriteRoles=new[]{"Admin","ThuNgan"} },
            new CatalogSpec("NhaCungCap","Nhà cung cấp","MaNCC",new FieldSpec("MaNCC","Mã NCC",20),new FieldSpec("TenNCC","Tên nhà cung cấp",150),new FieldSpec("DiaChi","Địa chỉ",200,false),new FieldSpec("DienThoai","Điện thoại",20,false)) { WriteRoles=new[]{"Admin","ThuKho"} },
            new CatalogSpec("NhânVien","Nhân viên","MaNV",new FieldSpec("MaNV","Mã nhân viên",20),new FieldSpec("TenNV","Họ và tên",100),new FieldSpec("GioiTinh","Giới tính",10,false,"Gender"),new FieldSpec("NgàySinh","Ngày sinh",0,false,"Date"),new FieldSpec("DienThoai","Điện thoại",20,false),new FieldSpec("DiaChi","Địa chỉ",200,false),FieldSpec.Lookup("MaNhom","Nhóm hàng","NhomHang","MaNhom","TenNhom",false),FieldSpec.Lookup("MaCa","Ca làm","CaLam","MaCa","TenCa",false),FieldSpec.Lookup("MaCV","Công việc","CongViec","MaCV","TenCV",false)),
            new CatalogSpec("DMHangHoa","Hàng hóa","MaVach",new FieldSpec("MaVach","Mã vạch",50),new FieldSpec("TenHangHoa","Tên hàng hóa",150),FieldSpec.Lookup("MaNhom","Nhóm hàng","NhomHang","MaNhom","TenNhom"),FieldSpec.Lookup("MaLoai","Thể loại","TheLoai","MaLoai","TenLoai"),FieldSpec.Lookup("MaDonVi","Đơn vị tính","DonViTinh","MaDonVi","TenDonVi"),new FieldSpec("NhapKhau","Hàng nhập khẩu",0,false,"Bool"),FieldSpec.Lookup("MaChatLieu","Chất liệu","ChatLieu","MaChatLieu","TenChatLieu",false),FieldSpec.Lookup("MaCongDung","Công dụng","CongDung","MaCongDung","TenCongDung",false),FieldSpec.Lookup("MaNoiSX","Nơi sản xuất","NoiSanXuat","MaNoiSX","TenNoiSX",false),new FieldSpec("SoLuong","Tồn kho",0,false,"Number"){Computed=true},new FieldSpec("DonGiaNhap","Giá nhập (VNĐ)",0,false,"Money"){Computed=true},new FieldSpec("DonGiaBan","Giá bán (VNĐ)",0,false,"Money"){Computed=true},new FieldSpec("Anh","Ảnh sản phẩm",500,false,"Image"),new FieldSpec("GhiChu","Ghi chú",1000,false)) { WriteRoles=new[]{"Admin","ThuKho"} }
        };
        public static CatalogSpec Get(string table) { return All.Single(s => s.Table == table); }
        public static string Q(string name) { return "[" + name.Replace("]", "]]") + "]"; }
        public static DataTable Lookup(FieldSpec field)
        { return DatabaseHelper.GetDataToTable("SELECT " + Q(field.LookupKey) + "," + Q(field.LookupLabel) + " FROM " + Q(field.LookupTable) + " ORDER BY " + Q(field.LookupLabel)); }
        public static DataTable Load(CatalogSpec spec, string search)
        {
            // Identifiers are exclusively from the immutable catalog definitions, never user input.
            string where = string.Join(" OR ", spec.Fields.Where(f => f.Kind == "Text").Select(f => Q(f.Name) + " LIKE @search ESCAPE '~'"));
            string value = "%" + search.Replace("~", "~~").Replace("%", "~%").Replace("_", "~_").Replace("[", "~[") + "%";
            return DatabaseHelper.GetDataToTable("SELECT * FROM " + Q(spec.Table) + " WHERE " + where + " ORDER BY " + Q(spec.Key), DatabaseHelper.Param("@search", value));
        }
        public static void Save(CatalogSpec spec, Dictionary<string, object> values, bool add)
        {
            Session.Require(spec.WriteRoles);
            string code=values[spec.Key]==null?"":values[spec.Key].ToString();
            if(code.Any(c=>!((c>='A'&&c<='Z')||(c>='a'&&c<='z')||(c>='0'&&c<='9')||c=='_'||c=='-'||c=='.')))
                throw new ArgumentException("Mã chỉ gồm chữ không dấu, số, dấu chấm, gạch dưới hoặc gạch ngang.");
            foreach (var f in spec.Fields.Where(f => !f.Computed))
            {
                object value = values[f.Name];
                if (f.Required && (value == null || string.IsNullOrWhiteSpace(value.ToString()))) throw new ArgumentException("Vui lòng nhập/chọn " + f.Label.ToLower() + ".");
                if (value is string && f.Length > 0 && value.ToString().Length > f.Length) throw new ArgumentException(f.Label + " không được quá " + f.Length + " ký tự.");
            }
            if (values.ContainsKey("DienThoai") && values["DienThoai"] != null && values["DienThoai"].ToString().Any(c => !(char.IsDigit(c) || c == '+' || c == ' ' || c == '-' || c == '(' || c == ')')))
                throw new ArgumentException("Điện thoại chỉ gồm số, dấu +, dấu cách, dấu - hoặc dấu ngoặc.");
            if (values.ContainsKey("NgàySinh") && values["NgàySinh"] != null && (DateTime)values["NgàySinh"] > DateTime.Today)
                throw new ArgumentException("Ngày sinh không được ở tương lai.");
            DatabaseHelper.InTransaction((conn, tx) =>
            {
                if (spec.Table == "DMHangHoa")
                {
                    int valid = Convert.ToInt32(DatabaseHelper.Scalar(conn, tx, "SELECT COUNT(*) FROM TheLoai WHERE MaLoai=@l AND MaNhom=@g", DatabaseHelper.Param("@l", values["MaLoai"]), DatabaseHelper.Param("@g", values["MaNhom"])));
                    if (valid != 1) throw new ArgumentException("Thể loại phải thuộc nhóm hàng đã chọn.");
                }
                if (spec.Table == "TheLoai" && !add && Convert.ToInt32(DatabaseHelper.Scalar(conn, tx, "SELECT COUNT(*) FROM DMHangHoa WHERE MaLoai=@k AND MaNhom<>@g", DatabaseHelper.Param("@k", values[spec.Key]), DatabaseHelper.Param("@g", values["MaNhom"]))) > 0)
                    throw new ArgumentException("Không đổi nhóm của thể loại đang được hàng hóa sử dụng.");
                var fields = spec.Fields.Where(f => !f.Computed || add).ToArray();
                string sql = add ? "INSERT " + Q(spec.Table) + "(" + string.Join(",", fields.Select(f => Q(f.Name))) + ") VALUES(" + string.Join(",", fields.Select((f,i) => "@p"+i)) + ")"
                    : "UPDATE " + Q(spec.Table) + " SET " + string.Join(",", fields.Where(f => f.Name != spec.Key).Select(f => Q(f.Name) + "=@p" + Array.IndexOf(fields,f))) + " WHERE " + Q(spec.Key) + "=@p" + Array.FindIndex(fields,f => f.Name == spec.Key);
                int changed = DatabaseHelper.Execute(conn, tx, sql, fields.Select((f,i) => DatabaseHelper.Param("@p"+i,values[f.Name])).ToArray());
                if (changed != 1) throw new InvalidOperationException("Bản ghi đã thay đổi hoặc bị xóa. Tải lại danh sách rồi thử lại.");
                return changed;
            });
        }
        public static void Delete(CatalogSpec spec, string key)
        {
            Session.Require(spec.WriteRoles);
            int changed = DatabaseHelper.ExecuteNonQuery("DELETE FROM " + Q(spec.Table) + " WHERE " + Q(spec.Key) + "=@k",DatabaseHelper.Param("@k",key));
            if (changed != 1) throw new InvalidOperationException("Bản ghi đã bị xóa. Vui lòng tải lại danh sách.");
        }
    }
}

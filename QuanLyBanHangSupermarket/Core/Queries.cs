using System;
using System.Data;

namespace QuanLyBanHangSupermarket
{
    public static class Queries
    {
        public static DataTable Invoices(bool sale,DateTime from,DateTime to,string code,string product,string partner)
        {
            if(from.Date>to.Date)throw new ArgumentException("Ngày bắt đầu không được sau ngày kết thúc.");
            string sql=sale
                ? @"SELECT h.SoHDB AS [Số hóa đơn],h.NgayBan AS [Ngày],nv.TenNV AS [Nhân viên],kh.TenKhach AS [Khách hàng],h.TongTien AS [Tổng tiền] FROM HoaDonBan h JOIN [NhânVien] nv ON nv.MaNV=h.MaNV JOIN KhachHang kh ON kh.MaKhach=h.MaKhach WHERE h.NgayBan>=@a AND h.NgayBan<@b AND h.SoHDB LIKE @code ESCAPE '~' AND (@p='' OR h.MaKhach=@p) AND (@c='' OR EXISTS(SELECT 1 FROM ChiTietHDB d WHERE d.SoHDB=h.SoHDB AND d.MaVach=@c)) ORDER BY h.NgayBan DESC,h.SoHDB DESC"
                : @"SELECT h.SoHDN AS [Số hóa đơn],h.NgayNhap AS [Ngày],nv.TenNV AS [Nhân viên],n.TenNCC AS [Nhà cung cấp],h.TongTien AS [Tổng tiền] FROM HoaDonNhap h JOIN [NhânVien] nv ON nv.MaNV=h.MaNV JOIN NhaCungCap n ON n.MaNCC=h.MaNCC WHERE h.NgayNhap>=@a AND h.NgayNhap<@b AND h.SoHDN LIKE @code ESCAPE '~' AND (@p='' OR h.MaNCC=@p) AND (@c='' OR EXISTS(SELECT 1 FROM ChiTietHDN d WHERE d.SoHDN=h.SoHDN AND d.MaVach=@c)) ORDER BY h.NgayNhap DESC,h.SoHDN DESC";
            code=(code??"").Replace("~","~~").Replace("%","~%").Replace("_","~_").Replace("[","~[");
            return DatabaseHelper.GetDataToTable(sql,DatabaseHelper.Param("@a",from.Date),DatabaseHelper.Param("@b",to.Date.AddDays(1)),DatabaseHelper.Param("@code","%"+code+"%"),DatabaseHelper.Param("@p",partner??""),DatabaseHelper.Param("@c",product??""));
        }
        public static DataTable Details(bool sale,string id)
        {
            return DatabaseHelper.GetDataToTable(sale
                ? "SELECT MaVach AS [Mã vạch],TenHang AS [Hàng hóa],SoLuong AS [Số lượng],DonGia AS [Đơn giá],GiamGia AS [Giảm (%)],ThanhTien AS [Thành tiền] FROM ChiTietHDB WHERE SoHDB=@id ORDER BY MaVach"
                : "SELECT MaVach AS [Mã vạch],TenHang AS [Hàng hóa],SoLuong AS [Số lượng],DonGia AS [Đơn giá],GiamGia AS [Giảm (%)],ThanhTien AS [Thành tiền] FROM ChiTietHDN WHERE SoHDN=@id ORDER BY MaVach",DatabaseHelper.Param("@id",id));
        }
        public static DataTable Report(int mode,DateTime month,int year,int quarter,string employee,DateTime from,DateTime to)
        {
            DateTime a,b;string sql;
            if(mode==0)
            {
                a=new DateTime(month.Year,month.Month,1);b=a.AddMonths(1);
                sql=@"SELECT d.MaVach AS [Mã vạch],MAX(d.TenHang) AS [Hàng hóa],SUM(CONVERT(bigint,d.SoLuong)) AS [Số lượng đã bán],SUM(d.ThanhTien) AS [Doanh thu] FROM ChiTietHDB d JOIN HoaDonBan h ON h.SoHDB=d.SoHDB WHERE h.NgayBan>=@a AND h.NgayBan<@b GROUP BY d.MaVach ORDER BY [Số lượng đã bán] DESC,d.MaVach";
            }
            else if(mode==1)
            {
                a=new DateTime(year,1,1);b=a.AddYears(1);
                sql=@"SELECT TOP(3) h.SoHDN AS [Số hóa đơn nhập],h.NgayNhap AS [Ngày nhập],n.TenNCC AS [Nhà cung cấp],h.TongTien AS [Tổng tiền nhập] FROM HoaDonNhap h JOIN NhaCungCap n ON n.MaNCC=h.MaNCC WHERE h.NgayNhap>=@a AND h.NgayNhap<@b ORDER BY h.TongTien DESC,h.SoHDN";
            }
            else if(mode==2)
            {
                if(string.IsNullOrEmpty(employee))throw new ArgumentException("Chọn nhân viên cần báo cáo.");
                a=new DateTime(1753,1,1);b=new DateTime(9999,12,31);
                sql=@"SELECT h.SoHDB AS [Số hóa đơn bán],h.NgayBan AS [Ngày bán],nv.TenNV AS [Nhân viên],k.TenKhach AS [Khách hàng],h.TongTien AS [Tổng tiền bán] FROM HoaDonBan h JOIN [NhânVien] nv ON nv.MaNV=h.MaNV JOIN KhachHang k ON k.MaKhach=h.MaKhach WHERE h.MaNV=@e ORDER BY h.NgayBan DESC,h.SoHDB";
            }
            else if(mode==3)
            {
                if(quarter<1||quarter>4)throw new ArgumentException("Quý phải từ 1 đến 4.");
                a=new DateTime(year,(quarter-1)*3+1,1);b=a.AddMonths(3);
                sql=@"SELECT TOP(3) k.MaKhach AS [Mã khách],k.TenKhach AS [Khách hàng],COUNT_BIG(*) AS [Số hóa đơn],SUM(h.TongTien) AS [Tổng tiền mua] FROM KhachHang k JOIN HoaDonBan h ON h.MaKhach=k.MaKhach WHERE h.NgayBan>=@a AND h.NgayBan<@b GROUP BY k.MaKhach,k.TenKhach ORDER BY [Tổng tiền mua] ASC,k.MaKhach";
            }
            else
            {
                if(from.Date>to.Date)throw new ArgumentException("Khoảng ngày không hợp lệ.");a=from.Date;b=to.Date.AddDays(1);
                sql=@"SELECT CAST(h.NgayBan AS date) AS [Ngày],COUNT(DISTINCT h.SoHDB) AS [Số hóa đơn],SUM(d.ThanhTien) AS [Doanh thu],SUM(d.SoLuong*d.GiaVon) AS [Giá vốn],SUM(d.ThanhTien-d.SoLuong*d.GiaVon) AS [Lãi gộp] FROM HoaDonBan h JOIN ChiTietHDB d ON d.SoHDB=h.SoHDB WHERE h.NgayBan>=@a AND h.NgayBan<@b GROUP BY CAST(h.NgayBan AS date) ORDER BY [Ngày]";
            }
            return DatabaseHelper.GetDataToTable(sql,DatabaseHelper.Param("@a",a),DatabaseHelper.Param("@b",b),DatabaseHelper.Param("@e",employee??""));
        }
    }
}

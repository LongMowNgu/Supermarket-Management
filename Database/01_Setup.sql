-- Run the entire file on .\SQLEXPRESS01. No DROP, DELETE or demo records.
-- Supports both a new database and the empty schema already on this machine.
USE master;
GO
IF DB_ID(N'QuanLyBanHangSupermarket') IS NULL
    EXEC(N'CREATE DATABASE QuanLyBanHangSupermarket');
GO
USE QuanLyBanHangSupermarket;
GO
SET XACT_ABORT ON;
BEGIN TRY
    BEGIN TRANSACTION;
    IF OBJECT_ID(N'dbo.NhomHang') IS NULL CREATE TABLE dbo.NhomHang(MaNhom varchar(20) NOT NULL PRIMARY KEY,TenNhom nvarchar(100) NOT NULL);
    IF OBJECT_ID(N'dbo.TheLoai') IS NULL CREATE TABLE dbo.TheLoai(MaLoai varchar(20) NOT NULL PRIMARY KEY,MaNhom varchar(20) NOT NULL REFERENCES dbo.NhomHang(MaNhom),TenLoai nvarchar(100) NOT NULL);
    IF OBJECT_ID(N'dbo.DonViTinh') IS NULL CREATE TABLE dbo.DonViTinh(MaDonVi varchar(20) NOT NULL PRIMARY KEY,TenDonVi nvarchar(50) NOT NULL);
    IF OBJECT_ID(N'dbo.ChatLieu') IS NULL CREATE TABLE dbo.ChatLieu(MaChatLieu varchar(20) NOT NULL PRIMARY KEY,TenChatLieu nvarchar(100) NOT NULL);
    IF OBJECT_ID(N'dbo.CongDung') IS NULL CREATE TABLE dbo.CongDung(MaCongDung varchar(20) NOT NULL PRIMARY KEY,TenCongDung nvarchar(100) NOT NULL);
    IF OBJECT_ID(N'dbo.NoiSanXuat') IS NULL CREATE TABLE dbo.NoiSanXuat(MaNoiSX varchar(20) NOT NULL PRIMARY KEY,TenNoiSX nvarchar(100) NOT NULL);
    IF OBJECT_ID(N'dbo.CaLam') IS NULL CREATE TABLE dbo.CaLam(MaCa varchar(20) NOT NULL PRIMARY KEY,TenCa nvarchar(50) NOT NULL);
    IF OBJECT_ID(N'dbo.CongViec') IS NULL CREATE TABLE dbo.CongViec(MaCV varchar(20) NOT NULL PRIMARY KEY,TenCV nvarchar(100) NOT NULL);
    IF OBJECT_ID(N'dbo.KhachHang') IS NULL CREATE TABLE dbo.KhachHang(MaKhach varchar(20) NOT NULL PRIMARY KEY,TenKhach nvarchar(100) NOT NULL,DiaChi nvarchar(200) NULL,DienThoai varchar(20) NULL);
    IF OBJECT_ID(N'dbo.NhaCungCap') IS NULL CREATE TABLE dbo.NhaCungCap(MaNCC varchar(20) NOT NULL PRIMARY KEY,TenNCC nvarchar(150) NOT NULL,DiaChi nvarchar(200) NULL,DienThoai varchar(20) NULL);
    IF OBJECT_ID(N'dbo.NhânVien') IS NULL
        CREATE TABLE dbo.[NhânVien](MaNV varchar(20) NOT NULL PRIMARY KEY,TenNV nvarchar(100) NOT NULL,GioiTinh nvarchar(10) NULL,[NgàySinh] date NULL,DienThoai varchar(20) NULL,DiaChi nvarchar(200) NULL,MaNhom varchar(20) NULL REFERENCES dbo.NhomHang(MaNhom),MaCa varchar(20) NULL REFERENCES dbo.CaLam(MaCa),MaCV varchar(20) NULL REFERENCES dbo.CongViec(MaCV));
    IF OBJECT_ID(N'dbo.DMHangHoa') IS NULL
        CREATE TABLE dbo.DMHangHoa(MaVach varchar(50) NOT NULL PRIMARY KEY,TenHangHoa nvarchar(150) NOT NULL,MaNhom varchar(20) NOT NULL REFERENCES dbo.NhomHang(MaNhom),MaLoai varchar(20) NOT NULL REFERENCES dbo.TheLoai(MaLoai),NhapKhau int NULL DEFAULT 0,MaDonVi varchar(20) NOT NULL REFERENCES dbo.DonViTinh(MaDonVi),MaChatLieu varchar(20) NULL REFERENCES dbo.ChatLieu(MaChatLieu),MaCongDung varchar(20) NULL REFERENCES dbo.CongDung(MaCongDung),MaNoiSX varchar(20) NULL REFERENCES dbo.NoiSanXuat(MaNoiSX),SoLuong int NOT NULL DEFAULT 0,DonGiaNhap decimal(18,2) NOT NULL DEFAULT 0,DonGiaBan decimal(18,2) NOT NULL DEFAULT 0,Anh varchar(255) NULL,GhiChu nvarchar(max) NULL);
    IF OBJECT_ID(N'dbo.HoaDonBan') IS NULL
        CREATE TABLE dbo.HoaDonBan(SoHDB varchar(50) NOT NULL PRIMARY KEY,MaNV varchar(20) NOT NULL REFERENCES dbo.[NhânVien](MaNV),NgayBan datetime NOT NULL,MaKhach varchar(20) NOT NULL REFERENCES dbo.KhachHang(MaKhach),TongTien decimal(18,2) NOT NULL);
    IF EXISTS(SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'dbo.DMHangHoa') AND name=N'Anh' AND (system_type_id<>231 OR max_length<1000))
        ALTER TABLE dbo.DMHangHoa ALTER COLUMN Anh nvarchar(500) NULL;
    IF OBJECT_ID(N'dbo.HoaDonNhap') IS NULL
        CREATE TABLE dbo.HoaDonNhap(SoHDN varchar(50) NOT NULL PRIMARY KEY,MaNV varchar(20) NOT NULL REFERENCES dbo.[NhânVien](MaNV),NgayNhap datetime NOT NULL,MaNCC varchar(20) NOT NULL REFERENCES dbo.NhaCungCap(MaNCC),TongTien decimal(18,2) NOT NULL);
    IF OBJECT_ID(N'dbo.ChiTietHDB') IS NULL
        CREATE TABLE dbo.ChiTietHDB(SoHDB varchar(50) NOT NULL REFERENCES dbo.HoaDonBan(SoHDB),MaVach varchar(50) NOT NULL REFERENCES dbo.DMHangHoa(MaVach),SoLuong int NOT NULL,GiamGia decimal(18,2) NULL,ThanhTien decimal(18,2) NOT NULL,CONSTRAINT PK_ChiTietHDB PRIMARY KEY(SoHDB,MaVach));
    IF OBJECT_ID(N'dbo.ChiTietHDN') IS NULL
        CREATE TABLE dbo.ChiTietHDN(SoHDN varchar(50) NOT NULL REFERENCES dbo.HoaDonNhap(SoHDN),MaVach varchar(50) NOT NULL REFERENCES dbo.DMHangHoa(MaVach),SoLuong int NOT NULL,DonGia decimal(18,2) NOT NULL,GiamGia decimal(18,2) NULL,ThanhTien decimal(18,2) NOT NULL,CONSTRAINT PK_ChiTietHDN PRIMARY KEY(SoHDN,MaVach));
    -- Never manufacture historical prices/discounts for legacy sales.
    IF COL_LENGTH(N'dbo.ChiTietHDB',N'DonGia') IS NULL AND EXISTS(SELECT 1 FROM dbo.ChiTietHDB)
        THROW 50001,N'Database có hóa đơn bán cũ chưa lưu đơn giá. Cần kiểm tra/chuyển đổi dữ liệu trước khi nâng cấp.',1;
    IF COL_LENGTH(N'dbo.ChiTietHDN',N'TenHang') IS NULL AND EXISTS(SELECT 1 FROM dbo.ChiTietHDN)
        THROW 50002,N'Database có hóa đơn nhập cũ. Cần xác nhận cách chuyển đổi giảm giá trước khi nâng cấp.',1;
    IF COL_LENGTH(N'dbo.ChiTietHDB',N'DonGia') IS NULL EXEC(N'ALTER TABLE dbo.ChiTietHDB ADD DonGia decimal(18,2) NOT NULL CONSTRAINT DF_CTHDB_DonGia DEFAULT 0');
    IF COL_LENGTH(N'dbo.ChiTietHDB',N'GiaVon') IS NULL EXEC(N'ALTER TABLE dbo.ChiTietHDB ADD GiaVon decimal(18,2) NOT NULL CONSTRAINT DF_CTHDB_GiaVon DEFAULT 0');
    IF COL_LENGTH(N'dbo.ChiTietHDB',N'TenHang') IS NULL EXEC(N'ALTER TABLE dbo.ChiTietHDB ADD TenHang nvarchar(150) NOT NULL CONSTRAINT DF_CTHDB_TenHang DEFAULT N''''');
    IF COL_LENGTH(N'dbo.ChiTietHDN',N'TenHang') IS NULL EXEC(N'ALTER TABLE dbo.ChiTietHDN ADD TenHang nvarchar(150) NOT NULL CONSTRAINT DF_CTHDN_TenHang DEFAULT N''''');
    IF OBJECT_ID(N'dbo.TaiKhoan') IS NULL
        CREATE TABLE dbo.TaiKhoan(TenDangNhap nvarchar(40) NOT NULL PRIMARY KEY,PasswordHash varchar(100) NOT NULL,Salt varchar(50) NOT NULL,VaiTro varchar(20) NOT NULL CHECK(VaiTro IN('Admin','ThuNgan','ThuKho')),MaNV varchar(20) NULL REFERENCES dbo.[NhânVien](MaNV),HoatDong bit NOT NULL DEFAULT 1,CONSTRAINT CK_TaiKhoan_Employee CHECK(VaiTro='Admin' OR MaNV IS NOT NULL));
    IF OBJECT_ID(N'dbo.KiemKe') IS NULL
        CREATE TABLE dbo.KiemKe(MaKiemKe varchar(50) NOT NULL PRIMARY KEY,NgayKiemKe datetime2 NOT NULL DEFAULT SYSDATETIME(),GhiChu nvarchar(500) NOT NULL,NguoiThucHien nvarchar(40) NOT NULL REFERENCES dbo.TaiKhoan(TenDangNhap));
    IF OBJECT_ID(N'dbo.ChiTietKiemKe') IS NULL
        CREATE TABLE dbo.ChiTietKiemKe(MaKiemKe varchar(50) NOT NULL REFERENCES dbo.KiemKe(MaKiemKe),MaVach varchar(50) NOT NULL REFERENCES dbo.DMHangHoa(MaVach),TonHeThong int NOT NULL CHECK(TonHeThong>=0),TonThucTe int NOT NULL CHECK(TonThucTe>=0),CONSTRAINT PK_ChiTietKiemKe PRIMARY KEY(MaKiemKe,MaVach));
    IF OBJECT_ID(N'dbo.LichSuKho') IS NULL
        CREATE TABLE dbo.LichSuKho(Id bigint IDENTITY NOT NULL PRIMARY KEY,MaVach varchar(50) NOT NULL REFERENCES dbo.DMHangHoa(MaVach),Ngay datetime2 NOT NULL DEFAULT SYSDATETIME(),ThayDoi int NOT NULL,TonSau int NOT NULL CHECK(TonSau>=0),Loai nvarchar(30) NOT NULL,ThamChieu varchar(50) NOT NULL,NguoiThucHien nvarchar(40) NOT NULL REFERENCES dbo.TaiKhoan(TenDangNhap));
    IF NOT EXISTS(SELECT 1 FROM sys.check_constraints WHERE name='CK_HangHoa_Values')
        ALTER TABLE dbo.DMHangHoa WITH CHECK ADD CONSTRAINT CK_HangHoa_Values CHECK(SoLuong>=0 AND DonGiaNhap>=0 AND DonGiaBan>=0 AND (NhapKhau IS NULL OR NhapKhau IN(0,1)));
    IF NOT EXISTS(SELECT 1 FROM sys.check_constraints WHERE name='CK_CTHDB_Values')
        EXEC(N'ALTER TABLE dbo.ChiTietHDB WITH CHECK ADD CONSTRAINT CK_CTHDB_Values CHECK(SoLuong>0 AND GiamGia BETWEEN 0 AND 100 AND ThanhTien>=0 AND DonGia>0 AND GiaVon>=0)');
    IF NOT EXISTS(SELECT 1 FROM sys.check_constraints WHERE name='CK_CTHDN_Values')
        ALTER TABLE dbo.ChiTietHDN WITH CHECK ADD CONSTRAINT CK_CTHDN_Values CHECK(SoLuong>0 AND DonGia>0 AND GiamGia BETWEEN 0 AND 100 AND ThanhTien>=0);
    IF NOT EXISTS(SELECT 1 FROM sys.foreign_keys WHERE parent_object_id=OBJECT_ID(N'dbo.ChiTietHDN') AND referenced_object_id=OBJECT_ID(N'dbo.HoaDonNhap'))
        ALTER TABLE dbo.ChiTietHDN WITH CHECK ADD CONSTRAINT FK_CTHDN_Header FOREIGN KEY(SoHDN) REFERENCES dbo.HoaDonNhap(SoHDN);
    IF NOT EXISTS(SELECT 1 FROM sys.foreign_keys WHERE parent_object_id=OBJECT_ID(N'dbo.ChiTietHDN') AND referenced_object_id=OBJECT_ID(N'dbo.DMHangHoa'))
        ALTER TABLE dbo.ChiTietHDN WITH CHECK ADD CONSTRAINT FK_CTHDN_Product FOREIGN KEY(MaVach) REFERENCES dbo.DMHangHoa(MaVach);
    IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.HoaDonBan') AND name='IX_HDB_Ngay') CREATE INDEX IX_HDB_Ngay ON dbo.HoaDonBan(NgayBan,MaNV) INCLUDE(TongTien,MaKhach);
    IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.HoaDonNhap') AND name='IX_HDN_Ngay') CREATE INDEX IX_HDN_Ngay ON dbo.HoaDonNhap(NgayNhap,MaNCC) INCLUDE(TongTien);
    IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.LichSuKho') AND name='IX_Kho_MaVach') CREATE INDEX IX_Kho_MaVach ON dbo.LichSuKho(MaVach,Ngay);
    COMMIT TRANSACTION;
    PRINT N'Database đã sẵn sàng. Mở ứng dụng và tạo tài khoản quản trị đầu tiên.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

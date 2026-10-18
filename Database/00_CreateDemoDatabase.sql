-- Create an isolated copy of the project schema for demo data.
-- Run this script in SSMS or sqlcmd against the target SQL Server instance.
-- It does not drop or alter the production database.
USE master;
GO
IF DB_ID(N'QuanLyBanHangSupermarket_Demo') IS NULL
    EXEC(N'CREATE DATABASE QuanLyBanHangSupermarket_Demo');
GO
USE QuanLyBanHangSupermarket_Demo;
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
    IF OBJECT_ID(N'dbo.NhânVien') IS NULL CREATE TABLE dbo.[NhânVien](MaNV varchar(20) NOT NULL PRIMARY KEY,TenNV nvarchar(100) NOT NULL,GioiTinh nvarchar(10) NULL,[NgàySinh] date NULL,DienThoai varchar(20) NULL,DiaChi nvarchar(200) NULL,MaNhom varchar(20) NULL REFERENCES dbo.NhomHang(MaNhom),MaCa varchar(20) NULL REFERENCES dbo.CaLam(MaCa),MaCV varchar(20) NULL REFERENCES dbo.CongViec(MaCV));
    IF OBJECT_ID(N'dbo.DMHangHoa') IS NULL CREATE TABLE dbo.DMHangHoa(MaVach varchar(50) NOT NULL PRIMARY KEY,TenHangHoa nvarchar(150) NOT NULL,MaNhom varchar(20) NOT NULL REFERENCES dbo.NhomHang(MaNhom),MaLoai varchar(20) NOT NULL REFERENCES dbo.TheLoai(MaLoai),NhapKhau int NULL DEFAULT 0,MaDonVi varchar(20) NOT NULL REFERENCES dbo.DonViTinh(MaDonVi),MaChatLieu varchar(20) NULL REFERENCES dbo.ChatLieu(MaChatLieu),MaCongDung varchar(20) NULL REFERENCES dbo.CongDung(MaCongDung),MaNoiSX varchar(20) NULL REFERENCES dbo.NoiSanXuat(MaNoiSX),SoLuong int NOT NULL DEFAULT 0,DonGiaNhap decimal(18,2) NOT NULL DEFAULT 0,DonGiaBan decimal(18,2) NOT NULL DEFAULT 0,Anh nvarchar(500) NULL,GhiChu nvarchar(max) NULL);
    IF OBJECT_ID(N'dbo.HoaDonBan') IS NULL CREATE TABLE dbo.HoaDonBan(SoHDB varchar(50) NOT NULL PRIMARY KEY,MaNV varchar(20) NOT NULL REFERENCES dbo.[NhânVien](MaNV),NgayBan datetime NOT NULL,MaKhach varchar(20) NOT NULL REFERENCES dbo.KhachHang(MaKhach),TongTien decimal(18,2) NOT NULL);
    IF OBJECT_ID(N'dbo.HoaDonNhap') IS NULL CREATE TABLE dbo.HoaDonNhap(SoHDN varchar(50) NOT NULL PRIMARY KEY,MaNV varchar(20) NOT NULL REFERENCES dbo.[NhânVien](MaNV),NgayNhap datetime NOT NULL,MaNCC varchar(20) NOT NULL REFERENCES dbo.NhaCungCap(MaNCC),TongTien decimal(18,2) NOT NULL);
    IF OBJECT_ID(N'dbo.ChiTietHDB') IS NULL CREATE TABLE dbo.ChiTietHDB(SoHDB varchar(50) NOT NULL REFERENCES dbo.HoaDonBan(SoHDB),MaVach varchar(50) NOT NULL REFERENCES dbo.DMHangHoa(MaVach),TenHang nvarchar(150) NOT NULL,SoLuong int NOT NULL,DonGia decimal(18,2) NOT NULL,GiaVon decimal(18,2) NOT NULL,GiamGia decimal(18,2) NULL,ThanhTien decimal(18,2) NOT NULL,CONSTRAINT PK_ChiTietHDB PRIMARY KEY(SoHDB,MaVach));
    IF OBJECT_ID(N'dbo.ChiTietHDN') IS NULL CREATE TABLE dbo.ChiTietHDN(SoHDN varchar(50) NOT NULL REFERENCES dbo.HoaDonNhap(SoHDN),MaVach varchar(50) NOT NULL REFERENCES dbo.DMHangHoa(MaVach),TenHang nvarchar(150) NOT NULL,SoLuong int NOT NULL,DonGia decimal(18,2) NOT NULL,GiamGia decimal(18,2) NULL,ThanhTien decimal(18,2) NOT NULL,CONSTRAINT PK_ChiTietHDN PRIMARY KEY(SoHDN,MaVach));
    IF OBJECT_ID(N'dbo.TaiKhoan') IS NULL CREATE TABLE dbo.TaiKhoan(TenDangNhap nvarchar(40) NOT NULL PRIMARY KEY,PasswordHash varchar(100) NOT NULL,Salt varchar(50) NOT NULL,VaiTro varchar(20) NOT NULL CHECK(VaiTro IN('Admin','ThuNgan','ThuKho')),MaNV varchar(20) NULL REFERENCES dbo.[NhânVien](MaNV),HoatDong bit NOT NULL DEFAULT 1,CONSTRAINT CK_TaiKhoan_Employee CHECK(VaiTro='Admin' OR MaNV IS NOT NULL));
    IF OBJECT_ID(N'dbo.KiemKe') IS NULL CREATE TABLE dbo.KiemKe(MaKiemKe varchar(50) NOT NULL PRIMARY KEY,NgayKiemKe datetime2 NOT NULL DEFAULT SYSDATETIME(),GhiChu nvarchar(500) NOT NULL,NguoiThucHien nvarchar(40) NOT NULL REFERENCES dbo.TaiKhoan(TenDangNhap));
    IF OBJECT_ID(N'dbo.ChiTietKiemKe') IS NULL CREATE TABLE dbo.ChiTietKiemKe(MaKiemKe varchar(50) NOT NULL REFERENCES dbo.KiemKe(MaKiemKe),MaVach varchar(50) NOT NULL REFERENCES dbo.DMHangHoa(MaVach),TonHeThong int NOT NULL CHECK(TonHeThong>=0),TonThucTe int NOT NULL CHECK(TonThucTe>=0),CONSTRAINT PK_ChiTietKiemKe PRIMARY KEY(MaKiemKe,MaVach));
    IF OBJECT_ID(N'dbo.LichSuKho') IS NULL CREATE TABLE dbo.LichSuKho(Id bigint IDENTITY NOT NULL PRIMARY KEY,MaVach varchar(50) NOT NULL REFERENCES dbo.DMHangHoa(MaVach),Ngay datetime2 NOT NULL DEFAULT SYSDATETIME(),ThayDoi int NOT NULL,TonSau int NOT NULL CHECK(TonSau>=0),Loai nvarchar(30) NOT NULL,ThamChieu varchar(50) NOT NULL,NguoiThucHien nvarchar(40) NOT NULL REFERENCES dbo.TaiKhoan(TenDangNhap));
    IF NOT EXISTS(SELECT 1 FROM sys.check_constraints WHERE name=N'CK_Demo_CTHDB') ALTER TABLE dbo.ChiTietHDB ADD CONSTRAINT CK_Demo_CTHDB CHECK(SoLuong>0 AND DonGia>0 AND GiaVon>=0 AND GiamGia BETWEEN 0 AND 100 AND ThanhTien>=0);
    IF NOT EXISTS(SELECT 1 FROM sys.check_constraints WHERE name=N'CK_Demo_CTHDN') ALTER TABLE dbo.ChiTietHDN ADD CONSTRAINT CK_Demo_CTHDN CHECK(SoLuong>0 AND DonGia>0 AND GiamGia BETWEEN 0 AND 100 AND ThanhTien>=0);
    IF NOT EXISTS(SELECT 1 FROM sys.check_constraints WHERE name=N'CK_Demo_HangHoa') ALTER TABLE dbo.DMHangHoa ADD CONSTRAINT CK_Demo_HangHoa CHECK(SoLuong>=0 AND DonGiaNhap>=0 AND DonGiaBan>=0 AND (NhapKhau IS NULL OR NhapKhau IN(0,1)));
    IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.HoaDonBan') AND name=N'IX_Demo_HDB_Ngay') CREATE INDEX IX_Demo_HDB_Ngay ON dbo.HoaDonBan(NgayBan,MaNV) INCLUDE(TongTien,MaKhach);
    IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.HoaDonNhap') AND name=N'IX_Demo_HDN_Ngay') CREATE INDEX IX_Demo_HDN_Ngay ON dbo.HoaDonNhap(NgayNhap,MaNCC) INCLUDE(TongTien);
    IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.ChiTietHDB') AND name=N'IX_Demo_CTHDB_MaVach') CREATE INDEX IX_Demo_CTHDB_MaVach ON dbo.ChiTietHDB(MaVach,SoHDB) INCLUDE(SoLuong,ThanhTien);
    IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.ChiTietHDN') AND name=N'IX_Demo_CTHDN_MaVach') CREATE INDEX IX_Demo_CTHDN_MaVach ON dbo.ChiTietHDN(MaVach,SoHDN) INCLUDE(SoLuong,ThanhTien);
    COMMIT TRANSACTION;
    PRINT N'Database demo đã sẵn sàng: QuanLyBanHangSupermarket_Demo';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

-- Synthetic sample data for the isolated QuanLyBanHangSupermarket_Demo database.
-- Run once per demo database. A marker prevents later runs from resetting stock.
-- Images are separate local files; Anh stores the project-relative path expected
-- when the app is launched from QuanLyBanHangSupermarket\bin\Release.
-- Image source URLs and attribution are listed in Database\seed_assets\SOURCES.md.
USE QuanLyBanHangSupermarket_Demo;
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;
IF OBJECT_ID(N'dbo.DemoSeedLog') IS NULL
    CREATE TABLE dbo.DemoSeedLog(SeedName varchar(40) NOT NULL PRIMARY KEY,AppliedAt datetime2 NOT NULL DEFAULT SYSDATETIME());
BEGIN TRY
    BEGIN TRANSACTION;
    IF EXISTS(SELECT 1 FROM dbo.DemoSeedLog WHERE SeedName='SeedDemoData_v2')
        THROW 51001,N'Dữ liệu mẫu phiên bản này đã được nạp; không chạy lại để tránh đặt lại tồn kho.',1;

    INSERT dbo.NhomHang(MaNhom,TenNhom)
    SELECT v.MaNhom,v.TenNhom FROM (VALUES
      ('DEMO_TP',N'Thực phẩm khô'),('DEMO_DU',N'Đồ uống'),('DEMO_GD',N'Gia dụng'),
      ('DEMO_G04',N'Rau củ quả'),('DEMO_G05',N'Thịt và thủy sản'),('DEMO_G06',N'Bánh mì và bánh ngọt'),
      ('DEMO_G07',N'Đông lạnh'),('DEMO_G08',N'Chăm sóc cá nhân'),('DEMO_G09',N'Giấy và văn phòng phẩm'),
      ('DEMO_G10',N'Đồ dùng trẻ em'),('DEMO_G11',N'Chăm sóc thú cưng'),('DEMO_G12',N'Gia vị và nước sốt'),
      ('DEMO_G13',N'Ngũ cốc và hạt'),('DEMO_G14',N'Đồ hộp'),('DEMO_G15',N'Đồ dùng nhà cửa')) v(MaNhom,TenNhom)
    WHERE NOT EXISTS(SELECT 1 FROM dbo.NhomHang x WHERE x.MaNhom=v.MaNhom);
    INSERT dbo.TheLoai(MaLoai,MaNhom,TenLoai)
    SELECT v.MaLoai,v.MaNhom,v.TenLoai FROM (VALUES
      ('DEMO_TL_GAO','DEMO_TP',N'Gạo và ngũ cốc'),('DEMO_TL_BANH','DEMO_TP',N'Bánh và mì'),
      ('DEMO_TL_SUA','DEMO_DU',N'Sữa và nước dinh dưỡng'),('DEMO_TL_NUOC','DEMO_DU',N'Nước giải khát'),
      ('DEMO_TL_VS','DEMO_GD',N'Vệ sinh nhà cửa'),('DEMO_TL_BEP','DEMO_GD',N'Dụng cụ nhà bếp'),
      ('DEMO_TL_RAU','DEMO_G04',N'Rau củ tươi'),('DEMO_TL_TRAI','DEMO_G04',N'Trái cây tươi'),('DEMO_TL_THIT','DEMO_G05',N'Thịt đóng gói'),
      ('DEMO_TL_BANHMI','DEMO_G06',N'Bánh mì'),('DEMO_TL_DONG','DEMO_G07',N'Thực phẩm đông lạnh'),('DEMO_TL_CA_NHAN','DEMO_G08',N'Chăm sóc cơ thể'),
      ('DEMO_TL_GIAY','DEMO_G09',N'Giấy và khăn'),('DEMO_TL_TRE','DEMO_G10',N'Đồ dùng trẻ em'),('DEMO_TL_THU_CUNG','DEMO_G11',N'Thức ăn thú cưng')) v(MaLoai,MaNhom,TenLoai)
    WHERE NOT EXISTS(SELECT 1 FROM dbo.TheLoai x WHERE x.MaLoai=v.MaLoai);
    INSERT dbo.DonViTinh(MaDonVi,TenDonVi)
    SELECT v.MaDonVi,v.TenDonVi FROM (VALUES
      ('DEMO_GOI',N'Gói'),('DEMO_HOP',N'Hộp'),('DEMO_CHAl',N'Chai'),('DEMO_LON',N'Lon'),('DEMO_KG',N'Kg'),('DEMO_CAI',N'Cái'),
      ('DEMO_DVT07',N'Bịch'),('DEMO_DVT08',N'Túi'),('DEMO_DVT09',N'Cuộn'),('DEMO_DVT10',N'Thùng'),('DEMO_DVT11',N'Vỉ'),
      ('DEMO_DVT12',N'Khay'),('DEMO_DVT13',N'Bao'),('DEMO_DVT14',N'Bộ'),('DEMO_DVT15',N'Quả')) v(MaDonVi,TenDonVi)
    WHERE NOT EXISTS(SELECT 1 FROM dbo.DonViTinh x WHERE x.MaDonVi=v.MaDonVi);
    INSERT dbo.ChatLieu(MaChatLieu,TenChatLieu)
    SELECT v.MaChatLieu,v.TenChatLieu FROM (VALUES
      ('DEMO_PLASTIC',N'Nhựa'),('DEMO_PAPER',N'Giấy'),('DEMO_GLASS',N'Thủy tinh'),('DEMO_METAL',N'Kim loại'),('DEMO_FOOD',N'Thực phẩm'),
      ('DEMO_MAT06',N'Cotton'),('DEMO_MAT07',N'Gỗ'),('DEMO_MAT08',N'Cao su'),('DEMO_MAT09',N'Vải không dệt'),('DEMO_MAT10',N'Bột giấy'),
      ('DEMO_MAT11',N'Nhôm'),('DEMO_MAT12',N'Thép không gỉ'),('DEMO_MAT13',N'Thành phần hữu cơ'),('DEMO_MAT14',N'Gốm sứ'),('DEMO_MAT15',N'Composite')) v(MaChatLieu,TenChatLieu)
    WHERE NOT EXISTS(SELECT 1 FROM dbo.ChatLieu x WHERE x.MaChatLieu=v.MaChatLieu);
    INSERT dbo.CongDung(MaCongDung,TenCongDung)
    SELECT v.MaCongDung,v.TenCongDung FROM (VALUES
      ('DEMO_EAT',N'Tiêu dùng thực phẩm'),('DEMO_DRINK',N'Giải khát'),('DEMO_CLEAN',N'Làm sạch'),('DEMO_COOK',N'Nấu ăn'),
      ('DEMO_USE05',N'Ăn sáng'),('DEMO_USE06',N'Bổ sung dinh dưỡng'),('DEMO_USE07',N'Bảo quản thực phẩm'),('DEMO_USE08',N'Chăm sóc tóc'),
      ('DEMO_USE09',N'Chăm sóc da'),('DEMO_USE10',N'Giặt giũ'),('DEMO_USE11',N'Vệ sinh cá nhân'),('DEMO_USE12',N'Cho thú cưng'),
      ('DEMO_USE13',N'Chế biến món ăn'),('DEMO_USE14',N'Đựng và bảo quản'),('DEMO_USE15',N'Tiện ích gia đình')) v(MaCongDung,TenCongDung)
    WHERE NOT EXISTS(SELECT 1 FROM dbo.CongDung x WHERE x.MaCongDung=v.MaCongDung);
    INSERT dbo.NoiSanXuat(MaNoiSX,TenNoiSX)
    SELECT v.MaNoiSX,v.TenNoiSX FROM (VALUES
      ('DEMO_VN',N'Việt Nam'),('DEMO_TH',N'Thái Lan'),('DEMO_JP',N'Nhật Bản'),('DEMO_KR',N'Hàn Quốc'),
      ('DEMO_NSX05',N'Úc'),('DEMO_NSX06',N'Mỹ'),('DEMO_NSX07',N'Canada'),('DEMO_NSX08',N'Pháp'),('DEMO_NSX09',N'Ý'),
      ('DEMO_NSX10',N'Đức'),('DEMO_NSX11',N'Indonesia'),('DEMO_NSX12',N'Malaysia'),('DEMO_NSX13',N'Singapore'),('DEMO_NSX14',N'Ấn Độ'),('DEMO_NSX15',N'Trung Quốc')) v(MaNoiSX,TenNoiSX)
    WHERE NOT EXISTS(SELECT 1 FROM dbo.NoiSanXuat x WHERE x.MaNoiSX=v.MaNoiSX);
    INSERT dbo.CaLam(MaCa,TenCa)
    SELECT v.MaCa,v.TenCa FROM (VALUES('DEMO_CA1',N'Ca sáng'),('DEMO_CA2',N'Ca chiều'),('DEMO_CA03',N'Ca tối'),('DEMO_CA04',N'Ca hành chính'),('DEMO_CA05',N'Ca đêm'),('DEMO_CA06',N'Ca cuối tuần'),('DEMO_CA07',N'Ca tăng cường'),('DEMO_CA08',N'Ca giao hàng'),('DEMO_CA09',N'Ca mở cửa'),('DEMO_CA10',N'Ca đóng cửa'),('DEMO_CA11',N'Ca sáng cuối tuần'),('DEMO_CA12',N'Ca chiều cuối tuần'),('DEMO_CA13',N'Ca lễ'),('DEMO_CA14',N'Ca bán thời gian'),('DEMO_CA15',N'Ca linh hoạt')) v(MaCa,TenCa)
    WHERE NOT EXISTS(SELECT 1 FROM dbo.CaLam x WHERE x.MaCa=v.MaCa);
    INSERT dbo.CongViec(MaCV,TenCV)
    SELECT v.MaCV,v.TenCV FROM (VALUES('DEMO_BH',N'Nhân viên bán hàng'),('DEMO_KHO',N'Nhân viên kho'),('DEMO_QL',N'Quản lý'),('DEMO_CV04',N'Thu ngân'),('DEMO_CV05',N'Trưởng ca'),('DEMO_CV06',N'Kiểm kê'),('DEMO_CV07',N'Nhận hàng'),('DEMO_CV08',N'Tư vấn khách hàng'),('DEMO_CV09',N'Đóng gói'),('DEMO_CV10',N'Trưng bày hàng hóa'),('DEMO_CV11',N'Quản lý chất lượng'),('DEMO_CV12',N'Giao hàng'),('DEMO_CV13',N'Bảo trì'),('DEMO_CV14',N'Kế toán'),('DEMO_CV15',N'Điều phối kho')) v(MaCV,TenCV)
    WHERE NOT EXISTS(SELECT 1 FROM dbo.CongViec x WHERE x.MaCV=v.MaCV);

    INSERT dbo.[NhânVien](MaNV,TenNV,GioiTinh,[NgàySinh],DienThoai,DiaChi,MaNhom,MaCa,MaCV)
    SELECT v.MaNV,v.TenNV,v.GioiTinh,v.NgaySinh,v.DienThoai,v.DiaChi,v.MaNhom,v.MaCa,v.MaCV
    FROM (VALUES
      ('DEMO_NV01',N'Nguyễn Minh An',N'Nam',CONVERT(date,'1998-04-12'),'0901000001',N'Quận 1, TP. Hồ Chí Minh','DEMO_TP','DEMO_CA1','DEMO_BH'),
      ('DEMO_NV02',N'Trần Gia Hân',N'Nữ',CONVERT(date,'1999-08-23'),'0901000002',N'Quận 3, TP. Hồ Chí Minh','DEMO_DU','DEMO_CA2','DEMO_BH'),
      ('DEMO_NV03',N'Lê Quốc Bảo',N'Nam',CONVERT(date,'1996-01-30'),'0901000003',N'Thủ Đức, TP. Hồ Chí Minh','DEMO_GD','DEMO_CA1','DEMO_KHO'),
      ('DEMO_NV04',N'Phạm Ngọc Mai',N'Nữ',CONVERT(date,'1997-11-05'),'0901000004',N'Bình Thạnh, TP. Hồ Chí Minh','DEMO_TP','DEMO_CA2','DEMO_QL'),
      ('DEMO_NV05',N'Đặng Tuấn Kiệt',N'Nam',CONVERT(date,'1995-06-14'),'0901000005',N'Quận 7, TP. Hồ Chí Minh','DEMO_G04','DEMO_CA03','DEMO_CV04'),
      ('DEMO_NV06',N'Vũ Thảo Nguyên',N'Nữ',CONVERT(date,'2000-02-09'),'0901000006',N'Quận 10, TP. Hồ Chí Minh','DEMO_G05','DEMO_CA04','DEMO_CV05'),
      ('DEMO_NV07',N'Nguyễn Hoài Phương',N'Nữ',CONVERT(date,'1998-10-18'),'0901000007',N'Phú Nhuận, TP. Hồ Chí Minh','DEMO_G06','DEMO_CA05','DEMO_CV06'),
      ('DEMO_NV08',N'Trương Thành Đạt',N'Nam',CONVERT(date,'1994-12-02'),'0901000008',N'Quận 4, TP. Hồ Chí Minh','DEMO_G07','DEMO_CA06','DEMO_CV07'),
      ('DEMO_NV09',N'Bùi Yến Nhi',N'Nữ',CONVERT(date,'1999-03-27'),'0901000009',N'Quận 11, TP. Hồ Chí Minh','DEMO_G08','DEMO_CA07','DEMO_CV08'),
      ('DEMO_NV10',N'Phan Minh Quân',N'Nam',CONVERT(date,'1997-07-16'),'0901000010',N'Quận 6, TP. Hồ Chí Minh','DEMO_G09','DEMO_CA08','DEMO_CV09'),
      ('DEMO_NV11',N'Đỗ Khánh Chi',N'Nữ',CONVERT(date,'2001-05-20'),'0901000011',N'Quận 8, TP. Hồ Chí Minh','DEMO_G10','DEMO_CA09','DEMO_CV10'),
      ('DEMO_NV12',N'Hoàng Anh Dũng',N'Nam',CONVERT(date,'1996-09-03'),'0901000012',N'Quận 12, TP. Hồ Chí Minh','DEMO_G11','DEMO_CA10','DEMO_CV11'),
      ('DEMO_NV13',N'Lý Ngọc Trâm',N'Nữ',CONVERT(date,'1998-12-11'),'0901000013',N'Nhà Bè, TP. Hồ Chí Minh','DEMO_G12','DEMO_CA11','DEMO_CV12'),
      ('DEMO_NV14',N'Mai Quốc Huy',N'Nam',CONVERT(date,'1995-04-25'),'0901000014',N'Bình Chánh, TP. Hồ Chí Minh','DEMO_G13','DEMO_CA12','DEMO_CV13'),
      ('DEMO_NV15',N'Ngô Hà My',N'Nữ',CONVERT(date,'2000-07-07'),'0901000015',N'Quận 2, TP. Hồ Chí Minh','DEMO_G14','DEMO_CA13','DEMO_CV14')) v(MaNV,TenNV,GioiTinh,NgaySinh,DienThoai,DiaChi,MaNhom,MaCa,MaCV)
    WHERE NOT EXISTS(SELECT 1 FROM dbo.[NhânVien] x WHERE x.MaNV=v.MaNV);
    INSERT dbo.KhachHang(MaKhach,TenKhach,DiaChi,DienThoai)
    SELECT v.MaKhach,v.TenKhach,v.DiaChi,v.DienThoai FROM (VALUES
      ('DEMO_KH01',N'Đỗ Thanh Vy',N'Quận 1, TP. Hồ Chí Minh','0912000001'),('DEMO_KH02',N'Võ Hoàng Nam',N'Quận 5, TP. Hồ Chí Minh','0912000002'),
      ('DEMO_KH03',N'Bùi Khánh Linh',N'Gò Vấp, TP. Hồ Chí Minh','0912000003'),('DEMO_KH04',N'Phan Đức Long',N'Tân Bình, TP. Hồ Chí Minh','0912000004'),
      ('DEMO_KH05',N'Ngô Mỹ Duyên',N'Bình Tân, TP. Hồ Chí Minh','0912000005'),('DEMO_KH06',N'Hoàng Nhật Minh',N'Thủ Đức, TP. Hồ Chí Minh','0912000006'),
      ('DEMO_KH07',N'Đặng Phương Thảo',N'Quận 2, TP. Hồ Chí Minh','0912000007'),('DEMO_KH08',N'Vương Quốc Thịnh',N'Quận 9, TP. Hồ Chí Minh','0912000008'),
      ('DEMO_KH09',N'Nguyễn Bảo Trân',N'Quận 7, TP. Hồ Chí Minh','0912000009'),('DEMO_KH10',N'Đinh Anh Khoa',N'Quận 4, TP. Hồ Chí Minh','0912000010'),
      ('DEMO_KH11',N'Lâm Thùy Dương',N'Bình Thạnh, TP. Hồ Chí Minh','0912000011'),('DEMO_KH12',N'Phạm Đức Tài',N'Gò Vấp, TP. Hồ Chí Minh','0912000012'),
      ('DEMO_KH13',N'Trần Ngọc Ánh',N'Tân Phú, TP. Hồ Chí Minh','0912000013'),('DEMO_KH14',N'Võ Trung Hiếu',N'Quận 10, TP. Hồ Chí Minh','0912000014'),
      ('DEMO_KH15',N'Bùi Thanh Hà',N'Quận 5, TP. Hồ Chí Minh','0912000015')) v(MaKhach,TenKhach,DiaChi,DienThoai)
    WHERE NOT EXISTS(SELECT 1 FROM dbo.KhachHang x WHERE x.MaKhach=v.MaKhach);
    INSERT dbo.NhaCungCap(MaNCC,TenNCC,DiaChi,DienThoai)
    SELECT v.MaNCC,v.TenNCC,v.DiaChi,v.DienThoai FROM (VALUES
      ('DEMO_NCC01',N'Công ty Nông sản An Phú',N'Long An','0283000001'),('DEMO_NCC02',N'Thực phẩm Minh Châu',N'Bình Dương','0283000002'),
      ('DEMO_NCC03',N'Gia dụng Thành Đạt',N'TP. Hồ Chí Minh','0283000003'),('DEMO_NCC04',N'Nông sản Tươi Xanh',N'Tiền Giang','0283000004'),
      ('DEMO_NCC05',N'Thực phẩm Đông Nam',N'Đồng Nai','0283000005'),('DEMO_NCC06',N'Bánh kẹo Phúc An',N'TP. Hồ Chí Minh','0283000006'),
      ('DEMO_NCC07',N'Kho lạnh Đại Phát',N'Bình Dương','0283000007'),('DEMO_NCC08',N'Hóa mỹ phẩm Hương Việt',N'TP. Hồ Chí Minh','0283000008'),
      ('DEMO_NCC09',N'Giấy tiêu dùng Sạch',N'Long An','0283000009'),('DEMO_NCC10',N'Đồ dùng Mẹ và Bé',N'TP. Hồ Chí Minh','0283000010'),
      ('DEMO_NCC11',N'Thức ăn thú cưng PetHome',N'Bình Dương','0283000011'),('DEMO_NCC12',N'Gia vị Bếp Việt',N'Tây Ninh','0283000012'),
      ('DEMO_NCC13',N'Ngũ cốc Hạt Lành',N'Đắk Lắk','0283000013'),('DEMO_NCC14',N'Đồ hộp Biển Xanh',N'Khánh Hòa','0283000014'),
      ('DEMO_NCC15',N'Gia dụng Tiện Ích',N'TP. Hồ Chí Minh','0283000015')) v(MaNCC,TenNCC,DiaChi,DienThoai)
    WHERE NOT EXISTS(SELECT 1 FROM dbo.NhaCungCap x WHERE x.MaNCC=v.MaNCC);

    INSERT dbo.DMHangHoa(MaVach,TenHangHoa,MaNhom,MaLoai,NhapKhau,MaDonVi,MaChatLieu,MaCongDung,MaNoiSX,SoLuong,DonGiaNhap,DonGiaBan,Anh,GhiChu)
    SELECT v.MaVach,v.TenHangHoa,v.MaNhom,v.MaLoai,v.NhapKhau,v.MaDonVi,v.MaChatLieu,v.MaCongDung,v.MaNoiSX,0,0,0,v.Anh,N'Dữ liệu demo tổng hợp, không phải hàng hóa thật.'
    FROM (VALUES
      ('8930000000001',N'Gạo thơm đồng bằng 5 kg','DEMO_TP','DEMO_TL_GAO',0,'DEMO_KG','DEMO_FOOD','DEMO_EAT','DEMO_VN',N'..\..\..\Database\seed_assets\item01.jpg'),
      ('8930000000002',N'Mì ăn liền vị rau nấm lốc 5 gói','DEMO_TP','DEMO_TL_BANH',0,'DEMO_GOI','DEMO_PAPER','DEMO_EAT','DEMO_VN',N'..\..\..\Database\seed_assets\item02.jpg'),
      ('8930000000003',N'Bánh quy bơ hộp 300 g','DEMO_TP','DEMO_TL_BANH',1,'DEMO_HOP','DEMO_PAPER','DEMO_EAT','DEMO_TH',N'..\..\..\Database\seed_assets\item03.jpg'),
      ('8930000000004',N'Sữa tươi tiệt trùng 1 lít','DEMO_DU','DEMO_TL_SUA',0,'DEMO_HOP','DEMO_PAPER','DEMO_DRINK','DEMO_VN',N'..\..\..\Database\seed_assets\item04.jpg'),
      ('8930000000005',N'Nước suối chai 500 ml','DEMO_DU','DEMO_TL_NUOC',0,'DEMO_CHAl','DEMO_PLASTIC','DEMO_DRINK','DEMO_VN',N'..\..\..\Database\seed_assets\item05.jpg'),
      ('8930000000006',N'Trà xanh đóng chai 450 ml','DEMO_DU','DEMO_TL_NUOC',0,'DEMO_CHAl','DEMO_PLASTIC','DEMO_DRINK','DEMO_VN',N'..\..\..\Database\seed_assets\item06.jpg'),
      ('8930000000007',N'Cà phê hòa tan hộp 20 gói','DEMO_DU','DEMO_TL_NUOC',1,'DEMO_HOP','DEMO_PAPER','DEMO_DRINK','DEMO_VN',N'..\..\..\Database\seed_assets\item07.jpg'),
      ('8930000000008',N'Nước rửa chén hương chanh 750 ml','DEMO_GD','DEMO_TL_VS',0,'DEMO_CHAl','DEMO_PLASTIC','DEMO_CLEAN','DEMO_VN',N'..\..\..\Database\seed_assets\item08.jpg'),
      ('8930000000009',N'Nước giặt túi 2 kg','DEMO_GD','DEMO_TL_VS',0,'DEMO_GOI','DEMO_PLASTIC','DEMO_CLEAN','DEMO_TH',N'..\..\..\Database\seed_assets\item09.jpg'),
      ('8930000000010',N'Khăn giấy hộp 180 tờ','DEMO_GD','DEMO_TL_VS',0,'DEMO_HOP','DEMO_PAPER','DEMO_CLEAN','DEMO_VN',N'..\..\..\Database\seed_assets\item10.jpg'),
      ('8930000000011',N'Chảo chống dính 24 cm','DEMO_GD','DEMO_TL_BEP',0,'DEMO_CAI','DEMO_METAL','DEMO_COOK','DEMO_KR',N'..\..\..\Database\seed_assets\item11.jpg'),
      ('8930000000012',N'Ly thủy tinh 350 ml','DEMO_GD','DEMO_TL_BEP',1,'DEMO_CAI','DEMO_GLASS','DEMO_COOK','DEMO_JP',N'..\..\..\Database\seed_assets\item12.jpg'),
      ('8930000000013',N'Táo đỏ nhập khẩu 1 kg','DEMO_G04','DEMO_TL_TRAI',1,'DEMO_KG','DEMO_FOOD','DEMO_EAT','DEMO_NSX06',N'..\..\..\Database\seed_assets\item13.jpg'),
      ('8930000000014',N'Ức gà phi lê đông lạnh 500 g','DEMO_G05','DEMO_TL_THIT',0,'DEMO_HOP','DEMO_FOOD','DEMO_EAT','DEMO_VN',N'..\..\..\Database\seed_assets\item14.jpg'),
      ('8930000000015',N'Bánh mì nguyên cám ổ 400 g','DEMO_G06','DEMO_TL_BANHMI',0,'DEMO_CAI','DEMO_FOOD','DEMO_USE05','DEMO_VN',N'..\..\..\Database\seed_assets\item15.jpg')) v(MaVach,TenHangHoa,MaNhom,MaLoai,NhapKhau,MaDonVi,MaChatLieu,MaCongDung,MaNoiSX,Anh)
    WHERE NOT EXISTS(SELECT 1 FROM dbo.DMHangHoa x WHERE x.MaVach=v.MaVach);
    UPDATE p SET Anh=v.Anh
    FROM dbo.DMHangHoa p JOIN (VALUES
      ('8930000000001',N'..\..\..\Database\seed_assets\item01.jpg'),('8930000000002',N'..\..\..\Database\seed_assets\item02.jpg'),
      ('8930000000003',N'..\..\..\Database\seed_assets\item03.jpg'),('8930000000004',N'..\..\..\Database\seed_assets\item04.jpg'),
      ('8930000000005',N'..\..\..\Database\seed_assets\item05.jpg'),('8930000000006',N'..\..\..\Database\seed_assets\item06.jpg'),
      ('8930000000007',N'..\..\..\Database\seed_assets\item07.jpg'),('8930000000008',N'..\..\..\Database\seed_assets\item08.jpg'),
      ('8930000000009',N'..\..\..\Database\seed_assets\item09.jpg'),('8930000000010',N'..\..\..\Database\seed_assets\item10.jpg'),
      ('8930000000011',N'..\..\..\Database\seed_assets\item11.jpg'),('8930000000012',N'..\..\..\Database\seed_assets\item12.jpg'),
      ('8930000000013',N'..\..\..\Database\seed_assets\item13.jpg'),('8930000000014',N'..\..\..\Database\seed_assets\item14.jpg'),
      ('8930000000015',N'..\..\..\Database\seed_assets\item15.jpg')) v(MaVach,Anh) ON p.MaVach=v.MaVach;

    DECLARE @Nhap TABLE(SoHDN varchar(50),MaNV varchar(20),NgayNhap datetime,MaNCC varchar(20));
    INSERT @Nhap VALUES
      ('DEMO_HDN_2024_01','DEMO_NV03','2024-01-12','DEMO_NCC01'),('DEMO_HDN_2024_08','DEMO_NV03','2024-08-20','DEMO_NCC02'),
      ('DEMO_HDN_2025_02','DEMO_NV03','2025-02-15','DEMO_NCC01'),('DEMO_HDN_2025_07','DEMO_NV03','2025-07-10','DEMO_NCC03'),
      ('DEMO_HDN_2025_11','DEMO_NV04','2025-11-22','DEMO_NCC02'),('DEMO_HDN_2026_03','DEMO_NV03','2026-03-08','DEMO_NCC01'),
      ('DEMO_HDN_2026_06','DEMO_NV04','2026-06-18','DEMO_NCC03'),('DEMO_HDN_2024_03','DEMO_NV05','2024-03-19','DEMO_NCC04'),
      ('DEMO_HDN_2024_10','DEMO_NV06','2024-10-09','DEMO_NCC05'),('DEMO_HDN_2025_04','DEMO_NV07','2025-04-12','DEMO_NCC06'),
      ('DEMO_HDN_2025_09','DEMO_NV08','2025-09-16','DEMO_NCC07'),('DEMO_HDN_2026_02','DEMO_NV09','2026-02-14','DEMO_NCC08'),
      ('DEMO_HDN_2026_05','DEMO_NV10','2026-05-11','DEMO_NCC09'),('DEMO_HDN_2026_08','DEMO_NV11','2026-08-18','DEMO_NCC10'),
      ('DEMO_HDN_2026_10','DEMO_NV12','2026-10-05','DEMO_NCC11');
    DECLARE @CTNhap TABLE(SoHDN varchar(50),MaVach varchar(50),SoLuong int,DonGia decimal(18,2),GiamGia decimal(18,2));
    INSERT @CTNhap VALUES
      ('DEMO_HDN_2024_01','8930000000001',80,78000,0),('DEMO_HDN_2024_01','8930000000004',100,22000,2),('DEMO_HDN_2024_01','8930000000005',120,4000,0),('DEMO_HDN_2024_01','8930000000010',80,13000,0),('DEMO_HDN_2024_01','8930000000006',50,8000,0),
      ('DEMO_HDN_2024_08','8930000000002',150,18000,0),('DEMO_HDN_2024_08','8930000000008',60,26000,3),('DEMO_HDN_2024_08','8930000000010',80,14000,0),('DEMO_HDN_2024_08','8930000000003',50,34000,0),('DEMO_HDN_2024_08','8930000000007',30,50000,0),
      ('DEMO_HDN_2025_02','8930000000001',90,81000,0),('DEMO_HDN_2025_02','8930000000003',70,35000,2),('DEMO_HDN_2025_02','8930000000006',100,8500,0),('DEMO_HDN_2025_02','8930000000011',30,112000,0),('DEMO_HDN_2025_02','8930000000012',25,40000,0),
      ('DEMO_HDN_2025_07','8930000000005',180,4200,0),('DEMO_HDN_2025_07','8930000000007',50,52000,0),('DEMO_HDN_2025_07','8930000000011',30,115000,5),('DEMO_HDN_2025_07','8930000000012',30,41000,0),
      ('DEMO_HDN_2025_11','8930000000002',160,19000,0),('DEMO_HDN_2025_11','8930000000008',75,27500,0),('DEMO_HDN_2025_11','8930000000012',45,42000,0),('DEMO_HDN_2025_11','8930000000009',50,65000,0),
      ('DEMO_HDN_2026_03','8930000000001',100,83000,0),('DEMO_HDN_2026_03','8930000000004',130,23500,0),('DEMO_HDN_2026_03','8930000000009',55,68000,2),
      ('DEMO_HDN_2026_06','8930000000003',85,37000,0),('DEMO_HDN_2026_06','8930000000006',120,9000,0),('DEMO_HDN_2026_06','8930000000011',35,119000,0),
      ('DEMO_HDN_2024_03','8930000000001',45,77500,0),('DEMO_HDN_2024_03','8930000000002',90,17500,0),
      ('DEMO_HDN_2024_10','8930000000008',45,27000,0),('DEMO_HDN_2024_10','8930000000010',65,14500,0),
      ('DEMO_HDN_2025_04','8930000000013',60,48000,0),('DEMO_HDN_2025_04','8930000000015',50,19000,0),
      ('DEMO_HDN_2025_09','8930000000013',40,50000,0),('DEMO_HDN_2025_09','8930000000014',80,65000,0),
      ('DEMO_HDN_2026_02','8930000000014',100,68000,0),('DEMO_HDN_2026_02','8930000000015',70,20000,0),
      ('DEMO_HDN_2026_05','8930000000005',150,4300,0),('DEMO_HDN_2026_05','8930000000006',100,9200,0),
      ('DEMO_HDN_2026_08','8930000000013',45,53000,0),('DEMO_HDN_2026_08','8930000000014',50,69000,0),
      ('DEMO_HDN_2026_10','8930000000015',90,21000,0),('DEMO_HDN_2026_10','8930000000011',25,121000,0);
    INSERT dbo.HoaDonNhap(SoHDN,MaNV,NgayNhap,MaNCC,TongTien)
    SELECT h.SoHDN,h.MaNV,h.NgayNhap,h.MaNCC,SUM(ROUND(d.SoLuong*d.DonGia*(1-d.GiamGia/100.0),2))
    FROM @Nhap h JOIN @CTNhap d ON d.SoHDN=h.SoHDN
    WHERE NOT EXISTS(SELECT 1 FROM dbo.HoaDonNhap x WHERE x.SoHDN=h.SoHDN)
    GROUP BY h.SoHDN,h.MaNV,h.NgayNhap,h.MaNCC;
    INSERT dbo.ChiTietHDN(SoHDN,MaVach,TenHang,SoLuong,DonGia,GiamGia,ThanhTien)
    SELECT d.SoHDN,d.MaVach,p.TenHangHoa,d.SoLuong,d.DonGia,d.GiamGia,ROUND(d.SoLuong*d.DonGia*(1-d.GiamGia/100.0),2)
    FROM @CTNhap d JOIN dbo.DMHangHoa p ON p.MaVach=d.MaVach
    WHERE NOT EXISTS(SELECT 1 FROM dbo.ChiTietHDN x WHERE x.SoHDN=d.SoHDN AND x.MaVach=d.MaVach);

    -- Fix demo item prices before calculating sales invoices and their cost snapshots.
    UPDATE p SET DonGiaNhap=q.DonGia,DonGiaBan=ROUND(q.DonGia*1.10,2)
    FROM dbo.DMHangHoa p
    OUTER APPLY(SELECT TOP(1) d.DonGia FROM dbo.ChiTietHDN d JOIN dbo.HoaDonNhap h ON h.SoHDN=d.SoHDN
      WHERE d.MaVach=p.MaVach AND d.SoHDN LIKE 'DEMO_HDN_%' ORDER BY h.NgayNhap DESC,h.SoHDN DESC) q
    WHERE p.MaVach LIKE '89300000000%' AND q.DonGia IS NOT NULL;

    DECLARE @Ban TABLE(SoHDB varchar(50),MaNV varchar(20),NgayBan datetime,MaKhach varchar(20));
    INSERT @Ban VALUES
      ('DEMO_HDB_2024_02','DEMO_NV01','2024-02-05','DEMO_KH01'),('DEMO_HDB_2024_09','DEMO_NV02','2024-09-14','DEMO_KH02'),
      ('DEMO_HDB_2025_01','DEMO_NV01','2025-01-21','DEMO_KH03'),('DEMO_HDB_2025_04','DEMO_NV02','2025-04-17','DEMO_KH01'),
      ('DEMO_HDB_2025_08','DEMO_NV01','2025-08-09','DEMO_KH04'),('DEMO_HDB_2025_12','DEMO_NV02','2025-12-20','DEMO_KH05'),
      ('DEMO_HDB_2026_01','DEMO_NV01','2026-01-11','DEMO_KH02'),('DEMO_HDB_2026_04','DEMO_NV02','2026-04-23','DEMO_KH06'),
      ('DEMO_HDB_2026_07','DEMO_NV01','2026-07-16','DEMO_KH03'),('DEMO_HDB_2026_09','DEMO_NV02','2026-09-05','DEMO_KH01'),
      ('DEMO_HDB_2024_06','DEMO_NV05','2024-06-12','DEMO_KH07'),('DEMO_HDB_2025_06','DEMO_NV06','2025-06-07','DEMO_KH08'),
      ('DEMO_HDB_2025_10','DEMO_NV07','2025-10-19','DEMO_KH09'),('DEMO_HDB_2026_03B','DEMO_NV08','2026-03-23','DEMO_KH10'),
      ('DEMO_HDB_2026_08B','DEMO_NV09','2026-08-28','DEMO_KH11');
    DECLARE @CTBan TABLE(SoHDB varchar(50),MaVach varchar(50),SoLuong int,GiamGia decimal(18,2));
    INSERT @CTBan VALUES
      ('DEMO_HDB_2024_02','8930000000001',3,0),('DEMO_HDB_2024_02','8930000000004',5,0),('DEMO_HDB_2024_02','8930000000005',12,0),
      ('DEMO_HDB_2024_09','8930000000002',8,0),('DEMO_HDB_2024_09','8930000000008',2,5),('DEMO_HDB_2024_09','8930000000010',4,0),
      ('DEMO_HDB_2025_01','8930000000001',4,0),('DEMO_HDB_2025_01','8930000000003',2,0),('DEMO_HDB_2025_01','8930000000006',6,0),
      ('DEMO_HDB_2025_04','8930000000005',10,0),('DEMO_HDB_2025_04','8930000000007',3,0),('DEMO_HDB_2025_04','8930000000011',1,0),
      ('DEMO_HDB_2025_08','8930000000002',10,0),('DEMO_HDB_2025_08','8930000000008',4,0),('DEMO_HDB_2025_08','8930000000012',2,0),
      ('DEMO_HDB_2025_12','8930000000001',7,0),('DEMO_HDB_2025_12','8930000000004',8,0),('DEMO_HDB_2025_12','8930000000009',2,0),
      ('DEMO_HDB_2026_01','8930000000003',5,0),('DEMO_HDB_2026_01','8930000000006',7,0),('DEMO_HDB_2026_01','8930000000010',6,0),
      ('DEMO_HDB_2026_04','8930000000001',6,0),('DEMO_HDB_2026_04','8930000000005',15,0),('DEMO_HDB_2026_04','8930000000009',3,0),
      ('DEMO_HDB_2026_07','8930000000002',12,0),('DEMO_HDB_2026_07','8930000000004',9,0),('DEMO_HDB_2026_07','8930000000007',4,0),
      ('DEMO_HDB_2026_09','8930000000001',8,0),('DEMO_HDB_2026_09','8930000000008',3,0),('DEMO_HDB_2026_09','8930000000011',1,0),
      ('DEMO_HDB_2024_06','8930000000002',5,0),('DEMO_HDB_2024_06','8930000000005',8,0),('DEMO_HDB_2024_06','8930000000010',3,0),
      ('DEMO_HDB_2025_06','8930000000001',3,0),('DEMO_HDB_2025_06','8930000000013',4,0),('DEMO_HDB_2025_06','8930000000006',5,0),
      ('DEMO_HDB_2025_10','8930000000014',6,0),('DEMO_HDB_2025_10','8930000000003',2,0),('DEMO_HDB_2025_10','8930000000009',1,0),
      ('DEMO_HDB_2026_03B','8930000000015',7,0),('DEMO_HDB_2026_03B','8930000000004',4,0),('DEMO_HDB_2026_03B','8930000000013',2,0),
      ('DEMO_HDB_2026_08B','8930000000013',6,0),('DEMO_HDB_2026_08B','8930000000014',5,0),('DEMO_HDB_2026_08B','8930000000015',3,0);
    INSERT dbo.HoaDonBan(SoHDB,MaNV,NgayBan,MaKhach,TongTien)
    SELECT h.SoHDB,h.MaNV,h.NgayBan,h.MaKhach,SUM(ROUND(d.SoLuong*p.DonGiaBan*(1-d.GiamGia/100.0),2))
    FROM @Ban h JOIN @CTBan d ON d.SoHDB=h.SoHDB JOIN dbo.DMHangHoa p ON p.MaVach=d.MaVach
    WHERE NOT EXISTS(SELECT 1 FROM dbo.HoaDonBan x WHERE x.SoHDB=h.SoHDB)
    GROUP BY h.SoHDB,h.MaNV,h.NgayBan,h.MaKhach;
    INSERT dbo.ChiTietHDB(SoHDB,MaVach,TenHang,SoLuong,DonGia,GiaVon,GiamGia,ThanhTien)
    SELECT d.SoHDB,d.MaVach,p.TenHangHoa,d.SoLuong,p.DonGiaBan,p.DonGiaNhap,d.GiamGia,
      ROUND(d.SoLuong*p.DonGiaBan*(1-d.GiamGia/100.0),2)
    FROM @CTBan d JOIN dbo.DMHangHoa p ON p.MaVach=d.MaVach
    WHERE NOT EXISTS(SELECT 1 FROM dbo.ChiTietHDB x WHERE x.SoHDB=d.SoHDB AND x.MaVach=d.MaVach);

    -- Stock reflects the seeded transactions. Keep all demo quantities positive.
    UPDATE p SET SoLuong=ISNULL(n.Nhap,0)-ISNULL(b.Ban,0),
      DonGiaNhap=ISNULL(g.DonGia,0),DonGiaBan=ROUND(ISNULL(g.DonGia,0)*1.10,2)
    FROM dbo.DMHangHoa p
    OUTER APPLY(SELECT SUM(d.SoLuong) Nhap FROM dbo.ChiTietHDN d WHERE d.MaVach=p.MaVach AND d.SoHDN LIKE 'DEMO_HDN_%') n
    OUTER APPLY(SELECT SUM(d.SoLuong) Ban FROM dbo.ChiTietHDB d WHERE d.MaVach=p.MaVach AND d.SoHDB LIKE 'DEMO_HDB_%') b
    OUTER APPLY(SELECT TOP(1) d.DonGia FROM dbo.ChiTietHDN d JOIN dbo.HoaDonNhap h ON h.SoHDN=d.SoHDN
      WHERE d.MaVach=p.MaVach AND d.SoHDN LIKE 'DEMO_HDN_%' ORDER BY h.NgayNhap DESC,h.SoHDN DESC) g
    WHERE p.MaVach LIKE '89300000000%';

    INSERT dbo.DemoSeedLog(SeedName) VALUES('SeedDemoData_v2');
    COMMIT TRANSACTION;
    PRINT N'Đã nạp dữ liệu demo. Tài khoản đăng nhập không được tạo; hãy tạo quản trị viên đầu tiên trong ứng dụng.';
    PRINT N'Ảnh sản phẩm được tham chiếu qua DMHangHoa.Anh; cần giữ thư mục Database\seed_assets bên cạnh repo.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

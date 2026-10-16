# Supermarket

Ứng dụng desktop phục vụ bài tập lớn môn Lập trình trực quan. Dùng C# WinForms, .NET Framework 4.7.2, control tiêu chuẩn và SQL Server Express. Không dùng thư viện UI ngoài.

Nguồn yêu cầu: `C:\Users\ADMIN\Downloads\20_Quan_ly_ban_hang_sieu_thi_da_sua (1).xlsx`, Sheet1. Hoàn thiện toàn bộ danh mục và 9 nghiệp vụ trong đề; bổ sung đăng nhập/phân quyền, in hóa đơn, kiểm kê, sao lưu/khôi phục.

Giảm giá theo phần trăm trên từng dòng. Giá bán cập nhật bằng 110% đơn giá nhập khi lưu phiếu nhập. Khách hàng mua ít nhất theo quý được xếp theo tổng tiền tăng dần và chỉ gồm khách đã mua trong quý. Sản phẩm bán chạy được xếp theo tổng số lượng giảm dần. Các trường hợp bằng nhau dùng mã để sắp xếp ổn định; báo cáo TOP 3 trả về tối đa 3 dòng.

Quản trị viên quản lý toàn bộ. Thu ngân lập hóa đơn bán và quản lý khách hàng. Thủ kho nhập hàng, quản lý hàng hóa/nhà cung cấp và kiểm kê. Không tạo tài khoản/mật khẩu mặc định hoặc dữ liệu mẫu.

Instance của người dùng: `.\SQLEXPRESS01`. Database ban đầu có các bảng nhưng không có dữ liệu. Mọi thử nghiệm ghi dữ liệu dùng database riêng, xóa sau khi thử. Không xóa dữ liệu thật để kiểm thử.

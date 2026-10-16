# Quản lý bán hàng Supermarket

Ứng dụng bài tập lớn môn Lập trình trực quan, dùng **C# WinForms .NET Framework 4.7.2**, control tiêu chuẩn và **SQL Server Express**. Không cần cài thư viện giao diện ngoài.

## Mở ứng dụng

Chạy `QuanLyBanHangSupermarket/bin/Release/QuanLyBanHangSupermarket.exe`. Giữ file `.exe.config` ở cạnh `.exe`. Database trên `.\SQLEXPRESS01` đã được nâng cấp trong phiên làm việc này, không có tài khoản hoặc dữ liệu mẫu.

Lần đầu mở, ứng dụng kiểm tra kết nối và cho tạo **quản trị viên đầu tiên**. Tự đặt tên đăng nhập 3–40 ký tự và mật khẩu 8–128 ký tự. Mật khẩu được lưu dưới dạng PBKDF2 SHA-256 với salt riêng, 120.000 vòng; không có mật khẩu mặc định.

Quy trình bắt đầu với database trống:

1. Vào **Danh mục**, tạo nhóm hàng, thể loại thuộc nhóm, đơn vị tính và các danh mục phụ cần dùng. Tạo ca làm/công việc trước khi gán cho nhân viên.
2. Tạo nhân viên, nhà cung cấp và khách hàng. Khách lẻ có thể tạo một bản ghi khách hàng riêng để lập hóa đơn.
3. Vào **Hàng hóa**, tạo sản phẩm. Tồn kho và giá ban đầu bằng 0; các trường này chỉ đọc trên màn hình danh mục.
4. Vào **Nhập hàng**, chọn nhân viên/nhà cung cấp, thêm sản phẩm với số lượng và giá nhập, lưu hóa đơn. Kho được cộng; giá bán được cập nhật bằng 110% giá nhập.
5. Vào **Bán hàng**, chọn nhân viên/khách hàng, thêm hàng hoặc nhập/quét mã vạch rồi nhấn Enter. Có thể áp dụng lại số lượng/giảm giá cho dòng đã chọn, bỏ dòng, lưu và xem trước/in hóa đơn.
6. Dùng **Tra cứu hóa đơn**, **Báo cáo**, **Kho và kiểm kê** để xem dữ liệu. Quản trị viên có thể tạo tài khoản thu ngân/thủ kho gắn với nhân viên.

## Chức năng đã triển khai

Toàn bộ danh mục trong đề: nhóm hàng, thể loại, đơn vị tính, chất liệu, công dụng, nơi sản xuất, ca làm, công việc, hàng hóa, nhân viên, khách hàng, nhà cung cấp. Có thêm/sửa/xóa, tìm kiếm, chọn/xem ảnh sản phẩm và xuất CSV. Không cho xóa danh mục/hàng hóa đã được giao dịch hoặc lịch sử kho sử dụng.

Chín yêu cầu từ file Excel:

1. Cập nhật tồn kho khi lưu nhập/bán hàng. Header, chi tiết, kho và lịch sử kho nằm trong một transaction; lỗi bất kỳ sẽ rollback toàn bộ.
2. Cập nhật giá nhập sản phẩm theo đơn giá trên phiếu nhập mới lưu.
3. Cập nhật giá bán bằng **110% giá nhập**, làm tròn hai chữ số lẻ.
4. Lọc hàng hóa theo **chất liệu, nhóm và công dụng**, kết hợp tìm mã/tên.
5. Tra cứu hóa đơn nhập theo **mã hàng, ngày và nhà cung cấp**; có thêm mã hóa đơn và tra cứu hóa đơn bán.
6. Báo cáo sản phẩm bán chạy theo **tháng/năm**, xếp theo tổng số lượng giảm dần.
7. Báo cáo tối đa **3 hóa đơn nhập có tổng tiền lớn nhất** trong năm đã chọn, kèm tổng tiền của danh sách.
8. Báo cáo toàn bộ hóa đơn bán của **nhân viên đã chọn** và tổng tiền bán.
9. Báo cáo tối đa **3 khách hàng có tổng tiền mua thấp nhất** trong quý/năm đã chọn. Chỉ tính khách có hóa đơn trong quý; khách chưa mua không nằm trong kết quả.

Giảm giá tính theo **% trên từng dòng**, trong khoảng 0–100%. Thành tiền = số lượng × đơn giá × (1 − giảm giá/100), làm tròn hai chữ số lẻ. Giá vốn và giá bán được lưu trên từng dòng hóa đơn bán; thay đổi giá sản phẩm sau đó không thay đổi hóa đơn cũ. Lãi gộp = doanh thu trừ giá vốn của hàng đã bán, không phải lợi nhuận sau mọi chi phí.

Các chức năng bổ sung:

- Đăng nhập, đổi mật khẩu, tạo/cập nhật/khóa tài khoản và phân quyền. Quản trị viên có toàn quyền; thu ngân bán hàng/quản lý khách hàng; thủ kho nhập hàng/quản lý hàng hóa, nhà cung cấp và kiểm kê. Không tự hạ quyền/khóa quản trị viên đang đăng nhập; luôn giữ ít nhất một quản trị viên hoạt động. Thay đổi quyền áp dụng khi đăng nhập lại.
- Kiểm kê tồn thực tế có lý do, phiếu kiểm kê và lịch sử người thực hiện. Nếu tồn hệ thống đã thay đổi kể từ khi tải danh sách, toàn bộ lần kiểm kê bị từ chối để tải lại và kiểm tra.
- Xem trước/in hóa đơn một hoặc nhiều trang qua chức năng in WinForms. Có thể chọn máy in PDF của Windows trong hộp thoại in.
- Xuất CSV UTF-8 dùng dấu phân cách `;`, mở được bằng Excel và bảo vệ chuỗi bắt đầu bằng ký tự công thức.
- Sao lưu `.bak` và khôi phục qua SQL Server. Khôi phục cần gõ `KHÔI PHỤC` và xác nhận, thay thế dữ liệu hiện tại và đóng app để đăng nhập lại. SQL Server cần quyền đọc/ghi thư mục chứa file.

Ảnh sản phẩm dùng đường dẫn file riêng; giữ file ảnh tại đường dẫn đã chọn. Khi chuyển máy, sao chép cả ảnh và chọn lại đường dẫn nếu cần. File `.bak` không chứa file ảnh.

## Cấu hình / cài trên máy khác

Cần Windows, .NET Framework 4.7.2 hoặc phiên bản 4.x tương thích, SQL Server và quyền truy cập bằng Windows Authentication.

Chạy toàn bộ `Database/01_Setup.sql` bằng SSMS hoặc:

```powershell
sqlcmd -S '.\SQLEXPRESS01' -E -C -f 65001 -b -i 'Database\01_Setup.sql'
```

Script dùng UTF-8 có BOM, chạy lại được, không xóa/nạp dữ liệu. Nếu gặp hóa đơn cũ chưa có thông tin giá/giảm giá theo schema mới, script dừng thay vì tự đoán dữ liệu lịch sử. Database mới cần tạo tài khoản quản trị qua ứng dụng.

Chỉnh `ConnStr` trong `QuanLyBanHangSupermarket/App.config` khi đổi server/database; nếu dùng bản đã build, chỉnh file `QuanLyBanHangSupermarket.exe.config` ở cạnh `.exe`. Mã đọc duy nhất cấu hình này, không có chuỗi kết nối hard-code khác.

## Build và kiểm tra

Mở solution `.slnx` bằng Visual Studio hiện tại hoặc mở trực tiếp `.csproj`. Cần .NET desktop development và targeting pack .NET Framework 4.7.2.

```powershell
& 'D:\VisualStudio2026\MSBuild\Current\Bin\MSBuild.exe' 'QuanLyBanHangSupermarket\QuanLyBanHangSupermarket.csproj' /t:Rebuild /p:Configuration=Release /v:minimal /nologo
& 'Tests\Run-Verification.ps1'
```

Nếu MSBuild ở nơi khác, truyền `-MSBuild 'đường dẫn MSBuild.exe'` cho script kiểm tra. Script tạo database với tên `QuanLyBanHangSupermarket_Verify_<mã duy nhất>`, kiểm tra tính tiền, CRUD, tìm kiếm, báo cáo, quyền, rollback, giao dịch đồng thời, kiểm kê, luồng hóa đơn và bố cục in; sau đó kiểm tra sao lưu/khôi phục và xóa database kiểm thử. Không chạy thử vào database thật. Các ảnh/log kiểm tra nằm trong `.codex/verification`.

Đã kiểm tra bố cục native ở nhiều kích thước cửa sổ. Không gửi lệnh in đến máy in vật lý trong quá trình kiểm tra.

## Cấu trúc mã

- `Core/Business.cs`: tính tiền, mật khẩu/tài khoản, lưu hóa đơn và kiểm kê.
- `Core/Catalogs.cs`: định nghĩa danh mục và thao tác tham số hóa.
- `Core/Queries.cs`: tra cứu và báo cáo theo đề.
- `Core/Exports.cs`: CSV và in hóa đơn nhiều trang.
- `UI/`: giao diện WinForms dùng control tiêu chuẩn, bố trí chung và các màn hình nghiệp vụ.
- `Database/01_Setup.sql`: tạo/nâng cấp schema.
- `Tests/`: kiểm tra tự động không dùng thư viện test ngoài.

Mã cũ được giữ trong `.codex/backups/source-before-upgrade.zip` và `.codex/legacy-forms`; các form cũ không còn thuộc project đang build. Database đã được sao lưu riêng trước khi nâng cấp trong thư mục Backup của SQL Server.
#   S u p e r m a r k e t - M a n a g e m e n t  
 
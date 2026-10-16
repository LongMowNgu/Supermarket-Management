using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace QuanLyBanHangSupermarket
{
    public class InvoiceSearchForm : WorkspacePage
    {
        readonly ComboBox type=Ui.Combo(),product=Ui.Combo(),partner=Ui.Combo();
        readonly DateTimePicker from=Ui.Date(),to=Ui.Date();
        readonly TextBox code=new TextBox {Width=190,MaxLength=50};
        readonly DataGridView invoices=Ui.Grid(),details=Ui.Grid();
        bool loading,loadedSale=true;
        public InvoiceSearchForm() : base("Tra cứu hóa đơn","Lọc theo mã hóa đơn, ngày, hàng hóa và khách hàng/nhà cung cấp. Chọn một hóa đơn để xem chi tiết hoặc in.")
        {
            type.Items.AddRange(new object[]{"Hóa đơn bán","Hóa đơn nhập"});type.SelectedIndex=0;type.Width=160;from.Value=new DateTime(DateTime.Today.Year,DateTime.Today.Month,1);
            var body=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=5};body.RowStyles.Add(new RowStyle(SizeType.Absolute,120));body.RowStyles.Add(new RowStyle(SizeType.Percent,55));body.RowStyles.Add(new RowStyle(SizeType.Absolute,34));body.RowStyles.Add(new RowStyle(SizeType.Percent,45));body.RowStyles.Add(new RowStyle(SizeType.Absolute,36));
            var filters=Ui.Bar();filters.Controls.Add(type);filters.Controls.Add(Ui.Label("Từ ngày"));filters.Controls.Add(from);filters.Controls.Add(Ui.Label("Đến ngày"));filters.Controls.Add(to);filters.Controls.Add(Ui.Label("Mã hóa đơn"));filters.Controls.Add(code);filters.Controls.Add(Ui.Label("Hàng hóa"));filters.Controls.Add(product);filters.Controls.Add(Ui.Label("Khách/NCC"));filters.Controls.Add(partner);filters.Controls.Add(Ui.Button("Tìm kiếm",Search,true));
            filters.Controls.Add(Ui.Button("Xem trước / In",()=>{if(invoices.CurrentRow==null)throw new ArgumentException("Chọn hóa đơn cần in.");Exports.PrintInvoice(loadedSale,invoices.CurrentRow.Cells[0].Value.ToString());}));filters.Controls.Add(Ui.Button("Xuất CSV",()=>Exports.Csv(invoices,"HoaDon")));
            Status=Ui.Label("");body.Controls.Add(filters,0,0);body.Controls.Add(invoices,0,1);body.Controls.Add(Ui.Label("Chi tiết hóa đơn",true),0,2);body.Controls.Add(details,0,3);body.Controls.Add(Status,0,4);Content(body);
            type.SelectedIndexChanged+=(s,e)=>Ui.Run(()=>{LoadPartners();Search();});
            invoices.SelectionChanged+=(s,e)=>{if(!loading&&invoices.CurrentRow!=null)Ui.Run(()=>Ui.Bind(details,Queries.Details(loadedSale,invoices.CurrentRow.Cells[0].Value.ToString())));};
            Shown+=(s,e)=>Ui.Run(()=>{Ui.Fill(product,"DMHangHoa","MaVach","TenHangHoa",true);LoadPartners();Search();});
        }
        void LoadPartners(){bool sale=type.SelectedIndex==0;Ui.Fill(partner,sale?"KhachHang":"NhaCungCap",sale?"MaKhach":"MaNCC",sale?"TenKhach":"TenNCC",true);}
        void Search()
        {
            bool sale=type.SelectedIndex==0;var result=Queries.Invoices(sale,from.Value,to.Value,code.Text.Trim(),Ui.Selected(product),Ui.Selected(partner));
            loading=true;loadedSale=sale;Ui.Bind(invoices,result);details.DataSource=null;loading=false;
            Message(result.Rows.Count==0?"Không có hóa đơn phù hợp.":result.Rows.Count+" hóa đơn · Tổng tiền: "+result.Rows.Cast<DataRow>().Sum(r=>Convert.ToDecimal(r["Tổng tiền"])).ToString("N2")+" VNĐ");
            if(invoices.CurrentRow!=null)Ui.Bind(details,Queries.Details(loadedSale,invoices.CurrentRow.Cells[0].Value.ToString()));
        }
    }
    public class ReportForm : WorkspacePage
    {
        readonly ComboBox mode=Ui.Combo(),employee=Ui.Combo();
        readonly NumericUpDown year=Ui.Number(9998),quarter=Ui.Number(4);
        readonly DateTimePicker month=Ui.Date(true),from=Ui.Date(),to=Ui.Date();
        readonly DataGridView grid=Ui.Grid();
        readonly FlowLayoutPanel period=Ui.Bar();
        public ReportForm() : base("Báo cáo","Báo cáo theo yêu cầu đề bài. Lãi gộp sử dụng giá vốn lưu tại thời điểm bán.")
        {
            Session.Require("Admin");mode.Width=460;mode.Items.AddRange(new object[]{"Sản phẩm bán chạy theo tháng (số lượng)","3 hóa đơn nhập nhiều tiền nhất theo năm","Hóa đơn và tổng tiền bán của nhân viên","3 khách mua ít nhất theo quý (tổng tiền)","Doanh thu và lãi gộp theo ngày"});
            year.Minimum=1753;year.Value=DateTime.Today.Year;quarter.Minimum=1;quarter.Value=(DateTime.Today.Month-1)/3+1;month.ShowUpDown=true;from.Value=new DateTime(DateTime.Today.Year,DateTime.Today.Month,1);
            var body=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=4};body.RowStyles.Add(new RowStyle(SizeType.Absolute,48));body.RowStyles.Add(new RowStyle(SizeType.Absolute,88));body.RowStyles.Add(new RowStyle(SizeType.Percent,100));body.RowStyles.Add(new RowStyle(SizeType.Absolute,48));
            var selection=Ui.Bar();selection.Controls.Add(mode);selection.Controls.Add(Ui.Button("Xem báo cáo",LoadReport,true));selection.Controls.Add(Ui.Button("Xuất CSV",()=>Exports.Csv(grid,"BaoCao")));
            Status=Ui.Label("");body.Controls.Add(selection,0,0);body.Controls.Add(period,0,1);body.Controls.Add(grid,0,2);body.Controls.Add(Status,0,3);Content(body);
            mode.SelectedIndexChanged+=(s,e)=>{SetPeriod();grid.DataSource=null;Message("Chọn điều kiện và nhấn Xem báo cáo.");};mode.SelectedIndex=0;
            Shown+=(s,e)=>Ui.Run(()=>{Ui.Fill(employee,"NhânVien","MaNV","TenNV");LoadReport();});
        }
        void SetPeriod()
        {
            // Detach persistent controls before disposing only the transient labels.
            foreach(Control c in period.Controls.Cast<Control>().ToArray()){period.Controls.Remove(c);if(c is Label)c.Dispose();}
            if(mode.SelectedIndex==0){period.Controls.Add(Ui.Label("Tháng/năm"));period.Controls.Add(month);}
            if(mode.SelectedIndex==1||mode.SelectedIndex==3){period.Controls.Add(Ui.Label("Năm"));period.Controls.Add(year);}
            if(mode.SelectedIndex==3){period.Controls.Add(Ui.Label("Quý"));period.Controls.Add(quarter);period.Controls.Add(Ui.Label("Chỉ tính khách có hóa đơn trong quý."));}
            if(mode.SelectedIndex==2){period.Controls.Add(Ui.Label("Nhân viên"));period.Controls.Add(employee);period.Controls.Add(Ui.Label("Tất cả hóa đơn của nhân viên đã chọn."));}
            if(mode.SelectedIndex==4){period.Controls.Add(Ui.Label("Từ ngày"));period.Controls.Add(from);period.Controls.Add(Ui.Label("Đến ngày"));period.Controls.Add(to);}
        }
        void LoadReport()
        {
            var dt=Queries.Report(mode.SelectedIndex,month.Value,(int)year.Value,(int)quarter.Value,Ui.Selected(employee),from.Value,to.Value);Ui.Bind(grid,dt);
            string moneyColumn=mode.SelectedIndex==0?"Doanh thu":mode.SelectedIndex==1?"Tổng tiền nhập":mode.SelectedIndex==2?"Tổng tiền bán":mode.SelectedIndex==3?"Tổng tiền mua":"Doanh thu";
            decimal sum=dt.Rows.Cast<DataRow>().Sum(r=>Convert.ToDecimal(r[moneyColumn]));
            Message(dt.Rows.Count==0?"Không có dữ liệu trong điều kiện đã chọn.":dt.Rows.Count+" dòng · "+moneyColumn+" của danh sách: "+sum.ToString("N2")+" VNĐ");
        }
    }
    public class StockForm : WorkspacePage
    {
        readonly DataGridView current=Ui.Grid(),history=Ui.Grid(),counts=Ui.Grid();
        readonly TextBox note=new TextBox {Width=440,MaxLength=500};
        DataTable stock;
        bool dirty;
        public override bool HasPendingChanges {get {return dirty||!string.IsNullOrWhiteSpace(note.Text);}}
        public StockForm() : base("Kho và kiểm kê","Nhập tồn thực tế, ghi lý do và xác nhận. Thay đổi kho luôn có lịch sử và người thực hiện.")
        {
            Session.Require("Admin","ThuKho");
            var body=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=3};body.RowStyles.Add(new RowStyle(SizeType.Absolute,88));body.RowStyles.Add(new RowStyle(SizeType.Percent,100));body.RowStyles.Add(new RowStyle(SizeType.Absolute,40));
            var bar=Ui.Bar();bar.Controls.Add(Ui.Label("Lý do kiểm kê *"));bar.Controls.Add(note);bar.Controls.Add(Ui.Button("Lưu kiểm kê",Save,true));bar.Controls.Add(Ui.Button("Tải lại",()=>{if(CanLeave())LoadData();}));bar.Controls.Add(Ui.Button("Xuất tồn kho",()=>Exports.Csv(current,"TonKho")));
            var tabs=new TabControl {Dock=DockStyle.Fill};foreach(var pair in new[]{Tuple.Create("Tồn kho / Kiểm kê",current),Tuple.Create("Lịch sử thay đổi kho",history),Tuple.Create("Các lần kiểm kê",counts)}){var tab=new TabPage(pair.Item1);tab.Controls.Add(pair.Item2);tabs.TabPages.Add(tab);}
            current.CellValueChanged+=(s,e)=>{if(e.RowIndex>=0){dirty=true;Message("Có số lượng kiểm kê chưa lưu.");}};
            current.DataError+=(s,e)=>{e.ThrowException=false;Message("Tồn thực tế cần là số nguyên từ 0 đến 1.000.000.");};
            current.CellValidating+=(s,e)=>{if(e.RowIndex>=0&&current.Columns[e.ColumnIndex].Name=="ThucTe"){int n;if(!int.TryParse(e.FormattedValue.ToString(),out n)||n<0||n>1000000){e.Cancel=true;Message("Tồn thực tế cần là số nguyên từ 0 đến 1.000.000.");}}};
            Status=Ui.Label("");body.Controls.Add(bar,0,0);body.Controls.Add(tabs,0,1);body.Controls.Add(Status,0,2);Content(body);Shown+=(s,e)=>Ui.Run(LoadData);
        }
        void LoadData()
        {
            stock=DatabaseHelper.GetDataToTable("SELECT MaVach,TenHangHoa,SoLuong,SoLuong AS ThucTe,DonGiaNhap,DonGiaBan FROM DMHangHoa ORDER BY TenHangHoa");Ui.Bind(current,stock);current.ReadOnly=false;
            foreach(DataGridViewColumn c in current.Columns)c.ReadOnly=c.Name!="ThucTe";
            string[] titles={"Mã vạch","Hàng hóa","Tồn hệ thống","Tồn thực tế (sửa)","Giá nhập","Giá bán"};for(int i=0;i<titles.Length;i++)current.Columns[i].HeaderText=titles[i];
            current.Columns["ThucTe"].DefaultCellStyle.BackColor=System.Drawing.Color.FromArgb(238,249,234);
            Ui.Bind(history,DatabaseHelper.GetDataToTable("SELECT TOP(500) Ngay AS [Ngày],MaVach AS [Mã vạch],Loai AS [Nghiệp vụ],ThayDoi AS [Thay đổi],TonSau AS [Tồn sau],ThamChieu AS [Chứng từ],NguoiThucHien AS [Người thực hiện] FROM LichSuKho ORDER BY Id DESC"));
            Ui.Bind(counts,DatabaseHelper.GetDataToTable("SELECT k.MaKiemKe AS [Phiếu kiểm kê],k.NgayKiemKe AS [Ngày],k.GhiChu AS [Lý do],k.NguoiThucHien AS [Người thực hiện],d.MaVach AS [Mã vạch],d.TonHeThong AS [Tồn trước],d.TonThucTe AS [Tồn sau] FROM KiemKe k JOIN ChiTietKiemKe d ON d.MaKiemKe=k.MaKiemKe ORDER BY k.NgayKiemKe DESC,d.MaVach"));
            dirty=false;note.Clear();Message(stock.Rows.Count+" mặt hàng. Sửa cột Tồn thực tế rồi Lưu kiểm kê. Lịch sử kho hiển thị 500 thay đổi gần nhất.");
        }
        void Save()
        {
            if(!ValidateChildren()||!current.EndEdit())return;((CurrencyManager)BindingContext[stock]).EndCurrentEdit();
            if(string.IsNullOrWhiteSpace(note.Text))throw new ArgumentException("Nhập lý do kiểm kê trước khi lưu.");
            if(!Ui.Confirm("Xác nhận tồn thực tế của "+stock.Rows.Count+" mặt hàng? Thao tác sẽ ghi lịch sử kiểm kê và cập nhật kho."))return;
            string id=Sales.CountStock(stock,note.Text.Trim());LoadData();Message("Đã lưu phiếu "+id+".");
        }
    }
}

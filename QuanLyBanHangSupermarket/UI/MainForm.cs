using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace QuanLyBanHangSupermarket
{
    public class MainForm : Form
    {
        readonly Panel host=new Panel {Dock=DockStyle.Fill};
        WorkspacePage active;
        readonly FlowLayoutPanel navigation=new FlowLayoutPanel {Dock=DockStyle.Fill,FlowDirection=FlowDirection.TopDown,WrapContents=false,AutoScroll=true,Padding=new Padding(18,20,18,16),BackColor=Ui.Surface};
        public MainForm()
        {
            Text="Supermarket · Quản lý bán hàng";Font=Ui.Body;BackColor=Color.White;AutoScaleMode=AutoScaleMode.Font;StartPosition=FormStartPosition.CenterScreen;ClientSize=new Size(1440,940);MinimumSize=new Size(1220,800);WindowState=FormWindowState.Maximized;
            var shell=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=2,RowCount=1,Margin=new Padding(0)};
            shell.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,228));shell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
            shell.Controls.Add(navigation,0,0);shell.Controls.Add(host,1,0);Controls.Add(shell);
            var brand=Ui.Label("SUPERMARKET",true);brand.ForeColor=Ui.Accent;brand.Font=new Font("Segoe UI",17F,FontStyle.Bold);brand.Margin=new Padding(0,0,0,8);navigation.Controls.Add(brand);
            var user=Ui.Label((Session.Username??"Chưa đăng nhập")+"\n"+RoleName(Session.Role));user.Margin=new Padding(0,0,0,24);navigation.Controls.Add(user);
            Nav("Tổng quan",()=>new DashboardForm(Open));Nav("Hàng hóa",()=>new CatalogForm(Catalogs.Get("DMHangHoa")));
            if(Session.Role=="Admin"||Session.Role=="ThuNgan")Nav("Bán hàng",()=>new InvoiceForm(true));
            if(Session.Role=="Admin"||Session.Role=="ThuKho")Nav("Nhập hàng",()=>new InvoiceForm(false));
            Nav("Tra cứu hóa đơn",()=>new InvoiceSearchForm());
            if(Session.IsAdmin)Nav("Báo cáo",()=>new ReportForm());
            if(Session.Role=="Admin"||Session.Role=="ThuKho")Nav("Kho và kiểm kê",()=>new StockForm());
            navigation.Controls.Add(Ui.Label("DANH MỤC",true));
            var chooser=Ui.Combo();chooser.Width=185;
            var options=Catalogs.All.Where(s=>s.Table!="DMHangHoa"&&(s.Table!="NhânVien"||Session.IsAdmin)).ToList();
            chooser.DataSource=options;chooser.DisplayMember="Title";chooser.SelectedIndex=-1;
            chooser.SelectionChangeCommitted+=(s,e)=>Open(new CatalogForm((CatalogSpec)chooser.SelectedItem));navigation.Controls.Add(chooser);
            if(Session.IsAdmin){Nav("Tài khoản và phân quyền",()=>new AccountForm());Nav("Sao lưu / khôi phục",()=>new BackupForm());}
            Nav("Đổi mật khẩu",()=>new PasswordForm());
            navigation.Controls.Add(Ui.Button("Thoát ứng dụng",Close));
            FormClosing+=(s,e)=>{if(active!=null&&!active.CanLeave())e.Cancel=true;};
            Shown+=(s,e)=>Open(new DashboardForm(Open));
        }
        public static string RoleName(string role){return role=="Admin"?"Quản trị viên":role=="ThuNgan"?"Thu ngân":role=="ThuKho"?"Thủ kho":"";}
        void Nav(string text,Func<WorkspacePage> factory)
        {
            var b=Ui.Button(text,()=>Open(factory()));b.Width=185;b.Height=40;b.AutoSize=false;b.TextAlign=ContentAlignment.MiddleLeft;b.FlatAppearance.BorderSize=0;b.BackColor=Ui.Surface;
            navigation.Controls.Add(b);
        }
        void Open(WorkspacePage page)
        {
            if(active!=null&&!active.CanLeave()){page.Dispose();return;}
            if(active!=null){host.Controls.Remove(active);active.Dispose();}
            active=page;page.TopLevel=false;page.FormBorderStyle=FormBorderStyle.None;page.Dock=DockStyle.Fill;host.Controls.Add(page);page.Show();
            foreach(Button b in navigation.Controls.OfType<Button>()){bool selected=b.Text==page.Text;b.BackColor=selected?Ui.Accent:Ui.Surface;b.ForeColor=selected?Color.White:Ui.Ink;}
        }
    }
    public class DashboardForm : WorkspacePage
    {
        readonly DataGridView recent=Ui.Grid(), low=Ui.Grid();
        readonly Label summary=Ui.Label("Đang tải dữ liệu…",true);
        public DashboardForm(Action<WorkspacePage> open) : base("Tổng quan","Theo dõi hoạt động trong tháng hiện tại và các mặt hàng cần nhập thêm.")
        {
            var body=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=3};body.RowStyles.Add(new RowStyle(SizeType.Absolute,85));body.RowStyles.Add(new RowStyle(SizeType.Absolute,55));body.RowStyles.Add(new RowStyle(SizeType.Percent,100));
            summary.Dock=DockStyle.Fill;summary.Font=new Font("Segoe UI",14F,FontStyle.Bold);body.Controls.Add(summary,0,0);
            var bar=Ui.Bar();if(Session.IsAdmin||Session.Role=="ThuNgan")bar.Controls.Add(Ui.Button("Lập hóa đơn bán",()=>open(new InvoiceForm(true)),true));
            if(Session.IsAdmin||Session.Role=="ThuKho")bar.Controls.Add(Ui.Button("Lập hóa đơn nhập",()=>open(new InvoiceForm(false))));
            bar.Controls.Add(Ui.Button("Tải lại tổng quan",LoadData));body.Controls.Add(bar,0,1);
            var tables=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=2,RowCount=2};tables.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,62));tables.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,38));tables.RowStyles.Add(new RowStyle(SizeType.Absolute,38));tables.RowStyles.Add(new RowStyle(SizeType.Percent,100));
            tables.Controls.Add(Ui.Label("Hóa đơn bán gần đây",true),0,0);tables.Controls.Add(Ui.Label("Tồn kho thấp (≤ 10)",true),1,0);tables.Controls.Add(recent,0,1);tables.Controls.Add(low,1,1);body.Controls.Add(tables,0,2);Content(body);Shown+=(s,e)=>Ui.Run(LoadData);
        }
        void LoadData()
        {
            DateTime start=new DateTime(DateTime.Today.Year,DateTime.Today.Month,1),end=start.AddMonths(1);
            var dt=DatabaseHelper.GetDataToTable("SELECT (SELECT COUNT(*) FROM DMHangHoa) AS Products,(SELECT COUNT(*) FROM HoaDonBan WHERE NgayBan>=@a AND NgayBan<@b) AS Invoices,(SELECT ISNULL(SUM(TongTien),0) FROM HoaDonBan WHERE NgayBan>=@a AND NgayBan<@b) AS Revenue",DatabaseHelper.Param("@a",start),DatabaseHelper.Param("@b",end));
            var r=dt.Rows[0];summary.Text="Tháng "+start.ToString("MM/yyyy")+"     |     "+r["Products"]+" mặt hàng     |     "+r["Invoices"]+" hóa đơn bán\nDoanh thu: "+Convert.ToDecimal(r["Revenue"]).ToString("N2")+" VNĐ";
            Ui.Bind(recent,DatabaseHelper.GetDataToTable("SELECT TOP(20) SoHDB AS [Số hóa đơn],NgayBan AS [Ngày bán],TongTien AS [Tổng tiền] FROM HoaDonBan ORDER BY NgayBan DESC,SoHDB DESC"));
            Ui.Bind(low,DatabaseHelper.GetDataToTable("SELECT MaVach AS [Mã vạch],TenHangHoa AS [Hàng hóa],SoLuong AS [Tồn] FROM DMHangHoa WHERE SoLuong<=10 ORDER BY SoLuong,TenHangHoa"));
        }
    }
}

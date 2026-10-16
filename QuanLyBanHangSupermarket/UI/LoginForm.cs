using System;
using System.Drawing;
using System.Windows.Forms;

namespace QuanLyBanHangSupermarket
{
    public class LoginForm : Form
    {
        readonly TextBox username=new TextBox {MaxLength=40}, password=new TextBox {UseSystemPasswordChar=true,MaxLength=128};
        readonly Label title=Ui.Label("Đăng nhập",true), detail=Ui.Label(""), feedback=Ui.Label("");
        readonly Button submit;
        bool first,ready;
        public LoginForm()
        {
            Text="Supermarket · Đăng nhập";Font=Ui.Body;BackColor=Ui.Surface;StartPosition=FormStartPosition.CenterScreen;ClientSize=new Size(540,510);MinimumSize=new Size(540,550);MaximizeBox=false;FormBorderStyle=FormBorderStyle.FixedDialog;
            var panel=new TableLayoutPanel {Dock=DockStyle.Fill,Padding=new Padding(44,32,44,32),ColumnCount=1,RowCount=10};
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
            var brand=Ui.Label("SUPERMARKET",true);brand.Font=new Font("Segoe UI",24F,FontStyle.Bold);brand.ForeColor=Ui.Accent;
            title.Font=new Font("Segoe UI",18F,FontStyle.Bold);detail.MaximumSize=new Size(445,0);feedback.MaximumSize=new Size(445,0);
            panel.Controls.Add(brand);panel.Controls.Add(title);panel.Controls.Add(detail);
            panel.Controls.Add(Ui.Label("Tên đăng nhập"));username.Dock=DockStyle.Top;panel.Controls.Add(username);
            panel.Controls.Add(Ui.Label("Mật khẩu (ít nhất 8 ký tự khi tạo tài khoản)"));password.Dock=DockStyle.Top;panel.Controls.Add(password);
            submit=Ui.Button("Đăng nhập",Submit,true);submit.Dock=DockStyle.Top;submit.Margin=new Padding(0,18,0,8);panel.Controls.Add(submit);AcceptButton=submit;
            panel.Controls.Add(Ui.Button("Kiểm tra lại kết nối",Check));panel.Controls.Add(feedback);Controls.Add(panel);
            Shown+=(s,e)=>Ui.Run(Check);
        }
        void Check()
        {
            ready=false;submit.Enabled=false;
            object exists=DatabaseHelper.ExecuteScalar("SELECT OBJECT_ID(N'dbo.TaiKhoan')");
            if(exists==null||exists==DBNull.Value)throw new InvalidOperationException("Database chưa nâng cấp. Chạy script Database/01_Setup.sql trước khi đăng nhập.");
            first=Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM TaiKhoan"))==0;
            title.Text=first?"Tạo quản trị viên đầu tiên":"Đăng nhập";
            detail.Text=first?"Database đã sẵn sàng. Tự đặt tài khoản quản trị để bắt đầu; không có mật khẩu mặc định.":"Dùng tài khoản được quản trị viên cấp để tiếp tục.";
            submit.Text=first?"Tạo tài khoản và tiếp tục":"Đăng nhập";ready=true;submit.Enabled=true;feedback.Text="Kết nối SQL Server thành công.";username.Focus();
        }
        void Submit()
        {
            if(!ready)return;submit.Enabled=false;
            try
            {
                string user=username.Text.Trim();
                if(first){Accounts.Create(user,password.Text,"Admin",null,true);first=false;}
                if(!Accounts.Authenticate(user,password.Text)){feedback.Text="Tên đăng nhập hoặc mật khẩu chưa đúng.";password.SelectAll();password.Focus();return;}
                DialogResult=DialogResult.OK;Close();
            } finally {submit.Enabled=true;}
        }
    }
}

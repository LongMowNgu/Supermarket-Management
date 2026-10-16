using System;
using System.Data;
using System.Windows.Forms;

namespace QuanLyBanHangSupermarket
{
    public class AccountForm : WorkspacePage
    {
        readonly TextBox username=new TextBox {Width=200,MaxLength=40},password=new TextBox {Width=210,UseSystemPasswordChar=true,MaxLength=128};
        readonly ComboBox role=Ui.Combo(),employee=Ui.Combo();
        readonly CheckBox enabled=new CheckBox {Text="Được đăng nhập",Checked=true,AutoSize=true};
        readonly DataGridView grid=Ui.Grid();
        bool adding=true;
        public AccountForm() : base("Tài khoản và phân quyền","Quản trị viên quản lý toàn bộ. Thu ngân bán hàng/khách hàng; thủ kho nhập hàng/nhà cung cấp/kiểm kê.")
        {
            Session.Require("Admin");role.Items.AddRange(new object[]{"Admin","ThuNgan","ThuKho"});role.SelectedIndex=1;
            var body=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=4};body.RowStyles.Add(new RowStyle(SizeType.Absolute,130));body.RowStyles.Add(new RowStyle(SizeType.Absolute,48));body.RowStyles.Add(new RowStyle(SizeType.Percent,100));body.RowStyles.Add(new RowStyle(SizeType.Absolute,48));
            var fields=Ui.Bar();fields.Controls.Add(Ui.Label("Tên đăng nhập"));fields.Controls.Add(username);fields.Controls.Add(Ui.Label("Mật khẩu mới"));fields.Controls.Add(password);fields.Controls.Add(Ui.Label("Vai trò"));fields.Controls.Add(role);fields.Controls.Add(Ui.Label("Nhân viên"));fields.Controls.Add(employee);fields.Controls.Add(enabled);fields.Controls.Add(Ui.Label("Khi cập nhật: để trống mật khẩu nếu không đổi. Không xóa tài khoản đã có lịch sử."));
            var actions=Ui.Bar();actions.Controls.Add(Ui.Button("Tài khoản mới",()=>{adding=true;username.Enabled=true;username.Clear();password.Clear();enabled.Checked=true;employee.SelectedIndex=-1;Message("Đang tạo tài khoản mới.");}));actions.Controls.Add(Ui.Button("Lưu tài khoản",Save,true));actions.Controls.Add(Ui.Button("Tải lại",LoadData));
            grid.CellClick+=(s,e)=>{if(e.RowIndex<0)return;var r=((DataRowView)grid.Rows[e.RowIndex].DataBoundItem).Row;adding=false;username.Text=r["TenDangNhap"].ToString();username.Enabled=false;role.SelectedItem=r["VaiTro"].ToString();employee.SelectedValue=r["MaNV"];enabled.Checked=Convert.ToBoolean(r["HoatDong"]);password.Clear();Message("Đang cập nhật "+username.Text+".");};
            Status=Ui.Label("");body.Controls.Add(fields,0,0);body.Controls.Add(actions,0,1);body.Controls.Add(grid,0,2);body.Controls.Add(Status,0,3);Content(body);Shown+=(s,e)=>Ui.Run(()=>{Ui.Fill(employee,"NhânVien","MaNV","TenNV");LoadData();});
        }
        void LoadData()
        {Ui.Bind(grid,DatabaseHelper.GetDataToTable("SELECT TenDangNhap,VaiTro,MaNV,HoatDong FROM TaiKhoan ORDER BY TenDangNhap"));grid.Columns[0].HeaderText="Tên đăng nhập";grid.Columns[1].HeaderText="Vai trò";grid.Columns[2].HeaderText="Mã nhân viên";grid.Columns[3].HeaderText="Hoạt động";Message("Chọn dòng để cập nhật, hoặc nhấn Tài khoản mới.");}
        void Save()
        {
            string u=username.Text.Trim(),r=role.SelectedItem==null?null:role.SelectedItem.ToString(),e=Ui.Selected(employee);
            if(adding)Accounts.Create(u,password.Text,r,e,false);
            else
            {
                Session.Require("Admin");
                if(r==null)throw new ArgumentException("Chọn vai trò.");
                if(r!="Admin"&&string.IsNullOrEmpty(e))throw new ArgumentException("Thu ngân/thủ kho phải gắn với một nhân viên.");
                if(u==Session.Username&&(!enabled.Checked||r!="Admin"))throw new ArgumentException("Không tự khóa hoặc hạ quyền tài khoản đang đăng nhập.");
                if(password.Text.Length>0&&password.Text.Length<8)throw new ArgumentException("Mật khẩu mới cần ít nhất 8 ký tự.");
                DatabaseHelper.InTransaction((conn,tx)=>
                {
                    if((r!="Admin"||!enabled.Checked)&&Convert.ToInt32(DatabaseHelper.Scalar(conn,tx,"SELECT COUNT(*) FROM TaiKhoan WITH(UPDLOCK,HOLDLOCK) WHERE VaiTro='Admin' AND HoatDong=1 AND TenDangNhap<>@u",DatabaseHelper.Param("@u",u)))==0)
                        throw new ArgumentException("Phải giữ ít nhất một quản trị viên hoạt động.");
                    int count=DatabaseHelper.Execute(conn,tx,"UPDATE TaiKhoan SET VaiTro=@r,MaNV=@e,HoatDong=@a WHERE TenDangNhap=@u",DatabaseHelper.Param("@r",r),DatabaseHelper.Param("@e",e),DatabaseHelper.Param("@a",enabled.Checked),DatabaseHelper.Param("@u",u));
                    if(count!=1)throw new ArgumentException("Tài khoản không tồn tại. Tải lại danh sách.");
                    if(password.Text.Length>0){var salt=Passwords.Salt();DatabaseHelper.Execute(conn,tx,"UPDATE TaiKhoan SET PasswordHash=@h,Salt=@s WHERE TenDangNhap=@u",DatabaseHelper.Param("@h",Passwords.Hash(password.Text,salt)),DatabaseHelper.Param("@s",Convert.ToBase64String(salt)),DatabaseHelper.Param("@u",u));}
                    return count;
                });
            }
            password.Clear();LoadData();Message("Đã lưu tài khoản. Thay đổi quyền áp dụng ở lần đăng nhập tiếp theo.");
        }
    }
    public class PasswordForm : WorkspacePage
    {
        readonly TextBox old=new TextBox {UseSystemPasswordChar=true,MaxLength=128},fresh=new TextBox {UseSystemPasswordChar=true,MaxLength=128},repeat=new TextBox {UseSystemPasswordChar=true,MaxLength=128};
        public PasswordForm() : base("Đổi mật khẩu","Đặt mật khẩu riêng cho tài khoản đang đăng nhập, tối thiểu 8 ký tự.")
        {
            var box=new TableLayoutPanel {Dock=DockStyle.Top,ColumnCount=2,AutoSize=true,MaximumSize=new System.Drawing.Size(700,0)};box.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,200));box.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
            var labels=new[]{"Mật khẩu hiện tại","Mật khẩu mới","Nhập lại mật khẩu mới"};var controls=new[]{old,fresh,repeat};
            for(int i=0;i<3;i++){box.RowStyles.Add(new RowStyle(SizeType.Absolute,50));box.Controls.Add(Ui.Label(labels[i]),0,i);controls[i].Dock=DockStyle.Fill;controls[i].Margin=new Padding(0,6,0,6);box.Controls.Add(controls[i],1,i);}
            box.Controls.Add(Ui.Button("Đổi mật khẩu",()=>{if(fresh.Text!=repeat.Text)throw new ArgumentException("Hai lần nhập mật khẩu mới chưa giống nhau.");Accounts.ChangePassword(old.Text,fresh.Text);old.Clear();fresh.Clear();repeat.Clear();Message("Đã đổi mật khẩu.");},true),1,3);Status=Ui.Label("");box.Controls.Add(Status,1,4);Content(box);
        }
    }
}

using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace QuanLyBanHangSupermarket
{
    public class BackupForm : WorkspacePage
    {
        readonly TextBox confirmation=new TextBox {Width=300};
        public BackupForm() : base("Sao lưu / khôi phục","Sao lưu database vào file .bak. SQL Server cần quyền đọc/ghi thư mục đã chọn.")
        {
            Session.Require("Admin");
            var box=new FlowLayoutPanel {Dock=DockStyle.Top,FlowDirection=FlowDirection.TopDown,WrapContents=false,AutoSize=true};
            box.Controls.Add(Ui.Label("Sao lưu lưu toàn bộ danh mục, tài khoản, hóa đơn và lịch sử kho."));box.Controls.Add(Ui.Button("Tạo bản sao lưu (.bak)",Backup,true));
            var warning=Ui.Label("Khôi phục sẽ thay thế toàn bộ database hiện tại.\nTạo bản sao lưu trước khi tiếp tục. Sau khôi phục, ứng dụng sẽ đóng để đăng nhập lại.",true);warning.Margin=new Padding(0,28,0,12);box.Controls.Add(warning);
            box.Controls.Add(Ui.Label("Gõ KHÔI PHỤC để bật thao tác khôi phục:"));box.Controls.Add(confirmation);box.Controls.Add(Ui.Button("Chọn bản sao lưu và khôi phục",Restore));
            box.Controls.Add(Ui.Label("Ảnh sản phẩm lưu theo đường dẫn file riêng. Sao lưu cả các file ảnh khi chuyển máy."));Status=Ui.Label("");Status.MaximumSize=new System.Drawing.Size(900,0);box.Controls.Add(Status);Content(box);
        }
        void Backup()
        {
            Session.Require("Admin");
            using(var dialog=new SaveFileDialog {Filter="SQL Server backup|*.bak",FileName="Supermarket_"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+".bak",OverwritePrompt=true})
            {
                if(dialog.ShowDialog()!=DialogResult.OK)return;
                using(var conn=DatabaseHelper.GetConnection())using(var cmd=new SqlCommand("BACKUP DATABASE [QuanLyBanHangSupermarket] TO DISK=@p WITH INIT,COPY_ONLY,CHECKSUM",conn)){cmd.Parameters.Add(DatabaseHelper.Param("@p",dialog.FileName));cmd.CommandTimeout=300;conn.Open();cmd.ExecuteNonQuery();}
                Message("Đã sao lưu: "+dialog.FileName);
            }
        }
        void Restore()
        {
            Session.Require("Admin");
            if(confirmation.Text.Trim()!="KHÔI PHỤC")throw new ArgumentException("Gõ chính xác KHÔI PHỤC để xác nhận.");
            using(var dialog=new OpenFileDialog {Filter="SQL Server backup|*.bak",CheckFileExists=true})
            {
                if(dialog.ShowDialog()!=DialogResult.OK)return;
                var builder=new SqlConnectionStringBuilder(DatabaseHelper.ConnectionString){InitialCatalog="master"};
                using(var conn=new SqlConnection(builder.ConnectionString))
                {
                    conn.Open();
                    using(var check=new SqlCommand("RESTORE HEADERONLY FROM DISK=@p",conn))
                    {
                        check.Parameters.Add(DatabaseHelper.Param("@p",dialog.FileName));using(var reader=check.ExecuteReader())
                            if(!reader.Read()||reader["DatabaseName"].ToString()!="QuanLyBanHangSupermarket"||Convert.ToInt32(reader["BackupType"])!=1)throw new ArgumentException("File phải là bản sao lưu toàn bộ của database QuanLyBanHangSupermarket.");
                    }
                    if(!Ui.Confirm("Thay thế toàn bộ dữ liệu hiện tại bằng file:\n"+dialog.FileName+"\n\nCác kết nối khác sẽ bị đóng. Tiếp tục khôi phục?"))return;
                    SqlConnection.ClearAllPools();bool single=false;
                    try
                    {
                        using(var cmd=new SqlCommand("ALTER DATABASE [QuanLyBanHangSupermarket] SET SINGLE_USER WITH ROLLBACK IMMEDIATE",conn)){cmd.CommandTimeout=300;cmd.ExecuteNonQuery();single=true;}
                        using(var cmd=new SqlCommand("RESTORE DATABASE [QuanLyBanHangSupermarket] FROM DISK=@p WITH REPLACE,CHECKSUM",conn)){cmd.Parameters.Add(DatabaseHelper.Param("@p",dialog.FileName));cmd.CommandTimeout=300;cmd.ExecuteNonQuery();}
                    }
                    finally{if(single)using(var cmd=new SqlCommand("ALTER DATABASE [QuanLyBanHangSupermarket] SET MULTI_USER",conn)){cmd.CommandTimeout=300;cmd.ExecuteNonQuery();}}
                }
                MessageBox.Show("Khôi phục hoàn tất. Mở lại ứng dụng và đăng nhập bằng tài khoản trong bản sao lưu.","Khôi phục",MessageBoxButtons.OK,MessageBoxIcon.Information);Application.Exit();
            }
        }
    }
}

using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace QuanLyBanHangSupermarket
{
    public static class Ui
    {
        public static readonly Color Ink=Color.FromArgb(26,43,52), Muted=Color.FromArgb(83,101,110), Accent=Color.FromArgb(0,104,91), Surface=Color.FromArgb(243,247,247), Line=Color.FromArgb(217,226,228);
        public static Font Body { get { return new Font("Segoe UI",10F); } }
        public static void Error(Exception ex)
        {
            string message = ex.Message;
            var sql = ex as SqlException;
            if(sql!=null)
            {
                if (sql.Number==2601 || sql.Number==2627) message="Mã đã tồn tại. Vui lòng nhập mã khác.";
                else if (sql.Number==547) message="Dữ liệu đang được sử dụng hoặc danh mục liên quan chưa hợp lệ. Kiểm tra lại các mục đã chọn; không xóa dữ liệu đã có giao dịch.";
                else if(sql.Number==208 || sql.Number==207) message="Cấu trúc database chưa được nâng cấp. Chạy Database/01_Setup.sql rồi mở lại ứng dụng.";
                else if(sql.Number==-2 || sql.Number==53 || sql.Number==2 || sql.Number==4060) message="Không kết nối được SQL Server/database. Kiểm tra dịch vụ SQLEXPRESS01 và ConnStr trong App.config.";
            }
            MessageBox.Show(message,"Không thể hoàn tất",MessageBoxButtons.OK,MessageBoxIcon.Warning);
        }
        public static bool Run(Action action)
        { try { action(); return true; } catch(Exception ex) { Error(ex); return false; } }
        public static bool Confirm(string text)
        { return MessageBox.Show(text,"Xác nhận",MessageBoxButtons.YesNo,MessageBoxIcon.Question,MessageBoxDefaultButton.Button2)==DialogResult.Yes; }
        public static Button Button(string text, Action click, bool primary=false)
        {
            var b=new Button {Text=text,AutoSize=true,MinimumSize=new Size(100,36),Height=36,FlatStyle=FlatStyle.Flat,Font=Body,Padding=new Padding(12,3,12,3),Margin=new Padding(0,3,8,3),BackColor=primary?Accent:Color.White,ForeColor=primary?Color.White:Ink,Cursor=Cursors.Hand};
            b.FlatAppearance.BorderColor=primary?Accent:Line;
            b.FlatAppearance.MouseOverBackColor=primary?Color.FromArgb(0,83,73):Surface;
            b.EnabledChanged+=(s,e)=>{b.BackColor=b.Enabled?(primary?Accent:Color.White):Surface;b.ForeColor=b.Enabled?(primary?Color.White:Ink):Muted;};
            b.Click+=(s,e)=>Run(click); return b;
        }
        public static Label Label(string text, bool strong=false)
        { return new Label { Text=text,AutoSize=true,ForeColor=strong?Ink:Muted,Font=strong?new Font("Segoe UI",10F,FontStyle.Bold):Body,Margin=new Padding(0,8,12,6) }; }
        public static FlowLayoutPanel Bar()
        { return new FlowLayoutPanel { Dock=DockStyle.Fill,AutoSize=false,WrapContents=true,Padding=new Padding(0),BackColor=Color.Transparent }; }
        public static DataGridView Grid()
        {
            var g=new DataGridView {Dock=DockStyle.Fill,ReadOnly=true,AllowUserToAddRows=false,AllowUserToDeleteRows=false,MultiSelect=false,SelectionMode=DataGridViewSelectionMode.FullRowSelect,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill,BackgroundColor=Color.White,BorderStyle=BorderStyle.None,RowHeadersVisible=false,EnableHeadersVisualStyles=false,Font=Body,ColumnHeadersHeight=40,ColumnHeadersHeightSizeMode=DataGridViewColumnHeadersHeightSizeMode.DisableResizing,CellBorderStyle=DataGridViewCellBorderStyle.SingleHorizontal,GridColor=Line,AutoGenerateColumns=true};
            g.ColumnHeadersDefaultCellStyle=new DataGridViewCellStyle {BackColor=Surface,ForeColor=Ink,Font=new Font("Segoe UI",10F,FontStyle.Bold),Padding=new Padding(6),SelectionBackColor=Surface,SelectionForeColor=Ink};
            g.DefaultCellStyle=new DataGridViewCellStyle {ForeColor=Ink,BackColor=Color.White,SelectionBackColor=Color.FromArgb(221,241,236),SelectionForeColor=Ink,Padding=new Padding(6,2,6,2),NullValue=""};
            g.AlternatingRowsDefaultCellStyle.BackColor=Color.FromArgb(249,251,251);g.RowTemplate.Height=36;
            g.BindingContext=new BindingContext();
            g.DataError+=(s,e)=> {e.ThrowException=false;};
            return g;
        }
        public static void Bind(DataGridView g,DataTable dt)
        {
            g.DataSource=dt;
            foreach(DataGridViewColumn c in g.Columns)
            {
                c.MinimumWidth=90;
                if(c.ValueType==typeof(decimal)) {c.DefaultCellStyle.Format="N2";c.DefaultCellStyle.Alignment=DataGridViewContentAlignment.MiddleRight;}
                if(c.ValueType==typeof(int)||c.ValueType==typeof(long)) {c.DefaultCellStyle.Format="N0";c.DefaultCellStyle.Alignment=DataGridViewContentAlignment.MiddleRight;}
                if(c.ValueType==typeof(DateTime)) c.DefaultCellStyle.Format="dd/MM/yyyy HH:mm";
            }
        }
        public static ComboBox Combo() { return new ComboBox {DropDownStyle=ComboBoxStyle.DropDownList,Width=200,Font=Body,IntegralHeight=false,MaxDropDownItems=14,Margin=new Padding(0,6,12,4)}; }
        public static string Selected(ComboBox c) { return c.SelectedValue==null || c.SelectedValue==DBNull.Value?null:c.SelectedValue.ToString(); }
        public static void Fill(ComboBox c,string table,string key,string name,bool all=false)
        {
            var dt=DatabaseHelper.GetDataToTable("SELECT "+Catalogs.Q(key)+","+Catalogs.Q(name)+" FROM "+Catalogs.Q(table)+" ORDER BY "+Catalogs.Q(name));
            if(all) {var r=dt.NewRow();r[key]="";r[name]="Tất cả";dt.Rows.InsertAt(r,0);}
            c.DataSource=dt;c.DisplayMember=name;c.ValueMember=key;c.SelectedIndex=all?0:-1;
        }
        public static NumericUpDown Number(decimal max=1000000, int digits=0)
        { return new NumericUpDown {Minimum=0,Maximum=max,DecimalPlaces=digits,ThousandsSeparator=true,Width=145,Font=Body,Margin=new Padding(0,6,12,4)}; }
        public static DateTimePicker Date(bool month=false)
        { return new DateTimePicker {Format=DateTimePickerFormat.Custom,CustomFormat=month?"MM/yyyy":"dd/MM/yyyy",Width=145,Font=Body,Margin=new Padding(0,6,12,4)}; }
    }
    public class WorkspacePage : Form
    {
        protected readonly TableLayoutPanel PageLayout;
        protected Label Status;
        public virtual bool HasPendingChanges { get { return false; } }
        public bool CanLeave() { return !HasPendingChanges || Ui.Confirm("Có dữ liệu chưa lưu. Bạn có muốn bỏ các thay đổi và rời màn hình này?"); }
        public WorkspacePage(string title,string description)
        {
            Text=title;Font=Ui.Body;BackColor=Color.White;Padding=new Padding(24,16,24,16);AutoScaleMode=AutoScaleMode.Font;
            PageLayout=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=2,Margin=new Padding(0)};
            PageLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
            PageLayout.RowStyles.Add(new RowStyle(SizeType.Absolute,76));PageLayout.RowStyles.Add(new RowStyle(SizeType.Percent,100));
            var header=new FlowLayoutPanel {Dock=DockStyle.Fill,FlowDirection=FlowDirection.TopDown,WrapContents=false,Margin=new Padding(0)};
            header.Controls.Add(new Label {Text=title,Font=new Font("Segoe UI",23F,FontStyle.Bold),ForeColor=Ui.Ink,AutoSize=true,Margin=new Padding(0)});
            header.Controls.Add(Ui.Label(description));PageLayout.Controls.Add(header,0,0);Controls.Add(PageLayout);
        }
        protected void Content(Control content)
        {
            var table=content as TableLayoutPanel;
            if(table!=null && table.ColumnCount==1 && table.ColumnStyles.Count==0)table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
            PageLayout.Controls.Add(content,0,1);
        }
        protected void Message(string text) { if(Status!=null) Status.Text=text; }
    }
}

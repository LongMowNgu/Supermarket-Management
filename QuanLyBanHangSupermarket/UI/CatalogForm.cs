using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace QuanLyBanHangSupermarket
{
    public class CatalogForm : WorkspacePage
    {
        readonly CatalogSpec spec;
        readonly Dictionary<string,Control> inputs=new Dictionary<string,Control>();
        readonly Dictionary<string,Dictionary<string,string>> lookupNames=new Dictionary<string,Dictionary<string,string>>();
        readonly DataGridView grid=Ui.Grid();
        readonly TextBox search=new TextBox {Width=240};
        readonly FlowLayoutPanel actions=Ui.Bar();
        readonly TableLayoutPanel editor=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=4,AutoScroll=true,Padding=new Padding(0,8,0,8)};
        readonly ComboBox group=Ui.Combo(), material=Ui.Combo(), use=Ui.Combo();
        bool editing,adding,loading;
        DataTable data;
        public override bool HasPendingChanges {get {return editing;}}
        public CatalogForm(CatalogSpec specification) : base(specification.Title,specification.Table=="DMHangHoa"?"Tìm theo nhóm, chất liệu, công dụng. Tồn kho và giá được cập nhật qua nghiệp vụ nhập hàng/kiểm kê.":"Quản lý danh mục, thêm và cập nhật thông tin.")
        {
            spec=specification;
            var body=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=5};
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
            body.RowStyles.Add(new RowStyle(SizeType.Absolute,spec.Table=="DMHangHoa"?88:46));
            body.RowStyles.Add(new RowStyle(SizeType.Absolute,spec.Fields.Length>8?300:spec.Fields.Length>3?210:105));
            body.RowStyles.Add(new RowStyle(SizeType.Absolute,48));body.RowStyles.Add(new RowStyle(SizeType.Percent,100));body.RowStyles.Add(new RowStyle(SizeType.Absolute,30));
            var filters=Ui.Bar();filters.Controls.Add(Ui.Label("Tìm mã/tên"));filters.Controls.Add(search);
            filters.Controls.Add(Ui.Button("Tìm kiếm",LoadData,true));filters.Controls.Add(Ui.Button("Tải lại",()=>{if(CanLeave()){SetEditing(false);LoadLookups();LoadData();}}));
            if(spec.Table=="DMHangHoa")
            {
                filters.Controls.Add(Ui.Label("Nhóm"));filters.Controls.Add(group);filters.Controls.Add(Ui.Label("Chất liệu"));filters.Controls.Add(material);filters.Controls.Add(Ui.Label("Công dụng"));filters.Controls.Add(use);
                group.Width=material.Width=use.Width=145;
                foreach(var c in new[]{group,material,use}) c.SelectedIndexChanged+=(s,e)=>{if(!loading)Filter();};
            }
            editor.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,140));editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));
            editor.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,140));editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));
            for(int i=0;i<spec.Fields.Length;i++)
            {
                var f=spec.Fields[i];int row=i/2,col=(i%2)*2;
                if(i%2==0) editor.RowStyles.Add(new RowStyle(SizeType.Absolute,40));
                editor.Controls.Add(Ui.Label(f.Label+(f.Required?" *":"")),col,row);
                Control control;
                if(f.Kind=="Lookup") control=Ui.Combo();
                else if(f.Kind=="Gender") {var c=Ui.Combo();c.Items.AddRange(new object[]{"Nam","Nữ","Khác"});control=c;}
                else if(f.Kind=="Number"||f.Kind=="Money") control=Ui.Number(f.Kind=="Money"?1100000000:2147483647,f.Kind=="Money"?2:0);
                else if(f.Kind=="Bool") control=new CheckBox {Text="Có",AutoSize=true};
                else if(f.Kind=="Date") {var d=Ui.Date();d.ShowCheckBox=true;d.Checked=false;control=d;}
                else control=new TextBox {MaxLength=f.Length,Font=Ui.Body};
                inputs.Add(f.Name,control);control.Dock=DockStyle.Fill;control.Margin=new Padding(0,4,16,4);control.AccessibleName=f.Label;
                if(f.Kind=="Image")
                {
                    var p=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=2,Margin=new Padding(0)};
                    p.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));p.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,90));p.Controls.Add(control,0,0);
                    var b=Ui.Button("Chọn…",()=>ChooseImage((TextBox)control));b.AutoSize=false;b.MinimumSize=new Size(60,28);b.Padding=new Padding(4);b.Dock=DockStyle.Fill;b.Margin=new Padding(0,0,12,0);p.RowStyles.Add(new RowStyle(SizeType.Percent,100));p.Controls.Add(b,1,0);editor.Controls.Add(p,col+1,row);
                    b.Enabled=Session.Role=="Admin"||Session.Role=="ThuKho";
                }
                else editor.Controls.Add(control,col+1,row);
            }
            actions.Controls.Add(Ui.Button("Thêm mới",()=>{adding=true;Clear();SetEditing(true);inputs[spec.Key].Focus();},true));
            actions.Controls.Add(Ui.Button("Sửa thông tin",()=>{if(grid.CurrentRow==null)throw new ArgumentException("Chọn một dòng trong danh sách.");ReadSelection();adding=false;SetEditing(true);}));
            actions.Controls.Add(Ui.Button("Xóa",Delete));actions.Controls.Add(Ui.Button("Lưu",Save,true));actions.Controls.Add(Ui.Button("Hủy sửa",()=>{if(CanLeave()){SetEditing(false);ReadSelection();}}));
            actions.Controls.Add(Ui.Button("Xuất CSV",()=>Exports.Csv(grid,spec.Title)));
            if(spec.Table=="DMHangHoa")actions.Controls.Add(Ui.Button("Xem ảnh",()=>
            {
                string path=inputs["Anh"].Text;
                if(string.IsNullOrWhiteSpace(path)||!File.Exists(path))throw new ArgumentException("Sản phẩm chưa có ảnh hoặc file ảnh không còn ở đường dẫn đã lưu.");
                using(var source=Image.FromFile(path))
                using(var view=new Form {Text="Ảnh sản phẩm",ClientSize=new Size(700,560),StartPosition=FormStartPosition.CenterParent})
                using(var image=new Bitmap(source))
                {view.Controls.Add(new PictureBox {Dock=DockStyle.Fill,SizeMode=PictureBoxSizeMode.Zoom,Image=image,BackColor=Color.White});view.ShowDialog();}
            }));
            Status=Ui.Label("");Status.Dock=DockStyle.Fill;
            body.Controls.Add(filters,0,0);body.Controls.Add(editor,0,1);body.Controls.Add(actions,0,2);body.Controls.Add(grid,0,3);body.Controls.Add(Status,0,4);Content(body);
            grid.SelectionChanged+=(s,e)=>{if(!loading&&!editing)ReadSelection();};
            grid.CellFormatting+=(s,e)=>
            {
                if(e.RowIndex<0||e.Value==null)return;string name=grid.Columns[e.ColumnIndex].Name;
                Dictionary<string,string> names;string display;
                if(lookupNames.TryGetValue(name,out names)&&names.TryGetValue(e.Value.ToString(),out display)){e.Value=display;e.FormattingApplied=true;}
            };
            search.KeyDown+=(s,e)=>{if(e.KeyCode==Keys.Enter){e.SuppressKeyPress=true;Ui.Run(LoadData);}};
            Shown+=(s,e)=>Ui.Run(()=>{LoadLookups();LoadData();SetEditing(false);});
            if(inputs.ContainsKey("MaNhom")&&spec.Table=="DMHangHoa") ((ComboBox)inputs["MaNhom"]).SelectedIndexChanged+=(s,e)=>{if(!loading)Ui.Run(LoadTypes);};
        }
        void ChooseImage(TextBox box)
        {
            if(!editing)throw new ArgumentException("Nhấn Thêm mới hoặc Sửa thông tin trước khi chọn ảnh.");
            using(var d=new OpenFileDialog {Filter="Ảnh|*.png;*.jpg;*.jpeg;*.bmp",CheckFileExists=true})
                if(d.ShowDialog()==DialogResult.OK)
                {
                    using(var image=Image.FromFile(d.FileName)) {if(image.Width<=0)throw new ArgumentException("Ảnh không hợp lệ.");}
                    box.Text=d.FileName;
                }
        }
        void LoadLookups()
        {
            loading=true;
            try
            {
                foreach(var f in spec.Fields.Where(f=>f.Kind=="Lookup"))
                {
                    Ui.Fill((ComboBox)inputs[f.Name],f.LookupTable,f.LookupKey,f.LookupLabel);
                    var source=(DataTable)((ComboBox)inputs[f.Name]).DataSource;
                    lookupNames[f.Name]=source.Rows.Cast<DataRow>().ToDictionary(r=>r[f.LookupKey].ToString(),r=>r[f.LookupLabel].ToString());
                }
                if(spec.Table=="DMHangHoa") {Ui.Fill(group,"NhomHang","MaNhom","TenNhom",true);Ui.Fill(material,"ChatLieu","MaChatLieu","TenChatLieu",true);Ui.Fill(use,"CongDung","MaCongDung","TenCongDung",true);}
            } finally {loading=false;}
        }
        void LoadTypes()
        {
            var c=(ComboBox)inputs["MaLoai"];string selected=Ui.Selected(c);
            c.DataSource=DatabaseHelper.GetDataToTable("SELECT MaLoai,TenLoai FROM TheLoai WHERE MaNhom=@g ORDER BY TenLoai",DatabaseHelper.Param("@g",Ui.Selected((ComboBox)inputs["MaNhom"])));
            c.DisplayMember="TenLoai";c.ValueMember="MaLoai";c.SelectedIndex=-1;if(selected!=null)c.SelectedValue=selected;
        }
        void LoadData()
        {
            if(editing)throw new ArgumentException("Lưu hoặc hủy sửa trước khi tìm kiếm.");
            loading=true;
            try
            {
                data=Catalogs.Load(spec,search.Text.Trim());Ui.Bind(grid,data);
                foreach(var f in spec.Fields)if(grid.Columns.Contains(f.Name))grid.Columns[f.Name].HeaderText=f.Label;
                if(grid.Columns.Contains("Anh"))grid.Columns["Anh"].Visible=false;
                if(grid.Columns.Contains("GhiChu"))grid.Columns["GhiChu"].Visible=false;
                if(spec.Table=="DMHangHoa")
                {
                    var visible=new[]{"MaVach","TenHangHoa","MaNhom","MaLoai","MaDonVi","SoLuong","DonGiaNhap","DonGiaBan"};
                    foreach(DataGridViewColumn c in grid.Columns)c.Visible=visible.Contains(c.Name);
                    grid.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.None;
                    for(int i=0;i<visible.Length;i++){var c=grid.Columns[visible[i]];c.DisplayIndex=i;c.Width=c.Name=="TenHangHoa"?210:c.Name=="SoLuong"?95:135;}
                }
            } finally {loading=false;}
            Filter();ReadSelection();
        }
        void Filter()
        {
            if(data==null)return;
            var clauses=new List<string>();
            if(spec.Table=="DMHangHoa")
                foreach(var entry in new[]{Tuple.Create("MaNhom",group),Tuple.Create("MaChatLieu",material),Tuple.Create("MaCongDung",use)})
                    if(!string.IsNullOrEmpty(Ui.Selected(entry.Item2)))clauses.Add(entry.Item1+"='"+Ui.Selected(entry.Item2).Replace("'","''")+"'");
            data.DefaultView.RowFilter=string.Join(" AND ",clauses);
            Message(data.DefaultView.Count==0?"Chưa có dữ liệu phù hợp. Nhấn Thêm mới để bắt đầu.":data.DefaultView.Count+" bản ghi. Dấu * là thông tin bắt buộc.");
        }
        void Clear()
        {
            loading=true;
            foreach(var c in inputs.Values)
            {
                if(c is TextBox)((TextBox)c).Clear();else if(c is ComboBox)((ComboBox)c).SelectedIndex=-1;
                else if(c is NumericUpDown)((NumericUpDown)c).Value=0;else if(c is CheckBox)((CheckBox)c).Checked=false;
                else if(c is DateTimePicker){((DateTimePicker)c).Value=DateTime.Today;((DateTimePicker)c).Checked=false;}
            }
            loading=false;
        }
        void ReadSelection()
        {
            if(grid.CurrentRow==null){Clear();return;}loading=true;
            try
            {
                var r=((DataRowView)grid.CurrentRow.DataBoundItem).Row;
                foreach(var f in spec.Fields)
                {
                    var c=inputs[f.Name];object v=r[f.Name];
                    if(c is TextBox)c.Text=v.ToString();
                    else if(c is ComboBox){if(v==DBNull.Value)((ComboBox)c).SelectedIndex=-1;else if(f.Kind=="Gender")((ComboBox)c).SelectedItem=v.ToString();else ((ComboBox)c).SelectedValue=v;}
                    else if(c is NumericUpDown)((NumericUpDown)c).Value=v==DBNull.Value?0:Convert.ToDecimal(v);
                    else if(c is CheckBox)((CheckBox)c).Checked=v!=DBNull.Value&&Convert.ToInt32(v)!=0;
                    else if(c is DateTimePicker){var d=(DateTimePicker)c;d.Checked=v!=DBNull.Value;if(d.Checked)d.Value=Convert.ToDateTime(v);}
                }
            }finally{loading=false;}
        }
        void SetEditing(bool value)
        {
            editing=value;bool permission=spec.WriteRoles.Contains(Session.Role);
            foreach(var f in spec.Fields)inputs[f.Name].Enabled=value&&!f.Computed&&(f.Name!=spec.Key||adding);
            foreach(Button b in actions.Controls.OfType<Button>())
                b.Enabled=b.Text=="Xuất CSV" || b.Text=="Xem ảnh" || (permission && ((b.Text=="Lưu"||b.Text=="Hủy sửa")?value:!value));
            grid.Enabled=!value;search.Enabled=!value;group.Enabled=material.Enabled=use.Enabled=!value;
        }
        void Save()
        {
            var values=new Dictionary<string,object>();
            foreach(var f in spec.Fields)
            {
                var c=inputs[f.Name];object value=null;
                if(c is TextBox)value=string.IsNullOrWhiteSpace(c.Text)?null:c.Text.Trim();
                else if(c is ComboBox)value=f.Kind=="Gender"?((ComboBox)c).SelectedItem:((ComboBox)c).SelectedValue;
                else if(c is NumericUpDown)value=f.Kind=="Number"?(object)Convert.ToInt32(((NumericUpDown)c).Value):((NumericUpDown)c).Value;
                else if(c is CheckBox)value=((CheckBox)c).Checked?1:0;
                else if(c is DateTimePicker)value=((DateTimePicker)c).Checked?(object)((DateTimePicker)c).Value.Date:null;
                values.Add(f.Name,value);
            }
            if(values.ContainsKey("Anh")&&values["Anh"]!=null&&!File.Exists(values["Anh"].ToString()))throw new ArgumentException("Không tìm thấy file ảnh. Chọn lại đường dẫn hoặc để trống.");
            Catalogs.Save(spec,values,adding);SetEditing(false);LoadData();Message("Đã lưu thông tin.");
        }
        void Delete()
        {
            if(grid.CurrentRow==null)throw new ArgumentException("Chọn bản ghi cần xóa.");
            string key=((DataRowView)grid.CurrentRow.DataBoundItem)[spec.Key].ToString();
            if(Ui.Confirm("Xóa bản ghi "+key+"? Dữ liệu đã có giao dịch sẽ được bảo vệ.")){Catalogs.Delete(spec,key);LoadData();}
        }
    }
}

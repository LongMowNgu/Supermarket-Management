using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace QuanLyBanHangSupermarket
{
    public class InvoiceForm : WorkspacePage
    {
        readonly bool sale;
        readonly ComboBox employee=Ui.Combo(),partner=Ui.Combo(),product=Ui.Combo();
        readonly NumericUpDown qty=Ui.Number(),price=Ui.Number(1000000000,2),discount=Ui.Number(100,2);
        readonly DateTimePicker date=Ui.Date();
        readonly TextBox barcode=new TextBox {Width=165,MaxLength=50};
        readonly DataGridView grid=Ui.Grid();
        readonly DataTable cart=Money.NewCart();
        readonly Label total=Ui.Label("Tổng cộng: 0,00 VNĐ",true);
        readonly Button save,print;
        readonly FlowLayoutPanel line=Ui.Bar(), meta=Ui.Bar(), rowActions=Ui.Bar();
        bool loading,saved;
        string savedId;
        public override bool HasPendingChanges {get {return !saved&&cart.Rows.Count>0;}}
        public InvoiceForm(bool isSale) : base(isSale?"Bán hàng":"Nhập hàng",isSale?"Quét/nhập mã vạch, thêm hàng và lưu hóa đơn. Giảm giá tính theo phần trăm.":"Lưu phiếu nhập sẽ cộng tồn kho và cập nhật giá bán bằng 110% giá nhập.")
        {
            sale=isSale;qty.Minimum=1;qty.Value=1;price.Maximum=sale?1100000000m:1000000000m;price.Enabled=!sale;product.Width=340;
            var body=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=6};
            body.RowStyles.Add(new RowStyle(SizeType.Absolute,85));body.RowStyles.Add(new RowStyle(SizeType.Absolute,110));body.RowStyles.Add(new RowStyle(SizeType.Absolute,48));body.RowStyles.Add(new RowStyle(SizeType.Percent,100));body.RowStyles.Add(new RowStyle(SizeType.Absolute,50));body.RowStyles.Add(new RowStyle(SizeType.Absolute,78));
            meta.Controls.Add(Ui.Label("Nhân viên *"));meta.Controls.Add(employee);meta.Controls.Add(Ui.Label(sale?"Khách hàng *":"Nhà cung cấp *"));meta.Controls.Add(partner);meta.Controls.Add(Ui.Label("Ngày"));meta.Controls.Add(date);meta.Controls.Add(Ui.Button("Nạp danh mục",LoadLookups));
            line.Controls.Add(Ui.Label("Mã vạch"));line.Controls.Add(barcode);line.Controls.Add(Ui.Label("Hàng hóa"));line.Controls.Add(product);line.Controls.Add(Ui.Label("Số lượng"));line.Controls.Add(qty);line.Controls.Add(Ui.Label("Đơn giá (VNĐ)"));line.Controls.Add(price);line.Controls.Add(Ui.Label("Giảm (%)"));line.Controls.Add(discount);line.Controls.Add(Ui.Button("Thêm vào hóa đơn",AddLine,true));
            rowActions.Controls.Add(Ui.Button("Áp dụng cho dòng đã chọn",UpdateLine));rowActions.Controls.Add(Ui.Button("Bỏ dòng đã chọn",()=>{if(grid.CurrentRow==null)return;cart.Rows.Remove(((DataRowView)grid.CurrentRow.DataBoundItem).Row);RefreshTotal();}));
            Ui.Bind(grid,cart);grid.Columns["GiaVon"].Visible=false;
            string[] labels={"Mã vạch","Hàng hóa","Số lượng","Đơn giá (VNĐ)","Giảm (%)","Thành tiền (VNĐ)"};for(int i=0;i<labels.Length;i++)grid.Columns[i].HeaderText=labels[i];
            grid.SelectionChanged+=(s,e)=>{if(grid.CurrentRow==null||loading)return;loading=true;var r=((DataRowView)grid.CurrentRow.DataBoundItem).Row;product.SelectedValue=r["MaVach"];qty.Value=(int)r["SoLuong"];price.Value=(decimal)r["DonGia"];discount.Value=(decimal)r["GiamGia"];loading=false;};
            var footer=Ui.Bar();save=Ui.Button("Lưu hóa đơn",Save,true);print=Ui.Button("Xem trước / In",()=>Exports.PrintInvoice(sale,savedId));print.Enabled=false;footer.Controls.Add(save);footer.Controls.Add(print);footer.Controls.Add(Ui.Button("Hóa đơn mới",Reset));
            Status=Ui.Label("Chưa lưu hóa đơn.");Status.MaximumSize=new Size(900,0);footer.Controls.Add(Status);total.Font=new Font("Segoe UI",19F,FontStyle.Bold);
            body.Controls.Add(meta,0,0);body.Controls.Add(line,0,1);body.Controls.Add(rowActions,0,2);body.Controls.Add(grid,0,3);body.Controls.Add(total,0,4);body.Controls.Add(footer,0,5);Content(body);
            product.SelectedIndexChanged+=(s,e)=>{if(!loading)Ui.Run(SelectProduct);};
            barcode.KeyDown+=(s,e)=>{if(e.KeyCode==Keys.Enter){e.SuppressKeyPress=true;Ui.Run(()=>{product.SelectedValue=barcode.Text.Trim();if(Ui.Selected(product)==null)throw new ArgumentException("Không tìm thấy mã vạch.");AddLine();barcode.Clear();barcode.Focus();});}};
            Shown+=(s,e)=>Ui.Run(LoadLookups);
        }
        void LoadLookups()
        {
            if(saved)return;loading=true;
            try
            {
                string oldEmployee=Ui.Selected(employee),oldPartner=Ui.Selected(partner);
                Ui.Fill(employee,"NhânVien","MaNV","TenNV");Ui.Fill(partner,sale?"KhachHang":"NhaCungCap",sale?"MaKhach":"MaNCC",sale?"TenKhach":"TenNCC");
                product.DataSource=DatabaseHelper.GetDataToTable("SELECT MaVach,MaVach+N' — '+TenHangHoa AS Ten FROM DMHangHoa ORDER BY TenHangHoa");product.DisplayMember="Ten";product.ValueMember="MaVach";product.SelectedIndex=-1;
                if(oldEmployee!=null)employee.SelectedValue=oldEmployee;if(oldPartner!=null)partner.SelectedValue=oldPartner;
                if(!Session.IsAdmin){employee.SelectedValue=Session.EmployeeId;employee.Enabled=false;}
            } finally {loading=false;}
        }
        DataRow Product()
        {
            if(Ui.Selected(product)==null)throw new ArgumentException("Chọn một hàng hóa.");
            var dt=DatabaseHelper.GetDataToTable("SELECT * FROM DMHangHoa WHERE MaVach=@c",DatabaseHelper.Param("@c",Ui.Selected(product)));
            if(dt.Rows.Count==0)throw new ArgumentException("Hàng hóa đã bị xóa. Nạp lại danh mục.");return dt.Rows[0];
        }
        void SelectProduct()
        {if(Ui.Selected(product)==null)return;var r=Product();price.Value=Convert.ToDecimal(r[sale?"DonGiaBan":"DonGiaNhap"]);qty.Value=1;discount.Value=0;Message("Tồn kho hiện tại: "+r["SoLuong"]+". Chỉ hóa đơn đã lưu mới cập nhật kho.");}
        void AddLine()
        {
            var p=Product();string code=p["MaVach"].ToString();int q=Convert.ToInt32(qty.Value);decimal pr=price.Value,d=discount.Value;
            Money.LineTotal(q,pr,d);var existing=cart.Rows.Find(code);
            if(existing!=null&&(pr!=(decimal)existing["DonGia"]||d!=(decimal)existing["GiamGia"]))throw new ArgumentException("Sản phẩm đã có với giá/giảm giá khác. Chọn dòng và nhấn Áp dụng để chỉnh lại.");
            int newQty=existing==null?q:checked((int)existing["SoLuong"]+q);
            if(sale&&newQty>Convert.ToInt32(p["SoLuong"]))throw new ArgumentException("Số lượng vượt tồn kho hiện tại ("+p["SoLuong"]+").");
            decimal amount=Money.LineTotal(newQty,pr,d);loading=true;
            if(existing==null)cart.Rows.Add(code,p["TenHangHoa"],newQty,pr,d,amount,p["DonGiaNhap"]);
            else{existing["SoLuong"]=newQty;existing["ThanhTien"]=amount;}loading=false;RefreshTotal();
        }
        void UpdateLine()
        {
            if(grid.CurrentRow==null)throw new ArgumentException("Chọn dòng cần sửa.");var r=((DataRowView)grid.CurrentRow.DataBoundItem).Row;
            if(Ui.Selected(product)!=r["MaVach"].ToString())throw new ArgumentException("Chọn đúng sản phẩm của dòng đang sửa; để đổi sản phẩm hãy bỏ dòng rồi thêm mới.");
            var p=Product();int q=(int)qty.Value;if(sale&&q>Convert.ToInt32(p["SoLuong"]))throw new ArgumentException("Số lượng vượt tồn kho.");
            decimal pr=sale?Convert.ToDecimal(p["DonGiaBan"]):price.Value;decimal amount=Money.LineTotal(q,pr,discount.Value);loading=true;
            r["SoLuong"]=q;r["DonGia"]=pr;r["GiamGia"]=discount.Value;r["ThanhTien"]=amount;loading=false;RefreshTotal();
        }
        void RefreshTotal(){total.Text="Tổng cộng: "+Money.Total(cart).ToString("N2")+" VNĐ";}
        void Save()
        {
            save.Enabled=false;
            try{savedId=Sales.Save(sale,Ui.Selected(employee),Ui.Selected(partner),date.Value,cart);saved=true;meta.Enabled=line.Enabled=rowActions.Enabled=false;print.Enabled=true;Message("Đã lưu "+savedId+". Kho đã cập nhật.");}
            finally{save.Enabled=!saved;}
        }
        void Reset()
        {
            if(!CanLeave())return;loading=true;cart.Clear();loading=false;saved=false;savedId=null;save.Enabled=true;print.Enabled=false;meta.Enabled=line.Enabled=rowActions.Enabled=true;date.Value=DateTime.Now;LoadLookups();RefreshTotal();Message("Chưa lưu hóa đơn.");
        }
    }
}

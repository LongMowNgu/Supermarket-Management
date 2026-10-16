using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace QuanLyBanHangSupermarket
{
    public static class Exports
    {
        public static string CsvCell(object value)
        {
            string text=value==null||value==DBNull.Value?"":value is DateTime?((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss"):Convert.ToString(value,CultureInfo.GetCultureInfo("vi-VN"));
            if(value is string && text.TrimStart().Length>0 && "=+-@".IndexOf(text.TrimStart()[0])>=0)text="'"+text;
            return "\""+text.Replace("\"","\"\"")+"\"";
        }
        public static void Csv(DataGridView grid,string name)
        {
            if(grid.Rows.Count==0)throw new ArgumentException("Không có dữ liệu để xuất.");
            using(var dialog=new SaveFileDialog {Filter="CSV (Excel)|*.csv",FileName=name+"_"+DateTime.Today.ToString("yyyyMMdd")+".csv",OverwritePrompt=true})
            {
                if(dialog.ShowDialog()!=DialogResult.OK)return;
                var columns=grid.Columns.Cast<DataGridViewColumn>().Where(c=>c.Visible).OrderBy(c=>c.DisplayIndex).ToArray();
                using(var writer=new StreamWriter(dialog.FileName,false,new UTF8Encoding(true)))
                {
                    writer.WriteLine("sep=;");writer.WriteLine(string.Join(";",columns.Select(c=>CsvCell(c.HeaderText))));
                    foreach(DataGridViewRow r in grid.Rows)if(!r.IsNewRow)writer.WriteLine(string.Join(";",columns.Select(c=>CsvCell(r.Cells[c.Index].Value))));
                }
                MessageBox.Show("Đã xuất "+grid.Rows.Count+" dòng vào file CSV.","Xuất dữ liệu",MessageBoxButtons.OK,MessageBoxIcon.Information);
            }
        }
        public static void PrintInvoice(bool sale,string id)
        {
            if(string.IsNullOrEmpty(id))throw new ArgumentException("Lưu hoặc chọn một hóa đơn trước khi in.");
            var header=DatabaseHelper.GetDataToTable(sale
                ? "SELECT h.SoHDB AS Id,h.NgayBan AS Ngay,nv.TenNV AS NhanVien,k.TenKhach AS DoiTac,h.TongTien FROM HoaDonBan h JOIN [NhânVien] nv ON nv.MaNV=h.MaNV JOIN KhachHang k ON k.MaKhach=h.MaKhach WHERE h.SoHDB=@id"
                : "SELECT h.SoHDN AS Id,h.NgayNhap AS Ngay,nv.TenNV AS NhanVien,k.TenNCC AS DoiTac,h.TongTien FROM HoaDonNhap h JOIN [NhânVien] nv ON nv.MaNV=h.MaNV JOIN NhaCungCap k ON k.MaNCC=h.MaNCC WHERE h.SoHDN=@id",DatabaseHelper.Param("@id",id));
            if(header.Rows.Count==0)throw new ArgumentException("Hóa đơn không tồn tại.");
            var details=Queries.Details(sale,id);var r=header.Rows[0];
            using(var printer=new InvoicePrinter(sale?"HÓA ĐƠN BÁN HÀNG":"HÓA ĐƠN NHẬP HÀNG",r,details))
            using(var preview=new PrintPreviewDialog {Document=printer.Document,Width=1150,Height=820,UseAntiAlias=true})preview.ShowDialog();
        }
    }
    public sealed class InvoicePrinter : IDisposable
    {
        readonly string title;
        readonly DataRow header;
        readonly DataTable rows;
        int index,page;
        public PrintDocument Document {get;private set;}
        public InvoicePrinter(string label,DataRow invoice,DataTable items)
        {
            title=label;header=invoice;rows=items;Document=new PrintDocument {DocumentName=invoice["Id"].ToString()};
            Document.DefaultPageSettings.Margins=new Margins(45,45,45,45);
            Document.BeginPrint+=(s,e)=>{index=0;page=0;};Document.PrintPage+=PrintPage;
        }
        void PrintPage(object sender,PrintPageEventArgs e)
        {
            page++;float x=e.MarginBounds.Left,y=e.MarginBounds.Top,width=e.MarginBounds.Width;
            using(var normal=new Font("Segoe UI",9F))using(var bold=new Font("Segoe UI",9F,FontStyle.Bold))using(var heading=new Font("Segoe UI",18F,FontStyle.Bold))
            {
                e.Graphics.DrawString(title,heading,Brushes.Black,x,y);y+=40;
                e.Graphics.DrawString("Số: "+header["Id"]+"     Ngày: "+Convert.ToDateTime(header["Ngay"]).ToString("dd/MM/yyyy HH:mm"),normal,Brushes.Black,x,y);y+=24;
                string info="Nhân viên: "+header["NhanVien"]+"\nKhách hàng / Nhà cung cấp: "+header["DoiTac"];
                float infoHeight=e.Graphics.MeasureString(info,normal,(int)width).Height+12;e.Graphics.DrawString(info,normal,Brushes.Black,new RectangleF(x,y,width,infoHeight));y+=infoHeight;
                float[] ratios={.15f,.30f,.10f,.16f,.10f,.19f};float pos=x;
                for(int c=0;c<rows.Columns.Count;c++){float w=width*ratios[c];e.Graphics.DrawString(rows.Columns[c].ColumnName,bold,Brushes.Black,new RectangleF(pos+3,y,w-6,40));pos+=w;}y+=42;
                e.Graphics.DrawLine(Pens.Gray,x,y,x+width,y);y+=6;
                while(index<rows.Rows.Count)
                {
                    var row=rows.Rows[index];string[] values=row.ItemArray.Select(v=>v is decimal?((decimal)v).ToString("N2"):v.ToString()).ToArray();
                    float h=24;
                    for(int c=0;c<values.Length;c++)h=Math.Max(h,e.Graphics.MeasureString(values[c],normal,(int)(width*ratios[c]-6)).Height+8);
                    if(y+h>e.MarginBounds.Bottom-70){e.HasMorePages=true;break;}
                    pos=x;for(int c=0;c<values.Length;c++){float w=width*ratios[c];e.Graphics.DrawString(values[c],normal,Brushes.Black,new RectangleF(pos+3,y,w-6,h));pos+=w;}
                    y+=h;e.Graphics.DrawLine(Pens.LightGray,x,y,x+width,y);index++;
                }
                if(index>=rows.Rows.Count){y+=16;e.Graphics.DrawString("TỔNG CỘNG: "+Convert.ToDecimal(header["TongTien"]).ToString("N2")+" VNĐ",bold,Brushes.Black,x,y);e.HasMorePages=false;}
                e.Graphics.DrawString("Trang "+page+" · Giảm giá tính theo % · Đơn vị tiền: VNĐ",normal,Brushes.Gray,x,e.MarginBounds.Bottom-20);
            }
        }
        public void Dispose(){Document.Dispose();}
    }
}

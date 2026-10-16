using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Drawing.Printing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using QuanLyBanHangSupermarket;

internal static class Verification
{
    static int checks;
    static string Output;
    static void Check(bool condition,string label){if(!condition)throw new Exception("FAIL: "+label);checks++;Console.WriteLine("PASS: "+label);}
    static void Reject(Action action,string label){bool failed=false;try{action();}catch(ArgumentException){failed=true;}catch(InvalidOperationException){failed=true;}catch(UnauthorizedAccessException){failed=true;}catch(System.Data.SqlClient.SqlException){failed=true;}Check(failed,label);}
    static int Count(string table){return Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM "+Catalogs.Q(table)));}
    static decimal Value(string sql){return Convert.ToDecimal(DatabaseHelper.ExecuteScalar(sql));}
    static Dictionary<string,object> Values(string table,string key,string name)
    {
        var spec=Catalogs.Get(table);var v=new Dictionary<string,object>();
        foreach(var f in spec.Fields)v[f.Name]=f.Computed?(f.Kind=="Number"?(object)0:0m):null;
        v[spec.Key]=key;v[spec.Fields.First(f=>f.Kind=="Text"&&f.Name!=spec.Key).Name]=name;return v;
    }
    static void Seed()
    {
        foreach(string table in new[]{"NhomHang","DonViTinh","ChatLieu","CongDung","NoiSanXuat","CaLam","CongViec"})Catalogs.Save(Catalogs.Get(table),Values(table,"T1","Kiểm thử O'Brien"),true);
        var type=Values("TheLoai","T1","Loại thử");type["MaNhom"]="T1";Catalogs.Save(Catalogs.Get("TheLoai"),type,true);
        Catalogs.Save(Catalogs.Get("KhachHang"),Values("KhachHang","C1","Khách một"),true);
        Catalogs.Save(Catalogs.Get("KhachHang"),Values("KhachHang","C2","Khách hai"),true);
        Catalogs.Save(Catalogs.Get("KhachHang"),Values("KhachHang","C0","Khách chưa mua"),true);
        Catalogs.Save(Catalogs.Get("NhaCungCap"),Values("NhaCungCap","S1","Nhà cung cấp"),true);
        var employee=Values("NhânVien","E1","Nhân viên thử");employee["MaCa"]="T1";employee["MaCV"]="T1";employee["MaNhom"]="T1";Catalogs.Save(Catalogs.Get("NhânVien"),employee,true);
        foreach(string code in new[]{"P1","P2","P3"})
        {
            var p=Values("DMHangHoa",code,"Sản phẩm "+code);foreach(string fk in new[]{"MaNhom","MaLoai","MaDonVi","MaChatLieu","MaCongDung","MaNoiSX"})p[fk]="T1";p["NhapKhau"]=0;Catalogs.Save(Catalogs.Get("DMHangHoa"),p,true);
        }
    }
    static DataTable Cart(string product,int quantity,decimal price,decimal discount=0)
    {var dt=Money.NewCart();dt.Rows.Add(product,"Tên sẽ được chụp lại",quantity,price,discount,Money.LineTotal(quantity,price,discount),0m);return dt;}
    [STAThread]
    static int Main(string[] args)
    {
        try
        {
            var cs=new System.Data.SqlClient.SqlConnectionStringBuilder(DatabaseHelper.ConnectionString);
            if(!cs.InitialCatalog.StartsWith("QuanLyBanHangSupermarket_Verify_",StringComparison.Ordinal))throw new Exception("Refusing to test against a non-test database.");
            Output=args.Length>0?args[0]:"artifacts";Directory.CreateDirectory(Output);
            Thread.CurrentThread.CurrentCulture=CultureInfo.GetCultureInfo("vi-VN");Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);
            Check(Money.LineTotal(3,10000m,10m)==27000m,"percentage discount");Check(Money.LineTotal(1,123.45m,12.5m)==108.02m,"two-decimal rounding");Check(Money.LineTotal(1,100m,100m)==0m,"100 percent discount");
            Reject(()=>Money.LineTotal(0,100,0),"zero quantity rejected");Reject(()=>Money.LineTotal(1,0,0),"zero price rejected");Reject(()=>Money.LineTotal(1,100,-1),"negative discount rejected");Reject(()=>Money.LineTotal(1,100,101),"over 100 percent rejected");
            Check(Money.SalePrice(123.45m)==135.80m,"sale price 110 percent");Check(Money.NewId("HDB_")!=Money.NewId("HDB_"),"invoice IDs unique");
            Check(Exports.CsvCell("=SUM(A1)").StartsWith("\"'="),"CSV formula text escaped");
            string password="Verification_"+Guid.NewGuid().ToString("N");
            using(var login=new LoginForm())
            {
                Call(login,"Check");Check(Field<bool>(login,"first"),"first-run login offers administrator setup");
                Field<TextBox>(login,"username").Text="verify_admin";Field<TextBox>(login,"password").Text=password;Call(login,"Submit");Check(login.DialogResult==DialogResult.OK,"first-run login creates administrator and authenticates");
            }
            Reject(()=>Accounts.Create("second",password,"Admin",null,true),"first account creation protected");Check(!Accounts.Authenticate("verify_admin","wrong"),"wrong password rejected");Check(Accounts.Authenticate("verify_admin",password),"correct login");
            Seed();Check(Count("DMHangHoa")==3,"all catalogs inserted");Check(Catalogs.Load(Catalogs.Get("NhomHang"),"O'Brien").Rows.Count==1,"apostrophe in parameterized search");Check(Catalogs.Load(Catalogs.Get("NhomHang"),"%' OR 1=1 --").Rows.Count==0,"search input treated literally");
            string import=Sales.Save(false,"E1","S1",new DateTime(2026,1,10),Cart("P1",10,100m,5));
            Check(Value("SELECT SoLuong FROM DMHangHoa WHERE MaVach='P1'")==10,"import increases stock");Check(Value("SELECT DonGiaBan FROM DMHangHoa WHERE MaVach='P1'")==110m,"import updates sale price");Check(Value("SELECT TongTien FROM HoaDonNhap WHERE SoHDN='"+import+"'")==950m,"import discount applied");
            int saleCount=Count("HoaDonBan"),detailCount=Count("ChiTietHDB"),logCount=Count("LichSuKho");
            Reject(()=>Sales.Save(true,"E1","C1",new DateTime(2026,1,15),Cart("P1",11,110)),"overselling rejected at commit");
            Check(Count("HoaDonBan")==saleCount&&Count("ChiTietHDB")==detailCount&&Count("LichSuKho")==logCount,"failed sale fully rolled back");
            string sale=Sales.Save(true,"E1","C1",new DateTime(2026,1,15),Cart("P1",3,110,10));
            Check(Value("SELECT SoLuong FROM DMHangHoa WHERE MaVach='P1'")==7,"sale reduces stock");Check(Value("SELECT TongTien FROM HoaDonBan WHERE SoHDB='"+sale+"'")==297m,"sale total correct");
            Sales.Save(false,"E1","S1",new DateTime(2026,1,16),Cart("P1",1,200));
            Check(Convert.ToDecimal(Queries.Details(true,sale).Rows[0]["Đơn giá"])==110m,"historical sale price preserved");
            Reject(()=>Sales.Save(true,"E1","C1",new DateTime(2026,1,17),Cart("P1",1,110)),"stale sale price rejected");
            Sales.Save(true,"E1","C2",new DateTime(2026,1,17),Cart("P1",1,220));
            Check(Queries.Invoices(false,new DateTime(2026,1,1),new DateTime(2026,1,31),"","P1","S1").Rows.Count==2,"import filters product/date/supplier");
            Check(Queries.Report(0,new DateTime(2026,1,1),2026,1,"E1",DateTime.Today,DateTime.Today).Rows.Count==1,"monthly best sellers");
            var least=Queries.Report(3,DateTime.Today,2026,1,"E1",DateTime.Today,DateTime.Today);Check(least.Rows.Count==2&&least.Rows[0]["Mã khách"].ToString()=="C2","least spenders ranked by amount, non-buyers excluded");
            Check(Queries.Report(1,DateTime.Today,2026,1,"E1",DateTime.Today,DateTime.Today).Rows.Count==2,"top imports yearly");Check(Queries.Report(2,DateTime.Today,2026,1,"E1",DateTime.Today,DateTime.Today).Rows.Count==2,"employee sales report");
            var profit=Queries.Report(4,DateTime.Today,2026,1,"E1",new DateTime(2026,1,1),new DateTime(2026,1,31));Check(profit.Rows.Cast<DataRow>().Sum(r=>Convert.ToDecimal(r["Lãi gộp"]))==17m,"gross profit uses historical cost");
            var count=DatabaseHelper.GetDataToTable("SELECT MaVach,SoLuong,SoLuong AS ThucTe FROM DMHangHoa");count.Rows[0]["ThucTe"]=9;Sales.CountStock(count,"Đếm thử");Check(Count("KiemKe")==1&&Count("ChiTietKiemKe")==3,"stock count audit recorded");
            Reject(()=>Sales.CountStock(count,"Danh sách cũ"),"stale stock count rejected");Check(Count("KiemKe")==1,"failed stock count rolled back");
            Accounts.Create("cashier",password,"ThuNgan","E1",false);Accounts.Authenticate("cashier",password);Reject(()=>Sales.Save(false,"E1","S1",new DateTime(2026,1,1),Cart("P2",1,50)),"cashier cannot import");Reject(()=>Catalogs.Save(Catalogs.Get("CaLam"),Values("CaLam","T2","Ca khác"),true),"cashier cannot edit restricted catalogs");
            Accounts.Authenticate("verify_admin",password);Reject(()=>Catalogs.Delete(Catalogs.Get("DMHangHoa"),"P1"),"product with transactions protected");
            Sales.Save(false,"E1","S1",new DateTime(2026,2,1),Cart("P3",1,100));
            var tasks=Enumerable.Range(0,2).Select(i=>Task.Run(()=>{try{Sales.Save(true,"E1","C1",new DateTime(2026,2,1),Cart("P3",1,110));return true;}catch(InvalidOperationException){return false;}})).ToArray();Task.WaitAll(tasks);
            Check(tasks.Count(t=>t.Result)==1&&Value("SELECT SoLuong FROM DMHangHoa WHERE MaVach='P3'")==0,"concurrent checkout prevents negative stock");
            string nextPassword="Changed_"+Guid.NewGuid().ToString("N");Accounts.ChangePassword(password,nextPassword);Check(!Accounts.Authenticate("verify_admin",password)&&Accounts.Authenticate("verify_admin",nextPassword),"password change invalidates old password");
            var mismatch=Values("DMHangHoa","P4","Sai nhóm");mismatch["MaNhom"]="OTHER";mismatch["MaLoai"]="T1";mismatch["MaDonVi"]="T1";mismatch["NhapKhau"]=0;Reject(()=>Catalogs.Save(Catalogs.Get("DMHangHoa"),mismatch,true),"product type must belong to selected group");
            var unicode=Catalogs.Load(Catalogs.Get("DMHangHoa"),"P1").Rows[0];
            DatabaseHelper.ExecuteNonQuery("UPDATE DMHangHoa SET Anh=@p WHERE MaVach='P1'",DatabaseHelper.Param("@p",@"C:\Ảnh sản phẩm\Sữa.png"));Check(DatabaseHelper.ExecuteScalar("SELECT Anh FROM DMHangHoa WHERE MaVach='P1'").ToString()==@"C:\Ảnh sản phẩm\Sữa.png","Unicode image paths preserved");
            DatabaseHelper.ExecuteNonQuery("UPDATE DMHangHoa SET Anh=NULL WHERE MaVach='P1'");
            VerifyInvoiceUi();VerifyPrinting(sale);if(!args.Contains("--skip-render"))Render();Console.WriteLine("COMPLETE: "+checks+" checks; test data remains only in the isolated database.");return 0;
        }
        catch(Exception e){Console.Error.WriteLine(e);return 1;}
    }
    static T Field<T>(object target,string name){return (T)target.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(target);}
    static void Call(object target,string method){target.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(target,null);}
    static void VerifyInvoiceUi()
    {
        using(var form=new InvoiceForm(true))
        {
            form.ShowInTaskbar=false;form.Opacity=0;form.Location=new Point(-5000,-5000);form.Show();Application.DoEvents();
            Field<ComboBox>(form,"employee").SelectedValue="E1";Field<ComboBox>(form,"partner").SelectedValue="C1";Field<ComboBox>(form,"product").SelectedValue="P1";
            Field<NumericUpDown>(form,"qty").Value=1;Field<NumericUpDown>(form,"discount").Value=10;Call(form,"AddLine");
            Check(Field<DataTable>(form,"cart").Rows.Count==1&&Money.Total(Field<DataTable>(form,"cart"))==198m,"checkout UI adds line and calculates total");
            Call(form,"Save");Check(!Field<Button>(form,"save").Enabled&&Field<Button>(form,"print").Enabled,"saved checkout locks saving and enables printing");
            Call(form,"Reset");Check(Field<DataTable>(form,"cart").Rows.Count==0&&Field<Button>(form,"save").Enabled,"checkout new invoice resets state");form.Close();
        }
    }
    static void VerifyPrinting(string sale)
    {
        var header=DatabaseHelper.GetDataToTable("SELECT SoHDB AS Id,NgayBan AS Ngay,N'Tên nhân viên' AS NhanVien,N'Tên khách hàng' AS DoiTac,TongTien FROM HoaDonBan WHERE SoHDB=@id",DatabaseHelper.Param("@id",sale));
        var details=Queries.Details(true,sale);
        using(var printer=new InvoicePrinter("HÓA ĐƠN BÁN HÀNG",header.Rows[0],details))
        using(var bitmap=new Bitmap(850,1100))using(var g=Graphics.FromImage(bitmap))
        {
            g.Clear(Color.White);var e=new PrintPageEventArgs(g,new Rectangle(45,45,760,1010),new Rectangle(0,0,850,1100),printer.Document.DefaultPageSettings);
            typeof(InvoicePrinter).GetMethod("PrintPage",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(printer,new object[]{null,e});bitmap.Save(Path.Combine(Output,"invoice-print.png"));Check(!e.HasMorePages,"single-page invoice printing");
        }
        var many=details.Clone();for(int i=0;i<200;i++)many.Rows.Add(details.Rows[0].ItemArray);
        using(var printer=new InvoicePrinter("HÓA ĐƠN NHIỀU DÒNG",header.Rows[0],many))
        using(var bitmap=new Bitmap(850,1100))using(var g=Graphics.FromImage(bitmap))
        {
            int pages=0;bool more;
            do {g.Clear(Color.White);var e=new PrintPageEventArgs(g,new Rectangle(45,45,760,1010),new Rectangle(0,0,850,1100),printer.Document.DefaultPageSettings);typeof(InvoicePrinter).GetMethod("PrintPage",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(printer,new object[]{null,e});more=e.HasMorePages;pages++;if(pages>30)throw new Exception("Printer failed to advance pages.");}while(more);
            Check(pages>1&&pages<30,"multi-page invoice printing advances and terminates");
        }
    }
    static void Capture(Form form,string name,int width,int height)
    {
        using(form)
        {
            form.ShowInTaskbar=false;form.Opacity=0;form.WindowState=FormWindowState.Normal;form.StartPosition=FormStartPosition.Manual;form.Location=new Point(-5000,-5000);form.ClientSize=new Size(width,height);form.Show();Application.DoEvents();
            using(var image=new Bitmap(form.Width,form.Height)){form.DrawToBitmap(image,new Rectangle(0,0,form.Width,form.Height));image.Save(Path.Combine(Output,name+".png"));}
            form.Close();
        }
    }
    static void Render()
    {
        Capture(new MainForm(),"main-1440",1440,940);Capture(new MainForm(),"main-1220",1220,800);
        Capture(new CatalogForm(Catalogs.Get("DMHangHoa")),"products",1050,750);Capture(new InvoiceForm(true),"checkout",1050,750);Capture(new InvoiceForm(false),"purchase",1050,750);
        Capture(new ReportForm(),"reports",1050,750);Capture(new StockForm(),"stock",1050,750);Capture(new LoginForm(),"login",540,510);
        Check(Directory.GetFiles(Output,"*.png").Length>=8,"native UI rendered at multiple window sizes");
    }
}

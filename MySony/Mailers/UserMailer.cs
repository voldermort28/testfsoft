using Mvc.Mailer;
using MyProject.Mailers;
using MyProject.Models;
using MyProject.ViewModels;

namespace MyProject.Mailers
{ 
    public class UserMailer : MailerBase, IUserMailer 	
	{
		public UserMailer()
		{
			MasterName="_Layout";
		}
		
		public virtual MvcMailMessage WelcomePurchase()
		{
			//ViewBag.Data = someObject;
			return Populate(x =>
			{
                x.Subject = "My Sony - Xác nhận đăng ký";
				x.ViewName = "WelcomePurchase";
                x.To.Add((string)CurrentHttpContext.Session["email"]);
			});
		}

        public virtual MvcMailMessage TouchTryEmail()
        {
            //ViewBag.Data = someObject;
            return Populate(x =>
            {
                x.Subject = "MyProject Style Up - Xác nhận đăng ký tham gia chương trình “Touch & Try”";
                x.ViewName = "TouchTryEmail";
                x.To.Add((string)CurrentHttpContext.Session["emailTouchTry"]);
            });
        }
 
		public virtual MvcMailMessage PasswordReset(string email, string link)
		{
            ViewBag.Email = email;
            ViewBag.Link = link;
			return Populate(x =>
			{
				x.Subject = "Tạo mật khẩu mới";
				x.ViewName = "PasswordReset";
                x.To.Add(email);
			});
		}

        public virtual MvcMailMessage ChangePassOk(string email)
        {
            return Populate(x =>
            {
                x.Subject = "Mật khẩu đã được thay đổi";
                x.ViewName = "ChangePassOk";
                x.To.Add(email);
            });
        }

        public virtual MvcMailMessage ResetPassByAdmin(string email, string customerName, string newPass)
        {
            ViewBag.CustomerName = customerName;
            ViewBag.NewPass = newPass;
            return Populate(x =>
            {
                x.Subject = "Mật khẩu đã được thay đổi";
                x.ViewName = "ResetPassByAdmin";
                x.To.Add(email);
            });
        }

        public MvcMailMessage CustomerService(customer cus, CustomerProductVM product)
        {
            ViewBag.Email = cus.email;
            ViewBag.Product = product.ProductName;
            ViewBag.Serial = product.Serial;
            ViewBag.Datebuy = product.BuyDate;
            return Populate(x =>
            {
                x.Subject = "Customer Product ";
                x.ViewName = "CustomerService";
                x.To.Add(cus.email);
            });
        }
 
	}
}
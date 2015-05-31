using Mvc.Mailer;
using MySony.Models;
using MySony.ViewModels;

namespace MySony.Mailers
{ 
    public interface IUserMailer
    {
        MvcMailMessage WelcomePurchase();
        MvcMailMessage TouchTryEmail();
        MvcMailMessage ChangePassOk(string email);
		MvcMailMessage PasswordReset(string email, string link);
        MvcMailMessage ResetPassByAdmin(string email, string customerName, string newPass);

        MvcMailMessage CustomerService(customer cus, CustomerProductVM product);
	}
}
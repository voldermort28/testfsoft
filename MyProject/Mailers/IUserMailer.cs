using Mvc.Mailer;
using MyProject.Models;
using MyProject.ViewModels;

namespace MyProject.Mailers
{ 
    public interface IUserMailer
    {
        MvcMailMessage WelcomePurchase();
        MvcMailMessage TouchTryEmail();
        MvcMailMessage ChangePassOk(string email);
		MvcMailMessage PasswordReset(string email, string link);
        MvcMailMessage ResetPassByAdmin(string email, string customerName, string newPass);

      
	}
}
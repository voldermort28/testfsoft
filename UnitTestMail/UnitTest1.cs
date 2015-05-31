using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySony.Mailers;

namespace UnitTestMail
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            // send mail
            IUserMailer mailer = new UserMailer();
            mailer.CustomerService("chibao2704@gmail.com", "Thanh cong").Send();
        }
    }
}

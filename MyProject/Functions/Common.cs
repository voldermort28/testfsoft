using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using DocumentFormat.OpenXml.Bibliography;
using MyProject.Models;
using System.Web;
using System.Data;
using System.Collections.Generic;
using System.Net.Mail;
using System.Net;
using Newtonsoft.Json;
using System.Web.Script.Serialization;
using System.Configuration;
using System.Web.SessionState;

namespace MyProject.Functions
{
     public class ReCaptcharVerify
    {
        public String Success { get; set; }
        public String ErrorCode {get;set;}
    }

     public class MySessionIDManager : SessionIDManager, ISessionIDManager
     {
         public override string CreateSessionID(HttpContext context)
         {
             return System.Guid.NewGuid().ToString("X");
         }

         public override bool Validate(string id)
         {
             try
             {
                 Guid testGuid = new Guid(id);

                 if (id == testGuid.ToString("X"))
                     return true;
             }
             catch
             {
             }

             return false;
         }
     }
    public class Common
    {
        // default keyword
        public const String passPhrase = "myadmin@2014";
        // This constant string is used as a "salt" value for the PasswordDeriveBytes function calls.
        // This size of the IV (in bytes) must = (keysize / 8).  Default keysize is 256, so the IV must be
        // 32 bytes long.  Using a 16 character string here gives us 32 bytes when converted to a byte array.
        private static readonly byte[] initVectorBytes = Encoding.ASCII.GetBytes("tu89geji340t89u2");

        // This constant is used to determine the keysize of the encryption algorithm.
        private const int keysize = 256;

        /// <summary>
        /// Encrypt string
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
        public static string Encrypt(string plainText)
        {
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            using (PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null))
            {
                byte[] keyBytes = password.GetBytes(keysize / 8);
                using (RijndaelManaged symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.Mode = CipherMode.CBC;
                    using (ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes))
                    {
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                            {
                                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                                cryptoStream.FlushFinalBlock();
                                byte[] cipherTextBytes = memoryStream.ToArray();
                                return Convert.ToBase64String(cipherTextBytes);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Decrypt string
        /// </summary>
        /// <param name="cipherText"></param>
        /// <returns></returns>
        public static string Decrypt(string cipherText)
        {
            byte[] cipherTextBytes = Convert.FromBase64String(cipherText);
            using (PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null))
            {
                byte[] keyBytes = password.GetBytes(keysize / 8);
                using (RijndaelManaged symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.Mode = CipherMode.CBC;
                    using (ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes))
                    {
                        using (MemoryStream memoryStream = new MemoryStream(cipherTextBytes))
                        {
                            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                            {
                                byte[] plainTextBytes = new byte[cipherTextBytes.Length];
                                int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                                return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                            }
                        }
                    }
                }
            }
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
        
        
        public static string MaHoa(string Pass)
        {
            string results = "";
            byte[] Array = System.Text.Encoding.UTF8.GetBytes(Pass);
            MD5CryptoServiceProvider MHMK = new MD5CryptoServiceProvider();
            Array = MHMK.ComputeHash(Array);
            foreach (byte a in Array)
            {
                results += a.ToString("X2");
            }
            return results;
        }

        public static Boolean IsLogedIn()
        {
            var b = true;
            var userid = HttpContext.Current.Session["UserID"];
            if (userid != null)
            {
                b = true;
            }
            else
            {
                b = false;
            }
            return b;
        }

        public static void SetUserLoggout(String userID)
        {
            var cookies = HttpContext.Current.Request.Cookies.AllKeys;
            foreach (String item in cookies)
            {
                HttpCookie myCookie = new HttpCookie(item);
                myCookie.Expires = DateTime.Now.AddDays(-1d);
                HttpContext.Current.Response.Cookies.Add(myCookie);
            }
           HttpContext.Current.Cache.Remove(userID);
            HttpContext.Current.Session.RemoveAll();
            HttpContext.Current.Session.Abandon();
            HttpContext.Current.Session.Clear();
            HttpContext.Current.Request.Cookies.Clear();
            HttpResponse.RemoveOutputCacheItem(HttpContext.Current.Request.CurrentExecutionFilePath);
            HttpContext.Current.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            HttpContext.Current.Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            HttpContext.Current.Response.Cache.SetNoStore();
            
        }

        public static String GetCurrentUserName()
        {
            var userid = Convert.ToInt32(HttpContext.Current.Session["UserID"]); 
            using (MyProject.Models.MyDatabaseEntities db  = new MyDatabaseEntities())
            {
                return "Nguoi dung";
            }
        }
        public static String GetUserName()
        {
            var userid = Convert.ToInt32(HttpContext.Current.Session["UserID"]);
            using (var db = new MyDatabaseEntities())
            {
                
                return "";
            }
        }
        public static void SetUserLogin(int userid)
        {
            HttpContext.Current.Session["UserID"] = userid;
        }
        /// <summary>
        /// Write log
        /// </summary>
        /// <param name="content">Log content</param>
        public static void WriteLog(string content)
        {
            using (MyDatabaseEntities db = new MyDatabaseEntities())
            {
                db.LogErrors.Add(new LogError()
                {
                    Content = content,
                    Created = DateTime.Now
                });
                db.SaveChanges();
            }
        }
        
        /// <summary>
        /// Check strong password
        /// </summary>
        /// <param name="pass">Password</param>
        /// <returns>True or False</returns>
        public static bool ValidatePassAdmin(String pass)
        {
            bool b = false;
            try
            {
                if (pass.Length < 8)
                {
                    return false;
                }
                //Regex reg = new Regex(@"^[+-]?\d+(\.\d+)?([eE][+-]?\d+)?$");
                Regex reg = new Regex(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*[0-9]).{8,}$");
                if (reg.IsMatch(pass))
                {
                    b = true;
                }
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message + "\n" + ex.StackTrace);
            }
            return b;
        }
        /// <summary>
        /// Check string input
        /// </summary>
        /// <param name="input">input</param>
        /// <returns>True  hava special
        /// or False</returns>
        public static bool ValidateString(String input)
        {
            bool b = false;
            var specialChars = new List<char> { '<', '>' };
            try
            {
            //    var withoutSpecial = new string(input.Where(c => Char.IsLetterOrDigit(c)
            //                                    || Char.IsWhiteSpace(c)).ToArray());

            //    b = input != withoutSpecial;
                var withoutSpecial = new string(input.Where(c => !specialChars.Contains(c)).ToArray());

                b = input != withoutSpecial;

            }
            catch (Exception ex)
            {
                WriteLog(ex.Message + "\n" + ex.StackTrace);
            }
            return b;
        }

        public static bool ValidatePassUser(String pass)
        {
            bool b = false;
            try
            {
                if (pass.Length < 6)
                {
                    return false;
                }
                //Regex reg = new Regex(@"^[+-]?\d+(\.\d+)?([eE][+-]?\d+)?$");
                Regex reg = new Regex(@"^(?=.*[a-z])(?=.*\d)");
                if (reg.IsMatch(pass))
                {
                    b = true;
                }
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message + "\n" + ex.StackTrace);
            }
            return b;
        }

        public static bool ValidateInput(String input)
        {
            var check = Regex.IsMatch(input, "[~!@#$%^&*()<>]");
            if (check)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static bool ValidateEmail(String email)
        {
            bool b = false;
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                b = addr.Address == email;
            }
            catch
            {
                b = false;
            }
            return b;
        }
        
        public static bool ValidateTel(String tel)
        {
            bool b = false;
            if (tel == null)
            {
                return b;
            }
            Regex reg = new Regex(@"[A-Z]");
            if (reg.IsMatch(tel.ToUpper()))
            {
                b = false;
            }
            else
            {
                b = true;
            }
            return b;
        }

        public static bool CheckCity(MyDatabaseEntities db, int cityId)
        {
            bool b = false;
            try
            {
                
            }
            catch (Exception)
            {
                b = false;
            }
            return b;
        }
  
        public static string Cheknull( string value)
        {
            string b = string.Empty;
            try
            {
                if (String.IsNullOrEmpty(value))
                {
                    return b;
                }
            }
            catch (Exception)
            {
                b = string.Empty;
            }
            return value;
        }

        public static bool ChekSuperAdmin()
        {
            bool status;
            try
            {
                using (var db = new MyDatabaseEntities())
                {
                    var adminId = (int)HttpContext.Current.Session["admss"];
                    var adminEmail = (string)HttpContext.Current.Session["admssemail"];
                    var obj = db.admins.FirstOrDefault(x => x.admin_id == adminId && x.email.Equals(adminEmail) && x.status_id == Constant.StatusActive);
                    if (obj != null && obj.admin_type == Constant.SuperAdmin)
                    {
                        status = true;
                    }
                    else
                    {
                        status = false;
                    }
                }                 
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message + "\n" + ex.StackTrace);
                status = false;
            }
            // hien tai deu vao duoc
            status = true;
            return status;
        }
        public static int GetAdminType()
        {
            
            if (HttpContext.Current.Session["admsstype"] != null)
            {
                return (int)HttpContext.Current.Session["admsstype"];
            }
            return 0;
        }

        public static String GenRandomString(int strLength)
        {
            var chars = "ABCDEFGHIJKLMNPQRSTUVWXYZ";
            var chars2 = chars.ToLower();
            var numbers = "0123456789";
            var specials = "!@#$%^&*.";
            var random = new Random();
            int strLength2 = strLength / 4;
            var result1 = new string(
                Enumerable.Repeat(chars, strLength2)
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());
            var result2 = new string(
                Enumerable.Repeat(numbers, strLength2)
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());
            var result3 = new string(
                Enumerable.Repeat(specials, strLength2)
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());
            var result4 = new string(
                Enumerable.Repeat(chars2, strLength - 3 * strLength2)
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());
            var result5 = result1 + result2 + result3 + result4;

            return result5;
        }

        
        public static Boolean VerifyCaptcha(String responseCaptchar, String ip)
        { 
            String secret = ConfigurationManager.AppSettings["recaptcha_secret"];//"6LdncgATAAAAAGQ7JX2P-rb1D0ZNe438qy6g1XB-";
            String url = String.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}&remoteip={2}",secret,responseCaptchar,ip);
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "GET";
            String test = String.Empty;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                test = reader.ReadToEnd();
                reader.Close();
                dataStream.Close();
            }

            JavaScriptSerializer j = new JavaScriptSerializer();
            ReCaptcharVerify res = j.Deserialize<ReCaptcharVerify>(test);
            return Convert.ToBoolean(res.Success);
        }

        public static Boolean NeedVerifyHuman()
        {
            Boolean b = false;
            String userIP = HttpContext.Current.Request.UserHostAddress;
            if (HttpContext.Current.Cache[userIP] != null)
            {
                b = true;
            }
            return b;
        }
    }
}
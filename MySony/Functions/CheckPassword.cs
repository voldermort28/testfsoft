using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MySony.Models;
using MySony.ViewModels;

namespace MySony.Functions
{
    public class CheckPassword
    {
        public static bool CheckOldPass(MySonyEntities db ,int customerId ,string newPass)
        {
            var customLst = ListPasswordChanges(db, customerId);

            if (customLst.Count > 0)
            {
                if (customLst.Any(obj => obj.password.Equals(newPass)))
                {                    
                    return false;
                }
            }
            return true;
        }

        public static List<HisPassword> ListPasswordChanges(MySonyEntities db, int customerId)
        {
            List<PassChange> obj;
            List<HisPassword> obj1;
            try
            {                 
                 obj1 = db.PassChanges.Where(x => x.customer_id == customerId)
                    .Select(x => new HisPassword
                    {
                        customer_id = x.customer_id,
                        password = x.password,
                        datechange = x.datechange
                    }).OrderByDescending(x => x.datechange).ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
            return obj1;
        }

        public static bool CheckValidateHisPassword(string currentPassword , string newPassword)
        {
            return CompareAllLetters(currentPassword, newPassword);
        }

        public static bool CompareAllLetters(string stringToCheck1, string stringToCheck2)
        {
            if (Math.Abs(stringToCheck2.Length - stringToCheck1.Length) > 2)
            {
                return false;
            }

            var string1LettersList = stringToCheck1.ToList();

            var string2LettersList = stringToCheck2.ToList();

            foreach (char charter in string1LettersList)
            {
                if (string2LettersList.Contains(charter))
                {
                    string2LettersList.Remove(charter);
                }
            }
            return string2LettersList.Count <= 2;
        }

    }
}
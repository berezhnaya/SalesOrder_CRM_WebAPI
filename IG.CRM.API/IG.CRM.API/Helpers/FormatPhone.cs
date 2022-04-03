using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace IG.CRM.API.Helpers
{
    public class FormatPhone
    {
        public static string FormatingPhone(string phone)
        { 
            var phonenumber = Convert.ToDouble(phone);

            if (phone.Length == 9)
                return string.Format("{0:38 (0##) ###-##-##}", phonenumber);

            if (phone.Length == 10)
                return string.Format("{0:38 (###) ###-##-##}", phonenumber);

            if (phone.Length == 11)
                return string.Format("{0:3# (###) ###-##-##}", phonenumber);

            if (phone.Length == 12)
                return string.Format("{0:## (###) ###-##-##}", phonenumber);

            if (phone.Length == 13)
            {
                return string.Format("{0:+## (###) ###-##-##}", phonenumber);
            }
            return null;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MBProtoLib.Utils
{
    public class Validation
    {
        public static bool IsMobile(string value)
        {
            bool c1 = value.Length == 11;
            bool c2 = value.StartsWith("09");
            bool c3 = value.Any(c => c == '1' || c == '2' || c == '3' || c == '4' || c == '5' || c == '6' || c == '7' || c == '8' || c == '9' || c == '0');
            return c1 &&
                c2 &&
                c3;
        }
        public static bool IsEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public static string NormalizeMobile(string value, bool toInternational = false)
        {
            try
            {
                if (!toInternational)
                {
                    if (value.StartsWith("98"))
                    {
                        return "0" + value.Substring(2);
                    }
                    return value;
                }
                else
                {
                    if (value.StartsWith("0"))
                    {
                        return "98" + value.Substring(1);
                    }
                    return value;
                }
            }
            catch
            {
                throw new Exceptions.AuthException(new Exceptions.AuthException.InternalServerError());
            }
        }

    }
}
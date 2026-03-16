using System;
using System.Security.Cryptography;
using System.Text;
using System.Web.Security;

namespace First_Aid_Made_Easy.BLL
{
    public static class SecurityHelper
    {
        /// <summary>
        /// Protects the specified string using MachineKey.
        /// Useful for sensitive data stored in cookies or sent locally.
        /// </summary>
        public static string Protect(string plainText, string purpose = "GeneralPurpose")
        {
            if (string.IsNullOrEmpty(plainText)) return null;

            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] protectedBytes = MachineKey.Protect(plainBytes, purpose);
            return HttpServerUtility.UrlTokenEncode(protectedBytes);
        }

        /// <summary>
        /// Unprotects the specified protected string.
        /// </summary>
        public static string Unprotect(string protectedText, string purpose = "GeneralPurpose")
        {
            if (string.IsNullOrEmpty(protectedText)) return null;

            try
            {
                byte[] protectedBytes = HttpServerUtility.UrlTokenDecode(protectedText);
                byte[] plainBytes = MachineKey.Unprotect(protectedBytes, purpose);
                return Encoding.UTF8.GetString(plainBytes);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Robust alternative to the simple Base64 encoding used in Common.cs.
        /// Note: This is still symmetric but harder to guess than double-Base64.
        /// </summary>
        public static string RobustEncode(string str)
        {
            if (string.IsNullOrEmpty(str)) return null;
            return Protect(str, "RobustEncoding");
        }

        public static string RobustDecode(string str)
        {
            if (string.IsNullOrEmpty(str)) return null;
            return Unprotect(str, "RobustEncoding");
        }
    }
}

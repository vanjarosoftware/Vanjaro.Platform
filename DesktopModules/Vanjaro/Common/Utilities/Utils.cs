using DotNetNuke.Abstractions;
using DotNetNuke.Services.FileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vanjaro.Common.Utilities
{
    public class Utils
    {

        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }
        public static string LimitWords(string Input, int MaxWords)
        {
            // split input string into words (max 21...last words go in last element)
            string[] Words = Input.Split(new char[] { ' ' }, MaxWords);

            // if we reach maximum words, replace last words with elipse
            if (Words.Length == MaxWords)
            {
                Words[MaxWords - 1] = "...";
            }
            else
            {
                return Input;  // nothing to do
            }

            // build new output string
            string Output = string.Join(" ", Words);
            return Output;
        }

        //static bool invalid = false;
        //update for tld https://msdn.microsoft.com/en-us/library/01escwtf(v=vs.90).aspx
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrEmpty(email) || email.Contains(' ') || !(email.Contains("@") && email.Contains(".")))
            {
                return false;
            }

            return true;
            //invalid = false;
            //if (String.IsNullOrEmpty(strIn))
            //    return false;

            //// Use IdnMapping class to convert Unicode domain names.
            //strIn = Regex.Replace(strIn, @"(@)(.+)$", Utils.DomainMapper);
            //if (invalid)
            //    return false;

            //// Return true if strIn is in valid e-mail format. 
            //return Regex.IsMatch(strIn,
            //       @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
            //       @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,17}))$",
            //       RegexOptions.IgnoreCase);
        }
        //update for tld https://msdn.microsoft.com/en-us/library/01escwtf(v=vs.90).aspx
        //private static string DomainMapper(Match match)
        //{
        //    // IdnMapping class with default property values.
        //    IdnMapping idn = new IdnMapping();

        //    string domainName = match.Groups[2].Value;
        //    try
        //    {
        //        domainName = idn.GetAscii(domainName);
        //    }
        //    catch (ArgumentException)
        //    {
        //        invalid = true;
        //    }
        //    return match.Groups[1].Value + domainName;
        //}

        public static string SanitizeEmail(string Email)
        {
            if (Email != null)
            {
                return Email.Trim().ToLower();
            }

            return null;
        }

        public static string BrowseUrl(int ModuleId)
        {
            return BrowseUrl(ModuleId, "");
        }

        public static string BrowseUrl(int ModuleId, string Key = "")
        {
            return BrowseUrl(ModuleId, true, Key, null);
        }

        public static string BrowseUrl(int ModuleId, bool InculdePopup, string Key = "")
        {
            return BrowseUrl(ModuleId, InculdePopup, Key, null);
        }

        public static string BrowseUrl(int ModuleId, bool InculdePopup, string Key = "", Dictionary<string, string> additionalParameters = null)
        {

            if (additionalParameters == null)
            {
                additionalParameters = new Dictionary<string, string>();
            }

            additionalParameters.Add("mid=", ModuleId.ToString());
            string[] additionalParametersArray = new string[additionalParameters.Count];
            int ctr = 0;
            foreach (KeyValuePair<string, string> item in additionalParameters)
            {
                additionalParametersArray[ctr] = item.Key + item.Value;
                ctr++;
            }
            string result = ServiceProvider.NavigationManager.NavigateURL(Key, additionalParametersArray);
            if (result.IndexOf('?') > -1)
            {
                result += '&';
            }
            else
            {
                result += '?';
            }

            if (InculdePopup)
            {
                if (DotNetNuke.Entities.Portals.PortalSettings.Current.EnablePopUps)
                {
                    result += "popUp=true";
                }
                else
                {
                    result += "popUp=true&hidecommandbar=true&SkinSrc=[g]skins/vanjaro/base";
                }
            }
            result = result.TrimEnd('?', '&');

            return result;
        }

        public static string ToBase36(int Value, int TotalWidth = 0, char PaddingChar = 'a')
        {
            // 32 is the worst cast buffer size for base 2 and int.MaxValue
            int i = 32;
            char[] buffer = new char[i];
            char[] baseChars = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
                         'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z'};

            int targetBase = baseChars.Length;

            do
            {
                buffer[--i] = baseChars[Value % targetBase];
                Value /= targetBase;
            }
            while (Value > 0);

            char[] result = new char[32 - i];
            Array.Copy(buffer, i, result, 0, 32 - i);

            return TotalWidth > 0 ? new string(result).PadLeft(TotalWidth, PaddingChar) : new string(result);
        }


        /// <summary>
        /// Gets a flag that dertermines if the file is an image of type jpg,jpeg,jpe,png.
        /// </summary>
        /// <param name="file">The file to test.</param>
        /// <returns>The flag as a boolean value.</returns>
        public static bool IsImageVersionable(IFileInfo file)
        {
            return ("jpg,jpeg,jpe,png" + ",").IndexOf(file.Extension.ToLowerInvariant().Replace(".", string.Empty) + ",") > -1;
        }
    }
}

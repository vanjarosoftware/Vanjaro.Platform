using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Vanjaro.Common.Utilities
{
    public class Security
    {
        public enum FilterFlag { NoScripting = 0 }
        public const RegexOptions RxOptions = RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled;
        private static readonly Regex[] RxListStrings = new[]
        {
            new Regex("<script[^>]*>.*?</script[^><]*>", RxOptions),
            new Regex("<script", RxOptions),
            new Regex("<input[^>]*>.*?</input[^><]*>", RxOptions),
            new Regex("<input[^>]*>", RxOptions),
            new Regex("<object[^>]*>.*?</object[^><]*>", RxOptions),
            new Regex("<embed[^>]*>.*?</embed[^><]*>", RxOptions),
            new Regex("<embed[^>]*>", RxOptions),
            new Regex("<applet[^>]*>.*?</applet[^><]*>", RxOptions),
            new Regex("<form[^>]*>.*?</form[^><]*>", RxOptions),
            new Regex("<option[^>]*>.*?</option[^><]*>", RxOptions),
            new Regex("<select[^>]*>.*?</select[^><]*>", RxOptions),
            new Regex("<iframe[^>]*>.*?</iframe[^><]*>", RxOptions),
            new Regex("<iframe.*?<", RxOptions),
            new Regex("<iframe.*?", RxOptions),
            new Regex("<ilayer[^>]*>.*?</ilayer[^><]*>", RxOptions),
            new Regex("<form[^>]*>", RxOptions),
            new Regex("</form[^><]*>", RxOptions),
            new Regex("onerror", RxOptions),
            new Regex("onmouseover", RxOptions),
            new Regex("javascript:", RxOptions),
            new Regex("vbscript:", RxOptions),
            new Regex("unescape", RxOptions),
            new Regex("alert[\\s(&nbsp;)]*\\([\\s(&nbsp;)]*'?[\\s(&nbsp;)]*[\"(&quot;)]?", RxOptions),
            new Regex(@"eval*.\(", RxOptions),
            new Regex("onload", RxOptions),
            new Regex("<base[^>]*>", Security.RxOptions),
            new Regex("</base[^><]*>", Security.RxOptions),
        };
        public static string InputFilter(string strInput, FilterFlag Filter, Regex[] NewRxListStrings)
        {
            if (strInput == null)
            {
                return null;
            }

            string tempInput = strInput;
            const string replacement = " ";
            if (Filter == FilterFlag.NoScripting)
            {
                if (NewRxListStrings != null && NewRxListStrings.Count() > 0)
                {
                    tempInput = NewRxListStrings.Aggregate(tempInput, (current, s) => s.Replace(current, replacement));
                }
                else
                {
                    tempInput = RxListStrings.Aggregate(tempInput, (current, s) => s.Replace(current, replacement));
                }
            }

            return tempInput;
        }
        public static string InputFilter(string strInput, FilterFlag Filter)
        {
            return InputFilter(strInput, Filter, null);
        }
        public static bool IsAllowedExtension(string extension, IEnumerable<string> Extensions)
        {
            if (string.IsNullOrEmpty(extension))
            {
                return false;
            }

            if (!Extensions.Any())
            {
                return true;
            }

            extension = extension.TrimStart('.').ToLowerInvariant();

            return Extensions.Contains(extension);
        }
        public static string ReplaceIllegalCharacters(string fileName)
        {
            string[] illegalCharacters = new string[] { "#" };
            foreach (string character in illegalCharacters)
            {
                fileName = fileName.Replace(character, "");
            }

            return fileName;
        }
    }
}
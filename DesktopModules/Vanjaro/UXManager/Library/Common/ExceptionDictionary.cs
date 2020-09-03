using System;
using System.Collections.Generic;
using System.Text;

namespace Vanjaro.UXManager.Library.Common
{
    public class ExceptionDictionary : Dictionary<string, Exception>
    {
        public override string ToString()
        {
            return ToString("\r\n", VerboseLevels.User);
        }
        public string ToString(VerboseLevels VerboseLevel)
        {
            return ToString("\r\n", VerboseLevel);
        }
        public string ToString(string Newline, VerboseLevels VerboseLevel)
        {
            StringBuilder sb = new StringBuilder();

            foreach (string s in Keys)
            {
                if (VerboseLevel == VerboseLevels.Admin && this[s] != null)
                {
                    sb.Append(s + Newline + Newline + this[s].Message + Newline);
                }
                else
                {
                    sb.Append(s + Newline);
                }
            }

            return sb.ToString();
        }
    }
}
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Text;

namespace Vanjaro.Common.Utilities
{
    public class JsonToHtml
    {
        public static string json2html(dynamic json)
        {
            string tag = json.tag;
            StringBuilder buf = new StringBuilder();
            // Empty Elements - HTML 4.01
            string[] empty = new string[] { "area", "base", "basefont", "br", "col", "frame", "hr", "img", "input", "isindex", "link", "meta", "param", "embed" };

            buf.Append("<" + json.tag);
            if (json.attr != null)
            {
                foreach (dynamic attr in json.attr)
                {
                    if (attr.Value.Type.ToString() == "Array")
                    {
                        StringBuilder sb = new StringBuilder();
                        JArray uiattr = new JArray();
                        uiattr = attr.Value;
                        foreach (string val in uiattr)
                        {
                            sb.Append(val + " ");
                        }
                        buf.Append(" " + attr.Name + "=\"" + sb.ToString() + "\"");
                    }
                    else if (attr.Value.Type.ToString() == "String")
                    {
                        buf.Append(" " + attr.Name + "=\"" + attr.Value.ToString() + "\"");
                    }
                }
            }
            if (empty.Contains(tag))
            {
                buf.Append("/");
            }

            buf.Append(">");
            if (json.text != null)
            {
                buf.Append(json.text);
            }

            if (json.child != null)
            {
                foreach (dynamic child in json.child)
                {
                    buf.Append(json2html(child));
                }
            }
            if (!empty.Contains(tag))
            {
                buf.Append("</" + tag + ">");
            }

            return buf.ToString();
        }



    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Vanjaro.Common.Utilities;

namespace Vanjaro.Common.Engines.TokenEngine
{
    public class DNNLocalizationEngine : ITokenEngine
    {
        private readonly string LocalResourceFile;
        private readonly string SharedResourceFile;
        private readonly bool ShowMissingKeys;

        /// <summary>
        /// Initializes the DNNTokenEngine
        /// </summary>
        /// <param name="DNNContext"></param>
        public DNNLocalizationEngine(string LocalResourceFile, string SharedResourceFile, bool ShowMissingKeys)
        {
            this.LocalResourceFile = LocalResourceFile;
            this.SharedResourceFile = SharedResourceFile;
            this.ShowMissingKeys = ShowMissingKeys;
        }

        public string Parse(string Template)
        {
            return Parse(Template, Thread.CurrentThread.CurrentUICulture.ToString());
        }
        /// <summary>
        /// Parses the given template for DNN Standard Tokens
        /// </summary>
        /// <param name="Template"></param>
        /// <returns></returns>
        public string Parse(string Template, string language)
        {
            if (!string.IsNullOrEmpty(Template))
            {
                List<string> matches =
                            Regex.Matches(Template.Replace(Environment.NewLine, ""), @"\[([^]]*)\]")
                                .Cast<Match>()
                                .Select(x => x.Groups[1].Value)
                                .ToList();
                foreach (string match in matches)
                {
                    string token = "[" + match + "]";

                    if (match.StartsWith("LS:") && !string.IsNullOrEmpty(SharedResourceFile))
                    {
                        string key = match.Replace("LS:", string.Empty);//.Split(',')[0];
                        string localizedValue = Localization.Get(key, "Text", SharedResourceFile, ShowMissingKeys, Localization.SharedMissingPrefix, language);
                        Template = Template.Replace(token, localizedValue);
                    }
                    else if (match.StartsWith("L:") && !string.IsNullOrEmpty(LocalResourceFile))
                    {
                        string key = match.Replace("L:", string.Empty);//.Split(',')[0];
                        string localizedValue = Localization.Get(key, "Text", LocalResourceFile, ShowMissingKeys, Localization.LocalMissingPrefix, language);
                        Template = Template.Replace(token, localizedValue);
                    }

                }
            }
            return Template;
        }
    }
}

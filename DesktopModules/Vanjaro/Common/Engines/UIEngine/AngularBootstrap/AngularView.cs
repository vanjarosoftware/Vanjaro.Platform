using System.Collections.Generic;

namespace Vanjaro.Common.Engines.UIEngine.AngularBootstrap
{
    public enum URLPathTypes
    {
        Literal = 0, Method = 1
    }
    public class AngularView
    {

        private List<string> _URLPaths = null;

        public AngularView()
        { }
        public AngularView(string Identifier, string TemplatePath, string AccessRoles)
        {
            this.Identifier = Identifier;
            this.TemplatePath = TemplatePath;
            this.AccessRoles = AccessRoles != null ? AccessRoles : string.Empty;
        }


        public List<string> UrlPaths
        {
            get
            {
                if (_URLPaths == null)
                {
                    _URLPaths = new List<string>();

                    if (TemplatePath.EndsWith(".html"))
                    {
                        _URLPaths.Add(TemplatePath.Substring(0, TemplatePath.LastIndexOf(".html")));
                    }
                    else
                    {
                        _URLPaths.Add(TemplatePath.Substring(0, TemplatePath.LastIndexOf(".htm")));
                    }
                }

                return _URLPaths;
            }
            set => _URLPaths = value;
        }

        public string TemplatePath { get; set; }
        public string AccessRoles { get; set; }
        public string Identifier { get; set; }
        public bool IsCommon { get; set; }
        public bool IsDefaultTemplate { get; set; }
        public bool IsCacheable { get; set; }
        public URLPathTypes URLPathType { get; set; }
        public Dictionary<string, string> Defaults { get; set; }

        public class AngularUrlPath
        {
            public AngularUrlPath(string UrlPath)
            {
                this.UrlPath = UrlPath;
            }
            public AngularUrlPath(string UrlPath, bool IsDefault)
            {
                this.UrlPath = UrlPath;
                this.IsDefault = IsDefault;
            }
            public string UrlPath { get; set; }
            public bool IsDefault { get; set; }
        }
    }
}
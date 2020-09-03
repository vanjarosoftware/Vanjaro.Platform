namespace Vanjaro.UXManager.Extensions.Menu.Logs.Data.Scripts
{
    public static class CommonScript
    {
        private static string _TablePrefix = null;
        private static string _DnnTablePrefix = null;

        public static string TablePrefix
        {
            get
            {
                if (_TablePrefix == null)
                {
                    _TablePrefix = DotNetNuke.Data.DataProvider.Instance().DatabaseOwner;
                }

                return _TablePrefix;
            }
        }
        public static string DnnTablePrefix
        {
            get
            {
                if (_DnnTablePrefix == null)
                {
                    _DnnTablePrefix = DotNetNuke.Data.DataProvider.Instance().DatabaseOwner + DotNetNuke.Data.DataProvider.Instance().ObjectQualifier;
                }

                return _DnnTablePrefix;
            }
        }
    }
}
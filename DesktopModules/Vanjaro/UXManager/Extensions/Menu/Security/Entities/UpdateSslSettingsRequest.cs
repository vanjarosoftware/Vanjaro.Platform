namespace Vanjaro.UXManager.Extensions.Menu.Security.Entities
{
    public class UpdateSslSettingsRequest
    {
        public bool SSLEnabled { get; set; }
        public bool SSLEnforced { get; set; }
        public string SSLURL { get; set; }
        public string STDURL { get; set; }
        public string SSLOffloadHeader { get; set; }
    }
}

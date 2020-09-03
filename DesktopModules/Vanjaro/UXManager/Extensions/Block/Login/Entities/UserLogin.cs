namespace Vanjaro.UXManager.Extensions.Block.Login.Entities
{
    public class UserLogin
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public bool Remember { get; set; }
        public string Username { get; set; }
        public bool HasAgreedToTerms { get; set; }
    }
}
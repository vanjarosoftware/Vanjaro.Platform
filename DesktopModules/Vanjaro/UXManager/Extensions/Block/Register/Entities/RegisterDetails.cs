namespace Vanjaro.UXManager.Extensions.Block.Register.Entities
{

    public class RegisterDetails
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
        public bool RandomPassword { get; set; }
        public bool Authorize { get; set; }
        public bool Notify { get; set; }
        public string ConfirmPassword { get; set; }
        public string DisplayName { get; set; }
        public bool IsSuperUser { get; set; }
        public string VerificationCode { get; set; }
    }
}
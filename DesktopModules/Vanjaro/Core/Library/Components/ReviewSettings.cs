namespace Vanjaro.Core.Components
{
    public class ReviewSettings
    {

        public bool IsPageDraft { get; set; }
        public bool IsContentApproval { get; set; }
        public bool IsLocked { get; set; }
        public string NextStateName { get; set; }
        public bool IsModeratorEditPermission { get; set; }
    }
}
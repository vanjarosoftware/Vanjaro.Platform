using System;
using System.Web;
using System.Linq;
using System.Collections.Generic;

namespace Vanjaro.UXManager.Extensions.Menu.Users.Entities
{
    public class ImportJob
    {
        public enum ImportTypes
        {
            UserAccounts = 0, SecurityRoles = 1
        }
        public enum ImportActions
        {
            Append = 0, AppendUpdate = 1, Update = 2, Delete = 3, Remove = 4
        }

        public ImportJob()
        {

        }
        public ImportJob(ImportTypes ImportType, ImportActions ImportAction, ImportOptions Options)
        {
            this.Options = Options;
            this.ImportType = ImportType;
            this.ImportAction = ImportAction;
        }

        public ImportTypes ImportType { get; set; }
        public ImportActions ImportAction { get; set; }
        public ImportOptions Options { get; set; }
    }
    public class ImportOptions
    {
        public ImportOptions()
        { }
        public bool GenerateUsername { get; set; }
        public string GenerateUsernamePattern { get; set; }
        public bool GeneratePassword { get; set; }
        public bool GenerateDisplayName { get; set; }
        public string GenerateDisplayNamePattern { get; set; }
        public bool ForceChangePassword { get; set; }
        public bool DeletePortalUsers { get; set; }
        public bool SendMembershipNotification { get; set; }
    }
}
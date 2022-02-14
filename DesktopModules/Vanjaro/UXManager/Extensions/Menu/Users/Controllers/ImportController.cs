using System;
using System.Linq;
using System.Net;
using System.Web;
using System.Data;
using System.Web.Http;
using System.Collections.Generic;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using System.Web.UI.WebControls;
using Vanjaro.Common.ASPNET.WebAPI;
using Dnn.PersonaBar.Users.Components;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.UXManager.Extensions.Menu.Users.Factories;
using Vanjaro.UXManager.Extensions.Menu.Users.Entities;
using System.IO;
using Vanjaro.Common.Utilities;
using DotNetNuke.Entities.Portals;
using Vanjaro.UXManager.Library.Common;

namespace Vanjaro.UXManager.Extensions.Menu.Users.Controllers
{
    
    [AuthorizeAccessRoles(AccessRoles = "admin")]
    public class ImportController : UIEngineController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(UsersController));

        public static List<IUIData> GetData(string Identifier, Dictionary<string, string> UIEngineInfo, UserInfo userInfo, Dictionary<string, string> parameters)
        {
            string ResourceFile = Localization.GetLocalResourceFile(UIEngineInfo["uitemplatepath"], UIEngineInfo["identifier"]);
            ImportOptions ImportOptions = new ImportOptions { GenerateDisplayName = true, GenerateUsername = true, GeneratePassword = true };

            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            Settings.Add("UseEmailAsUserName", new UIData { Name = "UseEmailAsUserName", Value = PortalSettings.Current.Registration.UseEmailAsUserName.ToString() });
            Settings.Add("ImportType", new UIData { Name = "ImportType", Options = Factory.ImportFactory.ImportType(ResourceFile), OptionsText = "Text", OptionsValue = "Value", Value = "UserAccounts" });
            Settings.Add("ImportAction", new UIData { Name = "ImportAction", Options = new List<StringText>(), OptionsText = "Text", OptionsValue = "Value", Value = "CreateUsers" });
            Settings.Add("UserAccount", new UIData { Name = "UserAccount", Options = Factory.ImportFactory.UserAccount(ResourceFile), OptionsText = "Text", OptionsValue = "Value", Value = "CreateUsers" });
            Settings.Add("SecurityRoles", new UIData { Name = "SecurityRoles", Options = Factory.ImportFactory.SecurityRoles(ResourceFile), OptionsText = "Text", OptionsValue = "Value", Value = "CreateSecurityRoles" });
            Settings.Add("GenerateDisplayName", new UIData { Name = "GenerateDisplayName", Options = Factory.ImportFactory.GenerateDisplayName(ResourceFile), OptionsText = "Text", OptionsValue = "Value", Value = "Email address" });
            Settings.Add("GenerateUserName", new UIData { Name = "GenerateUserName", Options = Factory.ImportFactory.GenerateUserName(ResourceFile), OptionsText = "Text", OptionsValue = "Value", Value = "Email address" });
            Settings.Add("DataFields", new UIData { Name = "DataFields", Options = new List<ImportField>(), Value = "" });
            Settings.Add("ImportOption", new UIData { Name = "ImportOption", Options = ImportOptions });
            Settings.Add("Controller", new UIData { Name = "Controller", Value = "Import" });
            Settings.Add("CsvParameter", new UIData { Name = "CsvParameter", Value = "" });
            Settings.Add("MaximumUsersImportLimit", new UIData { Name = "MaximumUsersImportLimit", Value = Managers.ImportManager.MaximumLimit.ToString() });
            return Settings.Values.ToList();
        }

        [HttpPost]
        public dynamic GetDataFields(dynamic Data)
        {
            string ImportType = string.Empty, ImportAction = string.Empty;
            ImportType = Data["ImportType"].ToString();
            ImportAction = Data["ImportAction"].ToString();

            return Factory.ImportFactory.GetDataFields(PortalSettings.PortalId, ImportType, ImportAction);
        }

        [HttpPost]
        public ActionResult ImportData(dynamic Data)
        {
            ActionResult actionResult = new ActionResult();

            DataTable DImport = Data["DImport"].ToObject<DataTable>();
            if (DImport.Rows.Count > Managers.ImportManager.MaximumLimit)
                actionResult.AddError("MaximumUsersImportLimit", DotNetNuke.Services.Localization.Localization.GetString("MaximumUsersImportLimit", Components.Constants.LocalResourcesFile).Replace("'+$scope.ui.data.MaximumUsersImportLimit.Value+'", Managers.ImportManager.MaximumLimit.ToString()));

            if (actionResult.IsSuccess)
            {
                DImport.TableName = "ImportedData";
                ImportOptions options = new ImportOptions();
                if (Data["ImportOption"] != null)
                    options = Data["ImportOption"].ToObject<ImportOptions>();

                ImportJob job = new ImportJob(ImportJob.ImportTypes.UserAccounts, ImportJob.ImportActions.Append, options);

                actionResult = Managers.ImportManager.UserImport(PortalSettings.PortalId, job, DImport);
            }

            return actionResult;
        }
        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
    public class StringText
    {
        public string Text { get; set; }
        public string Value { get; set; }
    }
}
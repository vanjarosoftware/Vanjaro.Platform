using DotNetNuke.Entities.Portals;
using DotNetNuke.Providers.FolderProviders.AzureFolderProvider;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.Api;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.Core.Components;
using Vanjaro.UXManager.Extensions.Menu.Azure.Entities;
using Vanjaro.UXManager.Library.Common;

namespace Vanjaro.UXManager.Extensions.Menu.Azure.Controllers
{
    [ValidateAntiForgeryToken]
    [AuthorizeAccessRoles(AccessRoles = "admin")]
    public class AddController : UIEngineController
    {
        internal static List<IUIData> GetData(int PortalID, Dictionary<string, string> Parameters)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            string FolderMapID = string.Empty;
            if (Parameters.ContainsKey("mapid"))
            {
                FolderMapID = Parameters["mapid"];
            }

            List<StringValue> Containers = new List<StringValue>
            {
                new StringValue { Text = Localization.GetString("PleaseSelect.Text", Components.Constants.LocalResourceFile), Value = "" }
            };
            Connector connector = Manager.ConnectorsManager.Get(PortalID, FolderMapID);
            bool UseHTTPS = true;
            bool UseDirectLink = true;
            string SyncBatchSize = Components.Constants.DefaultSyncBatchSize.ToString();

            if (connector != null && connector.Configurations != null)
            {
                if (connector.Configurations.ContainsKey(Components.Constants.UseHttps) && !string.IsNullOrEmpty(connector.Configurations[Components.Constants.UseHttps]))
                {
                    UseHTTPS = bool.Parse(connector.Configurations[Components.Constants.UseHttps]);
                }
                if (connector.Configurations.ContainsKey(Components.Constants.DirectLink) && !string.IsNullOrEmpty(connector.Configurations[Components.Constants.DirectLink]))
                {
                    UseDirectLink = bool.Parse(connector.Configurations[Components.Constants.DirectLink]);
                }
                if (connector.Configurations.ContainsKey(Components.Constants.SyncBatchSize))
                {
                    SyncBatchSize = connector.Configurations[Components.Constants.SyncBatchSize];
                }
            }

            Settings.Add("Connector", new UIData { Name = "Connector", Options = connector });
            Settings.Add("UseHTTPS", new UIData { Name = "UseHTTPS", Value = UseHTTPS.ToString() });
            Settings.Add("UseDirectLink", new UIData { Name = "UseDirectLink", Value = UseDirectLink.ToString() });
            Settings.Add("SyncBatchSize", new UIData { Name = "SyncBatchSize", Value = SyncBatchSize });
            Settings.Add("Containers", new UIData { Name = "Containers", Options = Containers, OptionsText = "Text", OptionsValue = "Value", Value = "" });
            return Settings.Values.ToList();
        }

        [HttpPost]
        public ActionResult AddContainer(string name, Connector postData)
        {
            return Manager.ConnectorsManager.CreateContainer(name, postData.Configurations[Components.Constants.AzureAccountName], postData.Configurations[Components.Constants.AzureAccountKey], bool.Parse(postData.Configurations[Components.Constants.UseHttps]));
        }

        [HttpPost]
        public ActionResult Save(Connector postData)
        {
            return Manager.ConnectorsManager.Save(postData, PortalSettings.PortalId);
        }

        [HttpGet]
        public ActionResult GetAllContainers(int id)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                List<string> strs = new List<string>();
                AzureFolderProvider azureFolderProvider = new AzureFolderProvider();
                FolderMappingInfo folderMappingInfo = AzureConnector.FindAzureFolderMappingStatic(PortalSettings.PortalId, new int?(id), false);
                if (folderMappingInfo != null)
                {
                    strs = azureFolderProvider.GetAllContainers(folderMappingInfo);
                }
                List<StringValue> Containers = new List<StringValue>
                {
                    new StringValue { Text = Localization.GetString("PleaseSelect.Text", Components.Constants.LocalResourceFile), Value = "" }
                };
                foreach (string item in strs)
                {
                    Containers.Add(new StringValue { Text = item, Value = item });
                }
                actionResult.Data = Containers;
                actionResult.IsSuccess = true;
            }
            catch (StorageException storageException1)
            {
                StorageException storageException = storageException1;
                Core.Managers.ExceptionManage.LogException(storageException);
                string httpStatusMessage = storageException.RequestInformation.HttpStatusMessage ?? storageException.Message;
                actionResult.AddError("InternalServerError", httpStatusMessage);
            }
            catch (Exception exception)
            {
                Core.Managers.ExceptionManage.LogException(exception);
                actionResult.AddError("InternalServerError", "An error has occurred connecting to the Azure account.");
            }
            return actionResult;
        }

        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}
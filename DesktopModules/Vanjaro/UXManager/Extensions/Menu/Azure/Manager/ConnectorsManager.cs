using DotNetNuke.Common;
using DotNetNuke.Services.Connections;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using Vanjaro.UXManager.Extensions.Menu.Azure.Entities;
using Vanjaro.UXManager.Library.Common;

namespace Vanjaro.UXManager.Extensions.Menu.Azure.Manager
{
    public class ConnectorsManager
    {

        public static ActionResult CreateContainer(string containerName, string accountName, string accountKey, bool useHttps)
        {
            ActionResult actionResult = new ActionResult();
            StorageCredentials sc;
            try
            {
                sc = new StorageCredentials(accountName, accountKey);
            }
            catch (Exception ex)
            {
                Core.Managers.ExceptionManage.LogException(ex);
                actionResult.AddError("AuthenticationFailure", Localization.GetString("AuthenticationFailure.ErrorMessage", Components.Constants.LocalResourceFile));
                return actionResult;
            }

            CloudStorageAccount csa = new CloudStorageAccount(sc, useHttps);
            CloudBlobClient blobClient = csa.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);

            try
            {
                if (container.CreateIfNotExists())
                {
                    BlobContainerPermissions permissions = container.GetPermissions();
                    permissions.PublicAccess = BlobContainerPublicAccessType.Container;
                    container.SetPermissions(permissions);
                }
                actionResult.IsSuccess = true;
                return actionResult;
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.ExtendedErrorInformation != null)
                {
                    switch (ex.RequestInformation.ExtendedErrorInformation.ErrorCode)
                    {
                        case "AccountNotFound":
                            actionResult.AddError("AccountNotFound", Localization.GetString("AccountNotFound.ErrorMessage", Components.Constants.LocalResourceFile));
                            break;
                        case "AuthenticationFailure":
                            actionResult.AddError("AuthenticationFailure", Localization.GetString("AuthenticationFailure.ErrorMessage", Components.Constants.LocalResourceFile));
                            break;
                        case "AccessDenied":
                            actionResult.AddError("AccessDenied", Localization.GetString("AccessDenied.ErrorMessage", Components.Constants.LocalResourceFile));
                            break;
                        case "ContainerAlreadyExists":
                            actionResult.IsSuccess = true;
                            return actionResult;
                        default:
                            Core.Managers.ExceptionManage.LogException(ex);
                            actionResult.AddError("NewContainer", Localization.GetString("NewContainer.ErrorMessage", Components.Constants.LocalResourceFile));
                            break;
                    }
                }
                else
                {
                    actionResult.AddError("InternalError", ex.Message);
                }
            }
            catch (Exception ex)
            {
                Core.Managers.ExceptionManage.LogException(ex);
                actionResult.AddError("NewContainer", Localization.GetString("NewContainer.ErrorMessage", Components.Constants.LocalResourceFile));
            }

            return actionResult;
        }

        public static ActionResult Save(Connector postData, int PortalId)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                string name = postData.Name;
                string displayName = postData.DisplayName;
                string id = postData.Id;
                AzureConnector AzureConnector = new AzureConnector();
                List<IConnector> connectors = AzureConnector.GetConnectors(PortalId).ToList();

                IConnector connector = connectors.FirstOrDefault(c => c.Id == id);

                if (connector == null && string.IsNullOrEmpty(id))
                {
                    connector = new AzureConnector
                    {
                        Id = null,
                        DisplayName = null
                    };
                }
                if (connector != null && !string.IsNullOrEmpty(displayName) && connector.DisplayName != displayName)
                {
                    connector.DisplayName = string.IsNullOrEmpty(displayName) ? "" : displayName;
                }

                bool validated = false;
                if (connector != null)
                {
                    bool saved = connector.SaveConfig(PortalId, postData.Configurations, ref validated,
                        out string customErrorMessage);
                    if (!saved)
                    {
                        string Message = string.IsNullOrEmpty(customErrorMessage)
                                    ? Localization.GetString("ErrSavingConnectorSettings.Text", Components.Constants.LocalResourceFile)
                                    : customErrorMessage;
                        actionResult.AddError("ErrSavingConnectorSettings", Message);
                        return actionResult;
                    }
                }
                actionResult.IsSuccess = true;
                actionResult.Data = connector?.Id;
                return actionResult;

            }
            catch (Exception ex)
            {
                Core.Managers.ExceptionManage.LogException(ex);
                actionResult.AddError("InternalServerError", "InternalServerError", ex);
                return actionResult;
            }
        }

        public static List<Connector> GetAll(int PortalId)
        {
            List<Connector> Connectors = new List<Connector>();
            AzureConnector AzureConnector = new AzureConnector();
            foreach (IConnector con in AzureConnector.GetConnectors(PortalId).ToList())
            {
                Connector connector = new Connector
                {
                    Id = con.Id,
                    Name = con.Name,
                    Type = con.Type,
                    DisplayName = con.DisplayName,
                    Connected = con.HasConfig(PortalId),
                    IconUrl = Globals.ResolveUrl(con.IconUrl)
                };
                Connectors.Add(connector);
            }
            return Connectors;
        }

        public static Connector Get(int PortalId, string Id)
        {
            AzureConnector AzureConnector = new AzureConnector();
            IConnector Connector = AzureConnector.GetConnectors(PortalId).Where(c => c.Id == Id).FirstOrDefault();
            Connector Connect = new Connector();
            if (Connector != null)
            {
                Connect.Id = Connector.Id;
                Connect.Name = Connector.Name;
                Connect.Type = Connector.Type;
                Connect.DisplayName = Connector.DisplayName;
                Connect.Connected = Connector.HasConfig(PortalId);
                Connect.IconUrl = Globals.ResolveUrl(Connector.IconUrl);
                Connect.PluginFolder = Globals.ResolveUrl(Connector.PluginFolder);
                Connect.Configurations = Connector.GetConfig(PortalId);
                Connect.SupportsMultiple = Connector.SupportsMultiple;
            }
            return Connect;
        }


    }
}
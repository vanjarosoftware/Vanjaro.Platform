using DotNetNuke.Collections;
using DotNetNuke.Data;
using DotNetNuke.Services.Connections;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.FileSystem.Internal;
using DotNetNuke.Services.Localization;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Vanjaro.UXManager.Extensions.Menu.Azure.Components;

namespace Vanjaro.UXManager.Extensions.Menu.Azure
{
    public class AzureConnector : IConnector
    {
        #region Properties
        private static readonly DataProvider dataProvider = DataProvider.Instance();
        private const string DefaultDisplayName = "Azure Storage";

        public string Name => "Azure";

        private string _displayName;
        public string DisplayName
        {
            get => string.IsNullOrEmpty(_displayName) ? DefaultDisplayName : _displayName;
            set => _displayName = value;
        }

        public string IconUrl => "~/DesktopModules/Vanjaro/UXManager/Extensions/Menu/Azure/Resources/Images/Azure.png";

        public string AppCssPath => "~/DesktopModules/Vanjaro/UXManager/Extensions/Menu/" + ExtensionInfo.Name + "/Resources/Stylesheets/app.css";

        public string PluginFolder => "~/DesktopModules/Vanjaro/UXManager/Extensions/Menu/Azure/";

        public bool IsEngageConnector => false;

        public string Id { get; set; }

        public ConnectorCategories Type => ConnectorCategories.FileSystem;

        public bool SupportsMultiple => true;

        #endregion

        #region Public Methods
        public IEnumerable<IConnector> GetConnectors(int portalId)
        {
            IList<FolderMappingInfo> connectors = FindAzureFolderMappings(portalId);
            if (connectors != null && connectors.Any())
            {
                connectors.ForEach(x => { Id = x.FolderMappingID.ToString(); });
                List<IConnector> finalCon = connectors.Select(x => (IConnector)Activator.CreateInstance(GetType())).ToList();
                finalCon.ForEach(x =>
                {
                    x.Id = connectors[finalCon.IndexOf(x)].FolderMappingID.ToString();
                    x.DisplayName = connectors[finalCon.IndexOf(x)].MappingName;
                });
                return finalCon;
            }
            return new List<IConnector>();
        }

        public void DeleteConnector(int portalId)
        {
            if (!string.IsNullOrEmpty(Id))
            {
                if (int.TryParse(Id, out int mappingId))
                {
                    DeleteAzureFolders(portalId, mappingId);
                    DeleteAzureFolderMapping(portalId, mappingId);
                }
            }
        }

        public bool HasConfig(int portalId)
        {
            FolderMappingInfo folderMapping = FindAzureFolderMapping(portalId, false, true);
            Id = Convert.ToString(folderMapping?.FolderMappingID);
            return GetConfig(portalId)["Connected"] == "true";
        }

        public IDictionary<string, string> GetConfig(int portalId)
        {
            Dictionary<string, string> configs = new Dictionary<string, string>();

            FolderMappingInfo folderMapping = FindAzureFolderMapping(portalId, false);

            Hashtable settings = folderMapping != null ? folderMapping.FolderMappingSettings : new Hashtable();


            configs.Add("AccountName", GetSetting(settings, Constants.AzureAccountName, true));
            configs.Add("AccountKey", GetSetting(settings, Constants.AzureAccountKey, true));
            configs.Add("Container", GetSetting(settings, Constants.AzureContainerName));
            configs.Add("UseHttps", GetSetting(settings, Constants.UseHttps));
            configs.Add("DirectLink", GetSetting(settings, Constants.DirectLink));
            configs.Add("SyncBatchSize", GetSetting(settings, Constants.SyncBatchSize));
            configs.Add("Connected", !string.IsNullOrEmpty(GetSetting(settings, Constants.AzureAccountName)) && !string.IsNullOrEmpty(GetSetting(settings, Constants.AzureContainerName)) ? "true" : "false");

            //This setting will improve the UI to set password-type inputs on secure settings
            configs.Add("SecureSettings", "AccountKey");
            configs.Add("Id", Convert.ToString(folderMapping?.FolderMappingID));
            return configs;
        }

        public bool SaveConfig(int portalId, IDictionary<string, string> values, ref bool validated, out string customErrorMessage)
        {
            customErrorMessage = string.Empty;
            string azureAccountName = values[Constants.AzureAccountName];
            string azureAccountKey = values[Constants.AzureAccountKey];
            string azureContainerName = values.ContainsKey(Constants.AzureContainerName) ? values[Constants.AzureContainerName] : string.Empty;

            bool emptyFields = string.IsNullOrEmpty(azureAccountKey) && string.IsNullOrEmpty(azureAccountName);

            validated = true;
            if (emptyFields)
            {
                if (SupportsMultiple)
                {
                    throw new Exception(Localization.GetString("ErrorRequiredFields", Constants.LocalResourceFile));
                }

                DeleteAzureFolderMapping(portalId);
                return true;
            }
            if (!Validation(azureAccountName, azureAccountKey, azureContainerName, bool.Parse(values[Constants.UseHttps])))
            {
                validated = false;
                return true;
            }
            if (FolderMappingNameExists(portalId, DisplayName,
                Convert.ToInt32(!string.IsNullOrEmpty(Id) ? Id : null)))
            {
                throw new Exception(Localization.GetString("ErrorMappingNameExists", Constants.LocalResourceFile));
            }

            try
            {
                IList<FolderMappingInfo> folderMappings = FindAzureFolderMappings(portalId);
                FolderMappingInfo folderMapping;
                if (SupportsMultiple && !string.IsNullOrEmpty(Id))
                {
                    folderMapping = folderMappings.FirstOrDefault(x => x.FolderMappingID.ToString() == Id);
                }
                else
                {
                    folderMapping = FindAzureFolderMapping(portalId);
                }

                Hashtable settings = folderMapping.FolderMappingSettings;

                string savedAccount = GetSetting(settings, Constants.AzureAccountName, true);
                string savedKey = GetSetting(settings, Constants.AzureAccountKey, true);
                string savedContainer = GetSetting(settings, Constants.AzureContainerName);

                bool accountChanged = savedAccount != azureAccountName || savedKey != azureAccountKey;

                if (accountChanged)
                {
                    DeleteAzureFolderMapping(portalId, folderMapping.FolderMappingID);
                    folderMapping = FindAzureFolderMapping(portalId);
                    settings = folderMapping.FolderMappingSettings;
                }
                else if (savedContainer != azureContainerName)
                {
                    DeleteAzureFolders(portalId, folderMapping.FolderMappingID);
                }

                FolderProvider folderProvider = FolderProvider.Instance(Constants.FolderProviderType);

                settings[Constants.AzureAccountName] = folderProvider.EncryptValue(azureAccountName);
                settings[Constants.AzureAccountKey] = folderProvider.EncryptValue(azureAccountKey);

                if (values.ContainsKey(Constants.AzureContainerName) && !string.IsNullOrEmpty(values[Constants.AzureContainerName]))
                {
                    settings[Constants.AzureContainerName] = values[Constants.AzureContainerName];
                }
                else
                {
                    folderMapping.FolderMappingSettings[Constants.AzureContainerName] = string.Empty;
                }

                if (values.ContainsKey(Constants.DirectLink) && values[Constants.DirectLink].ToLower() == "false")
                {
                    folderMapping.FolderMappingSettings[Constants.DirectLink] = "False";
                }
                else
                {
                    folderMapping.FolderMappingSettings[Constants.DirectLink] = "True";
                }

                if (values.ContainsKey(Constants.UseHttps) && values[Constants.UseHttps].ToLower() == "false")
                {
                    folderMapping.FolderMappingSettings[Constants.UseHttps] = "False";
                }
                else
                {
                    folderMapping.FolderMappingSettings[Constants.UseHttps] = "True";
                }

                if (folderMapping.MappingName != DisplayName && !string.IsNullOrEmpty(DisplayName) &&
                   DisplayName != DefaultDisplayName)
                {
                    folderMapping.MappingName = DisplayName;
                }
                if (values.ContainsKey(Constants.SyncBatchSize))
                {
                    folderMapping.FolderMappingSettings[Constants.SyncBatchSize] = values[Constants.SyncBatchSize];
                }

                FolderMappingController.Instance.UpdateFolderMapping(folderMapping);

                return true;
            }
            catch (Exception ex)
            {
                Core.Managers.ExceptionManage.LogException(ex);
                return false;
            }
        }

        #endregion

        #region Private Methods

        private bool Validation(string azureAccountName, string azureAccountKey, string azureContainerName, bool UseHTTPS)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(azureAccountName))
                {
                    throw new ConnectorArgumentException(Localization.GetString("AccountNameCannotBeEmpty.ErrorMessage",
                        Constants.LocalResourceFile));
                }
                if (string.IsNullOrWhiteSpace(azureAccountKey))
                {
                    throw new ConnectorArgumentException(Localization.GetString("AccountKeyCannotBeEmpty.ErrorMessage",
                        Constants.LocalResourceFile));
                }
                StorageCredentials sc = new StorageCredentials(azureAccountName, azureAccountKey);
                CloudStorageAccount csa = new CloudStorageAccount(sc, UseHTTPS);
                Microsoft.WindowsAzure.Storage.Blob.CloudBlobClient blobClient = csa.CreateCloudBlobClient();

                blobClient.DefaultRequestOptions.RetryPolicy = new NoRetry();

                IEnumerable<Microsoft.WindowsAzure.Storage.Blob.CloudBlobContainer> containers = blobClient.ListContainers();
                if (containers.Any())
                {
                    if (!string.IsNullOrEmpty(azureContainerName))
                    {
                        if (!containers.Any(container => container.Name == azureContainerName))
                        {
                            throw new Exception(Localization.GetString("ErrorInvalidContainerName",
                                Constants.LocalResourceFile));
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    throw new Exception(Localization.GetString("AccountNotFound.ErrorMessage",
                        Constants.LocalResourceFile));
                }
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.ExtendedErrorInformation != null)
                {
                    if (ex.RequestInformation.ExtendedErrorInformation.ErrorCode == "AccountNotFound")
                    {
                        throw new Exception(Localization.GetString("AccountNotFound.ErrorMessage",
                            Constants.LocalResourceFile));
                    }
                    else if (ex.RequestInformation.ExtendedErrorInformation.ErrorCode == "AccessDenied")
                    {
                        throw new Exception(Localization.GetString("AccessDenied.ErrorMessage",
                            Constants.LocalResourceFile));
                    }
                    else
                    {
                        throw new Exception(ex.RequestInformation.HttpStatusMessage);
                    }
                }

                throw new Exception(ex.RequestInformation.HttpStatusMessage ?? ex.Message);
            }
            catch (FormatException ex)
            {
                if (ex.GetType() == typeof(UriFormatException))
                {
                    throw new ConnectorArgumentException(Localization.GetString("InvalidAccountName.ErrorMessage",
                        Constants.LocalResourceFile));
                }
                throw new ConnectorArgumentException(Localization.GetString("InvalidAccountKey.ErrorMessage",
                    Constants.LocalResourceFile));
            }
        }

        private string GetSetting(Hashtable settings, string name, bool encrypt = false)
        {
            if (!settings.ContainsKey(name))
            {
                return string.Empty;
            }

            if (encrypt)
            {
                FolderProvider folderProvider = FolderProvider.Instance(Constants.FolderProviderType);
                return folderProvider.GetEncryptedSetting(settings, name);
            }

            return settings[name].ToString();
        }

        internal static FolderMappingInfo FindAzureFolderMappingStatic(int portalId, int? folderMappingId = null,
            bool autoCreate = true)
        {
            IEnumerable<FolderMappingInfo> folderMappings = FolderMappingController.Instance.GetFolderMappings(portalId)
                .Where(f => f.FolderProviderType == Constants.FolderProviderType);

            if (folderMappingId != null)
            {
                return folderMappings.FirstOrDefault(x => x.FolderMappingID == folderMappingId);
            }

            if (!folderMappings.Any() && autoCreate)
            {
                return CreateAzureFolderMappingStatic(portalId);
            }
            return folderMappings.FirstOrDefault();
        }

        private FolderMappingInfo FindAzureFolderMapping(int portalId, bool autoCreate = true, bool checkId = false)
        {
            List<FolderMappingInfo> folderMappings = FolderMappingController.Instance.GetFolderMappings(portalId)
                .Where(f => f.FolderProviderType == Constants.FolderProviderType).ToList();

            //Create new mapping if none is found.
            if (!folderMappings.Any() && autoCreate)
            {
                return CreateAzureFolderMapping(portalId, DefaultDisplayName);
            }

            if ((checkId && string.IsNullOrEmpty(Id)) || !SupportsMultiple)
            {
                return folderMappings.FirstOrDefault();
            }

            FolderMappingInfo folderMapping = folderMappings.FirstOrDefault(x => x.FolderMappingID.ToString() == Id);

            if (folderMapping == null && autoCreate)
            {
                folderMapping = CreateAzureFolderMapping(portalId);
            }
            return folderMapping;
        }

        private IList<FolderMappingInfo> FindAzureFolderMappings(int portalId)
        {
            return FolderMappingController.Instance.GetFolderMappings(portalId)
                .Where(f => f.FolderProviderType == Constants.FolderProviderType).ToList();
        }

        private bool FolderMappingNameExists(int portalId, string mappingName, int? exceptMappingId)
        {
            return FolderMappingController.Instance.GetFolderMappings(portalId)
                .Any(
                    f =>
                        f.MappingName.ToLowerInvariant() == mappingName.ToLowerInvariant() &&
                        (f.FolderMappingID != exceptMappingId));
        }

        private FolderMappingInfo CreateAzureFolderMapping(int portalId, string mappingName = "")
        {
            FolderMappingInfo folderMapping = CreateAzureFolderMappingStatic(portalId, mappingName);
            Id = folderMapping.FolderMappingID.ToString();
            return folderMapping;
        }

        private static FolderMappingInfo CreateAzureFolderMappingStatic(int portalId, string mappingName = "")
        {
            FolderMappingInfo folderMapping = new FolderMappingInfo
            {
                PortalID = portalId,
                MappingName =
                    string.IsNullOrEmpty(mappingName)
                        ? $"{DefaultDisplayName}_{DateTime.Now.Ticks}"
                        : mappingName,
                FolderProviderType = Constants.FolderProviderType
            };
            folderMapping.FolderMappingID = FolderMappingController.Instance.AddFolderMapping(folderMapping);
            return folderMapping;
        }

        private static void DeleteAzureFolderMapping(int portalId, int folderMappingId)
        {
            FolderMappingController.Instance.DeleteFolderMapping(portalId, folderMappingId);
        }

        private static void DeleteAzureFolderMapping(int portalId)
        {
            FolderMappingInfo folderMapping = FolderMappingController.Instance.GetFolderMappings(portalId)
                           .FirstOrDefault(f => f.FolderProviderType == Constants.FolderProviderType);

            if (folderMapping != null)
            {
                FolderMappingController.Instance.DeleteFolderMapping(portalId, folderMapping.FolderMappingID);
            }
        }

        private static void DeleteAzureFolders(int portalId, int folderMappingId)
        {
            IFolderManager folderManager = FolderManager.Instance;
            IEnumerable<IFolderInfo> folders = folderManager.GetFolders(portalId);

            IEnumerable<IFolderInfo> folderMappingFolders = folders.Where(f => f.FolderMappingID == folderMappingId);

            if (folderMappingFolders.Any())
            {
                // Delete files in folders with the provided mapping (only in the database)
                foreach (
                    IFileInfo file in
                        folderMappingFolders.Select<IFolderInfo, IEnumerable<IFileInfo>>(folderManager.GetFiles)
                            .SelectMany(files => files))
                {
                    dataProvider.DeleteFile(portalId, file.FileName, file.FolderId);
                }

                // Remove the folders with the provided mapping that doesn't have child folders with other mapping (only in the database and filesystem)
                IEnumerable<IFolderInfo> folders1 = folders; // copy the variable to not access a modified closure
                IEnumerable<IFolderInfo> removableFolders =
                    folders.Where(
                        f => f.FolderMappingID == folderMappingId && !folders1.Any(f2 => f2.FolderID != f.FolderID &&
                                                                                         f2.FolderPath.StartsWith(
                                                                                             f.FolderPath) &&
                                                                                         f2.FolderMappingID !=
                                                                                         folderMappingId));

                if (removableFolders.Count() > 0)
                {
                    foreach (IFolderInfo removableFolder in removableFolders.OrderByDescending(rf => rf.FolderPath))
                    {
                        DirectoryWrapper.Instance.Delete(removableFolder.PhysicalPath, false);
                        dataProvider.DeleteFolder(portalId, removableFolder.FolderPath);
                    }
                }

                // Update the rest of folders with the provided mapping to use the standard mapping
                folders = folderManager.GetFolders(portalId, false); // re-fetch the folders

                folderMappingFolders = folders.Where(f => f.FolderMappingID == folderMappingId);

                if (folderMappingFolders.Count() > 0)
                {
                    FolderMappingInfo defaultFolderMapping = FolderMappingController.Instance.GetDefaultFolderMapping(portalId);

                    foreach (IFolderInfo folderMappingFolder in folderMappingFolders)
                    {
                        folderMappingFolder.FolderMappingID = defaultFolderMapping.FolderMappingID;
                        folderManager.UpdateFolder(folderMappingFolder);
                    }
                }
            }
        }

        #endregion
    }
}
using Dnn.PersonaBar.Extensions.Components.Dto;
using Dnn.PersonaBar.Extensions.Components.Editors;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Authentication;
using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.UXManager.Library.Common;

namespace Vanjaro.UXManager.Extensions.Menu.Extensions.Controllers
{
    [AuthorizeAccessRoles(AccessRoles = "host")]
    [ValidateAntiForgeryToken]
    public class EditExtensionController : UIEngineController
    {
        internal static List<IUIData> GetData(int PortalID, Dictionary<string, string> Parameters)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            int pid = 0;
            try
            {
                pid = int.Parse(Parameters["pid"]);
            }
            catch { }

            PackageSettingsDto packageSettings = new PackageSettingsDto
            {
                PortalId = PortalID
            };

            PackageInfo package = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == pid);
            if (package != null)
            {
                Dnn.PersonaBar.Extensions.Components.Editors.IPackageEditor packageEditor = PackageEditorFactory.GetPackageEditor(package.PackageType);
                PackageInfoDto packageDetail = packageEditor?.GetPackageDetail(PortalID, package) ?? new PackageInfoDto(PortalID, package);
                Settings.Add("packageDetail", new UIData { Name = "packageDetail", Options = packageDetail });
                packageSettings.PackageId = packageDetail.PackageId;
                if (packageDetail is Dnn.PersonaBar.Extensions.Components.Dto.Editors.ModulePackagePermissionsDto)
                {
                    dynamic permissions = packageDetail as Dnn.PersonaBar.Extensions.Components.Dto.Editors.ModulePackagePermissionsDto;
                    Settings.Add("Permissions", new UIData { Name = "Permissions", Options = Managers.ExtensionsManager.GetPermission(permissions.Permissions) });
                }
            }
            Settings.Add("packageSettings", new UIData { Name = "packageSettings", Options = packageSettings });

            Settings.Add("ModuleCategory", new UIData { Name = "ModuleCategory", Options = Managers.ExtensionsManager.GetModuleCategories(), OptionsText = "Value", OptionsValue = "Key", Value = "" });
            Settings.Add("ModuleSharing", new UIData { Name = "ModuleSharing", Options = Managers.ExtensionsManager.GetModuleSharing(), OptionsText = "Value", OptionsValue = "Key", Value = "" });
            Settings.Add("ControlSourceFolder", new UIData { Name = "ControlSourceFolder", Options = new List<Managers.TempExt>(), OptionsText = "Value", OptionsValue = "Key" });
            Settings.Add("ControlSourceFile", new UIData { Name = "ControlSourceFile", Options = new List<Managers.TempExt>(), OptionsText = "Value", OptionsValue = "Key" });
            Settings.Add("ControlType", new UIData { Name = "ControlType", Options = Managers.ExtensionsManager.GetControlTypes(), OptionsText = "Value", OptionsValue = "Key" });
            Settings.Add("ControlIcon", new UIData { Name = "ControlIcon", Options = new List<Managers.TempExt>(), OptionsText = "Value", OptionsValue = "Key" });

            return Settings.Values.ToList();
        }

        [HttpPost]
        public ActionResult Save(PackageSettingsDto packageSettings)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                PackageInfo package = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == packageSettings.PackageId);
                if (package == null)
                {
                    actionResult.AddError("SavePackageSettings.PackageNotFound", Localization.GetString("SavePackageSettings.PackageNotFound", Components.Constants.ExtensionSharedResources));
                    return actionResult;
                }

                if (UserInfo.IsSuperUser)
                {
                    AuthenticationInfo authService = AuthenticationController.GetAuthenticationServiceByPackageID(package.PackageID);
                    bool isReadOnly = authService != null && authService.AuthenticationType == Components.Constants.DnnAuthTypeName;
                    if (isReadOnly)
                    {
                        actionResult.AddError("ReadOnlyPackage.SaveErrorMessage", Localization.GetString("ReadOnlyPackage.SaveErrorMessage", Components.Constants.ExtensionSharedResources));
                        return actionResult;
                    }

                    Type type = package.GetType();
                    bool needUpdate = false;
                    foreach (KeyValuePair<string, string> kvp in packageSettings.Settings)
                    {
                        PropertyInfo property = type.GetProperty(kvp.Key, BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.Public);
                        if (property != null && property.CanWrite)
                        {
                            string value = kvp.Value;
                            object propValue = property.GetValue(package);
                            if (propValue == null || propValue.ToString() != value)
                            {
                                object nativeValue = property.PropertyType == typeof(Version)
                                    ? new Version(value) : Convert.ChangeType(value, property.PropertyType);
                                property.SetValue(package, nativeValue);
                                needUpdate = true;
                            }
                        }
                    }

                    if (needUpdate)
                    {
                        PackageController.Instance.SaveExtensionPackage(package);
                    }
                }

                Dnn.PersonaBar.Extensions.Components.Editors.IPackageEditor packageEditor = PackageEditorFactory.GetPackageEditor(package.PackageType);
                if (packageEditor != null)
                {
                    packageEditor.SavePackageSettings(packageSettings, out string error);

                    if (!string.IsNullOrEmpty(error))
                    {
                        actionResult.AddError("error", error);
                        return actionResult;
                    }
                }

                PackageInfoDto packageDetail = packageEditor?.GetPackageDetail(packageSettings.PortalId, package) ?? new PackageInfoDto(packageSettings.PortalId, package);
                actionResult.Data = packageDetail;
                actionResult.IsSuccess = true;
                return actionResult;
            }
            catch (Exception ex)
            {
                actionResult.AddError("", "", ex);
                return actionResult;
            }
        }

        [HttpGet]
        public ActionResult GetSourceFolders()
        {
            return Managers.ExtensionsManager.GetSourceFolders();
        }

        [HttpGet]
        public ActionResult GetSourceFiles(string root)
        {
            return Managers.ExtensionsManager.GetSourceFiles(root);
        }
        [HttpGet]
        public ActionResult GetIcons(string controlPath)
        {
            return Managers.ExtensionsManager.GetIcons(controlPath);
        }

        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}
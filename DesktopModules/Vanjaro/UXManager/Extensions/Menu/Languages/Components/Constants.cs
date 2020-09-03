using System;
using System.Text.RegularExpressions;

namespace Vanjaro.UXManager.Extensions.Menu.Languages.Components
{
    public class Constants
    {
        public const string LocalResourcesFile = "~/DesktopModules/Vanjaro/UXManager/Extensions/Menu/" + ExtensionInfo.Name + "/Views/App_LocalResources/Shared.resx";
        public const string AuthFailureMessage = "Authorization has been denied for this request.";
        internal static readonly Regex FileInfoRegex = new Regex(
            @"\.([a-z]{2,3}\-[0-9A-Z]{2,4}(-[A-Z]{2})?)(\.(Host|Portal-\d+))?\.resx$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromSeconds(1));
    }

    public enum LanguageResourceMode
    {
        // for any unspecified item; these are usually the second level and lower folders/files
        Any,

        System, // aka. Global
        Global = System,

        Host,   // not supported in PersonaBar; kept here because the base code uses it

        Portal, // aka. Site
        Site = Portal,
    }
}
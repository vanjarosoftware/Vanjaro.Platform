using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Log.EventLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Xml;
using Vanjaro.UXManager.Extensions.Menu.Languages.Components;
using Vanjaro.UXManager.Extensions.Menu.Languages.Entities;
using Vanjaro.UXManager.Library.Common;

namespace Vanjaro.UXManager.Extensions.Menu.Languages.Managers
{
    public class ResourcesManager
    {
        private static string selectedResourceFile;
        private const string Locale_ResourceFile = "~/DesktopModules/Vanjaro/UXManager/Extensions/Menu/" + ExtensionInfo.Name + "/Views/Setting/App_LocalResources/Resources.resx";

        public static List<TreeView> GetRootResourcesFolders()
        {
            List<TreeView> result = new List<TreeView>
            {
                new TreeView { Name = Localization.GetString("LocalResources", Locale_ResourceFile), Value = "", Type = "folder", childrenCount = GetAllChildrenCount() },
                new TreeView { Name = Localization.GetString("GlobalResources", Locale_ResourceFile), Value = "_/App_GlobalResources", Type = "folder", childrenCount = GetAllChildrenCount("_/App_GlobalResources") },
                new TreeView { Name = Localization.GetString("SiteTemplates", Locale_ResourceFile), Value = "_/Portals/_default", Type = "folder", childrenCount = GetAllChildrenCount("_/Portals/_default") }
            };
            return result;
        }

        public static ActionResult GetSubRootResources(string currentFolder)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                List<KeyValuePair<string, string>> folders = new List<KeyValuePair<string, string>>();
                List<KeyValuePair<string, string>> files = new List<KeyValuePair<string, string>>();
                HttpServerUtility server = HttpContext.Current.Server;

                if (string.IsNullOrEmpty(currentFolder))
                {
                    folders.AddRange(new[]
                    {
                        "Admin",
                        "Controls",
                        "DesktopModules",
                        "Install",
                        "Providers"
                    }.Select(s => new KeyValuePair<string, string>(s, "_/" + s)));
                }
                else
                {
                    string foldername = currentFolder;
                    if (currentFolder.IndexOf("_/", StringComparison.Ordinal) == 0)
                    {
                        foldername = foldername.Substring(2);
                    }
                    IEnumerable<KeyValuePair<string, string>> directories = LanguagesManager.GetResxDirectories(server.MapPath("~/" + foldername));
                    IEnumerable<KeyValuePair<string, string>> directoryFiles = LanguagesManager.GetResxFiles(server.MapPath("~/" + foldername));
                    if (currentFolder.IndexOf("_/", StringComparison.Ordinal) == 0)
                    {
                        folders.AddRange(directories.Select(
                                s => new KeyValuePair<string, string>(s.Key, currentFolder + "/" + s.Key)));
                        files.AddRange(directoryFiles.Select(
                                f => new KeyValuePair<string, string>(f.Key, currentFolder + "/" + f.Key)));
                    }
                    else
                    {
                        folders.AddRange(directories);
                        files.AddRange(directoryFiles);
                    }
                }
                List<TreeView> TreeViews = new List<TreeView>();
                foreach (KeyValuePair<string, string> item in folders)
                {
                    TreeViews.Add(new TreeView { Name = item.Key, Value = item.Value, Type = "folder", childrenCount = GetAllChildrenCount(item.Value) });
                }

                foreach (KeyValuePair<string, string> item in files)
                {
                    TreeViews.Add(new TreeView { Name = item.Key, Value = item.Value, Type = "file" });
                }

                actionResult.IsSuccess = true;
                actionResult.Data = TreeViews;
            }
            catch (Exception ex)
            {
                actionResult.AddError("", "", ex);
            }
            return actionResult;
        }

        public static int GetAllChildrenCount(string currentFolder = null)
        {
            int Count = 0;
            try
            {
                List<KeyValuePair<string, string>> folders = new List<KeyValuePair<string, string>>();
                List<KeyValuePair<string, string>> files = new List<KeyValuePair<string, string>>();
                HttpServerUtility server = HttpContext.Current.Server;

                if (string.IsNullOrEmpty(currentFolder))
                {
                    folders.AddRange(new[]
                    {
                        "Admin",
                        "Controls",
                        "DesktopModules",
                        "Install",
                        "Providers"
                    }.Select(s => new KeyValuePair<string, string>(s, "_/" + s)));
                }
                else
                {
                    string foldername = currentFolder;
                    if (currentFolder.IndexOf("_/", StringComparison.Ordinal) == 0)
                    {
                        foldername = foldername.Substring(2);
                    }
                    IEnumerable<KeyValuePair<string, string>> directories = LanguagesManager.GetResxDirectories(server.MapPath("~/" + foldername));
                    IEnumerable<KeyValuePair<string, string>> directoryFiles = LanguagesManager.GetResxFiles(server.MapPath("~/" + foldername));
                    if (currentFolder.IndexOf("_/", StringComparison.Ordinal) == 0)
                    {
                        folders.AddRange(directories.Select(
                                s => new KeyValuePair<string, string>(s.Key, currentFolder + "/" + s.Key)));
                        files.AddRange(directoryFiles.Select(
                                f => new KeyValuePair<string, string>(f.Key, currentFolder + "/" + f.Key)));
                    }
                    else
                    {
                        folders.AddRange(directories);
                        files.AddRange(directoryFiles);
                    }
                }
                Count = folders.Count + files.Count;
            }
            catch (Exception ex)
            {
                Core.Managers.ExceptionManage.LogException(ex);
            }
            return Count;
        }

        public static ActionResult GetResxEntries(int PortalId, UserInfo UserInfo, string Mode, int lid, string resourceFile)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                Enum.TryParse(Mode, false, out LanguageResourceMode resourceMode);

                if (!UserInfo.IsSuperUser && resourceMode == LanguageResourceMode.Host)
                {
                    actionResult.AddError("AuthFailureMessage", Components.Constants.AuthFailureMessage);
                    return actionResult;
                }

                Locale language = LocaleController.Instance.GetLocale(lid);

                switch (resourceMode)
                {
                    case LanguageResourceMode.System:
                    case LanguageResourceMode.Host:
                    case LanguageResourceMode.Portal:
                        {

                            break;
                        }
                    default:
                        {
                            actionResult.AddError("BadRequest", "UnsupportedMode");
                            return actionResult;
                        }
                }

                if (language == null)
                {
                    actionResult.AddError("BadRequest", Localization.GetString("InvalidLocale.ErrorMessage", Locale_ResourceFile));
                    return actionResult;
                }

                selectedResourceFile = !string.IsNullOrEmpty(resourceFile) ? HttpContext.Current.Server.MapPath("~/" + resourceFile) : HttpContext.Current.Server.MapPath(Localization.GlobalResourceFile);


                Hashtable editTable = LoadFile(PortalId, resourceMode, "Edit", language.Code);
                Hashtable defaultTable = LoadFile(PortalId, resourceMode, "Default", language.Code);

                string fullPath = Path.GetFileName(ResourceFile(PortalId, language.Code, resourceMode).Replace(Globals.ApplicationMapPath, ""));
                string folder = ResourcesManager.ResourceFile(PortalId, language.Code, resourceMode).Replace(Globals.ApplicationMapPath, "").Replace("\\" + resourceFile, "");

                // check edit table and if empty, just use default
                if (editTable.Count == 0)
                {
                    editTable = defaultTable;
                }
                else
                {
                    //remove obsolete keys
                    ArrayList toBeDeleted = new ArrayList();
                    foreach (string key in editTable.Keys)
                    {
                        if (!defaultTable.Contains(key))
                        {
                            toBeDeleted.Add(key);
                        }
                    }
                    if (toBeDeleted.Count > 0)
                    {
                        Core.Managers.ExceptionManage.LogException(new DotNetNuke.Services.Exceptions.ModuleLoadException(Localization.GetString("Obsolete", Locale_ResourceFile)));
                        foreach (string key in toBeDeleted)
                        {
                            editTable.Remove(key);
                        }
                    }

                    //add missing keys
                    foreach (string key in defaultTable.Keys)
                    {
                        if (!editTable.Contains(key))
                        {
                            editTable.Add(key, defaultTable[key]);
                        }
                        else
                        {
                            // Update default value
                            Pair p = (Pair)editTable[key];
                            p.Second = ((Pair)defaultTable[key]).First;
                            editTable[key] = p;
                        }
                    }
                }

                actionResult.IsSuccess = true;
                List<LocalizationEntry> ResourceFileds = new List<LocalizationEntry>();
                foreach (string key in new SortedList(editTable).Keys)
                {
                    if (editTable.Contains(key))
                    {
                        ResourceFileds.Add(new LocalizationEntry { ResourceName = key, DefaultValue = ((Pair)editTable[key]).Second.ToString(), LocalizedValue = ((Pair)editTable[key]).First.ToString() });
                    }
                }

                actionResult.Data = ResourceFileds;

                return actionResult;
            }
            catch (Exception ex)
            {
                actionResult.AddError("", "", ex);
                return actionResult;
            }
        }

        public static string ResourceFile(int portalId, string language, LanguageResourceMode mode)
        {
            return Localization.GetResourceFileName(selectedResourceFile, language, mode.ToString(), portalId);
        }

        public static Hashtable LoadFile(int portalId, LanguageResourceMode mode, string type, string locale)
        {
            string file;
            Hashtable ht = new Hashtable();

            if (type == "Edit")
            {
                // Only load resources from the file being edited
                file = ResourceFile(portalId, locale, mode);
                LoadResource(ht, file);
            }
            else if (type == "Default")
            {
                // Load system default
                file = ResourceFile(portalId, Localization.SystemLocale, LanguageResourceMode.System);
                LoadResource(ht, file);

                if (mode == LanguageResourceMode.Host)
                {
                    // Load base file for selected locale
                    file = ResourceFile(portalId, locale, LanguageResourceMode.System);
                    LoadResource(ht, file);
                }
                else if (mode == LanguageResourceMode.Portal)
                {
                    //Load host override for default locale
                    file = ResourceFile(portalId, Localization.SystemLocale, LanguageResourceMode.Host);
                    LoadResource(ht, file);

                    if (locale != Localization.SystemLocale)
                    {
                        // Load base file for locale
                        file = ResourceFile(portalId, locale, LanguageResourceMode.System);
                        LoadResource(ht, file);

                        //Load host override for selected locale
                        file = ResourceFile(portalId, locale, LanguageResourceMode.Host);
                        LoadResource(ht, file);
                    }
                }
            }

            return ht;
        }

        private static void LoadResource(IDictionary ht, string filepath)
        {
            XmlDocument d = new XmlDocument { XmlResolver = null };
            bool xmlLoaded;
            try
            {
                d.Load(filepath);
                xmlLoaded = true;
            }
            catch (Exception)
            {
                Core.Managers.ExceptionManage.LogException(new DotNetNuke.Services.Exceptions.ModuleLoadException(Localization.GetString("Obsolete", Locale_ResourceFile)));
                xmlLoaded = false;
            }
            if (xmlLoaded)
            {
                XmlNodeList nLoopVariables = d.SelectNodes("root/data");
                if (nLoopVariables != null)
                {
                    foreach (XmlNode nLoopVariable in nLoopVariables)
                    {
                        XmlNode n = nLoopVariable;
                        if (n.NodeType != XmlNodeType.Comment)
                        {
                            XmlNode selectSingleNode = n.SelectSingleNode("value");
                            if (selectSingleNode != null)
                            {
                                string val = selectSingleNode.InnerText;
                                if (n.Attributes != null)
                                {
                                    if (ht[n.Attributes["name"].Value] == null)
                                    {
                                        ht.Add(n.Attributes["name"].Value, new Pair(val, val));
                                    }
                                    else
                                    {
                                        ht[n.Attributes["name"].Value] = new Pair(val, val);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static ActionResult SaveResxEntries(int PortalId, UserInfo UserInfo, int lid, UpdateTransaltionsRequest request)
        {
            ActionResult actionResult = new ActionResult();
            try
            {

                Enum.TryParse(request.Mode, false, out LanguageResourceMode resourceMode);

                if (!UserInfo.IsSuperUser && resourceMode == LanguageResourceMode.Host)
                {
                    actionResult.AddError("AuthFailureMessage", Components.Constants.AuthFailureMessage);
                    return actionResult;
                }

                Locale language = LocaleController.Instance.GetLocale(lid);

                switch (resourceMode)
                {
                    case LanguageResourceMode.System:
                    case LanguageResourceMode.Host:
                    case LanguageResourceMode.Portal:
                        break;
                    default:
                        actionResult.AddError("BadRequest", "UnsupportedMode");
                        return actionResult;
                }

                if (language == null)
                {
                    actionResult.AddError("BadRequest", Localization.GetString("InvalidLocale.ErrorMessage", Locale_ResourceFile));
                    return actionResult;
                }
                string resourceFile = request.ResourceFile.ToString();
                if (request.ResourceFile.ToString().IndexOf("_/", StringComparison.Ordinal) == 0)
                {
                    resourceFile = resourceFile.Substring(2);
                }
                if (string.IsNullOrEmpty(resourceFile))
                {
                    actionResult.AddError("BadRequest", string.Format(Localization.GetString("MissingResourceFileName", Locale_ResourceFile), language.Code));
                    return actionResult;
                }

                selectedResourceFile = HttpContext.Current.Server.MapPath("~/" + resourceFile);
                string message = SaveResourceFileFile(PortalId, resourceMode, language.Code, request.Entries);
                actionResult.IsSuccess = true;
                actionResult.Message = message;
                return actionResult;
            }
            catch (Exception ex)
            {
                actionResult.AddError("", "", ex);
                return actionResult;
            }
        }

        public static string SaveResourceFileFile(int portalId, LanguageResourceMode mode, string locale, IEnumerable<Entities.LocalizationEntry> entries)
        {
            XmlDocument resDoc = new XmlDocument { XmlResolver = null };
            XmlDocument defDoc = new XmlDocument { XmlResolver = null };

            string filename = ResourceFile(portalId, locale, mode);
            resDoc.Load(File.Exists(filename)
                ? filename :
                ResourceFile(portalId, Localization.SystemLocale, LanguageResourceMode.System));

            defDoc.Load(ResourceFile(portalId, Localization.SystemLocale, LanguageResourceMode.System));

            //store all changed resources
            Dictionary<string, string> changedResources = new Dictionary<string, string>();

            // only items different from default will be saved
            foreach (LocalizationEntry entry in entries)
            {
                string resourceKey = entry.ResourceName;
                string txtValue = entry.LocalizedValue;

                XmlNode node = resDoc.SelectSingleNode(GetResourceKeyXPath(resourceKey) + "/value");
                switch (mode)
                {
                    case LanguageResourceMode.System:
                        // this will save all items
                        if (node == null)
                        {
                            node = AddResourceKey(resDoc, resourceKey);
                        }
                        node.InnerText = txtValue;
                        if (txtValue != entry.DefaultValue)
                        {
                            changedResources.Add(resourceKey, txtValue);
                        }

                        break;
                    case LanguageResourceMode.Host:
                    case LanguageResourceMode.Portal:
                        // only items different from default will be saved
                        if (txtValue != entry.DefaultValue)
                        {
                            if (node == null)
                            {
                                node = AddResourceKey(resDoc, resourceKey);
                            }
                            node.InnerText = txtValue;
                            changedResources.Add(resourceKey, txtValue);
                        }
                        else
                        {
                            // remove item = default
                            XmlNode parent = node?.ParentNode;
                            if (parent != null)
                            {
                                resDoc.SelectSingleNode("//root")?.RemoveChild(parent);
                            }
                        }
                        break;
                }
            }

            // remove obsolete keys
            XmlNodeList nodeLoopVariables = resDoc.SelectNodes("//root/data");
            if (nodeLoopVariables != null)
            {
                foreach (XmlNode node in nodeLoopVariables)
                {
                    if (node.Attributes != null &&
                        defDoc.SelectSingleNode(GetResourceKeyXPath(node.Attributes["name"].Value)) == null)
                    {
                        node.ParentNode?.RemoveChild(node);
                    }
                }
            }

            // remove duplicate keys
            nodeLoopVariables = resDoc.SelectNodes("//root/data");
            if (nodeLoopVariables != null)
            {
                foreach (XmlNode node in nodeLoopVariables)
                {
                    if (node.Attributes != null)
                    {
                        XmlNodeList xmlNodeList = resDoc.SelectNodes(GetResourceKeyXPath(node.Attributes["name"].Value));
                        if (xmlNodeList != null && xmlNodeList.Count > 1)
                        {
                            node.ParentNode?.RemoveChild(node);
                        }
                    }
                }
            }

            switch (mode)
            {
                case LanguageResourceMode.System:
                    resDoc.Save(filename);
                    break;
                case LanguageResourceMode.Host:
                case LanguageResourceMode.Portal:
                    XmlNodeList xmlNodeList = resDoc.SelectNodes("//root/data");
                    if (xmlNodeList != null && xmlNodeList.Count > 0)
                    {
                        // there's something to save
                        resDoc.Save(filename);
                    }
                    else if (File.Exists(filename))
                    {
                        // nothing to be saved, if file exists delete
                        File.Delete(filename);
                    }
                    break;
            }

            if (changedResources.Count > 0)
            {
                string values = string.Join("; ", changedResources.Select(x => x.Key + "=" + x.Value));
                LogInfo log = new LogInfo { LogTypeKey = EventLogController.EventLogType.ADMIN_ALERT.ToString() };
                log.LogProperties.Add(new LogDetailInfo(Localization.GetString("ResourceUpdated", Locale_ResourceFile), ResourceFile(portalId, locale, mode)));
                log.LogProperties.Add(new LogDetailInfo("Updated Values", values));
                LogController.Instance.AddLog(log);
            }

            return string.Format(Localization.GetString("Updated", Locale_ResourceFile), ResourceFile(portalId, locale, mode));
        }
        private static string GetResourceKeyXPath(string resourceKeyName)
        {
            return "//root/data[@name=" + XmlUtils.XPathLiteral(resourceKeyName) + "]";
        }

        private static XmlNode AddResourceKey(XmlDocument resourceDoc, string resourceKey)
        {
            // missing entry
            XmlNode nodeData = resourceDoc.CreateElement("data");
            XmlAttribute attr = resourceDoc.CreateAttribute("name");
            attr.Value = resourceKey;
            nodeData.Attributes?.Append(attr);
            XmlNode selectSingleNode = resourceDoc.SelectSingleNode("//root");
            selectSingleNode?.AppendChild(nodeData);
            return nodeData.AppendChild(resourceDoc.CreateElement("value"));
        }
    }
}
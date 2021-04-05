using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Portals;
using System.Collections.Generic;
using System.Linq;
using Vanjaro.Common.Components;
using Vanjaro.Common.Data.Entities;
using Vanjaro.Common.Engines.UIEngine;

namespace Vanjaro.Common.Factories
{
    internal class SettingFactory
    {
        #region Private Members
        private readonly int ModuleID;
        private readonly string Identifier;
        #endregion

        #region Constructor
        public SettingFactory(int ModuleID, string Identifier)
        {
            this.ModuleID = ModuleID;
            this.Identifier = Identifier;
        }
        #endregion

        #region Internal Methods
        internal List<IUIData> GetAll()
        {
            return GetAll(ModuleID, Identifier);
        }
        internal Dictionary<string, IUIData> GetDictionary()
        {
            return GetDictionary(ModuleID, Identifier);
        }
        internal string GetValue(string Name)
        {
            return GetValue(ModuleID, Identifier, Name);
        }
        internal void Update(string Name, string Value)
        {
            UpdateValue(ModuleID, Identifier, Name, Value);
        }
        #endregion

        #region Private Static Methods

        private static void ApplyDefaults(int ModuleID, string Identifier, List<Setting> Defaults, List<Setting> Settings)
        {
            if (Defaults == null)
            {
                Engines.UIEngine.AngularBootstrap.AngularView View = AppFactory.GetViews().Where(t => t.Identifier == Identifier).FirstOrDefault();

                if (View != null && View.Defaults.Count > 0)
                {
                    Defaults = new List<Setting>();

                    foreach (string key in View.Defaults.Keys)
                    {
                        Defaults.Add(new Setting(key, View.Defaults[key]));
                    }
                }
            }

            if (Defaults != null && Settings != null)
            {
                foreach (Setting d in Defaults)
                {
                    Setting s = Settings.Where(i => i.Name == d.Name).SingleOrDefault();

                    if (s == null)
                    {
                        //Do Not Track Changes if explictly asked to do so
                        if (!d.DoNotTrackChanges)
                        {
                            d.IsNew = true;
                        }

                        Settings.Add(d);
                    }
                }

                //Apply ModuleID & Identifier
                Settings.ForEach(s => { s.Identifier = Identifier; s.ModuleID = ModuleID; });
            }
        }
        #endregion

        #region Internal Static Methods
        internal static List<Setting> GetAll(int ModuleID)
        {
            List<Setting> data = DotNetNuke.Common.Utilities.DataCache.GetCache(Constants.CachPrefix + ModuleID.ToString()) as List<Setting>;

            if (data == null)
            {
                data = Setting.Fetch("WHERE ModuleID=@0", ModuleID);
                DotNetNuke.Common.Utilities.DataCache.SetCache(Constants.CachPrefix + ModuleID.ToString(), data);
            }

            return data;
        }
        internal static List<Setting> GetSettings(int ModuleID, string Identifier)
        {
            List<Setting> data = DotNetNuke.Common.Utilities.DataCache.GetCache(Constants.CachPrefix + ModuleID.ToString() + Identifier) as List<Setting>;

            if (data == null)
            {
                data = Setting.Fetch("WHERE ModuleID=@0 AND Identifier =@1", ModuleID, Identifier);
                DotNetNuke.Common.Utilities.DataCache.SetCache(Constants.CachPrefix + ModuleID.ToString() + Identifier, data);
            }

            return data;
        }

        public static Setting GetSetting(int ModuleID, string Identifier, string Name)
        {
            return GetSettings(ModuleID, Identifier).Where(s => s.Name == Name).SingleOrDefault();
        }
        internal static Dictionary<string, IUIData> GetDictionary(int ModuleID, string Identifier)
        {
            return GetAll(ModuleID, Identifier).ToDictionary(x => x.Name, x => x);
        }
        internal static List<IUIData> GetAll(int ModuleID, string Identifier)
        {
            List<Setting> Settings = GetSettings(ModuleID, Identifier);

            //Apply Defaults
            ApplyDefaults(ModuleID, Identifier, null, Settings);

            return Settings.Cast<IUIData>().ToList();
        }

        internal static List<Setting> GetAllSettingsByModuleID(int ModuleID)
        {
            return Setting.Fetch("WHERE ModuleID=@0 ", ModuleID);
        }

        internal static void Update(List<Setting> Settings)
        {
            foreach (Setting s in Settings)
            {
                if (s.Value == null)
                {
                    return;
                }

                if (!s.DoNotTrackChanges)
                {
                    if (s.IsNew)
                    {
                        s.Insert();
                    }
                    else if (s.IsChanged)
                    {
                        s.Update();

                        //Check if Update Fails due to no previous insert... handle insert
                    }
                    else if (s.IsDeleted)
                    {
                        s.Delete();
                    }
                }
            }

            CacheFactory.ClearCache();
        }
        internal static void UpdateValue(int ModuleID, string Identifier, string Name, string Value)
        {
            Setting s = GetSetting(ModuleID, Identifier, Name);

            if (s == null)
            {
                s = new Setting
                {
                    ModuleID = ModuleID,
                    Identifier = Identifier,
                    Name = Name,
                    Value = Value
                };

                s.Insert();
            }
            else
            {
                s.Value = Value;
                s.Update();
            }

            CacheFactory.ClearCache();
        }

        internal static string GetValue(int ModuleID, string Identifier, string Name)
        {
            Setting s = GetSetting(ModuleID, Identifier, Name);
            if (s != null)
            {
                return s.Value;
            }

            //Check Default Value
            Engines.UIEngine.AngularBootstrap.AngularView View = AppFactory.GetViews().Where(t => t.Identifier == Identifier).FirstOrDefault();
            KeyValuePair<string, string> Default = View.Defaults.Where(d => d.Key == Name).FirstOrDefault();
            if (Default.Key != null)
            {
                return Default.Value;
            }

            return null;
        }
        internal static List<Setting> GetSettingsByName(string Name)
        {
            List<Setting> setting = Setting.Fetch("WHERE Name=@0", Name);
            return setting;
        }

        internal static IEnumerable<int> GetDistinctModuleIds(string Identifier)
        {
            return Setting.Query("WHERE Identifier=@0", Identifier).Select(s => s.ModuleID).Distinct();
        }

        public static string GetHostSetting(string Name, bool Secure, string defaultValue = "")
        {
            HostController hostController = new HostController();
            if (Secure)
                return hostController.GetEncryptedString(Name, Config.GetDecryptionkey());
            return hostController.GetString(Name, defaultValue);
        }

        public static string GetPortalSetting(string Name, int PortalId, bool Secure, string defaultValue = "")
        {
            if (Secure)
                return PortalController.GetEncryptedString(Name, PortalId, Config.GetDecryptionkey());
            return PortalController.GetPortalSetting(Name, PortalId, defaultValue);
        }
        #endregion
    }
}
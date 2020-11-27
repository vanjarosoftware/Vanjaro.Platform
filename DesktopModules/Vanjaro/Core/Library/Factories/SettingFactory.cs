using System.Collections.Generic;
using System.Linq;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.Common.Engines.UIEngine.AngularBootstrap;
using Vanjaro.Core.Data.Entities;

namespace Vanjaro.Core
{
    public static partial class Factories
    {
        public static class SettingFactory
        {
            internal static Dictionary<string, IUIData> GetDictionary(int PortalID, int TabID, string Identifier, List<AngularView> Views)
            {
                return GetAll(PortalID, TabID, Identifier, Views).ToDictionary(x => x.Name, x => x);
            }

            internal static List<IUIData> GetAll(int PortalID, int TabID, string Identifier, List<AngularView> Views)
            {
                List<Setting> Settings = GetSettings(PortalID, TabID, Identifier);

                //Apply Defaults
                ApplyDefaults(PortalID, TabID, Identifier, null, Settings, Views);

                return Settings.Cast<IUIData>().ToList();
            }

            internal static List<Setting> GetSettings(int PortalID, int TabID, string Identifier)
            {
                string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.Settings, Identifier, PortalID, TabID);
                List<Setting> data = CacheFactory.Get(CacheKey);
                if (data == null)
                {
                    data = Setting.Fetch("WHERE PortalID=@0 and TabID=@1 AND Identifier=@2", PortalID, TabID, Identifier);
                    CacheFactory.Set(CacheKey, data);
                }
                return data;
            }

            private static void ApplyDefaults(int PortalID, int TabID, string Identifier, List<Setting> Defaults, List<Setting> Settings, List<AngularView> Views)
            {
                if (Defaults == null)
                {
                    AngularView View = Views.Where(t => t.Identifier == Identifier).FirstOrDefault();

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

                    //Apply TabID & Identifier
                    Settings.ForEach(s => { s.Identifier = Identifier; s.TabID = TabID; });
                }
            }

            internal static void UpdateValue(int PortalID, int TabID, string Identifier, string Name, string Value)
            {
                CacheFactory.Clear(CacheFactory.Keys.Settings);
                Setting s = GetSetting(PortalID, TabID, Identifier, Name);
                if (s == null)
                {
                    s = new Setting
                    {
                        TabID = TabID,
                        Identifier = Identifier,
                        Name = Name,
                        PortalID = PortalID,
                        Value = Value
                    };

                    s.Insert();
                }
                else
                {
                    s.Value = Value;
                    s.Update();
                }
                CacheFactory.Clear(CacheFactory.Keys.Settings);
            }

            public static Setting GetSetting(int PortalID, int TabID, string Identifier, string Name)
            {
                return GetSettings(PortalID, TabID, Identifier).Where(s => s.Name == Name).SingleOrDefault();
            }

            public static string GetValue(int PortalID, int TabID, string Identifier, string Name, List<AngularView> Views)
            {
                Setting s = GetSetting(PortalID, TabID, Identifier, Name);
                if (s != null)
                {
                    return s.Value;
                }

                //Check Default Value
                if (Views != null)
                {
                    AngularView View = Views.Where(t => t.Identifier == Identifier).FirstOrDefault();
                    KeyValuePair<string, string> Default = View.Defaults.Where(d => d.Key == Name).FirstOrDefault();
                    if (Default.Key != null)
                    {
                        return Default.Value;
                    }
                }
                return null;
            }
        }
    }
}
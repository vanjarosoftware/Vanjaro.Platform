using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Vanjaro.UXManager.Library.Entities.Interface;

namespace Vanjaro.UXManager.Library
{
    public static partial class Factories
    {
        public class MenuFactory
        {
            private static List<IMenuItem> Extentions
            {
                get
                {
                    string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.Menu_Extension);
                    List<IMenuItem> MenuItem = CacheFactory.Get(CacheKey);
                    if (MenuItem == null)
                    {
                        List<IMenuItem> ServiceInterfaceAssemblies = new List<IMenuItem>();
                        string[] binAssemblies = Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin")).Where(c => c.EndsWith(".dll")).ToArray();
                        foreach (string Path in binAssemblies)
                        {
                            try
                            {
                                //get all assemblies 
                                IEnumerable<IMenuItem> AssembliesToAdd = from t in System.Reflection.Assembly.LoadFrom(Path).GetTypes()
                                                                         where t != (typeof(IMenuItem)) && (typeof(IMenuItem).IsAssignableFrom(t))
                                                                         select Activator.CreateInstance(t) as IMenuItem;

                                ServiceInterfaceAssemblies.AddRange(AssembliesToAdd.ToList<IMenuItem>());
                            }
                            catch { continue; }
                        }
                        MenuItem = ServiceInterfaceAssemblies;
                        CacheFactory.Set(CacheKey, ServiceInterfaceAssemblies);
                    }
                    return MenuItem;

                }
            }

            internal static List<IMenuItem> GetExtentions(bool CheckVisibilityPermisstion)
            {
                if (CheckVisibilityPermisstion)
                {
                    return Extentions.Where(x => x.Visibility).ToList();
                }
                else
                {
                    return Extentions;
                }
            }
        }
    }
}
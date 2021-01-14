using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Vanjaro.Core.Entities.Interface;
using static Vanjaro.Core.Factories;

namespace Vanjaro.Core
{
    public static partial class Managers
    {
        public class ExtensionManager
        {
            public static List<ICoreExtension> Extentions
            {
                get
                {
                    string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.Extensions);
                    List<ICoreExtension> items = CacheFactory.Get(CacheKey);
                    if (items == null)
                    {
                        List<ICoreExtension> ServiceInterfaceAssemblies = new List<ICoreExtension>();
                        string[] binAssemblies = Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin")).Where(c => c.EndsWith(".dll") && c.Contains("Vanjaro.Core.Extensions")).ToArray();
                        foreach (string Path in binAssemblies)
                        {
                            try
                            {
                                //get all assemblies 
                                IEnumerable<ICoreExtension> AssembliesToAdd = from t in System.Reflection.Assembly.LoadFrom(Path).GetTypes()
                                                                              where t != (typeof(ICoreExtension)) && (typeof(ICoreExtension).IsAssignableFrom(t))
                                                                              select Activator.CreateInstance(t) as ICoreExtension;

                                ServiceInterfaceAssemblies.AddRange(AssembliesToAdd.ToList<ICoreExtension>());
                            }
                            catch { continue; }
                        }
                        items = ServiceInterfaceAssemblies;
                        CacheFactory.Set(CacheKey, ServiceInterfaceAssemblies);
                    }
                    return items;
                }
            }

            public static bool IsAllowVjEditor()
            {
                return HttpContext.Current.Request.Cookies["IsVjEditor"] != null && !string.IsNullOrEmpty(HttpContext.Current.Request.Cookies["IsVjEditor"].Value);
            }
        }
    }
}
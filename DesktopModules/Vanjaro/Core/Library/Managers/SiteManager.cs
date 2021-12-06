using System;
using System.IO;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using Vanjaro.Core.Entities.Interface;
using static Vanjaro.Core.Factories;

namespace Vanjaro.Core
{
    public static partial class Managers
    {
        public class SiteManager
        {
            public static List<IPortalDelete> GetIPortalDelete()
            {
                return IPortalDeletes.ToList();
            }

            internal static List<IPortalDelete> IPortalDeletes
            {
                get
                {
                    string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.IPortalDelete);
                    List<IPortalDelete> items = CacheFactory.Get(CacheKey);
                    if (items == null)
                    {
                        List<IPortalDelete> ServiceInterfaceAssemblies = new List<IPortalDelete>();
                        string[] binAssemblies = Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin")).Where(c => c.EndsWith(".dll")).ToArray();
                        foreach (string path in binAssemblies)
                        {
                            try
                            {
                                //get all assemblies 
                                IEnumerable<IPortalDelete> AssembliesToAdd = from t in System.Reflection.Assembly.LoadFrom(path).GetTypes()
                                                                             where t.GetInterfaces().Contains(typeof(IPortalDelete))
                                                                             select Activator.CreateInstance(t) as IPortalDelete;

                                ServiceInterfaceAssemblies.AddRange(AssembliesToAdd.ToList<IPortalDelete>());
                            }
                            catch { continue; }
                        }
                        items = ServiceInterfaceAssemblies;
                        CacheFactory.Set(CacheKey, ServiceInterfaceAssemblies);
                    }
                    return items;
                }
            }
        }
    }
}
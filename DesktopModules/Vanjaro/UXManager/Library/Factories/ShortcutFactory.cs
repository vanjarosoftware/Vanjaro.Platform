using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Vanjaro.UXManager.Library.Entities.Interface;

namespace Vanjaro.UXManager.Library
{
    public static partial class Factories
    {
        public class ShortcutFactory
        {
            internal static List<IShortcut> GetShortcut()
            {
                string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.Shortcut);
                List<IShortcut> NotificationTask = CacheFactory.Get(CacheKey);
                if (NotificationTask == null)
                {
                    List<IShortcut> ServiceInterfaceAssemblies = new List<IShortcut>();
                    string[] binAssemblies = Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin")).Where(c => c.EndsWith(".dll") && c.Contains("Vanjaro.UXManager.Extensions")).ToArray();
                    foreach (string Path in binAssemblies)
                    {
                        try
                        {
                            //get all assemblies 
                            IEnumerable<IShortcut> AssembliesToAdd = from t in System.Reflection.Assembly.LoadFrom(Path).GetTypes()
                                                                     where t != (typeof(IShortcut)) && (typeof(IShortcut).IsAssignableFrom(t))
                                                                     select Activator.CreateInstance(t) as IShortcut;

                            ServiceInterfaceAssemblies.AddRange(AssembliesToAdd.ToList<IShortcut>());
                        }
                        catch { continue; }
                    }
                    NotificationTask = ServiceInterfaceAssemblies;
                    CacheFactory.Set(CacheKey, ServiceInterfaceAssemblies);
                }
                return NotificationTask.OrderBy(o => o.Shortcut.ViewOrder).ToList();
            }
        }
    }
}

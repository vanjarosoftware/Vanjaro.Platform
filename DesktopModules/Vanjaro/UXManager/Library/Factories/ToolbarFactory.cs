using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Vanjaro.UXManager.Library.Entities.Interface;

namespace Vanjaro.UXManager.Library
{
    public static partial class Factories
    {
        public class ToolbarFactory
        {
            //Sync Extensions Methods            
            internal static List<IToolbarItem> Extentions
            {
                get
                {
                    string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.Toolbar_Extension);
                    List<IToolbarItem> toolbarItem = CacheFactory.Get(CacheKey);
                    if (toolbarItem == null)
                    {
                        List<IToolbarItem> ServiceInterfaceAssemblies = new List<IToolbarItem>();
                        string[] binAssemblies = Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin")).Where(c => c.EndsWith(".dll") && c.Contains("Vanjaro.UXManager.Extensions.Toolbar")).ToArray();
                        foreach (string Path in binAssemblies)
                        {
                            try
                            {
                                //get all assemblies 
                                IEnumerable<IToolbarItem> AssembliesToAdd = from t in System.Reflection.Assembly.LoadFrom(Path).GetTypes()
                                                                            where t != (typeof(IToolbarItem)) && (typeof(IToolbarItem).IsAssignableFrom(t))
                                                                            select Activator.CreateInstance(t) as IToolbarItem;

                                ServiceInterfaceAssemblies.AddRange(AssembliesToAdd.ToList<IToolbarItem>());
                            }
                            catch { continue; }
                        }
                        toolbarItem = ServiceInterfaceAssemblies;
                        CacheFactory.Set(CacheKey, ServiceInterfaceAssemblies);
                    }
                    return toolbarItem;
                }
            }
        }
    }
}
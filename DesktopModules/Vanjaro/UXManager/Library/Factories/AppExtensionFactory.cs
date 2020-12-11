using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Permissions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Vanjaro.UXManager.Library.Entities;
using Vanjaro.UXManager.Library.Entities.Interface;

namespace Vanjaro.UXManager.Library
{
    public static partial class Factories    {
        public class AppExtensionFactory
        {
            internal static List<IAppExtension> Extentions            {                get                {
                    string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.App_Extension);                    List<IAppExtension> appItems = CacheFactory.Get(CacheKey);                    if (appItems == null)                    {                        List<IAppExtension> ServiceInterfaceAssemblies = new List<IAppExtension>();                        string[] binAssemblies = Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin")).Where(c => c.EndsWith(".dll") && c.Contains("Vanjaro.UXManager.Extensions.Apps")).ToArray();                        foreach (string Path in binAssemblies)                        {                            try                            {
                                //get all assemblies 
                                IEnumerable<IAppExtension> AssembliesToAdd = from t in System.Reflection.Assembly.LoadFrom(Path).GetTypes()
                                                                             where t != (typeof(IAppExtension)) && (typeof(IAppExtension).IsAssignableFrom(t))
                                                                             select Activator.CreateInstance(t) as IAppExtension;                                ServiceInterfaceAssemblies.AddRange(AssembliesToAdd.ToList<IAppExtension>());                            }                            catch { continue; }                        }                        appItems = ServiceInterfaceAssemblies;                        CacheFactory.Set(CacheKey, ServiceInterfaceAssemblies);                    }                    return appItems;                }            }

            internal static List<IAppExtension> ModuleExtentions            {                get                {
                    string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.App_Extension, "Module");                    List<IAppExtension> appItems = CacheFactory.Get(CacheKey);                    if (appItems == null)                    {                        List<IAppExtension> ServiceInterfaceAssemblies = new List<IAppExtension>();                        string[] binAssemblies = Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin")).Where(c => c.EndsWith(".dll") && c.Contains("Vanjaro.UXManager.Extensions.Apps")).ToArray();                        foreach (string Path in binAssemblies)                        {                            try                            {
                                //get all assemblies 
                                IEnumerable<IAppExtension> AssembliesToAdd = from t in System.Reflection.Assembly.LoadFrom(Path).GetTypes()
                                                                             where t != (typeof(IAppExtension)) && (typeof(IAppExtension).IsAssignableFrom(t))
                                                                             && t != (typeof(IModuleExtension)) && (typeof(IModuleExtension).IsAssignableFrom(t))
                                                                             select Activator.CreateInstance(t) as IAppExtension;                                ServiceInterfaceAssemblies.AddRange(AssembliesToAdd.ToList<IAppExtension>());                            }                            catch { continue; }                        }                        appItems = ServiceInterfaceAssemblies;                        CacheFactory.Set(CacheKey, ServiceInterfaceAssemblies);                    }                    return appItems;                }            }

            internal static string GetAccessRoles(UserInfo UserInfo)
            {
                List<string> AccessRoles = new List<string>();
                if (TabPermissionController.HasTabPermission("EDIT") || !Editor.Options.EditPage)
                {
                    AccessRoles.Add("pageedit");
                }
                return string.Join(",", AccessRoles.Distinct());
            }
        }
    }
}
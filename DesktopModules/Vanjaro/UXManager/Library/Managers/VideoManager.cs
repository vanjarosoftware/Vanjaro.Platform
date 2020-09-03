using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Vanjaro.Core.Entities.Interface;
using Vanjaro.UXManager.Library.Entities;
using static Vanjaro.UXManager.Library.Factories;

namespace Vanjaro.UXManager.Library
{
    public static partial class Managers
    {
        public class VideoManager
        {
            public static List<VideoEntity> GetVideoProviders(bool IsSupportBackground)
            {
                List<VideoEntity> result = new List<VideoEntity>();
                foreach (IVideoProvider provider in GetAvailableProviders())
                {
                    if (provider.IsSupportBackground && !IsSupportBackground)
                    {
                        result.Add(new VideoEntity() { Text = provider.Name, Value = provider.Name, Logo = provider.Logo, Link = provider.Link, ShowLogo = provider.ShowLogo });
                    }
                    else if (IsSupportBackground)
                    {
                        result.Add(new VideoEntity() { Text = provider.Name, Value = provider.Name, Logo = provider.Logo, Link = provider.Link, ShowLogo = provider.ShowLogo });
                    }
                }

                return result;
            }

            public static async Task<string> GetVideos(string Source, string Keyword, int PageNo, int PageSize, Dictionary<string, object> AdditionalData)
            {
                string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.Videos, Source, Keyword, PageNo, PageSize);
                string Videos = CacheFactory.Get(CacheKey);
                if (Videos == null)
                {
                    IVideoProvider provider = GetAvailableProviders().Where(p => p.Name == Source).FirstOrDefault();
                    if (provider == null)
                    {
                        return Videos;
                    }

                    Videos = await provider.GetVideos(Keyword, PageNo, PageSize, AdditionalData);
                    CacheFactory.Set(CacheKey, Videos);
                }
                return Videos;
            }

            private static List<IVideoProvider> GetAvailableProviders()
            {
                string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.VideoProvider);
                List<IVideoProvider> Items = CacheFactory.Get(CacheKey);
                if (Items == null)
                {
                    List<IVideoProvider> ServiceInterfaceAssemblies = new List<IVideoProvider>();
                    string[] binAssemblies = Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin")).Where(c => c.EndsWith(".dll") && c.Contains("Vanjaro")).ToArray();
                    foreach (string Path in binAssemblies)
                    {
                        try
                        {
                            //get all assemblies 
                            IEnumerable<IVideoProvider> AssembliesToAdd = from t in System.Reflection.Assembly.LoadFrom(Path).GetTypes()
                                                                          where t != (typeof(IVideoProvider)) && (typeof(IVideoProvider).IsAssignableFrom(t))
                                                                          select Activator.CreateInstance(t) as IVideoProvider;

                            ServiceInterfaceAssemblies.AddRange(AssembliesToAdd.ToList<IVideoProvider>());
                        }
                        catch { continue; }
                    }
                    Items = ServiceInterfaceAssemblies;
                    CacheFactory.Set(CacheKey, ServiceInterfaceAssemblies);
                }
                return Items.Where(x => x.Available).ToList();
            }
        }
    }
}
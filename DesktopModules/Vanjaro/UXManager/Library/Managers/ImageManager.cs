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
        public class ImageManager
        {
            public static List<ImageEntity> GetImageProviders()
            {
                List<ImageEntity> result = new List<ImageEntity>();
                foreach (IImageProvider provider in GetAvailableProviders())
                {
                    result.Add(new ImageEntity() { Text = provider.Name, Value = provider.Name, Logo = provider.Logo, Link = provider.Link, ShowLogo = provider.ShowLogo });
                }

                return result;
            }

            public static async Task<string> GetImages(string Source, string Keyword, int PageNo, int PageSize)
            {
                string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.Images, Source, Keyword, PageNo, PageSize);
                string Images = CacheFactory.Get(CacheKey);
                if (Images == null)
                {
                    IImageProvider provider = GetAvailableProviders().Where(p => p.Name == Source).FirstOrDefault();
                    if (provider == null)
                    {
                        return Images;
                    }

                    Images = await provider.GetImages(Keyword, PageNo, PageSize);
                    CacheFactory.Set(CacheKey, Images);
                }
                return Images;
            }

            private static List<IImageProvider> GetAvailableProviders()
            {
                string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.ImageProvider);
                List<IImageProvider> Items = CacheFactory.Get(CacheKey);
                if (Items == null)
                {
                    List<IImageProvider> ServiceInterfaceAssemblies = new List<IImageProvider>();
                    string[] binAssemblies = Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin")).Where(c => c.EndsWith(".dll") && c.Contains("Vanjaro")).ToArray();
                    foreach (string Path in binAssemblies)
                    {
                        try
                        {
                            //get all assemblies 
                            IEnumerable<IImageProvider> AssembliesToAdd = from t in System.Reflection.Assembly.LoadFrom(Path).GetTypes()
                                                                          where t != (typeof(IImageProvider)) && (typeof(IImageProvider).IsAssignableFrom(t))
                                                                          select Activator.CreateInstance(t) as IImageProvider;

                            ServiceInterfaceAssemblies.AddRange(AssembliesToAdd.ToList<IImageProvider>());
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
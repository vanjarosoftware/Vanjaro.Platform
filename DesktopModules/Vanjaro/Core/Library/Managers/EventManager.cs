using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Vanjaro.Core.Entities.Events;
using static Vanjaro.Core.Factories;

namespace Vanjaro.Core
{
    public static partial class Managers
    {
        public class EventManager
        {
            public static void TriggerEvent(VanjaroEventArgs Event, params object[] DataObject)
            {
                foreach (IVanjaroEvent i in VanjaroEvents)
                {
                    i.onAction(Event, DataObject);
                }
            }


            static List<IVanjaroEvent> VanjaroEvents
            {
                get
                {
                    string CacheKey = CacheFactory.GetCacheKey(CacheFactory.Keys.IVanjaroEvent);
                    List<IVanjaroEvent> items = CacheFactory.Get(CacheKey);
                    if (items == null)
                    {
                        List<IVanjaroEvent> ServiceInterfaceAssemblies = new List<IVanjaroEvent>();
                        string[] binAssemblies = Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin")).Where(c => c.EndsWith(".dll") && c.Contains("Vanjaro")).ToArray();
                        foreach (string Path in binAssemblies)
                        {
                            try
                            {
                                //get all assemblies 
                                IEnumerable<IVanjaroEvent> AssembliesToAdd = from t in System.Reflection.Assembly.LoadFrom(Path).GetTypes()
                                                                              where t != (typeof(IVanjaroEvent)) && (typeof(IVanjaroEvent).IsAssignableFrom(t))
                                                                              select Activator.CreateInstance(t) as IVanjaroEvent;

                                ServiceInterfaceAssemblies.AddRange(AssembliesToAdd.ToList<IVanjaroEvent>());
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
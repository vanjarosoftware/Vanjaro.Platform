using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Vanjaro.Common.Utilities
{
    public static class JObjectExtensions
    {
        public static IDictionary<string, object> ToDictionary(this JObject @object)
        {
            Dictionary<string, object> result = @object.ToObject<Dictionary<string, object>>();

            List<string> JObjectKeys = (from r in result
                                        let key = r.Key
                                        let value = r.Value
                                        where value != null && value.GetType() == typeof(JObject)
                                        select key).ToList();

            List<string> JArrayKeys = (from r in result
                                       let key = r.Key
                                       let value = r.Value
                                       where value != null && value.GetType() == typeof(JArray)
                                       select key).ToList();

            foreach (string key in JArrayKeys)
            {
                int counter = 0;
                foreach (JToken obj in ((JArray)result[key]))
                {
                    string Prefix = counter > 0 ? counter.ToString() + "." : string.Empty;

                    if (obj.GetType() == typeof(JObject))
                    {
                        foreach (KeyValuePair<string, object> pair in ToDictionary(obj as JObject))
                        {
                            result.Add(key + "." + Prefix + pair.Key, pair.Value);
                        }
                    }

                    counter++;
                }

                result.Remove(key);
            }

            foreach (string key in JObjectKeys)
            {
                foreach (KeyValuePair<string, object> pair in ToDictionary(result[key] as JObject))
                {
                    result.Add(key + "." + pair.Key, pair.Value);
                }

                result.Remove(key);
            }


            return result;
        }
    }
}
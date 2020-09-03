using Newtonsoft.Json;
using System.Collections.Generic;

namespace Vanjaro.Common.Engines.UIEngine
{
    public class UIDataHelper
    {
        public static void Convert(List<IUIData> UIDataObjects, dynamic UIDataDynamic)
        {
            if (UIDataDynamic == null)
            {
                return;
            }

            if (UIDataObjects == null || UIDataObjects.Count == 0)
            {
                return;
            }

            try
            {

                foreach (IUIData existingData in UIDataObjects)
                {
                    try
                    {
                        if (!existingData.DoNotTrackChanges)
                        {
                            dynamic newData = UIDataDynamic[existingData.Name];

                            if (newData != null)
                            {
                                string ValueType = newData.Value.Type.ToString().ToLower();

                                string newValue = null;

                                switch (ValueType)
                                {
                                    case "string":
                                        {

                                            newValue = newData.Value.Value;
                                            break;
                                        }
                                    case "array":
                                        {
                                            newValue = newData.Value.ToString();
                                            break;
                                        }
                                    case "boolean":
                                        {
                                            newValue = newData.Value.ToString().ToLower();
                                            break;
                                        }
                                    default:
                                        newValue = newData.Value.ToString().ToLower();
                                        break;
                                }


                                if (existingData.Options != null && newData.Options != null)
                                {
                                    try
                                    {
                                        existingData.Options = JsonConvert.DeserializeObject(newData.Options.ToString(), existingData.Options.GetType());
                                    }
                                    catch { }
                                }

                                if (existingData.Value == null)
                                {
                                    existingData.IsNew = true;
                                }

                                if (existingData.Value != newValue)
                                {
                                    existingData.IsChanged = true;
                                }

                                existingData.Value = newValue;


                            }
                            else
                            {
                                existingData.IsDeleted = true;
                            }
                        }
                    }
                    catch { }
                }

            }
            catch { }
        }

    }
}
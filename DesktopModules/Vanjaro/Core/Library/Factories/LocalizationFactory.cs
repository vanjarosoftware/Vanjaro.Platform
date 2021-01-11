using DotNetNuke.Services.Exceptions;
using System;
using System.Collections.Generic;
using Vanjaro.Core.Data.Entities;
using Vanjaro.Core.Data.PetaPoco;

namespace Vanjaro.Core
{
    public static partial class Factories
    {
        internal static class LocalizationFactory
        {
            internal static void AddUpdateProperty(List<Localization> Localizations)
            {
                VanjaroRepo db = VanjaroRepo.GetInstance();
                using (Transaction scope = db.GetTransaction())
                {
                    try
                    {
                        foreach (Localization localization in Localizations)
                        {
                            if (localization.LocalizationID > 0)
                            {
                                localization.Update();
                            }
                            else
                            {
                                localization.Insert();
                            }
                        }
                        scope.Complete();
                    }
                    catch (Exception ex)
                    {
                        Managers.ExceptionManage.LogException(ex);
                    }
                }
            }
            internal static void RemoveProperty(string EntityName, int EntityID)
            {
                Localization.Delete("WHERE EntityName=@0 AND EntityID=@1", EntityName, EntityID);
            }
            internal static void RemoveProperty(string Language, string EntityName, int EntityID)
            {
                Localization.Delete("WHERE Language=@0 AND EntityName=@1 AND EntityID=@2", Language, EntityName, EntityID);
            }
            internal static List<Localization> GetLocaleProperties(string EntityName)
            {
                return Localization.Fetch("WHERE EntityName=@0", EntityName);
            }
            internal static List<Localization> GetLocaleProperties(string Entity, int EntityID)
            {
                return GetLocaleProperties(null, Entity, EntityID);
            }
            internal static List<Localization> GetLocaleProperties(string Language, string EntityName, int EntityID)
            {
                List<int> EntityIDs = new List<int>
                {
                    EntityID
                };
                return GetLocaleProperties(Language, EntityName, EntityIDs, null);
            }
            internal static List<Localization> GetLocaleProperties(string Language, string EntityName, int EntityID, string PropertyName)
            {
                List<int> EntityIDs = new List<int>
                {
                    EntityID
                };
                return GetLocaleProperties(Language, EntityName, EntityIDs, PropertyName);
            }
            internal static List<Localization> GetLocaleProperties(string Language, string EntityName, List<int> EntityIDs, string PropertyName)
            {
                Sql query = Sql.Builder.Append("WHERE EntityID IN (@EntityIDs)", new { EntityIDs });

                if (!string.IsNullOrEmpty(Language))
                {
                    query.Append("AND Language=@0", Language);
                }

                if (!string.IsNullOrEmpty(EntityName))
                {
                    query.Append("AND EntityName=@0", EntityName);
                }

                if (!string.IsNullOrEmpty(PropertyName))
                {
                    query.Append("AND Name=@0", PropertyName);
                }

                List<Localization> Properties = Localization.Fetch(query);

                return Properties;
            }
        }
        public class Locales
        {
            public string Text { get; set; }
            public string Value { get; set; }
        }
    }
}

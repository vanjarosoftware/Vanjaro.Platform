using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Scheduling;
using DotNetNuke.Services.Search;
using DotNetNuke.Services.Search.Entities;
using DotNetNuke.Services.Search.Internals;
using System;
using System.Collections.Generic;
using System.Linq;
using Vanjaro.Core.Data.Entities;

namespace Vanjaro.Core.Services.Search
{
    /// -----------------------------------------------------------------------------
    /// Namespace:  DotNetNuke.Services.Search
    /// Project:    DotNetNuke.Search.Index
    /// Class:      TabIndexer
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The TabIndexer is an implementation of the abstract IndexingProvider
    /// class
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class TabIndexer : IndexingProviderBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(TabIndexer));
        private static readonly int TabSearchTypeId = SearchHelper.Instance.GetSearchTypeByName("tab").SearchTypeId;

        internal const string TabMetaDataPrefixTag = "tabMetaData_";

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Returns the number of SearchDocuments indexed with Tab MetaData for the given portal.
        /// </summary>
        /// <remarks>This replaces "GetSearchIndexItems" as a newer implementation of search.</remarks>
        /// -----------------------------------------------------------------------------
        public override int IndexSearchDocuments(int portalId, ScheduleHistoryItem schedule, DateTime startDateLocal, Action<IEnumerable<SearchDocument>> indexer)
        {
            Requires.NotNull("indexer", indexer);
            const int saveThreshold = 1024;
            int totalIndexed = 0;
            startDateLocal = GetLocalTimeOfLastIndexedItem(portalId, schedule.ScheduleID, startDateLocal);
            List<SearchDocument> searchDocuments = new List<SearchDocument>();
            TabInfo[] tabs = (
                from t in TabController.Instance.GetTabsByPortal(portalId).AsList()
                where t.LastModifiedOnDate > startDateLocal && (t.TabSettings["AllowIndex"] == null ||
                                                                "true".Equals(t.TabSettings["AllowIndex"].ToString(),
                                                                    StringComparison.CurrentCultureIgnoreCase))
                select t).OrderBy(t => t.LastModifiedOnDate).ThenBy(t => t.TabID).ToArray();

            if (tabs.Any())
            {
                foreach (TabInfo tab in tabs)
                {
                    try
                    {
                        foreach (Pages page in Managers.PageManager.GetLatestLocaleVersion(tab.TabID))
                        {
                            SearchDocument searchDoc = GetTabSearchDocument(page, tab);
                            searchDocuments.Add(searchDoc);
                        }

                        if (searchDocuments.Count >= saveThreshold)
                        {
                            totalIndexed += IndexCollectedDocs(indexer, searchDocuments, portalId, schedule.ScheduleID);
                        }
                    }
                    catch (Exception ex)
                    {
                        Exceptions.LogException(ex);
                    }
                }

                if (searchDocuments.Count > 0)
                {
                    totalIndexed += IndexCollectedDocs(indexer, searchDocuments, portalId, schedule.ScheduleID);
                }
            }

            return totalIndexed;
        }

        private static SearchDocument GetTabSearchDocument(Data.Entities.Pages tab, TabInfo tabInfo)
        {
            List<Localization> Localization = Managers.LocalizationManager.GetLocaleProperties(tab.Locale, "Page", tab.TabID, null);
            SearchDocument searchDoc = new SearchDocument
            {
                SearchTypeId = TabSearchTypeId,
                UniqueKey = TabMetaDataPrefixTag + tab.TabID,
                TabId = tab.TabID,
                PortalId = tab.PortalID,
                CultureCode = tab.Locale,
                ModifiedTimeUtc = tab.UpdatedOn.Value,
                Body = HtmlUtils.Clean(tab.Content, false),
                Description = GetDescription(tabInfo, Localization)
            };

            searchDoc.Keywords.Add("keywords", tabInfo.KeyWords);

            //Using TabName for searchDoc.Title due to higher prevalence and relavency || TabTitle will be stored as a keyword
            searchDoc.Title = GetName(tabInfo, Localization);
            searchDoc.Keywords.Add("title", GetTitle(tabInfo, Localization));

            if (tabInfo.Terms != null && tabInfo.Terms.Count > 0)
            {
                searchDoc.Tags = tabInfo.Terms.Select(t => t.Name);
            }

            if (Logger.IsTraceEnabled)
            {
                Logger.Trace("TabIndexer: Search document for metaData added for page [" + GetTitle(tabInfo, Localization) + " tid:" + tab.TabID + " pid:" + tab.ID + "]");
            }

            return searchDoc;
        }

        private static string GetTitle(TabInfo tabInfo, List<Localization> localization)
        {
            if (localization != null && localization.Count > 0)
            {
                return localization.Where(x => x.Name == "Title").FirstOrDefault() != null && !string.IsNullOrEmpty(localization.Where(x => x.Name == "Title").FirstOrDefault().Value) ? localization.Where(x => x.Name == "Title").FirstOrDefault().Value : tabInfo.Title;
            }
            return tabInfo.Title;
        }

        private static string GetName(TabInfo tabInfo, List<Localization> localization)
        {
            if (localization != null && localization.Count > 0)
            {
                return localization.Where(x => x.Name == "Name").FirstOrDefault() != null && !string.IsNullOrEmpty(localization.Where(x => x.Name == "Name").FirstOrDefault().Value) ? localization.Where(x => x.Name == "Name").FirstOrDefault().Value : tabInfo.TabName;
            }
            return tabInfo.TabName;
        }

        private static string GetDescription(TabInfo tabInfo, List<Localization> localization)
        {
            if (localization != null && localization.Count > 0)
            {
                return localization.Where(x => x.Name == "Description").FirstOrDefault() != null && !string.IsNullOrEmpty(localization.Where(x => x.Name == "Description").FirstOrDefault().Value) ? localization.Where(x => x.Name == "Description").FirstOrDefault().Value : tabInfo.Description;
            }
            return tabInfo.Description;
        }

        private int IndexCollectedDocs(Action<IEnumerable<SearchDocument>> indexer, ICollection<SearchDocument> searchDocuments, int portalId, int scheduleId)
        {
            indexer.Invoke(searchDocuments);
            int total = searchDocuments.Count;
            SetLocalTimeOfLastIndexedItem(portalId, scheduleId, searchDocuments.Last().ModifiedTimeUtc);
            searchDocuments.Clear();
            return total;
        }

        [Obsolete("Legacy Search (ISearchable) -- Deprecated in DNN 7.1. Use 'IndexSearchDocuments' instead.. Scheduled removal in v10.0.0.")]
        public override SearchItemInfoCollection GetSearchIndexItems(int portalId)
        {
            return null;
        }
    }
}
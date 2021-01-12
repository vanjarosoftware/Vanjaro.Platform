using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Scheduling;
using DotNetNuke.Services.Search.Internals;
using System;
using static Vanjaro.Core.Managers;

namespace Vanjaro.Core.Services.Search
{
    /// -----------------------------------------------------------------------------
    /// Namespace:  DotNetNuke.Services.Search
    /// Project:    DotNetNuke
    /// Class:      SearchEngineScheduler
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The SearchEngineScheduler implements a SchedulerClient for the Indexing of
    /// portal content.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class SearchEngineScheduler : SchedulerClient
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SearchEngineScheduler));

        public SearchEngineScheduler(ScheduleHistoryItem objScheduleHistoryItem)
        {
            ScheduleHistoryItem = objScheduleHistoryItem;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// DoWork runs the scheduled item
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public override void DoWork()
        {
            try
            {
                DateTime lastSuccessFulDateTime = SearchHelper.Instance.GetLastSuccessfulIndexingDateTime(ScheduleHistoryItem.ScheduleID);
                Logger.Trace("Search: Site Crawler - Starting. Content change start time " + lastSuccessFulDateTime.ToString("g"));
                ScheduleHistoryItem.AddLogNote(string.Format("Starting. Content change start time <b>{0:g}</b>", lastSuccessFulDateTime));

                SearchEngine searchEngine = new SearchEngine(ScheduleHistoryItem, lastSuccessFulDateTime);
                try
                {
                    searchEngine.DeleteOldDocsBeforeReindex();
                    searchEngine.DeleteRemovedObjects();
                    searchEngine.IndexContent();
                    searchEngine.CompactSearchIndexIfNeeded(ScheduleHistoryItem);
                }
                finally
                {
                    searchEngine.Commit();
                }

                ScheduleHistoryItem.Succeeded = true;
                ScheduleHistoryItem.AddLogNote("<br/><b>Indexing Successful</b>");
                SearchHelper.Instance.SetLastSuccessfulIndexingDateTime(ScheduleHistoryItem.ScheduleID, ScheduleHistoryItem.StartDate);

                Logger.Trace("Search: Site Crawler - Indexing Successful");
            }
            catch (Exception ex)
            {
                ScheduleHistoryItem.Succeeded = false;
                ScheduleHistoryItem.AddLogNote("<br/>EXCEPTION: " + ex.Message);
                Errored(ref ex);
                if (ScheduleHistoryItem.ScheduleSource != ScheduleSource.STARTED_FROM_BEGIN_REQUEST)
                {
                    ExceptionManager.LogException(ex);
                }
            }
        }

        #region Install Scheduler
        public static void Install()
        {
            ScheduleItem item = SchedulingProvider.Instance().GetSchedule("DotNetNuke.Services.Search.SearchEngineScheduler, DOTNETNUKE", string.Empty);
            if (item != null)
            {
                SchedulingProvider.Instance().DeleteSchedule(item);
            }

            if (SchedulingProvider.Instance().GetSchedule("Vanjaro.Core.Services.Search.SearchEngineScheduler,Vanjaro.Core", string.Empty) == null)
            {
                ScheduleItem Manager = new ScheduleItem
                {
                    TypeFullName = "Vanjaro.Core.Services.Search.SearchEngineScheduler,Vanjaro.Core",
                    Enabled = true,
                    TimeLapse = 1,
                    TimeLapseMeasurement = "m",
                    RetryTimeLapse = 30,
                    RetryTimeLapseMeasurement = "s",
                    RetainHistoryNum = 60,
                    FriendlyName = "Search: Site Crawler"
                };
                SchedulingProvider.Instance().AddSchedule(Manager);
            }
        }
        #endregion
    }
}
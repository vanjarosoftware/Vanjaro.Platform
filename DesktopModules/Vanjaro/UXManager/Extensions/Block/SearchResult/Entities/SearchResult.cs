using DotNetNuke.Web.InternalServices.Views.Search;
using System.Collections.Generic;

namespace Vanjaro.UXManager.Extensions.Block.SearchResult.Entities
{
    public class SearchResult
    {
        public SearchResult(string Keyword, Dictionary<string, string> Attributes)
        {
            if (!string.IsNullOrEmpty(Keyword))
            {
                dynamic sResults = Managers.SearchResultManager.Search(Keyword, Attributes).Data;
                More = sResults.more;
                TotalHits = sResults.totalHits;
                Results = sResults.results;
                int outPageIndex = 0;
                outPageIndex = Attributes.ContainsKey("data-block-pageindex") && int.TryParse(Attributes["data-block-pageindex"], out outPageIndex) ? outPageIndex : 1;
                CurrentPageIndex = outPageIndex;
            }
        }
        public bool More { get; set; }
        public int TotalHits { get; set; }
        public int CurrentPageIndex { get; set; } = 1;
        public bool LinkTargetOpenInNewTab { get; set; }
        public List<GroupedDetailView> Results { get; set; }
    }
}
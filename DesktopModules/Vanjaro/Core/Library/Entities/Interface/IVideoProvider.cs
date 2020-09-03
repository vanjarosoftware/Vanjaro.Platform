using System.Collections.Generic;
using System.Threading.Tasks;

namespace Vanjaro.Core.Entities.Interface
{
    public interface IVideoProvider
    {
        string Name { get; }
        bool Available { get; }
        bool ShowLogo { get; }
        string Logo { get; }
        string Link { get; }
        bool IsSupportBackground { get; }
        Task<string> GetVideos(string Keyword, int PageNo, int PageSize, Dictionary<string, object> AdditionalData);
    }
}
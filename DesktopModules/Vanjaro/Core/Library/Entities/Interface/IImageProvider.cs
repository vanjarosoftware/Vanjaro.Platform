using System.Threading.Tasks;

namespace Vanjaro.Core.Entities.Interface
{
    public interface IImageProvider
    {
        string Name { get; }
        bool Available { get; }
        bool ShowLogo { get; }
        string Logo { get; }
        string Link { get; }
        Task<string> GetImages(string Keyword, int PageNo, int PageSize);
    }
}
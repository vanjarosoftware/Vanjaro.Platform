using Vanjaro.URL.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vanjaro.URL.Managers
{
    public interface IURLService
    {
        void AddURL(string Language, string Slug, bool IsDefault);
        void RemoveURL(string Language, string Slug);
        List<URLEntity> URLs { get; set; }
        string PermLink { get; set; }
        string Entity { get; }
        int EntityID { get; }
    }
}

using System;
using DotNetNuke.Entities.Portals;

namespace Vanjaro.Core.Entities.Interface
{
    public interface IPortalDelete
    {
        void Delete(PortalInfo PortalInfo);
    }
}
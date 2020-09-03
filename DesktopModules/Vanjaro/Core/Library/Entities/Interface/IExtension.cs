using DotNetNuke.Entities.Users;
using System.Collections.Generic;
using Vanjaro.Common.Engines.UIEngine.AngularBootstrap;
using Vanjaro.Common.Entities.Apps;

namespace Vanjaro.Core.Entities.Interface
{
    public interface IExtension
    {
        AppInformation App { get; }
        List<AngularView> AngularViews { get; }
        string AccessRoles(UserInfo userInfo);
        string UIPath { get; }
        string AppCssPath { get; }
        string AppJsPath { get; }
        string UIEngineAngularBootstrapPath { get; }
        string[] Dependencies
        {
            get;
        }
    }
}
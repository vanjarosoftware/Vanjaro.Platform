using System;
using System.IO;
using System.Web;
using System.Linq;
using DotNetNuke.Services.Exceptions;

namespace Vanjaro.Core
{
    public static partial class Managers
    {
        public class ExceptionManager
        {
            public static void LogException(string Message)
            {
                LogException(new Exception(Message));
            }
            public static void LogException(Exception ex)
            {
                Exceptions.LogException(ex);
            }
        }
    }
}
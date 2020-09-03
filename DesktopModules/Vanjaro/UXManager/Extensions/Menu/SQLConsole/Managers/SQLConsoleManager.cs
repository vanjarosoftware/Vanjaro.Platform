using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Log.EventLog;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text.RegularExpressions;
using Vanjaro.UXManager.Library.Common;

namespace Vanjaro.UXManager.Extensions.Menu.SQLConsole.Managers
{
    public class SQLConsoleManager
    {
        private const string ScriptDelimiterRegex = "(?<=(?:[^\\w]+|^))GO(?=(?: |\\t)*?(?:\\r?\\n|$))";
        private static readonly Regex SqlObjRegex = new Regex(ScriptDelimiterRegex, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        public static List<string> GetConnections()
        {
            List<string> connections = new List<string>();
            foreach (ConnectionStringSettings connection in ConfigurationManager.ConnectionStrings)
            {
                if (connection.Name.ToLowerInvariant() != "localmysqlserver" && connection.Name.ToLowerInvariant() != "localsqlserver")
                {
                    connections.Add(connection.Name);
                }
            }

            return connections;
        }

        public static ActionResult RunQuery(string sqlConnection, dynamic Query)
        {
            ActionResult actionResult = new ActionResult();
            string connectionstring = Config.GetConnectionString(sqlConnection);
            int Timeout = 0;
            List<DataTable> outputTables = new List<DataTable>();
            string errorMessage = string.Empty;

            dynamic runAsQuery = RunAsScript(Query);
            if (runAsQuery)
            {
                errorMessage = DataProvider.Instance().ExecuteScript(connectionstring, Query, Timeout);
            }
            else
            {
                try
                {
                    dynamic dr = DataProvider.Instance().ExecuteSQLTemp(connectionstring, Query, Timeout, out errorMessage);
                    if (dr != null)
                    {
                        do
                        {
                            DataTable table = new DataTable { Locale = CultureInfo.CurrentCulture };
                            table.Load(dr);
                            outputTables.Add(table);
                        }
                        while (!dr.IsClosed);
                    }
                }
                catch (SqlException sqlException)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(sqlException);
                    errorMessage = sqlException.Message;
                }
            }
            RecordAuditEventLog(Query);

            if (string.IsNullOrEmpty(errorMessage))
            {
                actionResult.Data = runAsQuery ? null : outputTables;
                actionResult.IsSuccess = true;
                return actionResult;
            }
            else
            {
                actionResult.AddError("SqlException", errorMessage);
                return actionResult;
            }


        }

        private static void RecordAuditEventLog(string query)
        {
            UserInfo userInfo = new UserInfo();
            LogProperties props = new LogProperties { new LogDetailInfo("User", userInfo.Username), new LogDetailInfo("SQL Query", query) };

            //Add the event log with host portal id.
            LogInfo log = new LogInfo
            {
                LogUserID = userInfo.UserID,
                LogTypeKey = EventLogController.EventLogType.HOST_SQL_EXECUTED.ToString(),
                LogProperties = props,
                BypassBuffering = true,
                LogPortalID = Null.NullInteger
            };

            LogController.Instance.AddLog(log);
        }

        private static bool RunAsScript(string query)
        {
            return SqlObjRegex.IsMatch(query);
        }

    }
}
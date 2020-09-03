using DotNetNuke.Common.Utilities;
using System;
using System.IO;
using System.Web;

namespace Vanjaro.Common.Handlers
{
    /// <summary>
    /// Summary description for ImportMapHandler
    /// </summary>
    public class ImportMapHandler : IHttpHandler
    {
        private int PortalId = -1;
        private string GuidKey = string.Empty;
        public void ProcessRequest(HttpContext context)
        {
            if (context.Request.QueryString["portalid"] != null)
            {
                PortalId = int.Parse(context.Request.QueryString["portalid"]);
            }
            else
            {
                context.Response.StatusCode = 404;
            }

            if (context.Request.QueryString["guid"] != null)
            {
                GuidKey = context.Request.QueryString["guid"];
            }

            if (PortalId >= 0)
            {
                if (!string.IsNullOrEmpty(GuidKey))
                {
                    dynamic data = DataCache.GetCache(GuidKey);
                    context.Response.Clear();
                    context.Response.AddHeader("Content-Disposition", "attachment; filename=" + GuidKey + ".txt");
                    context.Response.ContentType = "text/plain";
                    context.Response.BinaryWrite(data);
                    context.Response.End();
                }
                else
                {
                    try
                    {
                        string Data = context.Request.Form["MapData"];
                        MemoryStream ReturnStream = new MemoryStream();
                        StreamWriter sw = new StreamWriter(ReturnStream);
                        sw.WriteLine(Data);
                        sw.Flush();
                        sw.Close();

                        byte[] byteArray = ReturnStream.ToArray();
                        ReturnStream.Flush();
                        ReturnStream.Close();

                        string Key = "DSF_Map_" + Guid.NewGuid();
                        DataCache.SetCache(Key, byteArray);
                        context.Response.ContentType = "text/plain";
                        context.Response.Write(Key);
                    }
                    catch (Exception)
                    {
                        context.Response.ContentType = "text/plain";
                        context.Response.Write("error");
                    }
                }
            }
        }
        public bool IsReusable => false;
    }
}
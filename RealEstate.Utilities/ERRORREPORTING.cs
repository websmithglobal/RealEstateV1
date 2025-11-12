using Newtonsoft.Json;
using System.Net;
using System.Text;
using static SQLHelper.Enumration;

namespace SQLHelper
{
    public class ERRORREPORTING
    {
        #region New Error Log
        public static void ErrorLog(Exception ex
            , string url
            , string ErrorLogAddBy
            , Guid userid
            , string Remarks = ""
            , string Request = ""
            , string Response = "")
        {
            try
            {
                string URL = "http://err.appsmith.co.in/api/ErrorLog";
                WebRequest request = WebRequest.Create(URL);
                request.Method = "POST";

                request.ContentType = "application/json";

                string InnerExceptionMessage = string.Empty;
                string InnerExceptionStackTrace = string.Empty;

                int logType = 1;

                if (ex != null && ex.InnerException != null)
                {
                    InnerExceptionMessage = ex.InnerException.Message;
                    InnerExceptionStackTrace = ex.InnerException.StackTrace ?? string.Empty;

                    logType = (int)LogType.Error;
                }
                else
                {
                    if (ex != null)
                    {
                        logType = (int)LogType.Error;
                    }
                }

                var values = new Dictionary<string, string>{
                    { "ProjectName", "RealEstateProject" },
                    { "LogType", logType.ToString() },
                    { "ErrorFrom", $"Susha/{url}"},
                    { "ErrorMessage",ex != null ? ex.Message : string.Empty },
                    { "StackTrace", ex != null ? ex.StackTrace ?? string.Empty : string.Empty  },
                    { "InnerExceptionMessage", InnerExceptionMessage },
                    { "InnerExceptionStackTrace",InnerExceptionStackTrace},
                    { "Remarks", Remarks },
                    { "ApiRequest", Request },
                    { "ApiResponse", Response },
                    { "ErrorLogAddBy", ErrorLogAddBy },
                    { "UserID", userid.ToString() },
                };

                string jsonFormat = JsonConvert.SerializeObject(values);

                Byte[] data = Encoding.UTF8.GetBytes(jsonFormat);

                request.ContentLength = data.Length;

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                WebResponse response = request.GetResponse();

                StreamReader reader = new StreamReader(response.GetResponseStream());
            }
            catch (Exception ee)
            {
                _ = ee.Message;
            }
        }
        #endregion
    }
}

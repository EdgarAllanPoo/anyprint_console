using System;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace AnyPrintConsole
{
    public class AnyPrintJob
    {
        public string code { get; set; }
        public string filename { get; set; }
        public string fileUrl { get; set; }
        public int copies { get; set; }
        public int pages { get; set; }
    }

    public class AnyPrintApiClient
    {
        private const string BaseUrl = "https://anyprint.id/api";

        public AnyPrintJob GetJob(string code)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var url = $"{BaseUrl}/jobs/{code}";
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.Timeout = 15000;

            try
            {
                using (var response = (HttpWebResponse)request.GetResponse())
                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    var json = reader.ReadToEnd();
                    return JsonConvert.DeserializeObject<AnyPrintJob>(json);
                }
            }
            catch (WebException ex)
            {
                if (ex.Response is HttpWebResponse errorResponse)
                {
                    if (errorResponse.StatusCode == HttpStatusCode.NotFound ||
                        errorResponse.StatusCode == HttpStatusCode.PaymentRequired)
                    {
                        throw new Exception("JOB_NOT_PAID");
                    }
                }

                throw new Exception("NETWORK_ERROR: " + ex.Message);
            }
        }


        public string DownloadFile(string fileUrl, string saveFolder)
        {
            var fileName = Path.GetFileName(fileUrl);
            var localPath = Path.Combine(saveFolder, fileName);

            using (var client = new WebClient())
            {
                client.DownloadFile(fileUrl, localPath);
            }

            return localPath;
        }
    }
}

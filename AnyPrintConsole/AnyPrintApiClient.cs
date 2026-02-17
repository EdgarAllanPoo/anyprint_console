using System;
using System.IO;
using System.Net;
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
        public string printMode { get; set; }
    }

    public class AnyPrintApiClient
    {
        private const string BaseUrl = "https://anyprint.id/api";

        public AnyPrintApiClient()
        {
            // Force modern TLS
            ServicePointManager.SecurityProtocol =
                SecurityProtocolType.Tls12 |
                SecurityProtocolType.Tls13;

            // Allow strong crypto
            ServicePointManager.Expect100Continue = true;
        }

        public AnyPrintJob GetJob(string code)
        {
            var url = $"{BaseUrl}/jobs/{code}";

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.Timeout = 15000;

            using (var response = (HttpWebResponse)request.GetResponse())
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                var json = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<AnyPrintJob>(json);
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

using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
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

        private static readonly HttpClient client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(15)
        };

        // ===================== GET JOB =====================

        public async Task<AnyPrintJob> GetJobAsync(string code)
        {
            var url = $"{BaseUrl}/jobs/{code}";

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.Timeout = 15000;

            try
            {
                using (var response = (HttpWebResponse)await request.GetResponseAsync())
                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    var json = await reader.ReadToEndAsync();
                    return JsonConvert.DeserializeObject<AnyPrintJob>(json);
                }
            }
            catch (WebException ex) when (ex.Response is HttpWebResponse errorResponse)
            {
                // Preserve status code for UI layer
                throw new WebException(
                    errorResponse.StatusCode.ToString(),
                    ex,
                    ex.Status,
                    errorResponse);
            }
        }

        // ===================== DOWNLOAD FILE =====================

        public async Task<string> DownloadFileAsync(string fileUrl, string saveFolder)
        {
            Directory.CreateDirectory(saveFolder);

            var fileName = Path.GetFileName(new Uri(fileUrl).AbsolutePath);
            var localPath = Path.Combine(saveFolder, fileName);

            using (var response = await client.GetAsync(fileUrl))
            {
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception(
                        $"Download failed: {(int)response.StatusCode} {response.ReasonPhrase}");
                }

                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = new FileStream(localPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await stream.CopyToAsync(fileStream);
                }
            }

            return localPath;
        }
    }
}

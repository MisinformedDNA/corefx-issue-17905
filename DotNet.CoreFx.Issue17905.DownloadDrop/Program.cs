using HtmlAgilityPack;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace DotNet.CoreFx.Issue17905.DownloadDrop
{
    class Program
    {
        private static HttpClient httpClient;

        static async Task Main(string[] args)
        {
            using (httpClient = new HttpClient())
            using (var stream = await httpClient.GetStreamAsync("https://github.com/dotnet/corefx/issues/17905"))
            {
                var html = new HtmlDocument();
                html.Load(stream);
                var comment = html
                    .GetElementbyId("issuecomment-365349091")
                    .SelectSingleNode("div[2]/table/tbody/tr/td");

                var list = comment
                    .SelectNodes("ul/li/a")
                    .Select(e => e.Attributes["href"].Value);

                Directory.CreateDirectory("files");
                foreach (var url in list)
                {
                    await DownloadFile(url);
                }
            }
        }

        private static async Task DownloadFile(string url)
        {
            Console.WriteLine($"Downloading {url}");
            var fileName = url.Split('/').Last();

            using (var inputStream = await httpClient.GetStreamAsync(url))
            using (var outputStream = File.Create($@"files\{fileName}"))
            {
                await inputStream.CopyToAsync(outputStream);
            }
        }
    }
}

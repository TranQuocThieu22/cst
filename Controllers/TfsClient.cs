using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace educlient.Controllers
{
    public class TfsClient
    {
        public static async Task<byte[]> LoadImage(string src, string user, string pass)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "AQTech TFS program");
                client.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(UTF8Encoding.UTF8.GetBytes($"{user}:{pass}")));
                var res = await client.GetAsync(src);

                try
                {
                    res.EnsureSuccessStatusCode();
                    using (var content = res.Content)
                    {
                        var data = await content.ReadAsStreamAsync();
                        using var gzip = new GZipStream(data, CompressionMode.Decompress);
                        using var ms = new MemoryStream();
                        gzip.CopyTo(ms);
                        return ms.ToArray(); // nullable
                    }
                }
                finally
                {
                    res.Dispose();
                }
            }
        }
    }
}

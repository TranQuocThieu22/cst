using AQFramework.Utilities;
using educlient.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace educlient.Services
{
    public interface ITFSAccountService
    {
        Task<EduClient> LoginAsync(LoginModel inputData);
    }

    public class TFSAccountService : ITFSAccountService
    {
        private const string pss = "76Z5N82AlUc9"; // Note: Storing secrets in code is not recommended
        private const string ServerUrl = "https://dev.aqtech.vn:1443/pw/LdapUtils.asmx?op=Login";
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IDbLiteContext database;
        public TFSAccountService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;

        }
        public async Task<EduClient> LoginAsync(LoginModel inputData)
        {
            if (string.IsNullOrWhiteSpace(inputData.username) || string.IsNullOrWhiteSpace(inputData.password))
            {
                throw new ArgumentException("Username and password are required.");
            }

            var input = Crypt.Encrypt($"{inputData.username},{inputData.password}", pss);
            var xmlRequest = $@"
                <soap:Envelope xmlns:soap=""http://www.w3.org/2003/05/soap-envelope"" xmlns:tem=""http://tempuri.org/"">
                   <soap:Header/>
                   <soap:Body>
                      <tem:Login>
                         <tem:input>{input}</tem:input>
                      </tem:Login>
                   </soap:Body>
                </soap:Envelope>";

            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(xmlRequest, Encoding.UTF8, "text/xml");
                var response =  httpClient.PostAsync(ServerUrl, content).Result;

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var responseContent = response.Content.ReadAsStringAsync().Result;
                    var xml = new XmlDocument();
                    xml.Load(new StringReader(responseContent));
                    var loginResult = xml.GetElementsByTagName("LoginResult").OfType<XmlNode>().FirstOrDefault()?.InnerText;

                    if (!string.IsNullOrEmpty(loginResult))
                    {
                        var decryptedResult = Crypt.Decrypt(loginResult, pss);
                        if (string.IsNullOrEmpty(decryptedResult))
                        {
                            throw new UnauthorizedAccessException("Invalid username or password");
                        }

                        var tempResult = JsonConvert.DeserializeObject<dynamic>(decryptedResult);
                        var tfsUserModel = new EduClient
                        {
                            MaTruong = "AQ",
                            Pass = "",  // Set appropriately based on your logic
                            TenTruong = "AQ",  // Set appropriately based on your logic
                            Roles = "TFS",  // Set appropriately based on your logic
                            Group = tempResult.group.ToObject<List<string>>(),
                            User = inputData.username
                        };
                        return tfsUserModel;

                    }
                }

                return null;

            }
        }

    }

}

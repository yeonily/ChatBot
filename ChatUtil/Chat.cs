using ChatUtil.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ChatUtil
{
    public class Chat
    {
        public string APIKEY { get; set; }
        public string APIModel { get; set; }
        public string APIBaseURL { get; set; }
        public string APIURLPath {  get; set; }

        public Chat(string APIKEY, string APIModel, string APIBaseURL, string APIURLPath)
        {
            this.APIKEY = APIKEY;
            this.APIModel = APIModel;
            this.APIBaseURL = APIBaseURL;
            this.APIURLPath = APIURLPath;
        }

        /// <summary>
        /// 채팅 발송
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        public async Task<string> SendChat(List<Message> messages)
        {
            using (HttpClient client = new HttpClient())
            {
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls | System.Net.SecurityProtocolType.Tls12;

                client.BaseAddress = new Uri(this.APIBaseURL);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.APIKEY);

                var requestbody = new
                {
                    messages = messages,
                    model = this.APIModel
                };

                var jsonBody = JsonConvert.SerializeObject(requestbody);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(this.APIURLPath, content);

                var responseBody = await response.Content.ReadAsStringAsync();

                return responseBody;
            }
        }
    }
}

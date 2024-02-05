using ChatBot.Models;
using ChatBot.Repository;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace ChatBot.Controllers.api
{
    public class HomeController : ApiController
    {
        readonly ChatRepository repo = new ChatRepository();

        [HttpPost]
        public async Task<string> Chat(UserMessage msg)
        {
            string result = "";
            string chatId = "";
            try
            {
                chatId = repo.SaveInput(msg);

                List<Message> messages = repo.getChatData(msg);
                string response = await repo.SendpromptAsync(messages);

                JObject jsondata = JObject.Parse(response);

                if (jsondata.ContainsKey("error"))
                {
                    JToken error = jsondata["error"];
                    if (error["code"].ToString() == "rate_limit_exceeded")
                    {
                        result = "API 정책에 의한 요청이 초과하였습니다.";
                    }
                    else if (error["code"].ToString() == "context_length_exceeded")
                    {
                        result = "질문이 가능한 Token 수를 초과하였습니다.";
                    }
                    else
                    {
                        result = "오류가 발생하였습니다. 잠시 후에 다시 시도하시기 바랍니다.";
                    }
                    repo.MakeLog(msg.userId, "fail", "[" + error["code"].ToString() + "]" + error["message"].ToString());
                }
                else
                {
                    result = jsondata["choices"][0]["message"]["content"].ToString();
                    repo.MakeLog(msg.userId, "success");
                }

                repo.SaveOutput(result, chatId);

                return result;
            }
            catch (Exception e)
            {
                result = "잠시 후에 다시 시도하고, 문제가 지속된다면 관리자에게 문의하십시오.";

                if (chatId != null)
                {
                    repo.SaveOutput(result, chatId);
                }

                repo.MakeLog(msg.userId, "fail", e.Message.ToString());

                return result;
            }
        }

        [HttpPost]
        public string MakeRoom(UserMessage msg)
        {
            try
            {
                return repo.MakeRoom(msg);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [HttpPost]
        public IEnumerable<HistoryData> chatHistory(UserMessage user)
        {
            try
            {
                return repo.getHistory(user.userId);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [HttpPost]
        public void Delete(UserMessage user)
        {
            try
            {
                repo.Delete(user.roomId);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [HttpPost]
        public IEnumerable<ChatDetail> ChatDetail(UserMessage user)
        {
            try
            {
                return repo.chatDetail(user.roomId);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}

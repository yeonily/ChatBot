using ChatBot.Global;
using ChatBot.Models;
using ChatDB;
using ChatUtil;
using ChatUtil.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI.WebControls;

namespace ChatBot.Repository
{
    public class ChatRepository
    {
        private DBConnection chatDB = new DBConnection(Myvar.DBConnectionString());
        private readonly string _apiKey = ConfigurationManager.AppSettings["API_KEY"];
        private readonly string _model = ConfigurationManager.AppSettings["API_MODEL"];
        private readonly string _baseUrl = ConfigurationManager.AppSettings["API_BASE_URL"];
        private readonly string _urlPath = ConfigurationManager.AppSettings["API_URL_PATH"];

        /// <summary>
        /// 채팅 실행
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        public async Task<string> SendpromptAsync(List<ChatBot.Models.Message> messages)
        {

            Chat chat = new Chat(_apiKey, _model, _baseUrl, _urlPath);

            var param = messages.Select(Message => new ChatUtil.Model.Message
            {
                role = Message.role,
                content = Message.content
            }).ToList();

            return await chat.SendChat(param);
        }

        /// <summary>
        /// 질문 저장 / 프로시저 사용
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public string SaveInput(UserMessage msg)
        {
            Dictionary<string, string> parameter = new Dictionary<string, string>();
            parameter.Add("@P_TYPE", "SaveInput");
            parameter.Add("@P_CONTENT", msg.content);
            parameter.Add("@P_CHAT_ROOM", msg.roomId);

            DataTable dt = chatDB.requestProcedureReturn("SP_CHAT_SET", parameter);

            if (dt != null && dt.Rows.Count > 0)
            {
                var chat = dt.Rows[0][0];
                string uuid = chat.ToString();

                return uuid;
            }
            else
            {
                throw new Exception("Failed to get ChatId.");
            }
        }

        /// <summary>
        /// 답변 저장 / 쿼리 실행
        /// </summary>
        /// <param name="content"></param>
        /// <param name="chatId"></param>
        public void SaveOutput(string content, string chatId)
        {
            string query = "INSERT INTO TB_CHAT_OUT (OUT_CHAT_ID, OUT_DATETIME, CONTENT, CHAT_ID) " +
                    "VALUES (NEWID(), GETDATE(), @P_CONTENT, @P_CHAT_ID);";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@P_CONTENT", content),
                new SqlParameter("@P_CHAT_ID", chatId),
            };

            chatDB.requestExecuteNonQuery(query, parameters);
        }

        /// <summary>
        /// 채팅룸 생성 / 프로시저 사용
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public string MakeRoom(Models.UserMessage msg)
        {
            Dictionary<string, string> dc = new Dictionary<string, string>();
            dc.Add("@P_TYPE", "MakeRoom");
            dc.Add("@P_ROOM_NAME", msg.content);
            dc.Add("@P_USER_ID", msg.userId);

            DataTable dt = chatDB.requestProcedureReturn("SP_CHAT_SET", dc);

            if (dt != null && dt.Rows.Count > 0)
            {
                var chat = dt.Rows[0][0];
                string uuid = chat.ToString();

                return uuid;
            }
            else
            {
                throw new Exception("Failed to get ChatRoomId.");
            }

        }

        /// <summary>
        /// 사용자 채팅 목록 조회 / 프로시저 사용
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public IEnumerable<Models.HistoryData> getHistory(string userId)
        {

            Dictionary<string, string> parameter = new Dictionary<string, string>();
            parameter.Add("@P_TYPE", "ChatList");
            parameter.Add("@P_USER_ID", userId);

            DataTable dt = chatDB.requestProcedureReturn("SP_CHAT_GET", parameter);

            IEnumerable<Models.HistoryData> historyDataList = dt.AsEnumerable().Select(row => new Models.HistoryData
            {
                RoomId = row.Field<string>("CHAT_ROOM"), 
                RoomName = row.Field<string>("ROOM_NAME"), 
                StartDate = row.Field<DateTime>("START_DATETIME"),
            }).ToList();

            return historyDataList;
        }

        /// <summary>
        /// 채팅 목록 삭제 / 쿼리 실행
        /// </summary>
        /// <param name="roomId"></param>
        public void Delete(string roomId)
        {             
            string query = "UPDATE TB_CHAT_ROOM " +
                           "SET DEL_YN = 1 " +
                           "WHERE CHAT_ROOM = @roomId";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@roomId", roomId),
            };

            chatDB.requestExecuteNonQuery(query, parameters);
        }

        /// <summary>
        /// 채팅 내용 조회 / 프로시저 사용
        /// </summary>
        /// <param name="roomId"></param>
        /// <returns></returns>
        public IEnumerable<Models.ChatDetail> chatDetail(string roomId)
        {
            Dictionary<string, string> dc = new Dictionary<string, string>();
            dc.Add("@P_TYPE", "ChatDetail");
            dc.Add("@P_CHAT_ROOM", roomId);

            DataTable dt = chatDB.requestProcedureReturn("SP_CHAT_GET", dc);

            IEnumerable<Models.ChatDetail> ChatDetailData = dt.AsEnumerable().Select(row => new Models.ChatDetail
            {
                ChatContent = row.Field<string>("ChatContent"),
                ChatOutContent = row.Field<string>("ChatOutContent"),
                ChatId = row.Field<string>("ChatId"),
            }).ToList();

            return ChatDetailData;
        }

        /// <summary>
        /// 이전 채팅 내용 조회하여 Message형식으로 저장(과거 채팅 이어서 하기 위함)
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public List<Models.Message> getChatData(Models.UserMessage msg)
        {
            List<Models.Message> messageList = new List<Models.Message>();
            Models.Message system = new Models.Message("system", "Your name is NamuGPT. You have to answer in the language entered.");
            messageList.Add(system);

            IEnumerable<ChatDetail> ChatDetailData = chatDetail(msg.roomId);

            var userDatas = ChatDetailData
                             .Select(s => new Models.Message()
                             {
                                 role = "user",
                                 content = s.ChatContent,
                             })
                             .ToList();

            var asstDatas = ChatDetailData
                             .Select(s => new Models.Message()
                             {
                                 role = "assistant",
                                 content = s.ChatOutContent,
                             })
                             .ToList();

            int maxLength = Math.Max(userDatas.Count, asstDatas.Count);
            for (int i = 0; i < maxLength; i++)
            {
                if (i < userDatas.Count) messageList.Add(userDatas[i]);
                if (i < asstDatas.Count) messageList.Add(asstDatas[i]);
            }
            Models.Message userInput = new Models.Message("user", msg.content);
            messageList.Add(userInput);

            return messageList;
        }

        /// <summary>
        /// api 실패 로그 저장 / 프로시저 사용
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="status"></param>
        /// <param name="message"></param>
        public void MakeLog(string userId, string status, string message)
        {
            Dictionary<string, string> dc = new Dictionary<string, string>();
            dc.Add("@P_TYPE", "FailLog");
            dc.Add("@P_STATUS", status);
            dc.Add("@P_USER_ID", userId);
            dc.Add("@P_MESSAGE", message);

            chatDB.requestProcedure("SP_LOG_SET", dc);
        }

        /// <summary>
        /// api 성공 로그 저장 / 프로시저 사용
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="status"></param>
        public void MakeLog(string userId, string status)
        {

            Dictionary<string, string> dc = new Dictionary<string, string>();
            dc.Add("@P_TYPE", "SuccessLog");
            dc.Add("@P_STATUS", status);
            dc.Add("@P_USER_ID", userId);

            chatDB.requestProcedure("SP_LOG_SET", dc);
        }

        /// <summary>
        /// 사용자 채팅 목록인지 확인 / 쿼리 실행
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roomId"></param>
        /// <returns></returns>
        public bool ListCheck(string userId, string roomId)
        {
            string query = "SELECT TOP 1 1 FROM TB_CHAT_ROOM " +
                           "WHERE USER_ID = @userId AND DEL_YN = 0 AND CHAT_ROOM = @roomId";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@userId", userId),
                new SqlParameter("@roomId", roomId),
            };

            DataTable dt = chatDB.requestExecuteNonQueryReturn(query, parameters);

            return dt.Rows.Count > 0;
        }
    }
}
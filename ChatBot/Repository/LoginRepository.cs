using ChatBot.Global;
using ChatDB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ChatBot.Repository
{
    public class LoginRepository
    {
        private DBConnection chatDB = new DBConnection(Myvar.DBConnectionString());
        
        /// <summary>
        /// 로그인 / 쿼리 실행
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool Login(string userId)
        {
            try
            {
                string query = "SELECT COUNT(USER_ID) FROM TB_USER WHERE USER_ID = @userId";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@userId", userId)
                };

                DataTable user = chatDB.requestExecuteNonQueryReturn(query, parameters);

                if (user == null || Convert.ToInt32(user.Rows[0][0]) == 0) return false;

                return true;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 로그인 실패 로그 저장 / 프로시저 사용
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="status"></param>
        /// <param name="message"></param>
        public void MakeLoginLog(string userId, string status, string message)
        {
            Dictionary<string, string> dc = new Dictionary<string, string>();
            dc.Add("@P_TYPE", "FailLoginLog");
            dc.Add("@P_STATUS", status);
            dc.Add("@P_USER_ID", userId);
            dc.Add("@P_MESSAGE", message);

            chatDB.requestProcedure("SP_LOG_SET", dc);
        }

        /// <summary>
        /// 로그인 성공 로그 저장 / 프로시저 사용
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="status"></param>
        public void MakeLoginLog(string userId, string status)
        {

            Dictionary<string, string> dc = new Dictionary<string, string>();
            dc.Add("@P_TYPE", "SuccessLoginLog");
            dc.Add("@P_STATUS", status);
            dc.Add("@P_USER_ID", userId);

            chatDB.requestProcedure("SP_LOG_SET", dc);
        }
    }
}
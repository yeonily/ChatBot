using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatDB
{
    public class DBConnection
    {
        private SqlConnection sqlConn = null;
        private string connectionString = null;

        public DBConnection() { }

        public DBConnection(string connectionString)
        {
            this.connectionString = connectionString;
        }

        /// <summary>
        /// 리턴이 없는 것.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public DataTable requestExecuteNonQueryReturn(string query, SqlParameter[] parameters)
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand command = new SqlCommand(query, conn);

                if (parameters != null)
                {
                    foreach (var parameter in parameters)
                    {
                        command.Parameters.Add(parameter);
                    }
                }

                SqlDataAdapter adpt = new SqlDataAdapter(command);
                adpt.Fill(dt);
            }
            return dt;
        }

        /// <summary>
        /// 파라미터가 있는 것.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        public void requestExecuteNonQuery(string query, SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                SqlCommand command = new SqlCommand();
                command.Connection = conn;
                command.CommandText = query;

                if (parameters != null)
                {
                    foreach (var parameter in parameters)
                    {
                        command.Parameters.Add(parameter);
                    }
                }

                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// 프로시저 호출하는 함수(리턴o)
        /// </summary>
        /// <param name="procedureName"></param>
        /// <param name="dic"></param>
        /// <returns></returns>
        public DataTable requestProcedureReturn(string procedureName, Dictionary<string, string> dic)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand command = new SqlCommand(procedureName, conn);
                    command.CommandType = CommandType.StoredProcedure;

                    //고정적이지 않게 Dictionary 생성
                    foreach (var di in dic)
                    {
                        SqlParameter p1 = new SqlParameter(di.Key, SqlDbType.NVarChar);
                        p1.Direction = ParameterDirection.Input;
                        p1.Value = di.Value;
                        command.Parameters.Add(p1);
                    }

                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 프로시저를 호출하는 데 사용되는 공통함수
        /// </summary>
        /// <param name="procedureName"></param>
        /// <param name="dc"></param>
        public void requestProcedure(string procedureName, Dictionary<string, string> dc)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                SqlCommand command = new SqlCommand(procedureName, conn);
                command.CommandType = CommandType.StoredProcedure;

                //고정적이지 않게 Dictionary 생성
                foreach (var e in dc)
                {
                    SqlParameter p1 = new SqlParameter(e.Key, SqlDbType.NVarChar);
                    p1.Direction = ParameterDirection.Input;
                    p1.Value = e.Value;
                    command.Parameters.Add(p1);
                }

                command.ExecuteNonQuery();
            }
        }
    }
}

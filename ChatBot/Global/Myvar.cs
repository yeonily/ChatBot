using ChatDB;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace ChatBot.Global
{
    public class Myvar
    {
        public static string DBConnectionString()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MssqlContext"].ConnectionString;
            return connectionString;
        }
    }
}
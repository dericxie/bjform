using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace KDAL
{
    public class KDATA
    {
        static string connstr = "Data Source=172.16.1.90;Initial Catalog=lhserp;Persist Security Info=True;User ID=sa;Password=Techsql";
        public static DataTable GetDataTable(string sqlstr)
       {
            SqlConnection sqlconn = new SqlConnection(connstr);
            SqlDataAdapter da = new SqlDataAdapter(sqlstr, sqlconn);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public static DataTable GetDataTable(string sqlstr,SqlParameter[] parameters)
        {

            SqlConnection sqlconn = new SqlConnection(connstr);
            SqlCommand cmd = sqlconn.CreateCommand();
            cmd.CommandText = sqlstr;
            cmd.Parameters.AddRange(parameters);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public static int ExecuteNonQuery(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(connstr))
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;

                    //foreach (SqlParameter  param in parameters)
                    //{
                    //    cmd.Parameters.Add(param);
                    //}
                    cmd.Parameters.AddRange(parameters);
                        return cmd.ExecuteNonQuery();
                         
               }
            }
        }
    }
}

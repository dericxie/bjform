using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Data.SqlClient;
using KDAL;
namespace jqgrid1.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login

        public ActionResult Index()
        {
            return View();
        }

        public string UserCheck()
        {
            string responsetxt = string.Empty;
            HttpContextBase context = this.HttpContext;
            /*string SubmitStr = context.Request.Params["submit"];

            JObject jo = (JObject)JsonConvert.DeserializeObject(SubmitStr);

            string userStr = jo["username"].ToString();
            string passStr =Hash_MD5_32( jo["password"].ToString());
            */

            string userStr = context.Request.Params["userdata"];
            string passStr = Hash_MD5_32( context.Request.Params["passdata"]);
            DataTable dt;

            Dictionary<string, string> responsejsontxt = new Dictionary<string, string>();

            SqlParameter[] spara = new SqlParameter[]
            {
                new SqlParameter("@user",userStr),
                new SqlParameter("@pass",passStr)
            };

            try
            {
                dt = KDATA.GetDataTable(@"select top 1 username,profile.value('(/root/Chinesename)[1]','varchar(max)') as chinesename,role
                                                        from QC_staff
                                                       where username=@user and password=@pass", spara);
            }catch(Exception ex)
            {
                throw ex;
            }

            if (dt.Rows.Count > 0)
            {
                Session["user"] = dt.Rows[0]["username"].ToString();
                Session["chinesename"]=dt.Rows[0]["chinesename"].ToString();
                Session["role"] = dt.Rows[0]["role"].ToString().Trim();
                responsejsontxt.Add("restxt", "loginsuccess");
                string urlheader = Request.ApplicationPath.Length == 1 ? string.Empty : Request.ApplicationPath;
                switch (dt.Rows[0]["role"].ToString().Trim()){
                    case "0":
                        responsejsontxt.Add("url", urlheader+"/Qcprocess");
                        break;
                    case "1":
                        responsejsontxt.Add("url", urlheader + "/Qcprocess");
                        break;
                    case "9":
                        responsejsontxt.Add("url", urlheader + "/Qcform");
                        break;
                    case "99":
                        responsejsontxt.Add("url", urlheader + "/Qcprocess");
                        break;
                    default:
                        responsejsontxt.Add("url", urlheader + "/");
                        break;
                }
            }else
            {
                responsejsontxt.Add("restxt", "loginerror");
                responsejsontxt.Add("url", Request.ApplicationPath + "/Login");
            }



            responsetxt=JsonConvert.SerializeObject(responsejsontxt);
            return responsetxt;
        }

        public static string Hash_MD5_32(string word, bool toUpper = true)
        {
            try
            {
                System.Security.Cryptography.MD5CryptoServiceProvider MD5CSP
                    = new System.Security.Cryptography.MD5CryptoServiceProvider();

                byte[] bytValue = System.Text.Encoding.UTF8.GetBytes(word);
                byte[] bytHash = MD5CSP.ComputeHash(bytValue);
                MD5CSP.Clear();

                //根据计算得到的Hash码翻译为MD5码
                string sHash = "", sTemp = "";
                for (int counter = 0; counter < bytHash.Count(); counter++)
                {
                    long i = bytHash[counter] / 16;
                    if (i > 9)
                    {
                        sTemp = ((char)(i - 10 + 0x41)).ToString();
                    }
                    else
                    {
                        sTemp = ((char)(i + 0x30)).ToString();
                    }
                    i = bytHash[counter] % 16;
                    if (i > 9)
                    {
                        sTemp += ((char)(i - 10 + 0x41)).ToString();
                    }
                    else
                    {
                        sTemp += ((char)(i + 0x30)).ToString();
                    }
                    sHash += sTemp;
                }

                //根据大小写规则决定返回的字符串
                return toUpper ? sHash : sHash.ToLower();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

    }
}
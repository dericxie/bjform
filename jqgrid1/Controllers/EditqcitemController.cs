using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using KDAL;
using System.Data;
using System.Data.SqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;



namespace jqgrid1.Controllers
{
    public class EditqcitemController : Controller
    {
        // GET: Editqcitem
        public ActionResult Index()
        {
            return View();
        }

        public string GetqcitemDetail()
        {
            string responsetxt = string.Empty;
            HttpContextBase context = this.HttpContext;
            string CodeStr = context.Request.Params["bjformcode"].Trim();
            string TypeStr = context.Request.Params["bjformtype"].Trim();
            string ItemStr = context.Request.Params["bjformitem"].Trim();
            string sqlStr = string.Empty;
            DataTable dt;

            SqlParameter[] spara = new SqlParameter[]
            {
                new SqlParameter("@code",CodeStr),
                new SqlParameter("@pcode",ItemStr)
            };

            if (TypeStr == "材料")
                sqlStr = @"select a.code,a.pcode,a.checkamount,b.Description,b.Size,b.Unit,a.purchaseamount,a.checkamount,a.checkrate,a.passamount,a.finalpassamount,a.passrate,a.note.value('(/root/staffnote)[1]','varchar(max)') as staffnote,a.note.value('(/root/directornote)[1]','varchar(max)') as directornote
                                                from bjform_d a, stockinfo b
                                                where a.pcode = b.code and a.code = @code and a.pcode = @pcode";
            else
                sqlStr = @"select code,Description,Size,Unit,purchaseamount,checkamount,checkrate,passamount,finalpassamount,passrate,note.value('(/root/staffnote)[1]','varchar(max)') as staffnote,note.value('(/root/directornote)[1]','varchar(max)') as directornote
                                 from bjform_d_others
                                 where code=@code and description=@pcode";


            try
            {
                dt = KDATA.GetDataTable(sqlStr, spara);
            }catch(Exception ex)
            {
                throw ex;
            }

            responsetxt = JsonConvert.SerializeObject(dt);
            return responsetxt;

        }

        public string Updateqcitem()
        {
            

            string responsetxt = string.Empty;
            HttpContextBase context = this.HttpContext;
            string SubmitStr = context.Request.Params["submit"];

            JObject jo = (JObject)JsonConvert.DeserializeObject(SubmitStr);

            string codestr = jo["code"].ToString();
            string pcodestr = string.Empty;
            if (jo["pcode"] != null)
            {
                pcodestr = jo["pcode"].ToString();
            }else
            {
                pcodestr = jo["desc"].ToString();
            }
            decimal checkamount =Convert.ToDecimal( jo["checkamount"]);
            decimal passamount =Convert.ToDecimal( jo["passamount"]);
            decimal finalpassamount = Convert.ToDecimal(jo["finalpassamount"]);
            decimal checkrate= Convert.ToDecimal(jo["checkrate"].ToString().TrimEnd('%'))/100;
            decimal passrate= Convert.ToDecimal(jo["passrate"].ToString().TrimEnd('%'))/100;
            string qcnotestr = jo["qcnote"].ToString();
            string directornotestr = jo["directornote"].ToString();
            string checkdatestr= DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            XmlDocument xdoc;
            XmlElement xelement;


            xdoc = new XmlDocument();
            xelement = xdoc.CreateElement("", "root", "");
            xdoc.AppendChild(xelement);

            XmlElement staffnote = xdoc.CreateElement("staffnote");
            staffnote.InnerText = qcnotestr;
            XmlElement directornote = xdoc.CreateElement("directornote");
            directornote.InnerText = directornotestr;

            xelement.AppendChild(staffnote);
            xelement.AppendChild(directornote);
            string notestr = xdoc.OuterXml;

            SqlParameter[] spara = new SqlParameter[]
            {
                new SqlParameter("@code",codestr),
                new SqlParameter("@pcode",pcodestr),
                new SqlParameter("@checkamount",checkamount),
                new SqlParameter("@passamount",passamount),
                new SqlParameter("@finalpassamount",finalpassamount),
                new SqlParameter("@checkrate",checkrate),
                new SqlParameter("@passrate",passrate),
                new SqlParameter("@note",notestr),
                new SqlParameter("@checkdate",checkdatestr)
                
            };

            string sqlStr = string.Empty;

            if (jo["pcode"] == null)
            {
                sqlStr = @"update bjform_d_others set checkamount=@checkamount,passamount=@passamount,finalpassamount=@finalpassamount,checkrate=@checkrate,passrate=@passrate,note=@note,checkdate=@checkdate 
                                        where code=@code and description=@pcode";
            }
            else
            {
                sqlStr = @"update bjform_d set checkamount=@checkamount,passamount=@passamount,finalpassamount=@finalpassamount,checkrate=@checkrate,passrate=@passrate,note=@note,checkdate=@checkdate 
                                                                               where code=@code and pcode=@pcode";

            }

            try
            {
                int resultcode = KDATA.ExecuteNonQuery(sqlStr, spara);
                if (resultcode > 0)
                    responsetxt = "updateqcitemsuccess";
                else
                    responsetxt = "error";
            } catch(Exception ex)
            {
                throw ex;
            }

            return responsetxt;
        }


        public string GetcurrentUser()
        {
            string responsetxt = string.Empty;
            Dictionary<string, string> responsejsontxt = new Dictionary<string, string>();
            responsejsontxt.Add("username", Session["user"].ToString());
            responsejsontxt.Add("chinesename", Session["chinesename"].ToString());
            responsejsontxt.Add("role", Session["role"].ToString());
            responsetxt = JsonConvert.SerializeObject(responsejsontxt);
            return responsetxt;
        }

    }
}
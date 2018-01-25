using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KDAL;
using System.Data;
using System.Data.SqlClient;
using Newtonsoft.Json;

namespace jqgrid1.Controllers
{
    public class QcProcessController : Controller
    {
        // GET: QcProcess
        public ActionResult Index()
        {
            if (Session["role"] != null)
            {
                if (Session["role"].ToString() == "0" || Session["role"].ToString() == "1" || Session["role"].ToString() == "99")
                {
                    return View();
                }
                else
                {
                    Session.Abandon();
                    Response.Redirect(Request.ApplicationPath+"/Login");
                }
            }
            else
            {
                Response.Redirect(Request.ApplicationPath+"/Login");
            }

            return View();

        }

        public string GetQcProcessFormInfo()
        {
            HttpContextBase context = this.HttpContext;
            string StatusStr = context.Request.Params["status"];
            string sqlStr = string.Empty;
            string userSQLStr = string.Empty;
            DataTable dt;

            if (Session["role"].ToString() == "1")
                userSQLStr = " where a.status in ('2','3','4') and a.qcstaff='" + Session["user"].ToString() + "'";
            else
                userSQLStr = " where a.status in ('1','2','3','4')";

            if (StatusStr ==null)
            {
                sqlStr = @"select a.urgent,f.bjtext as type,a.code,e.profile.value('(/root/Chinesename)[1]', 'nvarchar(max)') as maker,a.buyer,a.supplier,c.profile.value('(/root/Chinesename)[1]', 'nvarchar(max)') as qcmanager,
                            d.profile.value('(/root/Chinesename)[1]', 'nvarchar(max)') as qcstaff, b.bjtext as status,convert(varchar(20), a.makedate, 120) as makedate from bjform_m a
                            left join bjformstcode b on a.status = b.bjstatus left join QC_staff c on a.qcmanager = c.username left join QC_staff d on a.qcstaff = d.username left join QC_staff e on a.maker=e.username 
                            left join bjformstcode f on a.type=f.bjstatus"
                            +userSQLStr+
                            "order by a.makedate desc";
                try
                {
                    dt = KDATA.GetDataTable(sqlStr);
                }catch(Exception ex)
                {
                    throw ex;
                }
            }
            else
            {
                SqlParameter[] spara = new SqlParameter[]
                {
                    new SqlParameter("@status",StatusStr)
                };
                sqlStr = @"select a.code,a.maker,a.buyer,a.supplier,c.profile.value('(/root/Chinesename)[1]', 'nvarchar(max)') as qcmanager,
                            d.profile.value('(/root/Chinesename)[1]', 'nvarchar(max)') as qcstaff, bjtext as status,convert(varchar(20), a.makedate, 120) as makedate from bjform_m a
                            left join bjformstcode b on a.status = b.bjstatus left join QC_staff c on a.qcmanager = c.username left join QC_staff d on a.qcstaff = d.username 
                            where status=@status
                            order by a.makedate desc";
                try
                {
                    dt = KDATA.GetDataTable(sqlStr, spara);
                }catch(Exception ex)
                {
                    throw ex;
                }
            }

            return JsonConvert.SerializeObject(dt);
        }

        public string GetBjformDetail()
        {
            string responsetxt = string.Empty;
            DataTable dt;
            HttpContextBase context = this.HttpContext;
            string CodeStr = context.Request.Params["code"];
            string TypeStr = context.Request.Params["type"];
            string sqlStr = string.Empty;

            SqlParameter[] spara = new SqlParameter[]
            {
                new SqlParameter("@code",CodeStr)
            };


            if (TypeStr == "材料")
                sqlStr = "select a.pcode, b.Description as pdesc, b.size, b.unit, a.purchaseamount,a.checkrate, a.checkamount,a.passamount,a.passrate,a.finalpassamount, convert(varchar(20), a.checkdate, 120) as checkdate from bjform_d a, stockinfo b where a.pcode = b.code and a.code =@code";
            else
                sqlStr = "select Description as pdesc, size, unit,purchaseamount,checkrate,checkamount, passamount,passrate, finalpassamount,convert(varchar(20), checkdate, 120) as checkdate from bjform_d_others where code = @code";


            dt = KDATA.GetDataTable(sqlStr, spara);
            string returnjson = JsonConvert.SerializeObject(dt);
            return returnjson;
        }


        public string GetQcStaff()
        {
            HttpContextBase context = this.HttpContext;
            string staffStr = context.Request.Params["staff"];

            string responsetxt = "<select>";
            DataTable dt = KDATA.GetDataTable("select username,profile.value('(/root/Chinesename)[1]','nvarchar(max)') as chinesename from QC_staff where role="+staffStr+" order by username asc");
            for(int i = 0; i < dt.Rows.Count; i++)
            {
                responsetxt += "<option value = '" + dt.Rows[i]["chinesename"] + "'>" + dt.Rows[i]["chinesename"] + "</option >";
            }
            responsetxt += "</select>";

            return responsetxt;
        }

        public string GetStatusForQc()
        {
            string responsetxt = "<select>";
            DataTable dt = KDATA.GetDataTable("select bjtext from bjformstcode where bjstatus in ('1','2','4')");
            for(int i = 0; i < dt.Rows.Count; i++)
            {
                responsetxt += "<option value='" + dt.Rows[i]["bjtext"] + "'>" + dt.Rows[i]["bjtext"] + "</option>";
            }
            responsetxt += "</select>";

            return responsetxt;
        }


        public void EditDataFromStockinfo()
        {
            HttpContextBase context = this.HttpContext;
            NameValueCollection forms = context.Request.Form;
            string operationCode = forms.Get("oper");
            string submitstring = string.Empty;
            string responsetxt = string.Empty;

            switch (operationCode)
            {
                case "edit":
                    UpdateBjfromByQc(forms, out responsetxt);
                    Response.Write(responsetxt);
                    break;
                case "add":
                    break;
                case "del":
                    break;
                default:
                    break;
            }
        }

        public void UpdateBjfromByQc(NameValueCollection forms, out string responsetxt)
        {
            string codeStr = forms.Get("code").Trim();
            string qcmanagerStr = forms.Get("qcmanager");
            string qcstaffStr = forms.Get("qcstaff");
            string statusStr = forms.Get("status");

            SqlParameter[] spara = new SqlParameter[]
            {
                new SqlParameter("@code",codeStr),
                new SqlParameter("@manager",qcmanagerStr),
                new SqlParameter("@staff",qcstaffStr),
                new SqlParameter("@status",statusStr)
            };

            try
            {
                string sqlstr = "update bjform_m ";
                sqlstr += "set qcstaff=(select top 1 username from qc_staff where profile.value('(/root/Chinesename)[1]','nvarchar(max)')='"+qcstaffStr+"'),";
                sqlstr += "qcmanager=(select top 1 username from qc_staff where profile.value('(/root/Chinesename)[1]','nvarchar(max)')='"+qcmanagerStr+"'),";
                sqlstr += "status=(select top 1 bjstatus from bjformstcode where bjtext='"+statusStr+"') ";
                sqlstr += "where code='"+codeStr+"'";
                int resultcode = KDATA.ExecuteNonQuery(sqlstr);
                if (resultcode > 0)
                    responsetxt = "success";
                else
                    responsetxt = "error";
            }catch(Exception ex)
            {
                throw ex;
            }


        }

        public string GetcurrentUser()
        {
            string responsetxt = string.Empty;
            Dictionary<string, string> responsejsontxt = new Dictionary<string, string>();
            responsejsontxt.Add("username", Session["user"].ToString());
            responsejsontxt.Add("chinesename", Session["chinesename"].ToString());
            responsejsontxt.Add("role", Session["role"].ToString());
            responsetxt= JsonConvert.SerializeObject(responsejsontxt);
            return responsetxt;
        }

        public string LogoutProc()
        {
            string responsetxt = string.Empty;
            Dictionary<string, string> responsejsontxt = new Dictionary<string, string>();
            string urlheader = Request.ApplicationPath.Length == 1 ? string.Empty : Request.ApplicationPath;
            responsejsontxt.Add("logouturl", urlheader + "/");
            Session.Abandon();
            responsetxt = JsonConvert.SerializeObject(responsejsontxt);
            return responsetxt;            
        }
    }
}
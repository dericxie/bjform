using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using KDAL;
using RazorPDF;




namespace jqgrid1.Controllers
{
    public class QcformController : Controller
    {
        // GET: Qcform

        public ActionResult Index()
        {
            if (Session["role"] != null)
            {
                if (Session["role"].ToString() == "9")
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


        public string SearchStockinfo()
        {
            string returnjson=string.Empty;
            HttpContextBase context = this.HttpContext;
            string SearchStr = context.Request.Params["q"];
            if (SearchStr != null)
            {
                string sqlstr = "select code as id, description+' '+size as text from stockinfo where description like '%" + SearchStr.Trim() + "%' or size like '%"+SearchStr.Trim()+"%'";
                DataTable dt = KDATA.GetDataTable(sqlstr);
                returnjson = JsonConvert.SerializeObject(dt);
            }
            return returnjson;

        }

        public string GetStockinfoDetail()
        {
            string returnjson = string.Empty;
            HttpContextBase context = this.HttpContext;
            string SearchStr = context.Request.Params["q"];
            if (SearchStr != null)
            {
                string sqlstr= "select code as pcode,description as [desc],size,unit from stockinfo where code='"+SearchStr+"'";
                DataTable dt = KDATA.GetDataTable(sqlstr);
                returnjson = JsonConvert.SerializeObject(dt);
            }
            return returnjson;
        }

        public string GetBjFrominfo()
        {
            string sqlStr = @"select a.urgent,f.bjtext as type,a.code,e.profile.value('(/root/Chinesename)[1]', 'nvarchar(max)') as maker,a.buyer,a.supplier,c.profile.value('(/root/Chinesename)[1]', 'nvarchar(max)') as qcmanager,
                            d.profile.value('(/root/Chinesename)[1]', 'nvarchar(max)') as qcstaff, b.bjtext as status,convert(varchar(20), a.makedate, 120) as makedate from bjform_m a
                            left join bjformstcode b on a.status = b.bjstatus left join QC_staff c on a.qcmanager = c.username left join QC_staff d on a.qcstaff = d.username left join QC_staff e on a.maker=e.username 
                            left join bjformstcode f on a.type=f.bjstatus
                            order by a.makedate desc";
            DataTable dt = KDATA.GetDataTable(sqlStr);
            string returnjson = JsonConvert.SerializeObject(dt);
            return returnjson;
        }

        public string GenQCformCode()
        {
            string qcformcode ="BJ"+ DateTime.Now.ToString("yyyy").Substring(2) + DateTime.Now.ToString("MM") + DateTime.Now.ToString("dd") + DateTime.Now.ToString("ffff");
            string outstream = "<select><option value='"+qcformcode+"'>"+qcformcode+"</option></select>";
            return outstream;
        }

        public string GetUsername()
        {
            HttpContextBase context = this.HttpContext;
            string roleid = "9";
            string responsetxt = "<select>";
            DataTable dt = KDATA.GetDataTable("select username,profile.value('(/root/Chinesename)[1]','nvarchar(max)') as chinesename from QC_staff where role=" + roleid + " order by username asc");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                responsetxt += "<option value = '" + dt.Rows[i]["chinesename"] + "'>" + dt.Rows[i]["chinesename"] + "</option >";
            }
            responsetxt += "</select>";

            return responsetxt;
        }

        public string GetStatus()
        {
            string statusstr = "新建";
            string outstream = "<select><option value='"+statusstr+"'>" + statusstr + "</option></select>";
            return outstream;
        }


        public string GetMakeDate()
        {
            string timenow = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string outstream = "<select><option value='" + timenow + "'>" + timenow + "</option></select>";
            return outstream;
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
                    UpdateBjfrom(forms, out responsetxt);
                    Response.Write(responsetxt);
                    break;
                case "add":
                    AddBjForm(forms, out responsetxt);
                    Response.Write(responsetxt);
                    break;
                case "del":
                    break;
                default:
                    break;
            }
        }

        public void AddBjForm(NameValueCollection forms,out string responsetxt)
        {
            

            string bjformcode = forms.Get("code").ToString();
            string urgent = forms.Get("urgent").ToString();
            string type = forms.Get("type").ToString();
            string maker = forms.Get("maker").ToString();
            string buyer = forms.Get("buyer").ToString();
            string supplier = forms.Get("supplier").ToString();
            string status = "0";
            string makedate = forms.Get("makedate").ToString();
            SqlParameter[] spara = new SqlParameter[]
            {
                new SqlParameter("@code",bjformcode),
                new SqlParameter("@urgent",urgent),
                new SqlParameter("@type",type),
                new SqlParameter("@maker",maker),
                new SqlParameter("@buyer",buyer),
                new SqlParameter("@supplier",supplier),
                new SqlParameter("@status",status),
                new SqlParameter("@makedate",makedate)
            };

            try
            {
                int resultcode = KDATA.ExecuteNonQuery(@"insert into bjform_m (urgent,type,code,maker,buyer,supplier,status,makedate) 
                                                                               values (@urgent,@type,@code,
                                                                               (select top 1 username from qc_staff where profile.value('(/root/Chinesename)[1]','nvarchar(max)')=@maker) ,
                                                                               @buyer,@supplier,@status,@makedate)", spara);
                if (resultcode > 0)
                    responsetxt = "已保存";
                else
                    responsetxt = "保存有误，联系管理员";
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public void UpdateBjfrom(NameValueCollection forms, out string responsetxt)
        {
            string code = forms.Get("code").ToString().Trim();
            string buyer = forms.Get("buyer").ToString();
            string supplier = forms.Get("supplier").ToString();
            SqlParameter[] spara = new SqlParameter[]
            {
                new SqlParameter("@code",code),
                new SqlParameter("@buyer",buyer),
                new SqlParameter("@supplier",supplier)
            };

            try
            {
                int resultcode = KDATA.ExecuteNonQuery("update bjform_m set buyer=@buyer,supplier=@supplier where code=@code", spara);
                if (resultcode > 0)
                {
                    responsetxt = "已保存";
                }
                else
                {
                    responsetxt = "保存有误，联系管理员";
                }
            } catch (Exception ex)
            {
                throw ex;
            }
        }

        public string DelBjForm()
        {
            string responsetxt = string.Empty;
            HttpContextBase context = this.HttpContext;
            string codeStr = context.Request.Params["code"];
            SqlParameter[] spara = new SqlParameter[]
            {
                new SqlParameter("@code",codeStr)
            };
            try
            {
                int resultcode = KDATA.ExecuteNonQuery("delete bjform_m where status=0 and code=@code", spara);
                if (resultcode > 0)
                    responsetxt = "已删除报检单";
                else
                    responsetxt = "删除有误";
            }catch(Exception ex)
            {
                throw ex;
            }

            return responsetxt;
        }

        public string BjFormChgStatus()
        {
            string responsetxt = string.Empty;
            HttpContextBase context = this.HttpContext;
            string CodeStr = context.Request.Params["code"];
            string StatusStr = context.Request.Params["chgstatus"];
            SqlParameter[] spara = new SqlParameter[]
            {
                new SqlParameter("@code",CodeStr),
                new SqlParameter("@chgstatus",StatusStr)
            };
            try
            {
                int resultcode = KDATA.ExecuteNonQuery("update bjform_m set status=@chgstatus where code=@code",spara);
                if (resultcode > 0)
                    responsetxt = "success";
                else
                    responsetxt = "error";
            }catch(Exception ex)
            {
                throw ex;
            }

            return responsetxt;
        }

        public string AddBjFormDetail()
        {
            string responsetxt = string.Empty;
            HttpContextBase context = this.HttpContext;
            string CodeStr = context.Request.Params["code"];
            string TypeStr = context.Request.Params["ptype"];
            string PcodeStr = context.Request.Params["pcode"];
            string PdescStr = context.Request.Params["pdesc"];
            string PsizeStr = context.Request.Params["psize"];
            string PunitStr = context.Request.Params["punit"];
            float Checkamount = Convert.ToSingle(context.Request.Params["puramount"]);
            string sqlStr = string.Empty;



            SqlParameter[] spara = new SqlParameter[]
            {
                new SqlParameter("@code",CodeStr),
                new SqlParameter("@pcode",PcodeStr),
                new SqlParameter("@desc",PdescStr),
                new SqlParameter("@size",PsizeStr),
                new SqlParameter("@unit",PunitStr),
                new SqlParameter("@purchaseamount",Checkamount)
                
            };

            if (TypeStr == "80")
            {
                sqlStr = "insert into bjform_d (code, pcode, purchaseamount) values(@code, @pcode, @purchaseamount)";
            }
            else
            {
                sqlStr = "insert into bjform_d_others (code,description,size,unit,purchaseamount) values (@code,@desc,@size,@unit,@purchaseamount)";
            }

            try
            {
                int resultcode = KDATA.ExecuteNonQuery(sqlStr, spara);
                if (resultcode > 0)
                    responsetxt = "已保存";
                else
                    responsetxt = "保存有误，联系管理员";
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return responsetxt; 

        }

        public string DelBjFormDetail()
        {
            string responsetxt = string.Empty;
            HttpContextBase context = this.HttpContext;
            string codestr = context.Request.Params["code"];
            string typestr = context.Request.Params["type"];
            string pcodestr = context.Request.Params["pcode"];
            string[] pcodearray = pcodestr.Split('|');
            string pcodesql = string.Empty;
            string tablesql = string.Empty;
            foreach(string pcode in pcodearray)
            {
                pcodesql = pcodesql + ",'" + pcode + "'";
            }
            pcodesql = pcodesql.Substring(1);

            if (typestr == "材料")
                tablesql = "delete bjform_d where code='" + codestr.Trim() + "' and pcode in (" + pcodesql + ")";
            else
                tablesql = "delete bjform_d_others where code='" + codestr.Trim() + "' and description in (" + pcodesql + ")";

            try
            {
                int resultcode = KDATA.ExecuteNonQuery(tablesql);
                if (resultcode > 0)
                    responsetxt = "已保存";
                else
                    responsetxt = "保存有误，联系管理员";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        

            return responsetxt;
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


            dt = KDATA.GetDataTable(sqlStr,spara);
            string returnjson = JsonConvert.SerializeObject(dt);
            return returnjson;
        }

        public ActionResult BjReport()
        {
            HttpContextBase context = this.HttpContext;
            string CodeStr = context.Request.Params["code"];

            return new PdfResult(CodeStr, "BjReport");
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
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


namespace jqgrid1.Controllers
{
    public class DefaultController : Controller
    {
        // GET: Default
        public ActionResult Index()
        {
            return View();
        }



        public string GetDataFromStockinfo()
        {
            //HttpContextBase context = this.HttpContext;
           // NameValueCollection forms = context.Request.Form;
            DataTable dt = KDATA.GetDataTable("select code,codetype,productmodel from barcodeelement ");
            string returnjson= JsonConvert.SerializeObject(dt);
            return returnjson;
        }

        public void EditDataFromStockinfo()
        {
            HttpContextBase context = this.HttpContext;
            NameValueCollection forms = context.Request.Form;
            string operationCode = forms.Get("oper");
            string submitstring = string.Empty;

            switch (operationCode)
            {
                case "edit":
                    submitstring = forms.Get("code").ToString() + " " + forms.Get("codetype").ToString() + " " + forms.Get("productmodel").ToString();
                    break;
                case "add":
                    break;
                case "del":
                    break;
                default:
                    break;
            }
        }
    }
}
using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.BLL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace First_Aid_Made_Easy.Filters
{

    //public class Authorize : AuthorizeAttribute
    //{
    //    protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
    //    {
    //        filterContext.Result = new ViewResult
    //        {
    //            ViewName = "~/Views/Shared/NotAllowed.cshtml"
    //        };
    //    }
    //}

    public class BrowserFilter : ActionFilterAttribute, IActionFilter
    {

        void IActionFilter.OnActionExecuting(ActionExecutingContext filterContext)
        {

            HttpRequestBase rq = filterContext.RequestContext.HttpContext.Request;

            #region Browser Filter

            //System.Web.HttpRequestBase rq = filterContext.RequestContext.HttpContext.Request;
            //string brname = rq.Browser.Browser;
            //string brID = rq.Browser.Id;
            //string brname4 = rq.UserAgent;
            //bool flag = true;
            //if (brname == "Chrome" || brname == "Safari")
            //{
            //    flag = brname4.Contains("UBrowser") ||
            //           brname4.Contains("Puffin") ||
            //           brname4.Contains("OPR");
            //}

            //using (FAMEEntities db = new FAMEEntities())
            //{
            //    if (!db.tbl_UserAgent.Any(x => x.UserAgent == brname4))
            //    {
            //        db.tbl_UserAgent.Add(new tbl_UserAgent()
            //        {
            //            UserAgent = brname4,
            //            NameID = brname + " *** " + brID,
            //            Datetime = Common.GetCurrentDate(),
            //            Flag = !flag
            //        });

            //        db.SaveChanges();
            //    }
            //}

            //if (flag)
            //{
            //    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary{{ "controller", "Error" },
            //                             { "action", "NotAllowed" }

            //                             });
            //}
            #endregion

            #region
            //using (FAMEEntities db = new FAMEEntities())
            //{

            //    System.Web.HttpRequestBase rq = filterContext.RequestContext.HttpContext.Request;
            //    string brname = rq.Browser.Browser;
            //    string brname4 = rq.UserAgent;
            //    //bool flag = db.tbl_Browser.Where(x => x.isAllowed ?? false).Any(x=>x.Browser_NameVersion==brname);
            //    bool flag = true;
            //    if(brname == "Chrome"||brname=="Safari")
            //    {
            //        flag = brname4.Contains("Edg") || brname4.Contains("UBrowser") || brname4.Contains("Opera");
            //    }
            //    if (flag)
            //    {
            //        filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary{{ "controller", "Error" },
            //                             { "action", "NotAllowed" }

            //                             });
            //    }

            //}
            //using (FAMEEntities storeDb = new FAMEEntities())
            //{
            //    ActionLog log = new ActionLog()
            //    {
            //        Controller = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName,
            //        Action = string.Concat(filterContext.ActionDescriptor.ActionName, " (Logged By: Browser Filter)"),
            //        IP = filterContext.HttpContext.Request.UserHostAddress,
            //        DateTime = filterContext.HttpContext.Timestamp
            //    };
            //    storeDb.ActionLogs.Add(log);
            //    storeDb.SaveChanges();
            //    OnActionExecuting(filterContext);
            //}

            //string ip = filterContext.RequestContext.HttpContext.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            //if (!string.IsNullOrEmpty(ip))
            //{
            //    if (ip.IndexOf(",") > 0)
            //    {
            //        string[] ipRange = ip.Split(',');
            //        int le = ipRange.Length - 1;
            //        ip = ipRange[le];
            //    }
            //}
            //else
            //{
            //    ip = filterContext.RequestContext.HttpContext.Request.UserHostAddress;
            //}
            //System.Web.HttpBrowserCapabilitiesBase browser = filterContext.RequestContext.HttpContext.Request.Browser;
            //string brw_info = "Browser Capabilities\n"
            //    + "Type = " + browser.Type + "\n"
            //    + "Name = " + browser.Browser + "\n"
            //    + "Version = " + browser.Version + "\n"
            //    + "Major Version = " + browser.MajorVersion + "\n"
            //    + "Minor Version = " + browser.MinorVersion + "\n"
            //    + "Platform = " + browser.Platform + "\n"
            //    + "Is Beta = " + browser.Beta + "\n"
            //    + "Is Crawler = " + browser.Crawler + "\n"
            //    + "Is AOL = " + browser.AOL + "\n"
            //    + "Is Win16 = " + browser.Win16 + "\n"
            //    + "Is Win32 = " + browser.Win32 + "\n"
            //    + "Supports Frames = " + browser.Frames + "\n"
            //    + "Supports Tables = " + browser.Tables + "\n"
            //    + "Supports Cookies = " + browser.Cookies + "\n"
            //    + "Supports VBScript = " + browser.VBScript + "\n"
            //    + "Supports JavaScript = " +
            //        browser.EcmaScriptVersion.ToString() + "\n"
            //    + "Supports Java Applets = " + browser.JavaApplets + "\n"
            //    + "Supports ActiveX Controls = " + browser.ActiveXControls
            //          + "\n"
            //    + "Supports JavaScript Version = " +
            //        browser["JavaScriptVersion"] + "\n";
            //string brname4 = filterContext.RequestContext.HttpContext.Request.UserAgent;
            //string brname3 = filterContext.RequestContext.HttpContext.Request.Browser.ToString();
            //string brname1 = filterContext.RequestContext.HttpContext.Request.Browser.Id;
            //string brname5 = filterContext.RequestContext.HttpContext.Request.Browser.Platform;
            #endregion
        }
    }
}
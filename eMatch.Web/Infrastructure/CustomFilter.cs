using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Web.UI.WebControls;

namespace eMatch.Web.Infrastructure
{

    /// <summary>
    /// Custom filter to only allow logged in "Customers" to access the customer controller actions
    /// usage: [CustomerFilter]
    /// </summary>
    public class CustomerFilterAttribute : FilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            var userTypeCookie = filterContext.RequestContext.HttpContext.Request.Cookies["em"];

            if (Object.Equals(null, userTypeCookie) || userTypeCookie["type"] != "customer")
            {
                var routeDictionary = new RouteValueDictionary { { "action", "Login" }, { "controller", "Home" } };
                filterContext.Result = new RedirectToRouteResult(routeDictionary);
            }            
        }
    }

    /// <summary>
    /// Custom filter to only allow logged in "Merchants" to access the merchant controller actions
    /// usage: [MerchantFilter]
    /// </summary>
    public class MerchantFilterAttribute : FilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            var userTypeCookie = filterContext.RequestContext.HttpContext.Request.Cookies["em"];

            if (Object.Equals(null, userTypeCookie)|| userTypeCookie["type"] != "merchant")
            {
                var routeDictionary = new RouteValueDictionary { { "action", "Login" }, { "controller", "Home" } };
                filterContext.Result = new RedirectToRouteResult(routeDictionary);
            }
        }
    }

    //Leaving for reference, we may want a more secure cookie attribute?
    //public class LoggedInFilterAttribute : FilterAttribute, IAuthorizationFilter
    //{
    //    public void OnAuthorization(AuthorizationContext filterContext)
    //    {
    //        var authCookie = filterContext.RequestContext.HttpContext.Request.Cookies[FormsAuthentication.FormsCookieName];
    //        //FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value);
    //        //UserIdentity identity = newUserIdentity(ticket.Name);
    //        //UserPrincipal principal = newUserPrincipal(identity);
    //        //HttpContext.Current.User = principal;

    //        if (Object.Equals(null,authCookie))
    //        {
    //            var routeDictionary = new RouteValueDictionary { { "action", "Login" }, { "controller", "Home" } };
    //            filterContext.Result = new RedirectToRouteResult(routeDictionary);
    //        }
    //    }
    //}
}
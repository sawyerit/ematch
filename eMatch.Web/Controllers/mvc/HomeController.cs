using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using eMatch.Engine.Enitities.Users;
using System.Web.Mvc;
using eMatch.Engine.Services.Interfaces;
using eMatch.Web.Infrastructure;

namespace eMatch.Web.Controllers.mvc
{
    public class HomeController : Controller
    {
        private readonly IUserService _user;
        private readonly ISessionService _session;
        //private ILogger _logger;

        public HomeController(IUserService userService, ISessionService sessService)
        {
            _user = userService;
            _session = sessService;
        }

        public ActionResult Index()
        {
            return RedirectToAction("Login");
        }

        public ActionResult Login()
        {
            ViewBag.Title = "eMatch - Login Page";
            ViewBag.ErrorText = TempData["ErrorText"];

            return View();
        }

        [AcceptVerbs("POST")]
        public RedirectToRouteResult CreateAccount(User user)
        {
            _user.CreateUser(user);

            SetLoginCookie(user, false);
            _session.eMatchUser = user;

            return user.IsMerchant ?
                    RedirectToAction("Profile", "Merchant")
                    : RedirectToAction("Profile", "Customer");
        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public JsonResult DoesUserNameExist(string userEmailAddress)
        {
            return new JsonResult
            {
                Data = _user.DoesEmailExist(userEmailAddress),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        [AcceptVerbs("POST")]
        public ActionResult LoginUser(User user)
        {
            User userRetrieved = _user.GetUserByEmailAndPwd(user.Email, user.Password);

            if (!Object.Equals(null, userRetrieved))
            {
                //check the "remember me" value via the request object
                SetLoginCookie(userRetrieved, !Object.Equals(null, Request["remember-me"]));
                _session.eMatchUser = userRetrieved;

                //redirect to the appropriate profile based on user data
                return userRetrieved.IsMerchant ?
                    RedirectToAction("Profile", "Merchant")
                    : RedirectToAction("Offers", "Customer");
            }
            else
            {
                TempData["ErrorText"] = "Oops, either the name or password was incorrect.";
                return RedirectToAction("Login");
            }
        }

        public ActionResult LogoutUser()
        {
            //removes all cookies. May not want to remove all in the future
            string[] myCookies = Request.Cookies.AllKeys;
            foreach (string cookie in myCookies)
            {
                Response.Cookies[cookie].Expires = DateTime.Now.AddDays(-1);
            }

            //User user = System.Web.HttpContext.Current.Session["user"] as User;
            //HttpCookie cookie = new HttpCookie("em");
            //cookie["id"] = user.Id;
            //cookie["type"] = user.IsMerchant ? "merchant" : "customer";
            //cookie.Expires = DateTime.Now.AddDays(-1); 
            //ControllerContext.HttpContext.Response.Cookies.Add(cookie);

            return RedirectToAction("Login");
        }

        #region Helper methods

        private void SetLoginCookie(User user, bool persistent)
        {
            HttpCookie cookie = new HttpCookie("em");
            cookie["id"] = user.Id;
            cookie["type"] = user.IsMerchant ? "merchant" : "customer";
            cookie.Expires = persistent ? DateTime.Now.AddMonths(1) : DateTime.Today.AddDays(1);
            ControllerContext.HttpContext.Response.Cookies.Add(cookie);
        }

        #endregion
    }

}

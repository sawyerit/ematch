using eMatch.Engine.Enitities.Offers;
using eMatch.Engine.Enitities.Users;
using eMatch.Engine.Enitities.Campaigns;
using eMatch.Engine.Services.Interfaces;
using eMatch.Web.Infrastructure;
using eMatch.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace eMatch.Web.Controllers.mvc
{
    [MerchantFilter]
    public class MerchantController : Controller
    {
        private readonly IUserService _user;
        private readonly IOfferService _offer;
        private readonly ISessionService _session;

        public MerchantController(IUserService user, IOfferService offer, ISessionService sessService)
        {
            _user = user;
            _offer = offer;
            _session = sessService;
        }

        #region VIEW Actions

        public new ActionResult Profile()
        {
            User user = _session.eMatchUser;

            ViewBag.User = user.FirstName + " " + user.LastName;
            ViewBag.Title = "eMatch - My Merchant Profile";

            ViewBag.WasUpdated = TempData["WasUpdated"];

            return View(new MerchantViewModel { User = user, Profile = _user.GetProfile(user.Id) });
        }

        public ActionResult Campaign(string id)
        {
            User user = _session.eMatchUser;

            if (!user.TCAgreed) return RedirectToAction("Profile");

            ViewBag.User = user.FirstName + " " + user.LastName;
            ViewBag.Title = "eMatch - My Campaign Manager";

            Profile p = _user.GetProfile(user.Id);
            Campaign c = _offer.GetCampaignByProfileId(p.Id);

            MerchantViewModel mvm = new MerchantViewModel
            {
                Profile = p,
                User = user,
                Campaign = c,
                CurrentOffer = (id != null ? _offer.GetOffer(id) : new Offer())
            };
            return View(mvm);
        }

        public ActionResult Contact()
        {
            User user = _session.eMatchUser;
            if (!user.TCAgreed) return RedirectToAction("Profile");

            ViewBag.Title = "eMatch - Contact Manager";
            ViewBag.User = _session.eMatchUser.FirstName + " " + _session.eMatchUser.LastName;
            return View();
        }

        public ActionResult Dashboard()
        {
            User user = _session.eMatchUser;
            if (!user.TCAgreed) return RedirectToAction("Profile");

            ViewBag.Title = "eMatch - Dashboard";
            ViewBag.User = _session.eMatchUser.FirstName + " " + _session.eMatchUser.LastName;
            return View();
        }

        public ActionResult Virtual()
        {
            User user = _session.eMatchUser;
            if (!user.TCAgreed) return RedirectToAction("Profile");

            ViewBag.Title = "eMatch - Virtual Office";
            ViewBag.User = _session.eMatchUser.FirstName + " " + _session.eMatchUser.LastName;
            return View();
        }

        public ActionResult Refer()
        {
            User user = _session.eMatchUser;
            if (!user.TCAgreed) return RedirectToAction("Profile");

            ViewBag.Title = "eMatch - Refer a Merchant";
            ViewBag.User = _session.eMatchUser.FirstName + " " + _session.eMatchUser.LastName;
            return View();
        }
        
        #endregion

        #region GET Actions

        public ActionResult ExpireOffer(string id)
        {
            _offer.ExpireOffer(id);

            return RedirectToAction("Campaign");
        }

        public ActionResult DeleteOffer(string id)
        {
            _offer.InactivateOffer(id);

            return RedirectToAction("Campaign");
        }

        public ActionResult ActivateOffer(string id)
        {
            _offer.ActivateOffer(id);

            return RedirectToAction("Campaign");
        }

        #endregion

        #region AJAX Actions

        public JsonResult GetKeywords(string category)
        {
            return new JsonResult
            {
                Data = OfferMetaData.OfferCategories[category],
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        #endregion

        #region POSTS

        [AcceptVerbs("POST")]
        public ActionResult CreateOffer(Offer offer)
        {
            User user = _session.eMatchUser;

            //TODO: this needs clean up and safety checking
            offer.Expires = Object.Equals(null, Request["neverExpires"]) ? Convert.ToDateTime(Request["offerExpires"].ToString()) : DateTime.MaxValue;
            offer.Keywords = new List<string>();
            foreach (string item in Request["offerKeywords"].ToString().Split(','))
                offer.Keywords.Add(item);

            offer.ProfileId = _user.GetProfileID(user.Id);
            _offer.CreateOffer(offer);

            return RedirectToAction("Campaign");
        }

        [AcceptVerbs("POST")]
        public ActionResult UpdateProfile(User user)
        {
            //These values are not sent to the client and back to make things safer.
            //So, they are not part of the form.  
            //Since the SaveUser call needs the entire object, we need to feed it a fully inflated User object
            user.Id = _session.UserId;
            user.IsMerchant = _session.IsMerchant;
            user.TCAgreed = _session.eMatchUser.TCAgreed;

            _session.eMatchUser = user;
            _user.UpdateUser(user);

            Profile p = _user.GetProfile(user.Id);
            p.BusinessName = Request["businessname"].ToString();
            p.WebsiteURL = Request["websiteurl"].ToString();
            _user.SaveProfile(p);

            TempData["WasUpdated"] = true;

            return RedirectToAction("Profile");
        }

        [AcceptVerbs("POST")]
        public ActionResult Accept()
        {
            User user = _session.eMatchUser;
            user.TCAgreed = true;
            _session.eMatchUser = _user.UpdateUser(user);

            return RedirectToAction("Profile");
        }

        #endregion
    }
}
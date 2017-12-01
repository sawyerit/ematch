using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eMatch.Engine.Enitities.Users;
using eMatch.Web.Infrastructure;
using eMatch.Engine.Services.Interfaces;
using eMatch.Web.Models;
using System.Web.Routing;

namespace eMatch.Web.Controllers.mvc
{
    [CustomerFilter]
    public class CustomerController : Controller
    {
        private readonly IUserService _user;
        private readonly IOfferService _offer;
        private readonly ISessionService _session;
        private readonly IMatchService _match;

        public CustomerController(IUserService user, IOfferService offer, ISessionService sessService, IMatchService match)
        {
            _user = user;
            _offer = offer;
            _session = sessService;
            _match = match;
        }

        public new ActionResult Profile()
        {
            User user = _session.eMatchUser;

            ViewBag.User = user.FirstName + " " + user.LastName;
            ViewBag.Title = "eMatch - My Customer Profile";
            ViewBag.WasUpdated = TempData["WasUpdated"];

            return View(new CustomerViewModel { User = user, Profile = _user.GetProfile(user.Id) });

        }

        public ActionResult Offers()
        {
            User user = _session.eMatchUser;

            ViewBag.User = user.FirstName + " " + user.LastName;
            ViewBag.Title = "eMatch - My Offers";

            return View(new CustomerViewModel { User = user
                                                , Profile = _user.GetProfile(user.Id)
                                                , MatchingOffers = _match.GetActiveMatchesForUser(user.Id) });
        }

        public ActionResult Lounge()
        {
            User user = _session.eMatchUser;

            ViewBag.User = user.FirstName + " " + user.LastName;
            ViewBag.Title = "eMatch - My Lounge";

            return View();
        }

        public ActionResult Search()
        {
            User user = _session.eMatchUser;

            ViewBag.User = user.FirstName + " " + user.LastName;
            ViewBag.Title = "eMatch - Advanced Search";

            return View();
        }

        public ActionResult Watch()
        {
            User user = _session.eMatchUser;

            ViewBag.User = user.FirstName + " " + user.LastName;
            ViewBag.Title = "eMatch - My Watchlist";

            return View();
        }

        public ActionResult Preferences(string category)
        {
            User user = _session.eMatchUser;

            ViewBag.User = user.FirstName + " " + user.LastName;
            ViewBag.Title = "eMatch - My Preferences";
            category = string.IsNullOrEmpty(category) ? OfferMetaData.OfferCategories.Keys.First() : category;
            Profile profile = _user.GetProfile(user.Id);

            //This loops all categories and builds a filtered dictionary. We're only displaying one right now,
            //but I'm leaving this in case we want to show more than one category at once

            //filter the static category keywords so we dont' display whats already in the user pref keywords
            Dictionary<string, List<string>> filtered = new Dictionary<string, List<string>>();
            foreach (KeyValuePair<string, List<string>> cat in OfferMetaData.OfferCategories)
            {
                //get the users preference by category
                Preference pref = profile.Preferences.Where(x => x.Category == cat.Key).FirstOrDefault();
                //add users keywords to the merge list
                List<string> filteredKeywords = new List<string>();
                if (Object.Equals(null, pref)) pref = new Preference { Keywords = new List<string>(), Category = cat.Key };

                foreach (var item in cat.Value)
                    if (!pref.Keywords.Contains(item))
                        filteredKeywords.Add(item);
                //add the whole thing to the new merged dict
                filtered.Add(cat.Key, filteredKeywords);
            }

            return View(new CustomerViewModel
                        {
                            User = user
                            , Profile = profile
                            , CurrentCategory = category
                            , CurrentPreference = profile.Preferences.Where(x => x.Category == category).FirstOrDefault()
                            , FilteredCategories = filtered
                        });
        }

        [AcceptVerbs("POST")]
        public ActionResult UpdatePreferences(FormCollection values)
        {
            //todo: save the chosen values to the user profile cat/keyword list
            Profile p = _user.GetProfile(_session.UserId);
            string curCategory = Request["hidCurrentCategory"].ToString();

            Preference curPreference = p.Preferences.Where(x => x.Category == curCategory).FirstOrDefault();
            if (Object.Equals(null, curPreference))
            {
                curPreference = new Preference { Category = curCategory, Keywords = new List<string>() };
                p.Preferences.Add(curPreference);
            }

            curPreference.Keywords.Clear();

            foreach (var item in values)
            {
                //only checked items are passed in, so we can rebuild the list safely by the request objects
                if (item.ToString().StartsWith("chk"))
                    curPreference.Keywords.Add(Request[item.ToString()]);
            }

            if (curPreference.Keywords.Count == 0)
            {
                //removing category from preferences if the category has no key words associated with it
                var currentPreference = p.Preferences.FirstOrDefault(x => x.Category == curCategory);
                p.Preferences.Remove(currentPreference);
            }

            _user.SaveProfile(p);

            return RedirectToAction("Preferences", new { category = curCategory });
        }


        [AcceptVerbs("POST")]
        public ActionResult UpdateProfile(User user)
        {
            user.Id = _session.UserId;
            user.IsMerchant = false;

            //Get users notification settings
            user.MyNotifications = (Request["chkEmail"] != null ? Notifications.Email : 0)
                                    | (Request["chkSMS"] != null ? Notifications.SMS : 0)
                                    | (Request["chkMobileApp"] != null ? Notifications.MobileApp : 0);

            user.MyMemberLevel = (MembershipLevel)Enum.Parse(typeof(MembershipLevel), Request["selMemberLevel"].ToString());

            //save to session since its been updated
            _session.eMatchUser = user;
            _user.UpdateUser(user);

            Profile p = _user.GetProfile(user.Id);
            _user.SaveProfile(p);

            TempData["WasUpdated"] = true;

            return RedirectToAction("Profile");
        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")] //IE was caching the ajax call
        public JsonResult GetOfferCount()
        {
            return new JsonResult
            {
                //GetActiveMatchCount will get and enumerate all matches. This is prob inefficient to get a "count"
                //every 5 seconds. In the future, a different method should prob be defined for just a count.
                Data = _match.GetActiveMatchCount(_session.UserId),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
    }

}

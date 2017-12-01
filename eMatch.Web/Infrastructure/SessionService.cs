using eMatch.Engine.Enitities.Offers;
using eMatch.Engine.Enitities.Users;
using eMatch.Engine.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Web;

namespace eMatch.Web.Infrastructure
{
    public class SessionService : ISessionService
    {
        IUserService _users;
        IOfferService _offers;

        public SessionService(IUserService users, IOfferService offers)
        {
            _users = users;
            _offers = offers;
        }

        private static string _user = "User";

        public User eMatchUser
        {
            get
            {
                User u = HttpContext.Current.Session[SessionService._user] as User;
                if (Object.Equals(null, u))
                {
                    HttpCookie cookie = HttpContext.Current.Request.Cookies["em"];
                    u = _users.GetUser(cookie["id"]);
                    eMatchUser = u;
                }

                return u;
            }
            set
            {
                HttpContext.Current.Session[SessionService._user] = value;
            }
        }

        public string UserId
        {
            get
            {
                return eMatchUser.Id;
            }
        }

        public bool IsMerchant
        {
            get
            {
                return eMatchUser.IsMerchant;
            }
        }
    }
}

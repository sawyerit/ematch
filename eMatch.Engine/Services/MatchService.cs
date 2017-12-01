using eMatch.Engine.Enitities.Offers;
using eMatch.Engine.Enitities.Users;
using eMatch.Engine.Services.Interfaces;
using SendGrid;
using SendGrid.Transport;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Linq;

namespace eMatch.Engine.Services
{
    public class MatchService : IMatchService
    {
        IUserService _users;
        IOfferService _offers;

        public MatchService(IUserService users, IOfferService offers)
        {
            _users = users;
            _offers = offers;
        }

        public Dictionary<string, List<Offer>> GetMatchesForUser(string userId)
        {
            Dictionary<string, List<Offer>> matchedOffers = new Dictionary<string, List<Offer>>();
            var profile = _users.GetProfile(userId);

            foreach (var preference in profile.Preferences)
            {
                if (!matchedOffers.ContainsKey(preference.Category))
                    matchedOffers.Add(preference.Category, new List<Offer>());

                matchedOffers[preference.Category].AddRange(_offers.GetOffers(preference.Category, preference.Keywords));
            }

            return matchedOffers;
        }

        public Dictionary<string, List<Offer>> GetActiveMatchesForUser(string userId)
        {
            //maybe not the best place to do this, but it needs to happen so we don't match expired offers
            SearchService.ClearAllExpiredLuceneOffers(_offers.GetExpiredOfferIds());

            Dictionary<string, List<Offer>> matchedOffers = new Dictionary<string, List<Offer>>();
            var profile = _users.GetProfile(userId);

            foreach (var preference in profile.Preferences)
            {
                if (!matchedOffers.ContainsKey(preference.Category))
                    matchedOffers.Add(preference.Category, new List<Offer>());

                if (preference.Keywords.Count > 0)
                {
                    List<Offer> foundList = SearchService.Search(preference.Category + ", " + string.Join(", ", preference.Keywords)).ToList();
                    if (foundList.Count == 0)
                    {
                        foundList = SearchService.FuzzySearch(string.Join(", ", preference.Keywords)).ToList();
                    }
                    matchedOffers[preference.Category].AddRange(foundList.Where(x => x.Category == preference.Category));
                }
            }

            return matchedOffers;
        }

        public Dictionary<string, List<Offer>> GetNewMatchesForUser(string userId)
        {
            //TODO: Determine "new" matches, maybe we need to store a "last login date" on the user object
            //so we can determine what a NEW offer is.  We don't want to send a notification for every new offer all the time

            Dictionary<string, List<Offer>> matchedOffers = new Dictionary<string, List<Offer>>();
            var profile = _users.GetProfile(userId);

            //foreach (var preference in profile.Preferences)
            //{
            //    if (!matchedOffers.ContainsKey(preference.Category))
            //        matchedOffers.Add(preference.Category, new List<Offer>());

            //    matchedOffers[preference.Category].AddRange(_offers.GetActiveOffers(preference.Category, preference.Keywords));
            //}

            return matchedOffers;
        }

        public List<User> GetMatchesForOffer(string offerId)
        {
            //THIS WORKS, so we can send an email every time a new offer is created by a merchant using this method.
            //also see GetNewMatchesForUser

            //SendNotification(userId);

            throw new NotImplementedException();
        }

        public int GetActiveMatchCount(string userid)
        {
            int offerCount = 0;

            foreach (var item in GetActiveMatchesForUser(userid))
            {
                offerCount += item.Value.Count;
            }

            return offerCount;
        }


        private void SendNotification(string userId)
        {
            // Create the email object first, then add the properties.
            Mail myMessage = SendGrid.Mail.GetInstance();

            // Add the message properties.
            MailAddress sender = new MailAddress(@"eMatch <no_reply@ematchprototype.azurewebsites.net>");

            myMessage.From = sender;
            myMessage.AddTo(_users.GetUser(userId).Email);
            myMessage.Html = "<h2>eMatch.com</h2><br/><br/><p>Great news! We've found " + GetActiveMatchCount(userId) + " matched offers just for you!</p><br/><br/><p><a href='http://ematchprototype.azurewebsites.net'>Click here</a> to login and view your offer.</p>";
            myMessage.Subject = "New matched offers from eMatch!";

            var credentials = new NetworkCredential("azure_f25e34426d0d2b9142545e6518d024dd@azure.com", "YYg6fBsqWGVgf1Z");
            var transportWeb = Web.GetInstance(credentials);

            // Send the email.
            transportWeb.Deliver(myMessage);
        }
    }
}

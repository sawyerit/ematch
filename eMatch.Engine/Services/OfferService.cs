using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using eMatch.Engine.Data;
using eMatch.Engine.Enitities.Campaigns;
using eMatch.Engine.Enitities.Offers;
using eMatch.Engine.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver.Linq;

namespace eMatch.Engine.Services
{
    public class OfferService : IOfferService
    {
        private readonly IOfferRepository _repo;

        public OfferService(IOfferRepository repo)
        {
            _repo = repo;
        }

        public Campaign CreateCampaign(Campaign campaign)
        {
            return _repo.SaveCampaign(campaign);
        }

        public Campaign GetCampaignByProfileId(string id)
        {
            Campaign c = new Campaign();
            c.CurrentOffers = _repo.Offers.Where(x => x.ProfileId == id && x.Expires >= DateTime.Now && x.Status == Offer.StatusType.Active).ToList();
            c.PendingOffers = _repo.Offers.Where(x => x.ProfileId == id && x.Status == Offer.StatusType.Pending).ToList();
            c.ExpiredOffers = _repo.Offers.Where(x => x.ProfileId == id && x.Expires < DateTime.Now && x.Status == Offer.StatusType.Active).ToList();

            foreach (var item in c.CurrentOffers)
                item.Expires = DateTime.SpecifyKind(item.Expires.Value, DateTimeKind.Utc).ToLocalTime();

            foreach (var item in c.ExpiredOffers)
                item.Expires = DateTime.SpecifyKind(item.Expires.Value, DateTimeKind.Utc).ToLocalTime();

            return c;
        }

        public Offer CreateOffer(Offer offer)
        {            
            return _repo.SaveOffer(offer);
        }

        public Offer GetOffer(string offerId)
        {
            Offer o = _repo.Offers.FirstOrDefault(x => x.Id == offerId);
            o.Expires = DateTime.SpecifyKind(o.Expires.Value, DateTimeKind.Utc).ToLocalTime();
            return o;
        }

        public List<Offer> GetAllOffers()
        {
            return _repo.Offers.ToList();
        }

        public List<Offer> GetOffers(string category, List<string> keywords)
        {
            return _repo.Offers.Where(x => x.Category == category && x.Keywords.ContainsAny(keywords)).ToList();
        }

        public List<Offer> GetActiveOffers(string category, List<string> keywords)
        {
            var query = (from o in _repo.Offers.AsQueryable<Offer>()
                         where o.Category == category
                         && o.Keywords.ContainsAny(keywords)
                         && o.Status == Offer.StatusType.Active
                         && o.Expires > DateTime.Now
                         select o);

            return query.ToList();

            //return _repo.Offers.Where(x => x.Category == category && x.Keywords.ContainsAny(keywords)).Where(x => x.Status == Offer.StatusType.Active).ToList();
        }

        public List<Offer> GetAllOffers(string profileId)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// For removal of expired offers from Lucene
        /// </summary>
        /// <returns></returns>
        public List<String> GetExpiredOfferIds()
        {
            var query = (from o in _repo.Offers.AsQueryable<Offer>()
                         where o.Expires < DateTime.Now
                         select o.Id);

            return query.ToList();
        }

        public void InactivateOffer(string offerId)
        {
            SearchService.ClearLuceneOfferRecord(offerId);
            _repo.InactivateOffer(offerId);
        }

        public void ActivateOffer(string offerId)
        {
            _repo.ActivateOffer(offerId);
            SearchService.AddUpdateLuceneIndex(GetOffer(offerId));
        }

        public void ExpireOffer(string offerId)
        {
            SearchService.ClearLuceneOfferRecord(offerId);
            _repo.ExpireOffer(offerId);
        }

        public void DeleteOffer(string offerId)
        {
            _repo.DeleteOffer(offerId);
        }

        public void IndexAllActiveOffers()
        {
            var query = (from o in _repo.Offers.AsQueryable<Offer>()
                         where o.Status == Offer.StatusType.Active && o.Expires > DateTime.Now
                         select o);

            SearchService.AddUpdateLuceneIndex(query);
        }
    }
}

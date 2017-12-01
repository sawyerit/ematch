using eMatch.Engine.Enitities.Campaigns;
using eMatch.Engine.Enitities.Offers;
using System.Collections.Generic;

namespace eMatch.Engine.Services.Interfaces
{
    public interface IOfferService
    {
        //Campaigns
        Campaign CreateCampaign(Campaign campaign);
        Campaign GetCampaignByProfileId(string id);
        
        //Offers
        Offer CreateOffer(Offer offer);
        Offer GetOffer(string offerId);
        List<Offer> GetAllOffers();
        //all offers by profile id
        List<Offer> GetAllOffers(string profileId);
        List<Offer> GetOffers(string category, List<string> keywords);
        List<Offer> GetActiveOffers(string category, List<string> keywords);
        
        void IndexAllActiveOffers();
        void InactivateOffer(string offerId);
        void ActivateOffer(string offerId);
        void DeleteOffer(string offerId);
        void ExpireOffer(string offerId);

        List<string> GetExpiredOfferIds();
    }
}

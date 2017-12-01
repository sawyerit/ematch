using eMatch.Engine.Enitities.Campaigns;
using eMatch.Engine.Enitities.Offers;
using System.Linq;

namespace eMatch.Engine.Data
{
    public interface IOfferRepository
    {
        IQueryable<Campaign> Campaigns { get; }
        Campaign SaveCampaign(Campaign campaign);
        void DeleteCampaign(string id);

        IQueryable<Offer> Offers { get; }
        Offer SaveOffer(Offer offer);
        void ActivateOffer(string id);
        void InactivateOffer(string id);
        void DeleteOffer(string id);
        void ExpireOffer(string id);
    }
}

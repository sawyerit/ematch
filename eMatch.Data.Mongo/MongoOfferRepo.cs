using eMatch.Engine.Data;
using eMatch.Engine.Enitities.Campaigns;
using eMatch.Engine.Enitities.Offers;
using System;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;
using System.Configuration;

namespace eMatch.Data.Mongo
{
    public class MongoOfferRepo : IOfferRepository
    {

        readonly MongoDatabase db;

        public MongoOfferRepo()
        {
            var client = new MongoClient(ConfigurationManager.ConnectionStrings["Mongo"].ToString());
            MongoServer server = client.GetServer();
            db = server.GetDatabase("ematch");
        }

        public IQueryable<Offer> Offers
        {
            get { return db.GetCollection<Offer>("offers").AsQueryable<Offer>(); }
        }

        public Offer SaveOffer(Offer offer)
        {
            var offers = db.GetCollection<Offer>("offers");
            offer.Status = Offer.StatusType.Pending;
            offers.Save(offer);
            return offer;
        }
        
        public void ActivateOffer(string id)
        {
            var offers = db.GetCollection<Offer>("offers");
            var query = Query.EQ("_id", ObjectId.Parse(id));
            var upd = Update.Set("Status", Offer.StatusType.Active);
            offers.Update(query, upd);
        }

        public void ExpireOffer(string id)
        {
            var offers = db.GetCollection<Offer>("offers");
            var query = Query.EQ("_id", ObjectId.Parse(id));
            var upd = Update.Set("Expires", DateTime.Now);
            offers.Update(query, upd);
        }

        public void InactivateOffer(string id)
        {
            var offers = db.GetCollection<Offer>("offers");
            var query = Query.EQ("_id", ObjectId.Parse(id));
            var upd = Update.Set("Status", Offer.StatusType.Inactive);
            offers.Update(query, upd);
        }

        public void DeleteOffer(string id)
        {
            var offers = db.GetCollection<Offer>("offers");
            var query = Query.EQ("_id", ObjectId.Parse(id));
            offers.Remove(query);
        }
        
        public IQueryable<Campaign> Campaigns
        {
            get { return db.GetCollection<Campaign>("campaigns").AsQueryable<Campaign>(); }
        }

        public Campaign SaveCampaign(Campaign campaign)
        {
            var campaigns = db.GetCollection<Campaign>("campaigns");
            campaigns.Save(campaign);
            return campaign;
        }

        public void DeleteCampaign(string id)
        {
            var campaigns = db.GetCollection<Campaign>("campaigns");
            var query = Query.EQ("_id", ObjectId.Parse(id));
            campaigns.Remove(query);
        }
    }
}

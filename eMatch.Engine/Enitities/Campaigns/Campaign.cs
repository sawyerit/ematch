using eMatch.Engine.Enitities.Accounts;
using eMatch.Engine.Enitities.Offers;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace eMatch.Engine.Enitities.Campaigns
{
    public class Campaign
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string ProfileId { get; set; }

        public List<Offer> CurrentOffers { get; set; }
        public List<Offer> PendingOffers { get; set; }
        public List<Offer> ExpiredOffers { get; set; }
    }
}

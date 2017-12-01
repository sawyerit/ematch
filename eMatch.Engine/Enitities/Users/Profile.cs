using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace eMatch.Engine.Enitities.Users
{
    public class Profile
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }

        public string BusinessName { get; set; }
        public string WebsiteURL { get; set; }

        public List<Preference> Preferences { get; set; }

        public Profile()
        {
            Preferences = new List<Preference>();
        }
    }
}

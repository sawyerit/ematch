using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace eMatch.Engine.Enitities.Users
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Password { get; set; }
        public int Zip { get; set; }
        public bool IsMerchant { get; set; }
        public bool TCAgreed { get; set; }
        public Notifications MyNotifications {get; set;}
        public MembershipLevel MyMemberLevel { get; set; }
    }

    [Flags]
    public enum Notifications
    {
        None = 0,
        Email = 1,
        SMS = 2,
        MobileApp = 4,
        SMSAndEmail = SMS | Email,
        EmailAndMobileApp = Email | MobileApp,
        SMSAndMobileApp = SMS | MobileApp,
        EmailAndSMSAndMobile = Email | SMS | MobileApp
    }

    public enum MembershipLevel
    {
        Basic,
        Premium
    }
}

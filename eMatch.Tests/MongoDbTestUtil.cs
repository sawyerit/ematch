using System;
using System.Collections.Generic;
using eMatch.Engine.Enitities.Offers;
using eMatch.Engine.Enitities.Users;
using MongoDB.Bson;

namespace eMatch.Tests
{
    class MongoDbTestUtil
    {
        public static User CreateUserCustomer()
        {
            User user = new User
            {
                Address = "1234 West Apple Dr",
                City = "Westminster",
                State = "CO",
                Email = "jebtester@jeb.com",
                FirstName = "Jeb",
                LastName = "Baz",
                Id = null,
                IsMerchant = false,
                Password = "dude",
                Phone = "3035551212"
            };
            return user;
        }

        public static User CreateUserMerchant()
        {
            User user = new User
            {
                Address = "555 East Orange Circle",
                City = "Arvada",
                State = "CO",
                Email = "nej@nej.com",
                FirstName = "Nej",
                LastName = "Sissero",
                Id = ObjectId.GenerateNewId().ToString(),
                IsMerchant = false,
                Password = "dudet",
                Phone = "7209998877"
            };
            return user;
        }

        public static Profile CreateProfileWithPreferences(string userId = null, string profileId = null, string category = null, List<string> keyWords = null)
        {
            if (string.IsNullOrEmpty(userId))
            {
                userId = ObjectId.GenerateNewId().ToString();
            }

            if (string.IsNullOrEmpty(profileId))
            {
                profileId = ObjectId.GenerateNewId().ToString();
            }

            if (string.IsNullOrEmpty(category))
            {
                category = "discounts";
            }

            if (keyWords == null)
            {
                keyWords = new List<string>() { "coffee", "gofastjuice" };
            }

            var profile = new Profile
            {
                Id = profileId,
                UserId = userId,

                Preferences = new List<Preference>
                {
                    new Preference
                    {
                        Category = category,
                        Keywords = keyWords
                    }
                }
            };
            return profile;
        }

        public static Profile CreateProfileWithoutPreferences()
        {
            var profile = new Profile
            {
                Id = ObjectId.GenerateNewId().ToString(),
                UserId = ObjectId.GenerateNewId().ToString()
            };
            return profile;
        }

        public static Offer CreateOffer(string offerName = null, string createdBy = null, double daysFromNowToExpire = 10, Offer.StatusType status = Offer.StatusType.Pending)
        {
            if (string.IsNullOrEmpty(offerName))
            {
                offerName = "Duck n Coffee discount";
            }

            if (string.IsNullOrEmpty(createdBy))
            {
                createdBy = ObjectId.GenerateNewId().ToString();
            }

            var offer = new Offer
            {
                Id = ObjectId.GenerateNewId().ToString(),
                ProfileId = createdBy = ObjectId.GenerateNewId().ToString(),
                Category = "discounts",
                CreatedBy = createdBy,
                Description = "20% off coffee when you buy a duck~ O< quack!",
                Details = new Dictionary<string, string>
                {
                    {"percent", "20"},
                    {"code", "QASE3456G"}
                },
                Expires = DateTime.Now.AddDays(daysFromNowToExpire),
                Keywords = new List<string>
                {
                    "discounts",
                    "coffee",
                    "joe",
                    "gojuice",
                    "gofastjuice",
                    "get some"
                },
                Name = offerName,
                Status = status,
                IsRecurring = false
            };
            return offer;
        }

        public static Offer CreateStemmingOffer(string offerName = null, string createdBy = null, double daysFromNowToExpire = 10, Offer.StatusType status = Offer.StatusType.Pending)
        {
            if (string.IsNullOrEmpty(offerName))
            {
                offerName = "Early Bird Drink Special";
            }

            if (string.IsNullOrEmpty(createdBy))
            {
                createdBy = ObjectId.GenerateNewId().ToString();
            }

            var offer = new Offer
            {
                Id = ObjectId.GenerateNewId().ToString(),
                ProfileId = createdBy = ObjectId.GenerateNewId().ToString(),
                Category = "Drinks",
                CreatedBy = createdBy,
                Description = "Get your drink on early with our early bird happy hour from 3-5pm",
                Details = new Dictionary<string, string>
                {
                    {"percent", "20"},
                    {"code", "QASE3456G"}
                },
                Expires = DateTime.Now.AddDays(daysFromNowToExpire),
                Keywords = new List<string>
                {
                    "drink",
                    "gojuice",
                    "gofastjuice"
                },
                Name = offerName,
                Status = status,
                IsRecurring = false
            };
            return offer;
        }




    }
}

using eMatch.Engine.Enitities.Users;
using eMatch.Engine.Enitities.Offers;
using eMatch.Engine.Enitities.Campaigns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
namespace eMatch.Web.Models
{
    public class CustomerViewModel
    {
        public User User { get; set; }
        public Profile Profile { get; set; }
        public Offer CurrentOffer { get; set; }
        public Dictionary<string, List<Offer>> MatchingOffers { get; set; }
        public string CurrentCategory { get; set; }
        public Preference CurrentPreference { get; set; }
        
        public SelectList OfferCategoriesSelectList
        {
            get
            {
                return OfferMetaData.OfferCategorySelectList;
            }
        }
        public Dictionary<string, List<string>> Categories
        {
            get
            {
                return OfferMetaData.OfferCategories;
            }
        }
        public Dictionary<string, List<string>> FilteredCategories{get;set;}
    }
}
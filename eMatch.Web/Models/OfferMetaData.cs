using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;

namespace eMatch.Web.Models
{
    public static class OfferMetaData
    {
        public static SelectList OfferCategorySelectList
        {
            get
            {
                List<SelectListItem> selList = OfferCategories.Select(v => new SelectListItem
                {
                    Text = v.Key.ToString(),
                    Value = v.Key.ToString()
                }).ToList();

                selList.Insert(0, new SelectListItem { Text = "-- Select a Category --", Value = "" });

                return new SelectList(selList, "Value", "Text", "");
            }
        }

        public static Dictionary<string, List<string>> OfferCategories
        {
            get
            {
                return new Dictionary<string, List<string>> 
                { 
                    {"Hotels", new List<string>{"3 star and up", "2 star and up", "no minimum stay", "no blackout dates", "swimming pool", "Gym", "Pets Allowed", "Shuttle Service", "Free Breakfast", "Room Service", "Off Peak", "Air Conditioning", "Concierge", "Dining", "Hotel Bar"}},
                    {"Books", new List<string>{"Rare", "New", "Used", "Loan", "Kindle", "Nook", "Paperback", "Hardback", "Educational", "Technical", "Health and Well Being"}},
                    {"Food", new List<string>{"BOGO", "Free Drink", "Kids Eat Free", "Breakfast", "Lunch", "Dinner", "Seafood", "Mexican", "Italian", "Pasta", "American", "Burgers", "Steaks", "Vegan", "Vegetarian", "Paleo"}},
                    {"Drinks", new List<string>{"Happy Hour", "Microbrews", "Free Refills", "Whites and Reds", "Wine", "Specials" }},
                    {"Rentals", new List<string>{"Rental by Owner", "Weekly", "Monthly", "Subleasing", "First Month Free"}},
                    {"Discounts", new List<string>{"10 percent off", "20 percent off", "One Day Only", "Limited Time", "BOGO", "Free with Purchase"}}
                };
            }
        }
    }
}
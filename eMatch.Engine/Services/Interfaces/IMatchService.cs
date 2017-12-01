using eMatch.Engine.Enitities.Offers;
using eMatch.Engine.Enitities.Users;
using System.Collections.Generic;

namespace eMatch.Engine.Services.Interfaces
{
    public interface IMatchService
    {
        Dictionary<string, List<Offer>> GetMatchesForUser(string userId);
        Dictionary<string, List<Offer>> GetActiveMatchesForUser(string userId);
        List<User> GetMatchesForOffer(string offerId);
        
        int GetActiveMatchCount(string userid);
    }
}

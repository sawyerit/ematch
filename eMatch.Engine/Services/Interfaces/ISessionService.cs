using eMatch.Engine.Enitities.Offers;
using eMatch.Engine.Enitities.Users;
using System.Collections.Generic;

namespace eMatch.Engine.Services.Interfaces
{
  public interface ISessionService
  {
      User eMatchUser { get; set; }
      string UserId { get; }
      bool IsMerchant { get; }
  }
}

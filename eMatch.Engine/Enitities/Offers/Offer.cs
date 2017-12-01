using eMatch.Engine.Enitities.Accounts;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace eMatch.Engine.Enitities.Offers
{
  public class Offer
  {
    public enum StatusType
    {
      Pending,
      Active,
      Inactive
    }

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string ProfileId { get; set; }

    public string Category { get; set; }

    public string Name { get; set; }
    public string Description { get; set; }
    public Dictionary<string, string> Details { get; set; }
    public List<string> Keywords { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string CreatedBy { get; set; }
    public DateTime? Expires { get; set; }
    public bool IsRecurring { get; set; }
    public StatusType Status { get; set; }
  }
}

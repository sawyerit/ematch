using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace eMatch.Engine.Enitities.Accounts
{
  public class Account
  {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string Name { get; set; }
  }
}

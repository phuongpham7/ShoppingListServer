using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ShoppingList.Entities
{
    public class ShoppingItem
    {
        [BsonId]
        //[BsonRepresentation(BsonType.ObjectId)]
        //[BsonIgnoreIfDefault]
        public string Id { get; set; }
        public string Item { get; set; }
        public User User { get; set; }
        public bool Completed { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool Deleted { get; set; }
    }
}
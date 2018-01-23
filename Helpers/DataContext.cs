using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ShoppingList.Entities;

namespace ShoppingList.Helpers
{
    public class ShoppingListContext
    {
        private readonly IMongoDatabase _database = null;

        public ShoppingListContext(IOptions<AppSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            if (client != null)
                _database = client.GetDatabase(settings.Value.Database);
        }

        public IMongoCollection<User> Users
        {
            get 
            {
                return _database.GetCollection<User>("User");
            }
        }

        public IMongoCollection<ShoppingItem> ShoppingItems
        {
            get 
            {
                return _database.GetCollection<ShoppingItem>("ShoppingItem");
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ShoppingList.Entities;
using ShoppingList.Helpers;

namespace ShoppingList.Services
{
    public interface IShoppingItemService
    {
        Task<IEnumerable<ShoppingItem>> GetAllShoppingItems();
        Task<IEnumerable<ShoppingItem>> GetShoppingItemsByUserId(string userId);
        Task<ShoppingItem> GetShoppingItemById(string id);
        ShoppingItem CreateShoppingItem(ShoppingItem shoppingItem);
        Task<bool> RemoveShoppingItem(string id);
        Task<bool> UpdateShoppingItem(ShoppingItem shoppingItem);
    }

    public class ShoppingItemService : IShoppingItemService
    {
        private readonly IOptions<AppSettings> _settings;
        private readonly ShoppingListContext _context = null;

        public ShoppingItemService(IOptions<AppSettings> settings)
        {
            _settings = settings;
            _context = new ShoppingListContext(settings);
        }

        public ShoppingItem CreateShoppingItem(ShoppingItem shoppingItem)
        {
            try
            {
                shoppingItem.Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
                shoppingItem.CreatedOn = DateTime.Now;

                _context.ShoppingItems.InsertOneAsync(shoppingItem);

                return shoppingItem;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<ShoppingItem>> GetAllShoppingItems()
        {
            try 
            {
                return await _context.ShoppingItems.Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ShoppingItem> GetShoppingItemById(string id)
        {
            var filter = Builders<ShoppingItem>.Filter.Eq("Id", id);
            try
            {
                return await _context.ShoppingItems.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<ShoppingItem>> GetShoppingItemsByUserId(string userId)
        {
            var filter = Builders<ShoppingItem>.Filter.Eq(x => x.User.Id, userId);
            try
            {
                return await _context.ShoppingItems.Find(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> RemoveShoppingItem(string id)
        {
            try
            {
                DeleteResult actionResult = await _context.ShoppingItems.DeleteOneAsync(
                                                        Builders<ShoppingItem>.Filter.Eq("Id", id));
                return actionResult.IsAcknowledged && actionResult.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> UpdateShoppingItem(ShoppingItem shoppingItemParam)
        {
            try
            {
                var shoppingItem = await _context.ShoppingItems
                                                .Find(Builders<ShoppingItem>.Filter
                                                .Eq("Id", shoppingItemParam.Id))
                                                .FirstOrDefaultAsync();
                if (shoppingItem == null)   throw new AppException("Shopping Item not found");
                
                shoppingItem.Item = shoppingItemParam.Item;
                shoppingItem.Completed = shoppingItemParam.Completed;
                shoppingItem.UpdatedOn = DateTime.Now;

                ReplaceOneResult actionResult = await _context.ShoppingItems.
                    ReplaceOneAsync(n => n.Id.Equals(shoppingItemParam.Id), shoppingItem, new UpdateOptions { IsUpsert = true });
                return actionResult.IsAcknowledged && actionResult.ModifiedCount > 0;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
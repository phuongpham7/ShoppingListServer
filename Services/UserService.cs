using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ShoppingList.Entities;
using ShoppingList.Helpers;

namespace ShoppingList.Services
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllUsers();
        Task<User> GetUserById(string id);
        Task<User> GetUserByEmail(string email);
        Task CreateUser(User user, string password);
        Task<bool> RemoveUser(string id);
        Task<bool> UpdateUser(User user, string password);
        Task<User> Authenticate(string email, string password);
        
    }

    public class UserService : IUserService
    {
        private readonly IOptions<AppSettings> _settings;
        private readonly ShoppingListContext _context = null;

        public UserService(IOptions<AppSettings> settings)
        {
            _settings = settings;
            _context = new ShoppingListContext(settings);
        }

        public async Task<IEnumerable<User>> GetAllUsers()
        {
            try 
            {
                return await _context.Users.Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<User> GetUserById(string id)
        {
            var filter = Builders<User>.Filter.Eq("Id", id);
            try
            {
                return await _context.Users.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task CreateUser(User user, string passwrord)
        {
            if (string.IsNullOrWhiteSpace(passwrord))   throw new AppException("Password is required");
            var filter = Builders<User>.Filter.Eq("Email", user.Email);
            try
            {
                if (await _context.Users.Find(filter).FirstOrDefaultAsync() != null)
                    throw new AppException("Email " + user.Email + " is already taken");

                byte[] passwordHash, passwordSalt;
                CreatePasswordHash(passwrord, out passwordHash, out passwordSalt);
                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;

                user.CreatedOn = DateTime.Now;

                await _context.Users.InsertOneAsync(user);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> RemoveUser(string id)
        {
            try
            {
                DeleteResult actionResult = await _context.Users.DeleteOneAsync(
                                                        Builders<User>.Filter.Eq("Id", id));
                return actionResult.IsAcknowledged && actionResult.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> UpdateUser(User userParam, string password = null)
        {
            try
            {
                var user = await _context.Users.Find(Builders<User>.Filter.Eq("Id", userParam.Id))
                                                .FirstOrDefaultAsync();
                if (user == null)   throw new AppException("User not found");
                
                if (userParam.Email != user.Email)
                {
                    if (await _context.Users.Find(Builders<User>.Filter.Eq("Email", userParam.Email))
                                                .AnyAsync())
                        throw new AppException("Email " + userParam.Email + " is already taken");
                }
                user.FirstName = userParam.FirstName;
                user.LastName = userParam.LastName;
                user.Email = userParam.Email;
                user.UpdatedOn = DateTime.Now;

                // update password if it was entered
                if (!string.IsNullOrWhiteSpace(password))
                {
                    byte[] passwordHash, passwordSalt;
                    CreatePasswordHash(password, out passwordHash, out passwordSalt);
    
                    user.PasswordHash = passwordHash;
                    user.PasswordSalt = passwordSalt;
                }

                ReplaceOneResult actionResult = await _context.Users.
                    ReplaceOneAsync(n => n.Id.Equals(userParam.Id), user, new UpdateOptions { IsUpsert = true });
                return actionResult.IsAcknowledged && actionResult.ModifiedCount > 0;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<User> Authenticate(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                return null;
            
            var filter = Builders<User>.Filter.Eq("Email", email);
            try
            {
                var user = await _context.Users.Find(filter).FirstOrDefaultAsync();
                if (user == null)   return null;

                if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))    return null;

                return user;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<User> GetUserByEmail(string email)
        {
            var filter = Builders<User>.Filter.Eq("Email", email);
            try
            {
                return await _context.Users.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //private helper methods
        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            if (password == null)   throw new ArgumentException("password");
            if (string.IsNullOrWhiteSpace(password))    throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");

            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }            
        }

        private static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            if (password == null) throw new ArgumentException("password");
            if (string.IsNullOrWhiteSpace(password))    throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");
            if (storedHash.Length != 64) throw new ArgumentException("Invalid length of password hash (64 bytes expected).", "passwordHash");
            if (storedSalt.Length != 128) throw new ArgumentException("Invalid length of password salt (128 bytes expected).", "passwordSalt");

            using (var hmac = new System.Security.Cryptography.HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i])   return false;
                }
            }
            return true;
        }
    }
}
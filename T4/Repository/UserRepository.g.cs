
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Repository.Models;

namespace MyApp.Data.Repositories
{
    /// <summary>
    /// Repository for User entities
    /// Auto-generated on 2025-11-25 19:17:53
    /// Version used Samayas.CodeGenerator.Repositories.RepositoryGenerator, Samayas.CodeGenerator, Version=1.1.1.0, Culture=neutral, PublicKeyToken=null
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly List<User> _dataStore;

        public UserRepository()
        {
            _dataStore = new List<User>();
        }

        public async Task<User> GetByIdAsync(System.Guid id)
        {
            return await Task.FromResult(_dataStore.FirstOrDefault(x => x.Id.Equals(id)));
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await Task.FromResult(_dataStore.AsEnumerable());
        }

        public async Task AddAsync(User entity)
        {
            _dataStore.Add(entity);
            await Task.CompletedTask;
        }

        public async Task UpdateAsync(User entity)
        {
            User existing = _dataStore.FirstOrDefault(x => x.Id.Equals(entity.Id));
            if (existing != null)
            {
                int index = _dataStore.IndexOf(existing);
                _dataStore[index] = entity;
            }
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(System.Guid id)
        {
            User entity = _dataStore.FirstOrDefault(x => x.Id.Equals(id));
            if (entity != null)
            {
                _dataStore.Remove(entity);
            }
            await Task.CompletedTask;
        }
    }

    public interface IUserRepository
    {
        Task<User> GetByIdAsync(System.Guid id);
        Task<IEnumerable<User>> GetAllAsync();
        Task AddAsync(User entity);
        Task UpdateAsync(User entity);
        Task DeleteAsync(System.Guid id);
    }
}
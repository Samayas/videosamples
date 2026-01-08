
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Repository.Models;

namespace MyApp.Data.Repositories
{
    /// <summary>
    /// Repository for Group entities
    /// Auto-generated on 2025-11-25 19:17:52
    /// Version used Samayas.CodeGenerator.Repositories.RepositoryGenerator, Samayas.CodeGenerator, Version=1.1.1.0, Culture=neutral, PublicKeyToken=null
    /// </summary>
    public class GroupRepository : IGroupRepository
    {
        private readonly List<Group> _dataStore;

        public GroupRepository()
        {
            _dataStore = new List<Group>();
        }

        public async Task<Group> GetByIdAsync(int id)
        {
            return await Task.FromResult(_dataStore.FirstOrDefault(x => x.Id.Equals(id)));
        }

        public async Task<IEnumerable<Group>> GetAllAsync()
        {
            return await Task.FromResult(_dataStore.AsEnumerable());
        }

        public async Task AddAsync(Group entity)
        {
            _dataStore.Add(entity);
            await Task.CompletedTask;
        }

        public async Task UpdateAsync(Group entity)
        {
            Group existing = _dataStore.FirstOrDefault(x => x.Id.Equals(entity.Id));
            if (existing != null)
            {
                int index = _dataStore.IndexOf(existing);
                _dataStore[index] = entity;
            }
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(int id)
        {
            Group entity = _dataStore.FirstOrDefault(x => x.Id.Equals(id));
            if (entity != null)
            {
                _dataStore.Remove(entity);
            }
            await Task.CompletedTask;
        }
    }

    public interface IGroupRepository
    {
        Task<Group> GetByIdAsync(int id);
        Task<IEnumerable<Group>> GetAllAsync();
        Task AddAsync(Group entity);
        Task UpdateAsync(Group entity);
        Task DeleteAsync(int id);
    }
}
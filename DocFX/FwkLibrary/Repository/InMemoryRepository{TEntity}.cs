using FwkLibrary.Entities;
using FwkLibrary.Repository.Interfaces;

namespace FwkLibrary.Repository
{
    /// <summary>
    /// Provides an in‑memory implementation of <see cref="IReadOnlyRepository{TEntity}"/>.
    /// </summary>
    /// <typeparam name="TEntity">The type of stored entity.</typeparam>
    public sealed class InMemoryRepository<TEntity> : IReadOnlyRepository<TEntity> where TEntity : Entity
    {
        private readonly List<TEntity> items;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryRepository{TEntity}"/> class.
        /// </summary>
        /// <param name="items">
        /// The initial set of entities that will be available for querying.
        /// </param>
        public InMemoryRepository(IEnumerable<TEntity> items)
        {
            this.items = items == null ? new List<TEntity>() : new List<TEntity>(items);
        }

        /// <inheritdoc/>
        public IEnumerable<TEntity> GetAll()
        {
            return this.items.ToArray();
        }

        /// <inheritdoc/>
        public Task<TEntity?> FindByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            if (id == null)
            {
                return Task.FromResult<TEntity?>(null);
            }

            TEntity? match = this.items.FirstOrDefault(entity => string.Equals(entity.Id, id, StringComparison.OrdinalIgnoreCase));

            return Task.FromResult(match);
        }
    }
}

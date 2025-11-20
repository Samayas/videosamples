using FwkLibrary.Entities;

namespace FwkLibrary.Repository.Interfaces
{
    /// <summary>
    /// Defines the abstraction for a simple read‑only repository.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity returned by this repository.</typeparam>
    public interface IReadOnlyRepository<TEntity> where TEntity : Entity
    {
        /// <summary>
        /// Gets all entities from the underlying store.
        /// </summary>
        /// <returns>A sequence of all entities known to the repository.</returns>
        IEnumerable<TEntity> GetAll();

        /// <summary>
        /// Asynchronously finds an entity by its identifier.
        /// </summary>
        /// <param name="id">The identifier to search for.</param>
        /// <param name="cancellationToken">
        /// A token that can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation, containing the entity or <c>null</c>.
        /// </returns>
        Task<TEntity?> FindByIdAsync(string id, CancellationToken cancellationToken = default);
    }
}

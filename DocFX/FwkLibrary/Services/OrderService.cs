using FwkLibrary.Entities;
using FwkLibrary.Repository.Interfaces;

namespace FwkLibrary.Services
{
    /// <summary>
    /// Provides higher‑level operations for working with orders.
    /// </summary>
    /// <remarks>
    /// This service exists solely to demonstrate service‑style classes and async methods in API docs.
    /// </remarks>
    public sealed class OrderService
    {
        private readonly IReadOnlyRepository<Order> repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderService"/> class.
        /// </summary>
        /// <param name="repository">
        /// The repository used to retrieve <see cref="Order"/> instances.
        /// </param>
        public OrderService(IReadOnlyRepository<Order> repository)
        {
            this.repository = repository;
        }

        /// <summary>
        /// Gets all orders that have a total amount greater than or equal to the specified minimum.
        /// </summary>
        /// <param name="minimumAmount">The minimum order amount used for filtering.</param>
        /// <returns>
        /// A sequence of orders whose <see cref="Order.TotalAmount"/> is at least <paramref name="minimumAmount"/>.
        /// </returns>
        public IEnumerable<Order> GetHighValueOrders(decimal minimumAmount)
        {
            IEnumerable<Order> allOrders = this.repository.GetAll();
            IEnumerable<Order> result =
                from order in allOrders
                where order.TotalAmount >= minimumAmount
                select order;

            return result.ToArray();
        }

        /// <summary>
        /// Asynchronously determines whether the specified order is considered high value.
        /// </summary>
        /// <param name="orderId">The identifier of the order to check.</param>
        /// <param name="threshold">
        /// The minimum monetary amount that is required for the order to be considered high value.
        /// </param>
        /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
        /// <returns>
        /// A task that represents the asynchronous operation, containing <c>true</c> when the order
        /// exists and meets the threshold; otherwise, <c>false</c>.
        /// </returns>
        public async Task<bool> IsHighValueOrderAsync(string orderId, decimal threshold, CancellationToken cancellationToken = default)
        {
            Order? order = await this.repository.FindByIdAsync(orderId, cancellationToken).ConfigureAwait(false);
            if (order == null)
            {
                return false;
            }

            return order.TotalAmount >= threshold;
        }
    }
}

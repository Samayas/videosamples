using FwkLibrary.Entities;

namespace FwkLibrary
{
    /// <summary>
    /// Indicates the current processing state of an <see cref="Order"/>.
    /// </summary>
    public enum OrderStatus
    {
        /// <summary>
        /// The order has been created but not yet paid.
        /// </summary>
        Pending = 0,

        /// <summary>
        /// The order has been paid and is being prepared.
        /// </summary>
        Paid = 1,

        /// <summary>
        /// The order has been shipped to the customer.
        /// </summary>
        Shipped = 2,

        /// <summary>
        /// The order has been cancelled and will not be processed further.
        /// </summary>
        Cancelled = 3
    }
}

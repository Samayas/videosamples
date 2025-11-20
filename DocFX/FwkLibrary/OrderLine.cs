using FwkLibrary.Entities;

namespace FwkLibrary
{
    /// <summary>
    /// Represents a single line item in an <see cref="Order"/>.
    /// </summary>
    public sealed class OrderLine
    {
        /// <summary>
        /// Gets the product associated with this order line.
        /// </summary>
        /// <value>
        /// The product instance that is being purchased on this line.
        /// </value>
        public Product Product { get; }

        /// <summary>
        /// Gets the number of units of the <see cref="Product"/> that were ordered.
        /// </summary>
        /// <value>
        /// The quantity of the product on this line, expressed as a positive integer.
        /// </value>
        public int Quantity { get; }

        /// <summary>
        /// Gets the total amount for this order line.
        /// </summary>
        /// <value>
        /// The product of <see cref="Product.UnitPrice"/> and <see cref="Quantity"/>.
        /// </value>
        public decimal LineTotal
        {
            get { return Product.UnitPrice * Quantity; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderLine"/> class.
        /// </summary>
        /// <param name="product">The product that was ordered.</param>
        /// <param name="quantity">The number of units ordered.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="product"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="quantity"/> is less than one.</exception>
        public OrderLine(Product product, int quantity)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            if (quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be at least 1.");
            }

            Product = product;
            Quantity = quantity;
        }
    }
}

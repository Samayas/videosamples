namespace FwkLibrary.Entities
{
    /// <summary>
    /// Represents a product that can be added to an order in the demo domain.
    /// </summary>
    public class Product : Entity
    {
        /// <summary>
        /// Gets the human-readable name of the product.
        /// </summary>
        /// <value>
        /// The label used for displaying this product in lists and samples.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Gets the unit price of the product in demo currency.
        /// </summary>
        /// <value>
        /// The monetary value that is multiplied by the ordered quantity to calculate line totals.
        /// </value>
        public decimal UnitPrice { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Product"/> class.
        /// </summary>
        /// <param name="id">A unique identifier for the product.</param>
        /// <param name="name">The display name of the product.</param>
        /// <param name="unitPrice">The unit price used in calculations.</param>
        public Product(string id, string name, decimal unitPrice) : base(id)
        {
            Name = name ?? string.Empty;
            UnitPrice = unitPrice;
        }
    }
}

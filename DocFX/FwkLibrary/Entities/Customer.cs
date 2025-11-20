namespace FwkLibrary.Entities
{
    /// <summary>
    /// Represents a customer in the demo e‑commerce domain.
    /// </summary>
    public class Customer : Entity
    {
        private readonly List<string> tags = new List<string>();

        /// <summary>
        /// Gets the full display name of the customer.
        /// </summary>
        /// <value>
        /// The name that is shown in user interfaces and sample documentation.
        /// </value>
        public string FullName { get; }

        /// <summary>
        /// Gets the collection of tags associated with this customer.
        /// </summary>
        /// <value>
        /// A read-only collection of tag strings such as <c>"vip"</c> or <c>"internal-test"</c>.
        /// </value>
        public IReadOnlyCollection<string> Tags
        {
            get { return this.tags.AsReadOnly(); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Customer"/> class with the specified identifier and name.
        /// </summary>
        /// <param name="id">A unique business identifier such as <c>"CUST-001"</c>.</param>
        /// <param name="fullName">The full name that will be displayed in the generated documentation examples.</param>
        public Customer(string id, string fullName) : base(id)
        {
            FullName = fullName ?? string.Empty;
        }

        /// <summary>
        /// Adds a descriptive tag to the customer if it is not already present.
        /// </summary>
        /// <param name="tag">The tag to add, for example <c>"vip"</c> or <c>"internal-test"</c>.</param>
        /// <returns>
        /// <c>true</c> if the tag was added; otherwise, <c>false</c> when the tag already existed.
        /// </returns>
        public bool AddTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                return false;
            }

            if (this.tags.Contains(tag))
            {
                return false;
            }

            this.tags.Add(tag);

            return true;
        }
    }
}

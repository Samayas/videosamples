namespace FwkLibrary.Entities
{
    /// <summary>
    /// Represents a customer order that contains one or more <see cref="OrderLine"/> instances.
    /// </summary>
    public class Order : Entity
    {
        private readonly List<OrderLine> lines = new List<OrderLine>();

        /// <summary>
        /// Gets the identifier of the customer who placed the order.
        /// </summary>
        /// <value>
        /// The customer identifier that links the order to a specific <see cref="Customer"/>.
        /// </value>
        public string CustomerId { get; }

        /// <summary>
        /// Gets the current status of the order.
        /// </summary>
        /// <value>
        /// One of the <see cref="OrderStatus"/> values describing where the order is in the process.
        /// </value>
        public OrderStatus Status { get; private set; }

        /// <summary>
        /// Gets the lines that belong to this order.
        /// </summary>
        /// <value>
        /// A read-only list of <see cref="OrderLine"/> instances that make up the order.
        /// </value>
        public IReadOnlyCollection<OrderLine> Lines
        {
            get { return this.lines.AsReadOnly(); }
        }

        /// <summary>
        /// Gets the total monetary value of the order.
        /// </summary>
        /// <value>
        /// The sum of all <see cref="OrderLine.LineTotal"/> values for the order.
        /// </value>
        public decimal TotalAmount
        {
            get
            {
                decimal total = 0m;
                foreach (OrderLine line in this.lines)
                {
                    total += line.LineTotal;
                }

                return total;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Order"/> class.
        /// </summary>
        /// <param name="id">The order identifier used to distinguish orders.</param>
        /// <param name="customerId">The identifier of the associated customer.</param
        public Order(string id, string customerId) : base(id)
        {
            CustomerId = customerId ?? string.Empty;
            Status = OrderStatus.Pending;
        }

        /// <summary>
        /// Adds the specified order line to the order.
        /// </summary>
        /// <param name="line">The line to add.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="line"/> is <c>null</c>.</exception>
        public void AddLine(OrderLine line)
        {
            if (line == null)
            {
                throw new ArgumentNullException(nameof(line));
            }

            this.lines.Add(line);
        }

        /// <summary>
        /// Changes the status of the order.
        /// </summary>
        /// <param name="status">The new status to apply.</param>
        public void ChangeStatus(OrderStatus status)
        {
            Status = status;
        }
    }
}

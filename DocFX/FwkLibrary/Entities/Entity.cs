namespace FwkLibrary.Entities
{
    /// <summary>
    /// Represents the base type for all entities that have a string identifier.
    /// </summary>
    /// <remarks>
    /// This abstract base class is used only to demonstrate inheritance in the generated documentation.
    /// </remarks>
    public abstract class Entity
    {
        /// <summary>
        /// Gets the technical identifier of the entity.
        /// </summary>
        /// <value>
        /// A non-empty string that uniquely identifies this entity instance in the demo domain.
        /// </value>
        public string Id { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Entity"/> class.
        /// </summary>
        /// <param name="id">
        /// A non-empty identifier that uniquely represents this entity instance in the demo domain.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="id"/> is <c>null</c> or whitespace.
        /// </exception>
        protected Entity(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Id must not be null or whitespace.", nameof(id));
            }

            Id = id;
        }

        /// <summary>
        /// Returns a human-readable representation of the entity instance.
        /// </summary>
        /// <returns>
        /// A string that contains the runtime type name and the identifier of the entity.
        /// </returns>
        public override string ToString()
        {
            return string.Format("{0} ({1})", GetType().Name, Id);
        }
    }
}

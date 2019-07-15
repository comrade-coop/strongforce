namespace StrongForce.Core
{
	/// <summary>
	/// Used to generate address values, which are to be injected in all entities in the StrongForce instance.
	/// </summary>
	public interface IAddressFactory
	{
		/// <summary>
		/// Creates an Address which identifies entities in the StrongForce instance.
		/// </summary>
		/// <returns>Generated Address.</returns>
		Address Create();
	}
}
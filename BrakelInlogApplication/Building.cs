using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrakelInlogApplication
{
	/// <summary>
	/// Class that represents a building/location in the system
	/// </summary>
	public class Building
	{
		/// <summary>
		/// The building id used to identify the building
		/// </summary>
		public Guid BuildingID { get; set; }

		/// <summary>
		/// The access role of the user in this building
		/// </summary>
		public AccessRole AccessRole { get; set; }

	}

	/// <summary>
	/// Enum for the different access roles
	/// </summary>
	public enum AccessRole
	{
		/// <summary>
		/// Administrators have full access to the building, in read and edit mode.
		/// </summary>
		Administrator,

		/// <summary>
		/// ReadOnly users can only read the sensor results, but can not make changes
		/// </summary>
		ReadOnly,

		/// <summary>
		/// None means the user has no rights in this building, buildings with these rights should not be shown in the list
		/// </summary>
		None
	}
}

using System;
using System.Data.SqlClient;

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
		public UInt32 BuildingID { get; set; }

		/// <summary>
		/// The name of the building for human readable context
		/// </summary>
		public String BuildingName { get; set; }

		/// <summary>
		/// The access role of the user in this building
		/// </summary>
		public AccessRole AccessRole { get; set; }

		/// <summary>
		/// Parent building of the current building, 0 if none
		/// </summary>
		public UInt32 Parent { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this instance has alarm.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance has alarm; otherwise, <c>false</c>.
		/// </value>
		public Boolean HasAlarm { get; set; }

		/// <summary>
		/// Returns a JSON representation of the current object
		/// </summary>
		/// <returns>The JSON representing the current object</returns>
		public String ToJSONString()
		{
			return String.Format(@"{{""BuildingID"":{0},""BuildingName"":""{1}"",""AccessRole"":""{2}"",""Parent"":{3},""HasAlarm"":{4}}}",
			                     BuildingID, BuildingName, AccessRole.ToString(), Parent, HasAlarm.ToString().ToLower());
		}

		/// <summary>
		/// Parses the input string and converts it to a value of the AccessRole Enumeration
		/// </summary>
		/// <param name="rightsName">The string representation of the AccessRole</param>
		/// <returns>The AccessRole</returns>
		public static AccessRole ParseAccessRightsFromString(String rightsName)
		{
			switch (rightsName)
			{
				case "Administrator":
					{
						return AccessRole.Administrator;
					}
				case "ReadOnly":
					{
						return AccessRole.ReadOnly;
					}
				default:
					{
						return AccessRole.None;
					}
			}
		}

		/// <summary>
		/// Gets the endpoint (ip or hostname, and a port) for the given building, if any.
		/// </summary>
		/// <returns>
		/// True if an enpoint is known, false otherwise
		/// </returns>
		/// <param name='buildingId'>
		/// Building identifier.
		/// </param>
		/// <param name='buildingEndpoint'>
		/// The building's endpoint
		/// </param>
		public static Boolean GetBuildingEndpoint (UInt32 buildingId, out String buildingEndpoint)
		{
			using (var connection = new SqlConnection(ConstantHelper.ConnectionString)) {
				connection.Open ();

				string query = String.Format ("SELECT [endpoint] FROM [building] WHERE [buildingId] = {0}", buildingId);
				var command = new SqlCommand (query, connection);
				
				var result = command.ExecuteScalar() as String;
				buildingEndpoint = result;
			}
			return !String.IsNullOrEmpty(buildingEndpoint);
		}
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
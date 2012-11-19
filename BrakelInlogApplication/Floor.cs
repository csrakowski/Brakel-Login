using System;
using System.Collections.Generic;

namespace BrakelInlogApplication
{
	/// <summary>
	/// Class that represents a floor inside a building
	/// </summary>
	public class Floor : Building
	{
		/// <summary>
		/// List of rooms belonging to the floor
		/// </summary>
		public List<Room> Rooms { get; set; }

		/// <summary>
		/// Returns a JSON representation of the current object
		/// </summary>
		/// <returns>The JSON representing the current object</returns>
		public new String ToJSONString()
		{
			string result = base.ToJSONString();
			string arrayJSON = "";
			Rooms.ForEach(
				r => arrayJSON += ("," + r.ToJSONString())
				);
			if (arrayJSON.Length > 0)
				arrayJSON = arrayJSON.Substring(1);

			result = result.Substring(0, result.Length - 2);

			result = String.Format(@"{0}, ""Rooms"":[{1}]}}", result, arrayJSON);
			return result;
		}
	}
}
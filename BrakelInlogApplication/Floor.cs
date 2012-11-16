using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
			string ArrayJSON = "";
			Rooms.ForEach(
				r => ArrayJSON += ("," + r.ToJSONString())
			);
			if (ArrayJSON.Length > 0)
				ArrayJSON = ArrayJSON.Substring(1);

			result = result.Substring(0, result.Length - 2);

			result = String.Format(@"{0}, ""Rooms"":[{1}]}}", result, ArrayJSON);
			return result;
		}
	}
}
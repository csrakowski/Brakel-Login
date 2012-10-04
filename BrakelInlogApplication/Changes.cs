using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrakelInlogApplication
{
	/// <summary>
	/// Class used for taking actions with groups
	/// </summary>
	public class Changes
	{
		/// <summary>
		/// The group id this change will affect
		/// </summary>
		public int GroupID { get; set; }

		/// <summary>
		/// The value the group will be changed to
		/// </summary>
		public int ChangeValue { get; set; }

		/// <summary>
		/// Status boolean, null for new, true if success, false if failed
		/// </summary>
		public bool? ChangeStatus { get; set; }

		/// <summary>
		/// Returns a JSON representation of the current object
		/// </summary>
		/// <returns>The JSON representing the current object</returns>
		public String ToJSONString()
		{
			return String.Format(@"{{ ""GroupID"":""{0}"", ""ChangeValue"":""{1}"", ""ChangeStatus"":""{2}"" }}", GroupID, ChangeValue, ChangeStatus.ToString());
		}
	}
}

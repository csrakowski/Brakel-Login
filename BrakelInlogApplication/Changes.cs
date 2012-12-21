using System;

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
		public UInt32 GroupID { get; set; }

		/// <summary>
		/// Gets or sets the name of the group.
		/// </summary>
		/// <value>
		/// The name of the group.
		/// </value>
		public String GroupName { get; set; }

		/// <summary>
		/// The value the group will be changed to, 0 - 255
		/// </summary>
		public UInt32 ChangeValue { get; set; }

		/// <summary>
		/// Status boolean, null for new, true if success, false if failed
		/// </summary>
		public Boolean ChangeStatus { get; set; }

		/// <summary>
		/// Returns a JSON representation of the current object
		/// </summary>
		/// <returns>The JSON representing the current object</returns>
		public String ToJSONString ()
		{
			if (String.IsNullOrEmpty (GroupName)) {
				GroupName = String.Format("Groep {0}", GroupID);
			}
			return String.Format(@"{{""GroupID"":""{0}"",""GroupName"":""{1}"",""ChangeValue"":""{2}"",""ChangeStatus"":""{3}""}}", GroupID, GroupName, ChangeValue, ChangeStatus.ToString().ToLower());
		}
	}
}

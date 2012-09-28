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
	}
}

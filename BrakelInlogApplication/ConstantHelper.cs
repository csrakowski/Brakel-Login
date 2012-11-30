using System.Configuration;
using System;

namespace BrakelInlogApplication
{
	/// <summary>
	/// Helper class that wraps around the .NET ConfigurationManager and provides easier access
	/// </summary>
	public class ConstantHelper
	{
		/// <summary>
		/// The connectionstring for the datasource provider
		/// </summary>
		public static string ConnectionString
		{
			get { return ConfigurationManager.ConnectionStrings["BasisDB"].ConnectionString ?? ""; }
		}

		/// <summary>
		/// Url for the test building (Development)
		/// </summary>
		public static string TestBuilding
		{
			get { return ConfigurationManager.AppSettings["TestBuilding"] ?? ""; }
		}

		/// <summary>
		/// Device id of the Test iPad (Development)
		/// </summary>
		public static string TestIPad
		{
			get { return ConfigurationManager.AppSettings["TestIPad"] ?? ""; }
		}		

		/// <summary>
		/// Gets the timeout of the building makeChanges call
		/// </summary>
		public static int BuildingTimeout
		{
			get { return Int32.Parse(ConfigurationManager.AppSettings["BuildingTimeout"] ?? "350000"); }
		}

		/// <summary>
		/// Gets the poll interval.
		/// </summary>
		/// <value>
		/// The poll interval.
		/// </value>
		public static int PollInterval
		{
			get { return Int32.Parse(ConfigurationManager.AppSettings["PollInterval"] ?? "3600000"); }
		}

		/// <summary>
		/// Gets the max poll errors.
		/// </summary>
		/// <value>
		/// The max poll errors.
		/// </value>
		public static int MaxPollErrors
		{
			get { return Int32.Parse(ConfigurationManager.AppSettings["MaxPollErrors"] ?? "5"); }
		}
	}
}
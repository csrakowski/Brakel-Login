using System.Configuration;

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
	}
}
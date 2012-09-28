using System.Configuration;

namespace BrakelInlogApplication
{
	class ConstantHelper
	{
		public static string ConnectionString
		{
			get
			{
				return ConfigurationManager.AppSettings["ConnectionInfo"] ?? "";
			}
		}
	}
}
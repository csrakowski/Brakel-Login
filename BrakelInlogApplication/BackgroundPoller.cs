using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Text;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Threading;

namespace BrakelInlogApplication
{
	/// <summary>
	/// Helper class to assist in background thread polling of the building
	/// </summary>
	public sealed class BackgroundPoller
	{
		/// <summary>
		/// The Instance of this class
		/// </summary>
		public static BackgroundPoller Instance = new BackgroundPoller();

		/// <summary>
		/// Private to prevent instantiation
		/// </summary>
		private BackgroundPoller()
		{
			//init thread pool
		}

		/// <summary>
		/// PollingResult
		/// </summary>
		/// <param name="userToken">The userToken of the user who initiated the poll</param>
		/// <param name="building">The building this result is about</param>
		/// <param name="json">The result in JSON format</param>
		public delegate void PollingResult(Guid userToken, int building, string json);

		/// <summary>
		/// Event fired when a building result has changed
		/// </summary>
		public event PollingResult OnResultChanged;

		/// <summary>
		/// Starts polling the target building for an update on the changes
		/// </summary>
		/// <param name="userToken">The userToken of the user who initiated the poll</param>
		/// <param name="buildingId">The building to poll</param>
		public void StartPollingBuilding (Guid userToken, int buildingId)
		{
			bool notDone = true;
			while (notDone) {
				Thread.Sleep(60 * 60 * 1000); // 60 seconds

				#region Make HTTP Request
				string requestBody = "";
				if (requestBody.Length > 1)
					requestBody = requestBody.Substring (1);
				requestBody = @"{ ""command"": ""progress"" }\r\n\r\n";
			
				//Get ip from database based on building id
				string targetBuilding = ConstantHelper.TestBuilding;
			
				HttpWebRequest request = (HttpWebRequest)WebRequest.Create (targetBuilding);
				request.Method = "POST";
				request.Timeout = (5 * 60 * 1000); // 5 seconds
				request.KeepAlive = false;
				request.SendChunked = false;
			
				byte[] byte1 = Encoding.ASCII.GetBytes (requestBody);
				request.ContentType = "application/json";
				request.ContentLength = byte1.Length;
				Stream newStream = request.GetRequestStream ();
				newStream.Write (byte1, 0, byte1.Length);
				#endregion
				JObject result = null;
				try {
					HttpWebResponse response = (HttpWebResponse)request.GetResponse ();
				
					var str = response.GetResponseStream ();
					byte[] buffer = new byte[str.Length];
					str.Read (buffer, 0, (int)str.Length);
				
					string resultString = Encoding.ASCII.GetString (buffer);
					Debug.WriteLine (resultString);
					result = JObject.Parse (resultString);
				} catch (Exception ex) {
					Debug.WriteLine (ex.Message);
				}
			
				if(result != null)
				{
					JArray changesArray = result ["changes"] as JArray;
					if(changesArray.Count == 0) {
						notDone = false;
					} else {
						JArray resultArray = new JArray ();
						foreach (var item in changesArray) {
							if (Boolean.Parse (item ["ChangeStatus"].ToString () ?? Boolean.FalseString)) {
								resultArray.Add (item);
							}
						}
						if (resultArray.Count > 0) {
							OnResultChanged.Invoke (userToken, buildingId, resultArray.ToString ());
						}
					}
				}
			}
		}
	}
}
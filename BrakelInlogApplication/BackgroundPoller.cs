using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Newtonsoft.Json.Linq;
using System.Net.Sockets;

namespace BrakelInlogApplication
{
	/// <summary>
	/// Helper class to assist in background thread polling of the building
	/// </summary>
	public sealed class BackgroundPoller
	{
		#region Delegates
		/// <summary>
		/// PollingResult
		/// </summary>
		/// <param name="userToken">The userToken of the user who initiated the poll</param>
		/// <param name="building">The building this result is about</param>
		/// <param name="json">The result in JSON format</param>
		public delegate void PollingResult(Guid userToken, int building, string json);
		#endregion

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
			//ThreadPool.QueueUserWorkItem(StartAsyncTask, null);
			//private void StartAsyncTask(Object workItemState)

			int errorCount = ConstantHelper.MaxPollErrors;

			string requestBody = @"{ ""command"": ""progress"" }\r\n\r\n";
			string targetBuilding = Building.GetBuildingIp(buildingId);

			bool notDone = true;
			while (notDone)
			{
				Thread.Sleep(ConstantHelper.PollInterval);

				JObject result = null;
				try
				{
					#region Make Request
					string host = targetBuilding.Split (':')[0];
					int port = Int32.Parse (targetBuilding.Split (':')[1]);
					byte[] byte1 = Encoding.ASCII.GetBytes(requestBody);

					TcpClient socket = new TcpClient(host, port);
					NetworkStream stream = socket.GetStream();
					stream.Write(byte1, 0, byte1.Length);
					stream.Flush();
					
					byte[] buff = new byte[2048];
					int bytesRead = stream.Read(buff, 0, buff.Length);
					string resultString = Encoding.ASCII.GetString(buff, 0, bytesRead);

					Debug.WriteLine(resultString);
					result = JObject.Parse(resultString);
					#endregion
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex.Message);
					if(errorCount-- == 0)
					{
						OnResultChanged.Invoke(userToken, buildingId, "[ \"crash\":\"true\" ]");
						break;
					}
				}

				if (result != null)
				{
					var changesArray = result["changes"] as JArray;
					if (changesArray.Count == 0)
					{
						notDone = false;
					}
					else
					{
						var resultArray = new JArray();
						foreach (JToken item in changesArray)
						{
							if (Boolean.Parse(item["ChangeStatus"].ToString() ?? Boolean.FalseString))
							{
								resultArray.Add(item);
							}
						}
						if (resultArray.Count > 0)
						{
							OnResultChanged.Invoke(userToken, buildingId, resultArray.ToString());
						}
					}
				}
			}
		}
	}
}
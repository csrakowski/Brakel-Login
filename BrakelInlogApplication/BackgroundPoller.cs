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
		public delegate void PollingResult(Guid userToken, UInt32 building, String json);
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
			//.NET 4 optimizes throughput by default, best leave that algorithm alone.
			//ThreadPool.SetMinThreads(5, 5);
			//ThreadPool.SetMaxThreads(50, 50);
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
		public void StartPollingBuilding (Guid userToken, UInt32 buildingId)
		{
			object[] args = { userToken, buildingId, OnResultChanged };
			ThreadPool.QueueUserWorkItem(BackgroundPollerWorker.StartAsyncTask, args);
		}

		private class BackgroundPollerWorker
		{
			public static void StartAsyncTask(Object workItemState)
			{
				Guid userToken = (Guid)((object[])workItemState)[0];
				UInt32 buildingId = (UInt32)((object[])workItemState)[1];
				PollingResult OnResultChanged = (PollingResult)((object[])workItemState)[2];

				int errorCount = ConstantHelper.MaxPollErrors;
				
				string requestBody = @"{""command"":""progress""}\n";
				string targetBuilding = Building.GetBuildingIp(buildingId);

				Debug.WriteLine("Start polling");

				bool done = false;
				do
				{
					Thread.Sleep(ConstantHelper.PollInterval);
					
					JObject result = null;
					TcpClient socket = null;
					NetworkStream stream = null;
					try
					{
						#region Make Request
						string host = targetBuilding.Split (':')[0];
						int port = Int32.Parse (targetBuilding.Split (':')[1]);
						byte[] byte1 = Encoding.ASCII.GetBytes(requestBody);
						
						socket = new TcpClient(host, port);
						socket.SendTimeout = ConstantHelper.BuildingTimeout;
						socket.ReceiveTimeout = ConstantHelper.BuildingTimeout;

						stream = socket.GetStream();
						stream.Write(byte1, 0, byte1.Length);
						stream.Flush();

						Debug.WriteLine("Polling request: " +requestBody);
						
						byte[] buff = new byte[2048];
						int bytesRead = stream.Read(buff, 0, buff.Length);
						string resultString = Encoding.ASCII.GetString(buff, 0, bytesRead);

						Debug.WriteLine("Polling response: " +resultString);
						result = JObject.Parse(resultString);
						#endregion
					}
					catch (Exception ex)
					{
						Debug.WriteLine("Polling error: " + ex.Message);
						if(--errorCount <= 0)
						{
							done = true;
							OnResultChanged.Invoke(userToken, buildingId, "[ \"crash\":\"true\" ]");
							break;
						}
					}
					finally
					{
						if(stream != null)
							stream.Close();
						if(socket != null)
							socket.Close();
					}

					if (result != null)
					{
						try
						{
							var changesArray = result["changes"] as JArray;
							if (changesArray.Count == 0)
							{
								done = true;
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
								Debug.WriteLine("Got {0} changes: {1}", resultArray.Count, resultArray.ToString());
								if (resultArray.Count > 0)
								{
									OnResultChanged.Invoke(userToken, buildingId, resultArray.ToString());
								}
							}
						}
						catch(Exception e)
						{
							Debug.WriteLine(e.Message);
							Debug.WriteLine(e.StackTrace);
						}
					}
				} while (!done);
			}
		}
	}
}
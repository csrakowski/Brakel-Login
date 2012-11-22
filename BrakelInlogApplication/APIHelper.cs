using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using PushNotifications;

namespace BrakelInlogApplication
{
	/// <summary>
	/// The actual implementation of the API calls happens in this helper class
	/// </summary>
	public sealed class APIHelper
	{
		/// <summary>
		/// The private instance for easy instantiation
		/// </summary>
		private static readonly APIHelper _instance = new APIHelper();

		/// <summary>
		/// Private to prevent instantiation
		/// </summary>
		private APIHelper()
		{
			//Register event handler for background polling
			BackgroundPoller.Instance.OnResultChanged += OnPollingResult;
		}

		/// <summary>
		/// The Instance of this class
		/// </summary>
		public static APIHelper Instance
		{
			get { return _instance; }
		}

		/// <summary>
		/// Validates the user's credentials and returns a token that will be used to validate other requests
		/// </summary>
		/// <param name="username">The username</param>
		/// <param name="passwordHash">The hashed password</param>
		/// <returns>A token not equal to all 0 on succes, a token of all 0 on failure</returns>
		public Guid Login(string username, string passwordHash)
		{
			Guid userToken = Guid.Empty;

			using (var connection = new SqlConnection(ConstantHelper.ConnectionString))
			{
				connection.Open();

				// perform work with connection
				string query = String.Format("SELECT [hash] FROM [user] WHERE [username] = '{0}'", username);
				var command = new SqlCommand(query, connection);
				var sqlHash = command.ExecuteScalar() as String;

				//validate credentials
				if (passwordHash.Equals(sqlHash, StringComparison.OrdinalIgnoreCase))
				{
					//generate token
					userToken = Guid.NewGuid();

					//register token in db to the user
					query = String.Format("INSERT INTO [token] ([username], [token], [createDateTime]) VALUES('{0}','{1}','{2}')",
					                      username, userToken, DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
					command = new SqlCommand(query, connection);
					int result = command.ExecuteNonQuery();
					if (result < 1)
					{
						userToken = Guid.Empty;
					}
				}
			}

			//return id
			return userToken;
		}

		/// <summary>
		/// Returns a list of buildngs which the current user can see, and the permissions he has in those buildings
		/// </summary>
		/// <param name="userToken">The current user's token</param>
		/// <returns>The list of Buildings</returns>
		public List<Building> GetBuildings(Guid userToken)
		{
			var buildings = new List<Building>();

			using (var connection = new SqlConnection(ConstantHelper.ConnectionString))
			{
				connection.Open();
				string query = String.Format("SELECT [username] FROM [token] WHERE [token] = '{0}'", userToken);
				var command = new SqlCommand(query, connection);

				//Validate token
				var username = command.ExecuteScalar() as String;
				if (String.IsNullOrWhiteSpace(username))
				{
					throw new APIException("The provided userToken is invalid or has expired", "userToken");
				}
				else
				{
					//join buildings on users rights
					query = String.Format(@"SELECT	[building].*, [userBuildingCouple].[accessRights] FROM [building]
													LEFT JOIN [userBuildingCouple] ON [building].[buildingId] = [userBuildingCouple].[buildingId]
													LEFT JOIN [user] ON [user].[userId] = [userBuildingCouple].[userId]
											WHERE	[user].[username] = '{0}'", username);
					command = new SqlCommand(query, connection);

					//Fill collection
					SqlDataReader reader = command.ExecuteReader();
					while (reader.Read())
					{
						buildings.Add(new Building
							              {
								              AccessRole = Building.ParseAccessRightsFromString(reader["accessRights"].ToString()),
								              BuildingID = Int32.Parse(reader["buildingId"].ToString()),
								              BuildingName = reader["name"].ToString(),
								              Parent = Int32.Parse(reader["parentId"].ToString())
							              });
					}
				}
			}
			//return collection
			return buildings;
		}

		/// <summary>
		/// Method to iniate making changes to groups
		/// </summary>
		/// <param name="userToken">The user token</param>
		/// <param name="buildingId">The building id for the building in which the groups are</param>
		/// <param name="changes">The list of changes you want to commit</param>
		/// <returns>The list of changes with a boolean value to indicate succes of the operation per change</returns>
		public List<Changes> MakeChangesToGroups(Guid userToken, int buildingId, List<Changes> changes)
		{
			if (userToken != Guid.Empty)
			{
				if (buildingId != 0)
				{
					#region Make HTTP Request

					string requestBody = "";
					changes.ForEach(
						i => requestBody += ("," + i.ToJSONString())
						);
					if (requestBody.Length > 1)
						requestBody = requestBody.Substring(1);
					requestBody = @"{ ""changes"": [" + requestBody + "] }\r\n\r\n";

					//Get ip from database based on building id
					string targetBuilding = ConstantHelper.TestBuilding;

					var request = (HttpWebRequest) WebRequest.Create(targetBuilding);
					request.Method = "POST";
					request.Timeout = (5*60*1000); // 5 seconds
					request.KeepAlive = false;
					request.SendChunked = false;

					byte[] byte1 = Encoding.ASCII.GetBytes(requestBody);
					request.ContentType = "application/json";
					request.ContentLength = byte1.Length;
					Stream newStream = request.GetRequestStream();
					newStream.Write(byte1, 0, byte1.Length);

					#endregion

					JObject result = null;
					try
					{
						var response = (HttpWebResponse) request.GetResponse();

						Stream str = response.GetResponseStream();
						var buffer = new byte[str.Length];
						str.Read(buffer, 0, (int) str.Length);

						string resultString = Encoding.ASCII.GetString(buffer);
						Debug.WriteLine(resultString);
						result = JObject.Parse(resultString);
					}
					catch (Exception ex)
					{
						Debug.WriteLine(ex.Message);
						foreach (Changes item in changes)
						{
							item.ChangeStatus = false;
						}
						return changes;
					}

					var changesArray = result["changes"] as JArray;

					//parse json and update changes
					foreach (JToken item in changesArray)
					{
						Changes ch = changes.FirstOrDefault(i => i.GroupID.ToString() == item["GroupID"].ToString());
						if (ch != default(Changes))
						{
							ch.ChangeStatus = Boolean.Parse(item["ChangeStatus"].ToString() ?? Boolean.FalseString);
						}
					}

					//Start background polling					
					BackgroundPoller.Instance.StartPollingBuilding(userToken, buildingId);

					//return initial result
					return changes;
				}
				else
				{
					throw new APIException("The provided buildingId is invalid", "buildingId");
				}
			}
			else
			{
				throw new APIException("The provided userToken is invalid or expired", "userToken");
			}
		}

		/// <summary>
		/// Get the current user's screen layout for the selected building
		/// </summary>
		/// <param name="userToken">The current user's token</param>
		/// <param name="buildingId">The building for which you want the layout</param>
		/// <returns>A string representation of the XML, which describes the layout of the application</returns>
		public string GetUserLayout(Guid userToken, int buildingId)
		{
			string resultLayout = "";

			using (var connection = new SqlConnection(ConstantHelper.ConnectionString))
			{
				connection.Open();
				string query = String.Format("SELECT [username] FROM [token] WHERE [token] = '{0}'", userToken);
				var command = new SqlCommand(query, connection);

				//Validate token
				var username = command.ExecuteScalar() as String;
				if (String.IsNullOrWhiteSpace(username))
				{
					throw new APIException("The provided userToken is invalid or has expired", "userToken");
				}
				else
				{
					//get the layout for the user - building combination
					query = String.Format(@"SELECT	[userBuildingCouple].[screenLayout] FROM [userBuildingCouple]													
													LEFT JOIN [user] ON [user].[userId] = [userBuildingCouple].[userId]
											WHERE	[user].[username] = '{0}' and [userBuildingCouple].[buildingId] = {1}",
					                      username, buildingId);
					command = new SqlCommand(query, connection);

					//get the value and store it in the string
					SqlDataReader reader = command.ExecuteReader();
					if (reader.HasRows)
					{
						while (reader.Read())
						{
							resultLayout = reader["screenLayout"].ToString();
						}
					}
					else
					{
						throw new APIException("The provided buildingId is invalid", "buildingId");
					}
				}
			}

			//return layout json as a string
			return resultLayout;
		}

		/// <summary>
		/// Eventhandler for the polling result change
		/// </summary>
		/// <param name="userToken">The userToken of the user who initiated the poll</param>
		/// <param name="buildingId">The building this result is about</param>
		/// <param name="json">The result in JSON format</param>
		public static void OnPollingResult(Guid userToken, int buildingId, string json)
		{
			string deviceID = ConstantHelper.TestIPad; //get from database based on userToken
			string message = String.Format(@"{{ ""building"":{0}, ""changes"": {1} }}", buildingId, json);
			PushNotification.SendPushNotification(deviceID, message);
		}

		/// <summary>
		/// Get all floors in the building and the accessrights the user has in those rooms
		/// </summary>
		/// <param name="userToken">The current user's token</param>
		/// <param name="buildingId">The building id</param>
		/// <param name="getRoomsRecursivly">boolean to indicate if rooms should be retrieved recursivly, or ignored</param>
		/// <returns>The list of Floors</returns>
		public List<Floor> GetFloors(Guid userToken, int buildingId, bool getRoomsRecursivly)
		{
			var floors = new List<Floor>();

			using (var connection = new SqlConnection(ConstantHelper.ConnectionString))
			{
				connection.Open();
				string query = String.Format("SELECT [username] FROM [token] WHERE [token] = '{0}'", userToken);
				var command = new SqlCommand(query, connection);

				//Validate token
				var username = command.ExecuteScalar() as String;
				if (String.IsNullOrWhiteSpace(username))
				{
					throw new APIException("The provided userToken is invalid or has expired", "userToken");
				}
				else
				{
					//join buildings on users rights
					query = String.Format(@"SELECT	[building].*, [userBuildingCouple].[accessRights] FROM [building]
													LEFT JOIN [userBuildingCouple] ON [building].[parentId] = [userBuildingCouple].[buildingId]
													LEFT JOIN [user] ON [user].[userId] = [userBuildingCouple].[userId]
											WHERE	[user].[username] = '{0}' and [building].[parentId] = {1}", username, buildingId);
					command = new SqlCommand(query, connection);

					//Fill collection
					SqlDataReader reader = command.ExecuteReader();
					while (reader.Read())
					{
						var floor = new Floor
						{
							AccessRole = Building.ParseAccessRightsFromString(reader["accessRights"].ToString()),
							BuildingID = Int32.Parse(reader["buildingId"].ToString()),
							BuildingName = reader["name"].ToString(),
							Parent = Int32.Parse(reader["parentId"].ToString())
						};
						if (getRoomsRecursivly)
						{
							floor.Rooms = GetRooms(userToken, floor.BuildingID);
						}
						floors.Add(floor);
					}
				}
			}
			//return collection
			return floors;
		}

		/// <summary>
		/// Get all rooms on a certain floor
		/// </summary>
		/// <param name="userToken">The current user's token</param>
		/// <param name="floorId">The floor id</param>
		/// <returns>The list of rooms</returns>
		public List<Room> GetRooms(Guid userToken, int floorId)
		{
			var rooms = new List<Room>();

			using (var connection = new SqlConnection(ConstantHelper.ConnectionString))
			{
				connection.Open();
				string query = String.Format("SELECT [username] FROM [token] WHERE [token] = '{0}'", userToken);
				var command = new SqlCommand(query, connection);

				//Validate token
				var username = command.ExecuteScalar() as String;
				if (String.IsNullOrWhiteSpace(username))
				{
					throw new APIException("The provided userToken is invalid or has expired", "userToken");
				}
				else
				{
					//join buildings on users rights?
					query = String.Format(@"SELECT	[room].* FROM [room]
													LEFT JOIN [building] ON [room].[floorId] = [building].[buildingId]
											WHERE	[building].[buildingId] = {0}", floorId);
					command = new SqlCommand(query, connection);

					//Fill collection
					SqlDataReader reader = command.ExecuteReader();
					while (reader.Read())
					{
						rooms.Add(new Room()
						{
							RoomID = Int32.Parse(reader["roomId"].ToString()),
							RoomName = reader["roomName"].ToString(),
							BuildingID = Int32.Parse(reader["buildingId"].ToString()),
							XCoordinate = Int32.Parse(reader["xCoordinate"].ToString()),
							YCoordinate = Int32.Parse(reader["yCoordinate"].ToString()),
							Width = Int32.Parse(reader["width"].ToString()),
							Height = Int32.Parse(reader["height"].ToString()),
							IsEnabled = Boolean.Parse(reader["enabled"].ToString()),
							HasAlarmValue = Boolean.Parse(reader["hasAlarm"].ToString())
						});
					}
				}
			}
			//return collection
			return rooms;
		}
	}
}
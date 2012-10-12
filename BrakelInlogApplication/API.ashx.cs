using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Xml;
using Newtonsoft.Json.Linq;


namespace BrakelInlogApplication
{
	/// <summary>
	/// The APIException is used to indicate a certain argument does not meet API definition in term of formatting, it's actual value is not evaluated.
	/// </summary>
	public class APIException : ArgumentException
	{
		/// <summary>
		/// Initializes a new instance of the BrakelInlogApplication.APIException class.
		/// </summary>
		public APIException() : base() { }
		/// <summary>
		///  Initializes a new instance of the BrakelInlogApplication.APIException class with a specified error message.
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		public APIException(string message) : base(message) { }
		/// <summary>
		/// Initializes a new instance of the BrakelInlogApplication.APIException class with a specified error message and a reference to the inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		/// <param name="innerException">The exception that is the cause of the current exception. If the innerException parameter is not a null reference, the current exception is raised in a catch block that handles the inner exception.</param>
		public APIException(string message, Exception innerException) : base(message, innerException) { }
		/// <summary>
		/// Initializes a new instance of the BrakelInlogApplication.APIException class with a specified error message and the name of the parameter that causes this exception.
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		/// <param name="paramName">The name of the parameter that caused the current exception.</param>
		public APIException(string message, string paramName) : base(message, paramName) { }
		/// <summary>
		/// Initializes a new instance of the BrakelInlogApplication.APIException class with a specified error message, the parameter name, and a reference to the inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		/// <param name="paramName">The name of the parameter that caused the current exception.</param>
		/// <param name="innerException">The exception that is the cause of the current exception. If the innerException parameter is not a null reference, the current exception is raised in a catch block that handles the inner exception.</param>
		public APIException(string message, string paramName, Exception innerException) : base(message, paramName, innerException) { }
	}

	/// <summary>
	/// Entrypoint for the Async handler
	/// </summary>
	public class API : IHttpAsyncHandler
	{
		/// <summary>
		/// Boolean to indicate the handler is reusable which causes Singleton like behaviour (Performance tweak)
		/// </summary>
		public bool IsReusable { get { return true; } }

		/// <summary>
		/// Constructor, this will only be called once (Singleton behaviour)
		/// </summary>
		public API()
		{
		}

		/// <summary>
		/// Entrypoint for the request handeling (Called by the ASP.NET runtime, do not invoke manually!)
		/// </summary>
		/// <param name="context">The current HttpContext</param>
		/// <param name="cb">The AsyncCallback</param>
		/// <param name="extraData">Extra data object</param>
		/// <returns>The AsyncResult</returns>
		public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, Object extraData)
		{
			AsynchOperation asynch = new AsynchOperation(cb, context, extraData);
			asynch.StartAsyncWork();
			return asynch;
		}

		/// <summary>
		/// Exit point for the request handeling, all cleanup code should go here (Called by the ASP.NET runtime, do not invoke manually!)
		/// </summary>
		/// <param name="result">The AsyncResult returned by BeginProcessRequest</param>
		public void EndProcessRequest(IAsyncResult result)
		{
		}

		/// <summary>
		/// Old style entry point for handlers, should not be used.
		/// </summary>
		/// <param name="context">The current HttpContext</param>
		public void ProcessRequest(HttpContext context)
		{
			throw new InvalidOperationException();
		}
	}

	/// <summary>
	/// Handler class used to handle the async requests
	/// </summary>
	class AsynchOperation : IAsyncResult
	{
		#region Properties
		private HttpContext _context;
		private AsyncCallback _callback;

		private bool _completed;
		bool IAsyncResult.IsCompleted { get { return _completed; } }

		private Object _state;
		Object IAsyncResult.AsyncState { get { return _state; } }

		WaitHandle IAsyncResult.AsyncWaitHandle { get { return null; } }
		bool IAsyncResult.CompletedSynchronously { get { return false; } }
		#endregion
		#region Framework Registration Methods
		/// <summary>
		/// Constructor (Called in BeginProcessRequest, do not invoke manually!) 
		/// </summary>
		/// <param name="callback">The AsyncCalback</param>
		/// <param name="context">The current HttpContext</param>
		/// <param name="state">The state object</param>
		public AsynchOperation(AsyncCallback callback, HttpContext context, Object state)
		{
			_callback = callback;
			_context = context;
			_state = state;
			_completed = false;
		}

		/// <summary>
		/// Method to start the async task
		/// </summary>
		public void StartAsyncWork()
		{
			ThreadPool.QueueUserWorkItem(new WaitCallback(StartAsyncTask), null);
		}
		#endregion

		/// <summary>
		/// Internal handler method that handles the request
		/// </summary>
		/// <param name="workItemState"></param>
		private void StartAsyncTask(Object workItemState)
		{
			_context.Response.ContentType = "application/JSON";
			String result = "";
			try
			{
				string command = _context.Request.QueryString["command"] ?? "";
				if (!String.IsNullOrWhiteSpace(command))
				{
					switch (command)
					{
						case "login":
							{
								#region Prepare to handle login request
								string username = _context.Request.Form["username"];
								string hash = _context.Request.Form["hash"];
								//string username = _context.Request.QueryString["username"];
								//string hash = _context.Request.QueryString["hash"];
								if (!String.IsNullOrWhiteSpace(username))
								{
									if (!String.IsNullOrWhiteSpace(hash))
									{
										Guid userToken = login(username, hash);
										result = String.Format(@"{{ ""userToken"":""{0}"" }}", userToken);
									}
									else
									{
										throw new APIException("No valid passwordhash was provided", "hash");
									}
								}
								else
								{
									throw new APIException("No valid username was provided", "username");
								}
								#endregion
								break;
							}
						case "getBuildings":
							{
								#region Prepare to handle getBuildings request
								string userTokenString = _context.Request.Form["userToken"];
								//string userTokenString = _context.Request.QueryString["userToken"];
								Guid userToken;
								if (Guid.TryParse(userTokenString, out userToken))
								{
									List<Building> buildings = getBuildings(userToken);
									buildings.ForEach(
										i => result += ("," + i.ToJSONString())
									);
									if (result.Length > 1)
										result = result.Substring(1);
									result = @"{ ""buildings"": [" + result + "] }";
								}
								else
								{
									throw new APIException("No valid userToken was provided", "userToken");
								}
								#endregion
								break;
							}
						case "getLayout":
							{
								#region Prepare to handle getLayout request
								string userTokenString = _context.Request.Form["userToken"];
								string buildingIdString = _context.Request.Form["buildingId"];
								//string userTokenString = _context.Request.QueryString["userToken"];
								//string buildingIdString = _context.Request.QueryString["buildingId"];
								Guid userToken;
								if (Guid.TryParse(userTokenString, out userToken))
								{
									int buildingId;
									if (Int32.TryParse(buildingIdString, out buildingId))
									{
										string layoutXMLString = getUserLayout(userToken, buildingId);
										result = @"{ ""layout"":""" + layoutXMLString.Replace("\"", "\\\"") + @"""}";
									}
									else
									{
										throw new APIException("No valid buildingId was provided", "buildingId");
									}
								}
								else
								{
									throw new APIException("No valid userToken was provided", "userToken");
								}
								#endregion
								break;
							}
						case "changeGroups":
							{
								#region Prepare to handle changeGroups request
								string userTokenString = _context.Request.Form["userToken"];
								string buildingIdString = _context.Request.Form["buildingId"];
								//string userTokenString = _context.Request.QueryString["userToken"];
								//string buildingIdString = _context.Request.QueryString["buildingId"];
								Guid userToken;
								if (Guid.TryParse(userTokenString, out userToken))
								{
									int buildingId;
									if (Int32.TryParse(buildingIdString, out buildingId))
									{
										string changesString = _context.Request.QueryString["changes"];
										JObject obj = JObject.Parse(changesString);
										List<Changes> changes = new List<Changes>();
										changes = makeChangesToGroups(userToken, buildingId, changes);
										changes.ForEach(
											i => result += ("," + i.ToJSONString())
										);
										if (result.Length > 1)
											result = result.Substring(1);
										result = @" { ""changes"": [" + result + "] }";
									}
									else
									{
										throw new APIException("No valid buildingId was provided", "buildingId");
									}
								}
								else
								{
									throw new APIException("No valid userToken was provided", "userToken");
								}
								#endregion
								break;
							}
						default:
							{
								throw new APIException(String.Format("'{0}' is not a valid command", command), "command");
							}
					}
				}
				else
				{
					throw new APIException("Invalid invocation, please specify a command", "command");
				}
			}
			catch (APIException ex)
			{
				result = String.Format(@"{{ ""error"":""{0}"" }}", ex.Message.Replace('\n', ' ').Replace('\r', ' '));
			}
			catch (Exception ex)
			{
				result = String.Format(@"{{ ""error"":""{0}"", ""stacktrace"":""{1}"" }}", ex.Message.Replace('\n', ' ').Replace('\r', ' '), ex.StackTrace.Replace('\n', ' ').Replace('\r', ' ').Replace("\\", "\\\\"));
			}
			finally
			{
				_context.Response.Write(result);
				_completed = true;
				_callback(this);
			}
		}

		/// <summary>
		/// Validates the user's credentials and returns a token that will be used to validate other requests
		/// </summary>
		/// <param name="username">The username</param>
		/// <param name="passwordHash">The hashed password</param>
		/// <returns>A token not equal to all 0 on succes, a token of all 0 on failure</returns>
		private Guid login(string username, string passwordHash)
		{
			Guid userToken = Guid.Empty;

			using (SqlConnection connection = new SqlConnection(ConstantHelper.ConnectionString))
			{
				connection.Open();

				// perform work with connection
				string query = String.Format("SELECT [hash] FROM [user] WHERE [username] = '{0}'", username);
				SqlCommand command = new SqlCommand(query, connection);
				string sqlHash = command.ExecuteScalar() as String;

				//validate credentials
				if (passwordHash.Equals(sqlHash, StringComparison.OrdinalIgnoreCase))
				{
					//generate token
					userToken = Guid.NewGuid();

					//register token in db to the user
					query = String.Format("INSERT INTO [token] ([username], [token], [createDateTime]) VALUES('{0}','{1}','{2}')", username, userToken, DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
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
		private List<Building> getBuildings(Guid userToken)
		{
			List<Building> buildings = new List<Building>();

			using (SqlConnection connection = new SqlConnection(ConstantHelper.ConnectionString))
			{
				connection.Open();
				string query = String.Format("SELECT [username] FROM [token] WHERE [token] = '{0}'", userToken);
				SqlCommand command = new SqlCommand(query, connection);

				//Validate token
				string username = command.ExecuteScalar() as String;
				if (String.IsNullOrWhiteSpace(username))
				{
					throw new APIException("The provided userToken is invalid or has expired", "userToken");
				}
				else
				{
					//join buildings on users rights
					query = String.Format(@"SELECT	[building].*, [userBuildingCouple].[accessRights] FROM [building]
													LEFT JOIN [userBuildingCouple] ON building.buildingId = [userBuildingCouple].[buildingId]
													LEFT JOIN [user] ON [user].[userId] = [userBuildingCouple].[userId]
											WHERE	[user].[username] = '{0}'", username);
					command = new SqlCommand(query, connection);

					//Fill collection
					SqlDataReader reader = command.ExecuteReader();
					while (reader.Read())
					{
						buildings.Add(new Building()
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
		private List<Changes> makeChangesToGroups(Guid userToken, int buildingId, List<Changes> changes)
		{
			if (userToken != Guid.Empty)
			{
				if (buildingId != 0)
				{
					foreach (var item in changes)
					{
						item.ChangeStatus = true;
					}
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

			//validate token
			//validate user has rights to make changes in this building
			//process changes
			//set boolean to indicate success
			//return list
		}

		/// <summary>
		/// Get the current user's screen layout for the selected building
		/// </summary>
		/// <param name="userToken">The current user's token</param>
		/// <param name="buildingId">The building for which you want the layout</param>
		/// <returns>A string representation of the XML, which describes the layout of the application</returns>
		private string getUserLayout(Guid userToken, int buildingId)
		{
			string resultLayout = "";

			using (SqlConnection connection = new SqlConnection(ConstantHelper.ConnectionString))
			{
				connection.Open();
				string query = String.Format("SELECT [username] FROM [token] WHERE [token] = '{0}'", userToken);
				SqlCommand command = new SqlCommand(query, connection);

				//Validate token
				string username = command.ExecuteScalar() as String;
				if (String.IsNullOrWhiteSpace(username))
				{
					throw new APIException("The provided userToken is invalid or has expired", "userToken");
				}
				else
				{
					//get the layout for the user - building combination
					query = String.Format(@"SELECT	[userBuildingCouple].[screenLayout] FROM [userBuildingCouple]													
													LEFT JOIN [user] ON [user].[userId] = [userBuildingCouple].[userId]
											WHERE	[user].[username] = '{0}' and [userBuildingCouple].[buildingId] = {1}", username, buildingId);
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

			//return layout xml as a string
			return resultLayout;
		}
	}
}
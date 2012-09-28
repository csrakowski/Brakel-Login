using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Xml;


namespace BrakelInlogApplication
{
	/// <summary>
	/// The APIException is used to indicate a certain argument does not meet API definition in term of formatting, it's actual value is not evaluated.
	/// </summary>
	public class APIException : ArgumentException
	{
		/// <summary>
		/// Initializes a new instance of the System.ArgumentException class.
		/// </summary>
		public APIException(): base() { }
		/// <summary>
		///  Initializes a new instance of the System.ArgumentException class with a specified error message.
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		public APIException(string message) : base(message) { }
		/// <summary>
		/// Initializes a new instance of the System.ArgumentException class with a specified error message and a reference to the inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		/// <param name="innerException">The exception that is the cause of the current exception. If the innerException parameter is not a null reference, the current exception is raised in a catch block that handles the inner exception.</param>
		public APIException(string message, Exception innerException) : base(message, innerException) { }
		/// <summary>
		/// Initializes a new instance of the System.ArgumentException class with a specified error message and the name of the parameter that causes this exception.
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		/// <param name="paramName">The name of the parameter that caused the current exception.</param>
		public APIException(string message, string paramName) : base(message, paramName) { }
		/// <summary>
		/// Initializes a new instance of the System.ArgumentException class with a specified error message, the parameter name, and a reference to the inner exception that is the cause of this exception.
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
							string username = _context.Request.QueryString["username"];
							if (!String.IsNullOrWhiteSpace(username))
							{
								if (!String.IsNullOrWhiteSpace(username))
								{
									string hash = _context.Request.QueryString["hash"];
									Guid userToken = login(username, hash);
									result = String.Format(@"{{ ""status"":""{0}"", ""token"":""{1}"" }}", ((userToken == Guid.Empty) ? "failed" : "success"), userToken);
								}
								else
								{
									throw new APIException("Error: No valid password was provided", "password");
								}
							}
							else
							{
								throw new APIException("Error: No valid username was provided", "username");
							}
							break;
						}
						case "getBuildings":
						{
							string userTokenString = _context.Request.QueryString["userToken"];
							Guid userToken;
							if (Guid.TryParse(userTokenString, out userToken))
							{
								List<Building> buildings = getBuildings(userToken);
								result = "";
							}
							else
							{
								throw new APIException("Error: No valid userToken was provided", "userToken");
							}
							break;
						}
						case "getLayout":
						{
							string userTokenString = _context.Request.QueryString["userToken"];
							string buildingIdString = _context.Request.QueryString["buildingId"];
							Guid userToken;
							if (Guid.TryParse(userTokenString, out userToken))
							{
								Guid buildingId;
								if (Guid.TryParse(buildingIdString, out buildingId))
								{
									string layoutXMLString = getUserLayout(userToken, buildingId);
								}
								else
								{
									throw new APIException("Error: No valid buildingId was provided", "buildingId");
								}
							}
							else
							{
								throw new APIException("Error: No valid userToken was provided", "userToken");
							}
							break;
						}
						case "changeGroups":
						{
							string userTokenString = _context.Request.QueryString["userToken"];
							string buildingIdString = _context.Request.QueryString["buildingId"];
							Guid userToken;
							if (Guid.TryParse(userTokenString, out userToken))
							{
								Guid buildingId;
								if (Guid.TryParse(buildingIdString, out buildingId))
								{
									string layoutXMLString = getUserLayout(userToken, buildingId);
								}
								else
								{
									throw new APIException("Error: No valid buildingId was provided", "buildingId");
								}
							}
							else
							{
								throw new APIException("Error: No valid userToken was provided", "userToken");
							}
							break;
						}
						default:
						{
							throw new APIException(String.Format("Error: '{0}' is not a valid command", command), "command");
						}
					}
				}
				else
				{
					throw new APIException("Error: Invalid invocation, please specify a command", "command");
				}
			}
			catch (Exception ex)
			{
				result = String.Format(@"{{ ""message"":""{0}"", ""stacktrace"":""{1}"" }}", ex.Message, ex.StackTrace);				
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
			throw new NotImplementedException();
			//using (SqlConnection connection = new SqlConnection(ConstantHelper.ConnectionString))
			//{
			//    connection.Open();
			//    // perform work with connection
			//}

			//validate credentials,
			//generate token
			//register token in db to the user
			//return id
		}

		/// <summary>
		/// Returns a list of buildngs which the current user can see, and the permissions he has in those buildings
		/// </summary>
		/// <param name="userToken">The current user's token</param>
		/// <returns>The list of Buildings</returns>
		private List<Building> getBuildings(Guid userToken)
		{
			throw new NotImplementedException();
			
			//Validate token
			//join buildings on users rights
			//return collection
		}

		/// <summary>
		/// Method to iniate making changes to groups
		/// </summary>
		/// <param name="userToken">The user token</param>
		/// <param name="buildingId">The building id for the building in which the groups are</param>
		/// <param name="changes">The list of changes you want to commit</param>
		/// <returns>The list of changes with a boolean value to indicate succes of the operation per change</returns>
		private List<Changes> makeChangesToGroups(Guid userToken, Guid buildingId, List<Changes> changes)
		{
			throw new NotImplementedException();

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
		private string getUserLayout(Guid userToken, Guid buildingId)
		{
			throw new NotImplementedException();

			//validate token
			//get the layout for the user - building combination
			//return layout xml as a string
		}
	}
}
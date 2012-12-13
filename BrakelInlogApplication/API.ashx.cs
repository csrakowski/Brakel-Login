using System;
using System.Collections.Generic;
using System.Threading;
using System.Web;
using PushNotifications;
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
		public APIException()
		{
		}

		/// <summary>
		///  Initializes a new instance of the BrakelInlogApplication.APIException class with a specified error message.
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		public APIException(string message) : base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the BrakelInlogApplication.APIException class with a specified error message and a reference to the inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		/// <param name="innerException">The exception that is the cause of the current exception. If the innerException parameter is not a null reference, the current exception is raised in a catch block that handles the inner exception.</param>
		public APIException(string message, Exception innerException) : base(message, innerException)
		{
		}

		/// <summary>
		/// Initializes a new instance of the BrakelInlogApplication.APIException class with a specified error message and the name of the parameter that causes this exception.
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		/// <param name="paramName">The name of the parameter that caused the current exception.</param>
		public APIException(string message, string paramName) : base(message, paramName)
		{
		}

		/// <summary>
		/// Initializes a new instance of the BrakelInlogApplication.APIException class with a specified error message, the parameter name, and a reference to the inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		/// <param name="paramName">The name of the parameter that caused the current exception.</param>
		/// <param name="innerException">The exception that is the cause of the current exception. If the innerException parameter is not a null reference, the current exception is raised in a catch block that handles the inner exception.</param>
		public APIException(string message, string paramName, Exception innerException)
			: base(message, paramName, innerException)
		{
		}
	}

	/// <summary>
	/// Entrypoint for the Async handler
	/// </summary>
	public class API : IHttpAsyncHandler
	{
		#region IHttpAsyncHandler Members

		/// <summary>
		/// Boolean to indicate the handler is reusable which causes Singleton like behaviour (Performance tweak)
		/// </summary>
		public bool IsReusable
		{
			get { return true; }
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
			var asynch = new AsynchOperation(cb, context, extraData);
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

		#endregion
	}

	/// <summary>
	/// Handler class used to handle the async requests
	/// </summary>
	internal class AsynchOperation : IAsyncResult
	{
		#region Properties

		private readonly AsyncCallback _callback;
		private readonly HttpContext _context;
		private readonly Object _state;

		private bool _completed;

		bool IAsyncResult.IsCompleted
		{
			get { return _completed; }
		}

		Object IAsyncResult.AsyncState
		{
			get { return _state; }
		}

		WaitHandle IAsyncResult.AsyncWaitHandle
		{
			get { return null; }
		}

		bool IAsyncResult.CompletedSynchronously
		{
			get { return false; }
		}

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
			ThreadPool.QueueUserWorkItem(StartAsyncTask, null);
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
								string device = _context.Request.Form["device"] ?? "";
								if (!String.IsNullOrWhiteSpace(username))
								{
									if (!String.IsNullOrWhiteSpace(hash))
									{
										Guid userToken = APIHelper.Instance.Login(username, hash, device);
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
								Guid userToken;
								if (Guid.TryParse(userTokenString, out userToken))
								{
									List<Building> buildings = APIHelper.Instance.GetBuildings(userToken);
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
						case "getFloors":
							{
								#region Prepare to handle getFloors request

								String userTokenString = _context.Request.Form["userToken"];
								String buildingIdString = _context.Request.Form["buildingId"];
								Boolean getRooms = Boolean.Parse(_context.Request.Form["getRooms"] ?? Boolean.FalseString);
								Guid userToken;
								if (Guid.TryParse(userTokenString, out userToken))
								{
									UInt32 buildingId;
									if (UInt32.TryParse(buildingIdString, out buildingId))
									{
										List<Floor> floors = APIHelper.Instance.GetFloors(userToken, buildingId, getRooms);
										floors.ForEach(
											i => result += ("," + i.ToJSONString())
											);
										if (result.Length > 1)
											result = result.Substring(1);
										result = @"{ ""floors"": [" + result + "] }";
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
						case "getRooms":
							{
								#region Prepare to handle getFloors request

								string userTokenString = _context.Request.Form["userToken"];
								string floorIdString = _context.Request.Form["floorId"];
								Guid userToken;
								if (Guid.TryParse(userTokenString, out userToken))
								{
									UInt32 floorId;
									if (UInt32.TryParse(floorIdString, out floorId))
									{
										List<Room> rooms = APIHelper.Instance.GetRooms(userToken, floorId);
										rooms.ForEach(
											i => result += ("," + i.ToJSONString())
											);
										if (result.Length > 1)
											result = result.Substring(1);
										result = @"{ ""rooms"": [" + result + "] }";
									}
									else
									{
										throw new APIException("No valid floorId was provided", "floorId");
									}
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
								Guid userToken;
								if (Guid.TryParse(userTokenString, out userToken))
								{
									UInt32 buildingId;
									if (UInt32.TryParse(buildingIdString, out buildingId))
									{
										string layoutXMLString = APIHelper.Instance.GetUserLayout(userToken, buildingId);
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

								string userTokenString = _context.Request.Form["userToken"] ?? Guid.NewGuid().ToString();
								string buildingIdString = _context.Request.Form["buildingId"] ?? "1";
								Guid userToken;
								if (Guid.TryParse(userTokenString, out userToken))
								{
									UInt32 buildingId;
									if (UInt32.TryParse(buildingIdString, out buildingId))
									{
										string changesString = _context.Request.Form["changes"] ?? "[]";
										var obj = JArray.Parse(changesString);
										var changes = new List<Changes>();
										foreach (var item in obj) {
											changes.Add (new Changes() {
												GroupID = UInt32.Parse (item["GroupID"].ToString()),
												ChangeValue = UInt32.Parse (item["ChangeValue"].ToString())
											});
										}
										if(changes.Count == 0)
										{
											var r = new Random();
											var max = r.Next(2, 5);
											for (uint j = 0; j < max; j++)
											{
												changes.Add(new Changes { GroupID = (1 + j), ChangeValue = (uint)r.Next(0, 255) });
											}
										}
										changes = APIHelper.Instance.MakeChangesToGroups(userToken, buildingId, changes);
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
						case "getGroups":
							{
								#region Prepare to handle getGroups request

								string userTokenString = _context.Request.Form["userToken"];
								string buildingIdString = _context.Request.Form["buildingId"];
								Guid userToken;
								if (Guid.TryParse(userTokenString, out userToken))
								{
									UInt32 buildingId;
									if (UInt32.TryParse(buildingIdString, out buildingId))
									{
										List<Changes> changes = APIHelper.Instance.GetGroups(userToken, buildingId);
										changes.ForEach(
											i => result += ("," + i.ToJSONString())
										);
										if (result.Length > 1)
											result = result.Substring(1);
										result = @" { ""groups"": [" + result + "] }";
									}
								}
								#endregion

								break;
							}
						case "testPush":
							{
								#region Handle testPush
								
								string deviceID = ConstantHelper.TestIPad;
								string message = _context.Request.QueryString["message"];
								bool status = PushNotification.SendPushNotification(deviceID, message);
								result = String.Format(@"{{ ""status"":{0} }}", status.ToString().ToLower());
								
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
				result = String.Format(@"{{ ""error"":""{0}"", ""stacktrace"":""{1}"" }}",
				                       ex.Message.Replace('\n', ' ').Replace('\r', ' '),
				                       ex.StackTrace.Replace('\n', ' ').Replace('\r', ' ').Replace("\\", "\\\\"));
			}
			finally
			{
				_context.Response.Write(result);
				_completed = true;
				_callback(this);
			}
		}
	}
}
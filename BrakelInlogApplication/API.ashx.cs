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
	/// Entrypoint for the Async handler
	/// </summary>
	class API : IHttpAsyncHandler
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
			context.Response.Write("<p>Begin IsThreadPoolThread is " + Thread.CurrentThread.IsThreadPoolThread + "</p>\r\n");
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

		/// <summary>
		/// Internal handler method that handles the request
		/// </summary>
		/// <param name="workItemState"></param>
		private void StartAsyncTask(Object workItemState)
		{
			string str = "<p>Completion IsThreadPoolThread is " + Thread.CurrentThread.IsThreadPoolThread + "</p>\r\n";
			_context.Response.Write(str);
			Debug.WriteLine(str);

			_context.Response.Write("Hello World from Async Handler!");
			
			_completed = true;
			_callback(this);
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
		}

		/// <summary>
		/// Returns a list of buildngs which the current user can see, and the permissions he has in those buildings
		/// </summary>
		/// <param name="userToken">The current user's token</param>
		/// <returns>The list of Buildings</returns>
		private List<Building> getBuildings(Guid userToken)
		{
			throw new NotImplementedException();
			//using (SqlConnection connection = new SqlConnection(ConstantHelper.ConnectionString))
			//{
			//    connection.Open();
			//    // perform work with connection
			//}
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
			//using (SqlConnection connection = new SqlConnection(ConstantHelper.ConnectionString))
			//{
			//    connection.Open();
			//    // perform work with connection
			//}
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
			//using (SqlConnection connection = new SqlConnection(ConstantHelper.ConnectionString))
			//{
			//    connection.Open();
			//    // perform work with connection
			//}
		}
	}
}
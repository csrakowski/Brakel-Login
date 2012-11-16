using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PushNotifications
{
	/// <summary>
	/// PushNotification helper class
	/// </summary>
	public sealed class PushNotification
	{
		/// <summary>
		/// Private constructor, prevent instantiation
		/// </summary>
		private PushNotification ()
		{

		}

		/// <summary>
		/// Method used to send pushnotifications
		/// </summary>
		/// <param name="deviceID">The deviceID of the device the notification needs to be send to</param>
		/// <param name="message">The message that needs to be send</param>
		/// <returns></returns>
		public static bool SendPushNotification(string deviceID, string message)
		{
			//bool status = true;

			throw new NotImplementedException("SendPushNotification staat op de planning voor later deze dag");

			//return status;
		}
	}
}

using System;
using PushSharp.Apple;
using PushSharp;
using System.IO;

namespace PushNotifications
{
    /// <summary>
    /// PushNotification helper class
    /// </summary>
    public static class PushNotification
    {
		/// <summary>
		/// Internal counter
		/// </summary>
	    private static int _count = 1;

		private const bool sandBox = true;

		/// <summary>
		/// The apple certificate.
		/// </summary>
		private static byte[] appleCert;

		/// <summary>
		/// Initializes the <see cref="PushNotifications.PushNotification"/> class.
		/// </summary>
		static PushNotification()
		{
			appleCert = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "brakelnotify.p12"));
		}

	    /// <summary>
        /// Method used to send pushnotifications
        /// </summary>
        /// <param name="deviceID">The deviceID of the device the notification needs to be send to</param>
        /// <param name="message">The message that needs to be send</param>
        /// <returns>Boolean indicating result status</returns>
        public static bool SendPushNotification(string deviceID, string message)
        {
			try
			{
				System.Diagnostics.Debug.WriteLine ("Sending push message WithBadge({0}), WithAlert({1}), ForDeviceToken({2})", _count, message, deviceID);
				using (var push = new PushService())
				{
					//Configure and start Apple APNS
					push.StartApplePushService(new ApplePushChannelSettings(!sandBox, appleCert, "brakel"));
					
					//Fluent construction of an iOS notification
					push.QueueNotification(NotificationFactory.Apple()
					                       .ForDeviceToken(deviceID)
					                       .WithAlert(message)
					                       .WithBadge(_count++));
				}
			} catch (Exception ex) {
				System.Diagnostics.Debug.WriteLine(ex.Message);
				System.Diagnostics.Debug.WriteLine(ex.StackTrace);
				while (ex.InnerException != null ) {
					ex = ex.InnerException;
					System.Diagnostics.Debug.WriteLine(ex.Message);
					System.Diagnostics.Debug.WriteLine(ex.StackTrace);
				}
				return false;
			}
	        return true;
        }
    }
}
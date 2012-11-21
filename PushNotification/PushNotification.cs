﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Security;
using System.Text;
using System.Net.Security;
using PushSharp.Common;
using PushSharp.Apple;
using PushSharp;
using System.IO;

namespace PushNotifications
{
    /// <summary>
    /// PushNotification helper class
    /// </summary>
    public class PushNotification
    {

        static int count;
        /// <summary>
        /// Private constructor, prevent instantiation
        /// </summary>
        private PushNotification()
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
            bool status = true;
            bool sandBox = true;

            if (count == null)
            {
                count = 1;
            }
            else
            {
                count++;
            }

            PushService push = new PushService();

            var appleCert = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../PushNotification/brakelnotify.p12"));

            //Configure and start Apple APNS
            push.StartApplePushService(new ApplePushChannelSettings(!sandBox, appleCert, "brakel"));

            //Fluent construction of an iOS notification
            push.QueueNotification(NotificationFactory.Apple()
                .ForDeviceToken(deviceID)
                .WithAlert(message)
                .WithBadge(count));

            return status;
        }
    }
}

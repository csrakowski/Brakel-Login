﻿using System;

namespace BrakelInlogApplication
{
	/// <summary>
	/// Class that represents a room inside a building
	/// </summary>
	public class Room
	{
		/// <summary>
		/// The room id used to identify the room
		/// </summary>
		public UInt32 RoomID { get; set; }

		/// <summary>
		/// Optional name of the room for human readable context
		/// </summary>
		public String RoomName { get; set; }

		/// <summary>
		/// Parent building of the current room
		/// </summary>
		public UInt32 BuildingID { get; set; }

		/// <summary>
		/// The X Coordinate of the room on the map
		/// </summary>
		public Int32 XCoordinate { get; set; }

		/// <summary>
		/// The Y Coordinate of the room on the map
		/// </summary>
		public Int32 YCoordinate { get; set; }

		/// <summary>
		/// The width of the room on the map
		/// </summary>
		public UInt32 Width { get; set; }

		/// <summary>
		/// The height of the room on the map
		/// </summary>
		public UInt32 Height { get; set; }

		/// <summary>
		/// Boolean value to indicate wether the room is clickable
		/// </summary>
		public Boolean IsEnabled { get; set; }

		/// <summary>
		/// Boolean value to indicate wether the room is currently in alarm mode
		/// </summary>
		public Boolean HasAlarmValue { get; set; }

		/// <summary>
		/// Returns a JSON representation of the current object
		/// </summary>
		/// <returns>The JSON representing the current object</returns>
		public String ToJSONString()
		{
			return
				String.Format(@"{{""RoomID"":{0},""RoomName"":""{1}"",""BuildingID"":""{2}"",""X"":""{3}"",""Y"":""{4}"",""Width"":""{5}"",""Height"":""{6}"",""Enabled"":{7},""HasAlarm"":{8}}}",
					RoomID, RoomName, BuildingID, XCoordinate, YCoordinate, Width, Height, IsEnabled.ToString().ToLower(), HasAlarmValue.ToString().ToLower());
		}
	}
}
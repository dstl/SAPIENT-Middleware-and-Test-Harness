// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: SapientCommsCommon.cs$
// <copyright file="SapientCommsCommon.cs" company="QinetiQ">
// Crown-Owned Copyright (c) supplied by QinetiQ. See Release/Supply Conditions
// </copyright>

namespace SapientServices.Communication
{
    /// <summary>
    /// delegates class for Sapient Communications
    /// </summary>
    public class SapientCommsCommon
    {
        /// <summary>
        /// delegate for Data Received Callback
        /// </summary>
        /// <param name="msgBuffer">message data</param>
        /// <param name="size">message size in bytes</param>
        /// <param name="client">connection object</param>
        public delegate void DataReceivedCallback(ref byte[] msgBuffer, int size, IConnection client);

        /// <summary>
        /// delegate for Status Callback
        /// </summary>
        /// <param name="statusMsg">status message</param>
        /// <param name="client">connection object</param>
        public delegate void StatusCallback(string statusMsg, IConnection client);
    }
}

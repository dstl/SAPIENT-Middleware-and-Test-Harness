// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: IConnection.cs$
// <copyright file="IConnection.cs" company="QinetiQ">
// Crown-Owned Copyright (c) supplied by QinetiQ. See Release/Supply Conditions
// </copyright>

namespace SapientServices.Communication
{
    /// <summary>
    /// Interface for Sending Data over a communications connection
    /// </summary>
    public interface IConnection
    {
        /// <summary>
        /// Gets a Connection unique identifier
        /// </summary>
        uint ConnectionID { get; }

        /// <summary>
        /// interface method to Send Message
        /// </summary>
        /// <param name="msg">message to send</param>
        /// <param name="msg_size">size in bytes</param>
        /// <returns>whether successful</returns>
        bool SendMessage(byte[] msg, int msg_size);

        /// <summary>
        /// interface method to Set No Delay
        /// </summary>
        /// <param name="no_delay">set connection to no delay</param>
        void SetNoDelay(bool no_delay);

        /// <summary>
        /// set whether to end subsequent messages with null termination
        /// </summary>
        /// <param name="useNullTermination">whether to include null termination on sent messages</param>
        void SetSendNullTermination(bool useNullTermination);
    }
}

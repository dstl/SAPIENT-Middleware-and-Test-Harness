// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: MessageSender.cs$
// <copyright file="MessageSender.cs" company="QinetiQ">
// Crown-Owned Copyright (c) supplied by QinetiQ. See Release/Supply Conditions
// </copyright>

namespace SapientASMsimulator
{
    using System;
    using System.Threading;
    using SapientServices;
    using SapientServices.Communication;

    /// <summary>
    /// Send Message
    /// </summary>
    public class MessageSender
    {
        /// <summary>
        /// Gets or sets a value indicating whether to fragment data to test communication resilience
        /// </summary>
        public static bool FragmentData { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to put a delay between data fragments
        /// </summary>
        public static int PacketDelay { get; set; }

        /// <summary>
        /// Send Data over Sapient connection
        /// </summary>
        /// <param name="messenger">connection object</param>
        /// <param name="message">message string to send</param>
        /// <param name="logger">file data logger</param>
        /// <returns>true if successful</returns>
        public static bool Send(IConnection messenger, string message, SapientLogger logger)
        {
            // add nulkl termination 
            if (Properties.Settings.Default.sendNullTermination)
            {
                message += '\0';
            }

            byte[] record_bytes = System.Text.Encoding.UTF8.GetBytes(message);

            bool retval = false;

            // fragment packet to test handling of partial messages
            if (FragmentData && (record_bytes.Length > 1500))
            {
                retval = messenger.SendMessage(record_bytes, 1500);

                if (retval)
                {
                    Thread.Sleep(PacketDelay);
                    byte[] remainingBytes = new byte[record_bytes.Length - 1500];
                    Array.Copy(record_bytes, 1500, remainingBytes, 0, record_bytes.Length - 1500);
                    retval = messenger.SendMessage(remainingBytes, remainingBytes.Length);
                }
            }
            else
            {
                retval = messenger.SendMessage(record_bytes, record_bytes.Length);
            }

            if (logger != null && Properties.Settings.Default.Log)
            {
                logger.Log(message);
            }

            return retval;
        }
    }
}

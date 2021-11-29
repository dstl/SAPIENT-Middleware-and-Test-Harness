// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: MessageSender.cs$
// Crown-Owned Copyright (c) supplied by QinetiQ. See Release/Supply Conditions

using System;
using System.Threading;
using SapientServices.Communication;

namespace SapientHldmmSimulator
{
    /// <summary>
    /// Send Message
    /// </summary>
    public class MessageSender
    {
        public static bool FragmentData { get; set; }

        public static int PacketDelay { get; set; }

        public static bool Send(IConnection messenger, string message)
        {
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

            return retval;
        }
    }
}

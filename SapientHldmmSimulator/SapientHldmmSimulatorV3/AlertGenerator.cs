// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: AlertGenerator.cs$
// Crown-Owned Copyright (c) supplied by QinetiQ. See Release/Supply Conditions

namespace SapientHldmmSimulator
{
    using System;
    using System.Threading;
    using log4net;
    using SapientServices.Communication;
    using SapientServices.Data;

    /// <summary>
    /// Generate Alert messages
    /// </summary>
    public class AlertGenerator : XmlGenerators
    {
        /// <summary>
        /// log4net logger
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Properties

        /// <summary>
        /// Gets the Alert ID field to use in alert message
        /// </summary>
        public int AlertID { get; private set; }

        /// <summary>
        /// Gets or sets an Image URL to provide with the detection message
        /// </summary>
        public static string ImageURL { get; set; }

        #endregion

        /// <summary>
        /// Send an Alert message
        /// </summary>
        /// <param name="comms_connection">connection to use to send message</param>
        public void GenerateAlert(object comms_connection)
        {
            IConnection messenger = (IConnection)comms_connection;
            do
            {
                Alert alert = GenerateAlert(this.AlertID);
                string xmlstring = ConfigXMLParser.Serialize(alert);
                bool retval = MessageSender.Send(messenger, xmlstring);

                if (retval)
                {
                    this.MessageCount++;

                    Log.InfoFormat("Send Alert: {0}", this.AlertID);
                }
                else
                {
                    Log.ErrorFormat("Send Alert Failed: {0}", this.AlertID);
                }

                this.AlertID++;

                if (this.LoopMessages)
                {
                    Thread.Sleep(this.LoopTime);
                }
            } while (this.LoopMessages);
        }

        /// <summary>
        /// Generate example alert
        /// </summary>
        /// <param name="alertId">alert ID field value</param>
        /// <returns>Populated Alert object</returns>
        public static Alert GenerateAlert(int alertId)
        {
            double sensorX = Properties.Settings.Default.startLongitude;
            double sensorY = Properties.Settings.Default.startLatitude;

            if (sensorY > 90)
            {
                sensorY += 100;
            }
            else
            {
                sensorY = 0.001;
            }

            Alert alert = new Alert
            {
                alertID = alertId,
                alertType = "Information",
                sourceID = ASMId,
                status = "Active",
                description = "Text description of alert",
                timestamp = DateTime.UtcNow,
                location = new location { X = sensorX, Y = sensorY },
                regionID = 1,
                debugText = "debug"
            };

            alert.priority = "High";
            alert.ranking = 0.9;
            alert.rankingSpecified = true;

            alert.associatedDetection = new[]
            {
                new AlertAssociatedDetection
                {
                    sourceID = 0,
                    objectID = 123,
                    timestampSpecified = true,
                    timestamp = alert.timestamp,
                    location = new location { X = sensorX + 0.001, Y = sensorY },
                    description = "location description"
                }
            };

            if (!string.IsNullOrEmpty(ImageURL))
            {
                alert.associatedFile = new[]
                {
                    new AlertAssociatedFile
                    {
                        type = "image",
                        url = ImageURL,
                    },
                };
            }

            return alert;
        }
    }
}

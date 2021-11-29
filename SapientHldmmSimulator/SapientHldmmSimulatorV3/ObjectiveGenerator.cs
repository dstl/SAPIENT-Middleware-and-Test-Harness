// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: ObjectiveGenerator.cs$
// Crown-Owned Copyright (c) supplied by QinetiQ. See Release/Supply Conditions

namespace SapientHldmmSimulator
{
    using System;
    using System.Threading;
    using log4net;
    using SapientServices.Communication;
    using SapientServices.Data;

    /// <summary>
    /// Generate Objective Messages
    /// </summary>
    public class ObjectiveGenerator : XmlGenerators
    {
        #region Properties

        public bool ChangeObjectiveID { get; set; }

        #endregion

        /// <summary>
        /// log4net logger
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Data Members

        private double maxLoopLatitude = Properties.Settings.Default.maxLatitude;

        private double longitude = start_longitude;
        private double latitude = start_latitude;

        /// <summary>
        /// message identifier
        /// </summary>
        private int messageId = 1;

        #endregion

        /// <summary>
        /// send out objective messages, will loop until latitude is greater than 52.69
        /// there is a wait of 100ms between each send
        /// </summary>
        /// <param name="comms_connection">IConnection messenger object used to send messages over</param>
        public void GenerateObjectives(object comms_connection, int sensorID)
        {
            var messenger = (IConnection)comms_connection;

            var objective = new Objective();

            objective.sourceID = 0;
            objective.sensorID = sensorID;
            objective.location = new location { X = longitude, Y = latitude };

            do
            {
                objective.objectiveID = this.messageId;
                objective.objectID = this.messageId;
                objective.objectIDSpecified = true;
                objective.regionID = 1;
                objective.status = "Proposed";
                objective.regionIDSpecified = true;
                objective.timestamp = DateTime.UtcNow;
                objective.description = string.Format("Objective for sensor {0} go to location {1} {2}", objective.sensorID, longitude, latitude);
                objective.information = string.Format("Objective {0} Information", this.messageId);
                {
                    objective.location = new location { X = longitude, Y = latitude };

                    if (isUTM)
                    {
                        longitude += 1.0;
                        latitude += 1.0;
                    }
                    else
                    {
                        longitude += 0.00001;
                        latitude += 0.00001;
                    }
                }

                if (latitude > maxLoopLatitude)
                {
                    latitude = start_latitude;
                    longitude = start_longitude;
                    messageId++;
                }
                else if (ChangeObjectiveID)
                {
                    messageId++;
                }

                var xmlstring = ConfigXMLParser.Serialize(objective);

                bool retval = MessageSender.Send(messenger, xmlstring);

                if (retval)
                {
                    MessageCount++;
                }
                else
                {
                    Log.ErrorFormat("Send Objective Failed {0}", MessageCount);
                }

                if (LoopMessages)
                {
                    Thread.Sleep(LoopTime);
                }

            } while (LoopMessages);
        }
    }
}

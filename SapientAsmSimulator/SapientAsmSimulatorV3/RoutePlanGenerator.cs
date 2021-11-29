// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: RoutePlanGenerator.cs$
// <copyright file="RoutePlanGenerator.cs" company="QinetiQ">
// Crown-Owned Copyright (c) supplied by QinetiQ. See Release/Supply Conditions
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;
using log4net;
using SapientServices;
using SapientServices.Communication;
using SapientServices.Data;

namespace SapientASMsimulator
{
    /// <summary>
    /// Generate Route Plan Messages
    /// </summary>
    public class RoutePlanGenerator : XmlGenerators
    {
        #region Properties

        public bool ChangeRoutePlanID { get; set; }

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
        /// send out route plan messages, will loop until latitude is greater than 52.69
        /// there is a wait of 100ms between each send
        /// </summary>
        /// <param name="comms_connection">IConnection messenger object used to send messages over</param>
        public void GenerateRoutePlans(object comms_connection, SapientLogger logger)
        {
            var messenger = (IConnection)comms_connection;

            var routePlan = new RoutePlan();

            routePlan.sensorID = ASMId;
            routePlan.taskID = 1;
            routePlan.taskIDSpecified = true;

            do
            {
                routePlan.objectiveID = this.messageId;
                routePlan.objectiveIDSpecified = true;
                routePlan.timestamp = DateTime.UtcNow;
                routePlan.eta = routePlan.timestamp.AddSeconds(60);
                routePlan.etaSpecified = true;
                routePlan.routeName = string.Format("Route for ASM {0}", routePlan.sensorID);
                routePlan.routeDescription = string.Format("Route Plan for sensor {0} going to location {1} {2}", routePlan.sensorID, longitude, latitude);

                locationList routePlan1 = new locationList();
                List<location> locList = new List<location>();

                double xDelta = 1;
                double yDelta = 1;

                if (isUTM)
                {
                    xDelta = 1.0;
                    yDelta = 1.0;
                }
                else
                {
                    xDelta = 0.00001;
                    yDelta = 0.00001;
                }

                double waypointSeparation = xDelta * 10;


                for (int i = 0; i < 4; i++)
                {
                    location loc = new location
                    {
                        X = longitude + xDelta * i * waypointSeparation,
                        Y = latitude + yDelta * i * waypointSeparation
                    };

                    locList.Add(loc);
                }

                routePlan1.location = locList.ToArray();
                routePlan.locationList = routePlan1;

                if (latitude < maxLoopLatitude)
                {
                    latitude += yDelta;
                    longitude += xDelta;
                    messageId++;
                }
                else
                {
                    latitude = start_latitude;
                    longitude = start_longitude;

                    if (ChangeRoutePlanID)
                    {
                        messageId++;
                    }
                }

                var xmlstring = ConfigXMLParser.Serialize(routePlan);

                bool retval = MessageSender.Send(messenger, xmlstring, logger);

                if (retval)
                {
                    MessageCount++;
                }
                else
                {
                    Log.ErrorFormat("Send Route Plan Failed {0}", MessageCount);
                }

                if (LoopMessages)
                {
                    Thread.Sleep(LoopTime);
                }

            } while (LoopMessages);
        }
    }
}

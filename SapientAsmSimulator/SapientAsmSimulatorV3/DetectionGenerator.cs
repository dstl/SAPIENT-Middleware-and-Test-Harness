// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: DetectionGenerator.cs$
// <copyright file="DetectionGenerator.cs" company="QinetiQ">
// Crown-Owned Copyright (c) supplied by QinetiQ. See Release/Supply Conditions
// </copyright>

namespace SapientASMsimulator
{
    using System;
    using System.Threading;
    using log4net;
    using SapientServices;
    using SapientServices.Communication;
    using SapientServices.Data;

    /// <summary>
    /// Generate Detection messages
    /// </summary>
    public class DetectionGenerator : XmlGenerators
    {
        /// <summary>
        /// Log4net logger
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Default range value in meters
        /// </summary>
        private static double startRange = 5;

        /// <summary>
        /// Default azimuth value in degrees
        /// </summary>
        private static double startAzimuth = 50;

        /// <summary>
        /// Maximum range to use for looped messages
        /// </summary>
        private static double maxRange = 100;

        /// <summary>
        /// Maximum latitude or Y coordinate to use for looped messages
        /// </summary>
        private double maxLoopLatitude = Properties.Settings.Default.maxLatitude;

        /// <summary>
        /// Longitude or X coordinate to use in detection location
        /// </summary>
        private double longitude = start_longitude;

        /// <summary>
        /// Latitude or Y coordinate to use in detection location
        /// </summary>
        private double latitude = start_latitude;

        /// <summary>
        /// Range to use if providing spherical location
        /// </summary>
        private double range = startRange;

        /// <summary>
        /// Azimuth to use if providing spherical location
        /// </summary>
        private double azimuth = startAzimuth;

        /// <summary>
        /// Object ID to use in the detection message
        /// </summary>
        private int detectionObjectId = 1;

        /// <summary>
        /// Gets or sets a value indicating whether to change object Id after each message
        /// </summary>
        public bool ChangeObjectID { get; set; }

        /// <summary>
        /// Gets or sets an Image URL to provide with the detection message
        /// </summary>
        public string ImageURL { get; set; }

        /// <summary>
        /// Gets or sets the class type to provide in the detection message
        /// </summary>
        public string TypeString { get; set; }

        /// <summary>
        /// Gets or sets the affiliation type to provide in the detection message
        /// </summary>
        public string Affiliation { get; set; }

        /// <summary>
        /// Generate example detection report
        /// </summary>
        /// <returns>Populated Detection Report object</returns>
        public DetectionReport GenerateDetection()
        {
            // Create detection message
            DetectionReport detection = new DetectionReport
            {
                timestamp = DateTime.UtcNow,
                sourceID = ASMId,
                reportID = this.MessageCount,
                objectID = this.detectionObjectId,
            };

            if (!string.IsNullOrEmpty(ImageURL))
            {
                detection.associatedFile = new[]
                {
                    new DetectionReportAssociatedFile
                    {
                        type = "image",
                        url = ImageURL,
                    },
                };
            }

            if (this.TypeString != string.Empty)
            {
                DetectionReportClass reportClass = new DetectionReportClass
                {
                    type = this.TypeString,
                    confidence = 0.7f,
                    confidenceSpecified = true                    
                };

                detection.@class = new DetectionReportClass[] { reportClass };
            }

            if (Properties.Settings.Default.detectionCoordinateSystem == "RangeBearing")
            {
                detection.rangeBearing = new rangeBearing { Az = this.azimuth, R = this.range };
                this.range += 5;
            }
            else
            {
                detection.location = new location { X = this.longitude, Y = this.latitude };

                if (isUTM)
                {
                    this.longitude += 1.0;
                    this.latitude += 1.0;
                }
                else
                {
                    this.longitude += 0.00001;
                    this.latitude += 0.00001;
                }
            }

            if (this.latitude > this.maxLoopLatitude)
            {
                this.latitude = start_latitude;
                this.longitude = start_longitude;
                this.detectionObjectId++;
            }
            else if (this.range > maxRange)
            {
                this.range = startRange;
                this.detectionObjectId++;
            }
            else if (this.ChangeObjectID)
            {
                this.detectionObjectId++;
            }

            // Zodiac additional fields
            detection.affiliation = new DetectionReportAffiliation()
            {
                type = "Unknown"
            };

            if(Affiliation!=string.Empty)
            {
                detection.affiliation.type = Affiliation;
            }
            
            detection.detectionSensorID = ASMId;
            detection.detectionSensorIDSpecified = true;

            this.MessageCount++;

            return detection;
        }

        /// <summary>
        /// send out detection messages, will loop until latitude is greater than maxLoopLatitude
        /// there is a wait of LoopTime between each send
        /// </summary>
        /// <param name="comms_connection">IConnection messenger object used to send messages over</param>
        /// <param name="logger">file data logger</param>
        public void GenerateDetections(object comms_connection, SapientLogger logger)
        {
            var messenger = (IConnection)comms_connection;
            do
            {
                DetectionReport detection = this.GenerateDetection();

                var xmlstring = ConfigXMLParser.Serialize(detection);

                bool retval = MessageSender.Send(messenger, xmlstring, logger);

                if (!retval)
                {
                    Log.ErrorFormat("Send Detection Failed: {0}", this.MessageCount);
                }

                if (this.LoopMessages)
                {
                    Thread.Sleep(this.LoopTime);
                }
            } while (this.LoopMessages);
        }
    }
}

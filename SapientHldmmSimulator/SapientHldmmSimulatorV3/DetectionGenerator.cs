// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: DetectionGenerator.cs$
// Crown-Owned Copyright (c) supplied by QinetiQ. See Release/Supply Conditions

using System;
using System.Threading;
using log4net;
using SapientServices.Communication;
using SapientServices.Data;

namespace SapientHldmmSimulator
{
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

        #region public methods

        /// <summary>
        /// send out detection messages, will loop until latitude is greater than 52.69
        /// there is a wait of 100ms between each send
        /// </summary>
        /// <param name="comms_connection">IConnection messenger object used to send messages over</param>
        /// <param name="form">main windows form</param> 
        public void GenerateHLDetections(object comms_connection, TaskForm form)
        {
            var messenger = (IConnection)comms_connection;

            var sub = new subClass
            {
                level = 1,
                levelSpecified = true,
                type = "Vehicle Class",
                value = "4 Wheeled",
                confidence = 0.7f,
                confidenceSpecified = true,
                subClass1 = new[]
                      {
                    new subClass
                    {
                        level = 2, levelSpecified = true, type = "Size", value = "Heavy", confidence = 0.6f, confidenceSpecified = true,
                        subClass1 = new[]
                        {
                            new subClass {level = 3, levelSpecified = true, type = "Vehicle Type", value = "Truck"},
                            new subClass {level = 3, levelSpecified = true, type = "Vehicle Type", value = "Bus"}
                        }
                    }
                }
            };

            var detection = new DetectionReport
            {
                sourceID = 0,
                taskID = 0,
                ////state = "lostInView",
                detectionConfidence = 1.0f,
                detectionConfidenceSpecified = true,
                trackInfo = new[]
                                      {
                                    new DetectionReportTrackInfo {type = "confidence", value = 0.9f, e = 0.01f, eSpecified = true},
                                    new DetectionReportTrackInfo {type = "speed", value = 2.0f},
                                },
                objectInfo = new[]
                                      {
                                    new DetectionReportObjectInfo {type = "height", value = 1.8f, e = 0.1f, eSpecified = true},
                                    new DetectionReportObjectInfo {type = "majorLength", value = 1.2f},
                                },
                colour = "red",
                @class = new[] { new DetectionReportClass { type = "Human", confidence = 0.9f, confidenceSpecified = true, subClass = new[] { sub } } },
                behaviour = new[] { new DetectionReportBehaviour { type = "Walking", confidence = 0.5f, confidenceSpecified = true },
                                                    new DetectionReportBehaviour { type = "Running"} },
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

            detection.location = new location { X = longitude, Y = latitude };

            do
            {
                detection.reportID = MessageCount;
                detection.objectID = this.detectionObjectId;
                detection.timestamp = DateTime.UtcNow;

                {
                    detection.location = new location { X = longitude, Y = latitude };

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
                    detectionObjectId++;
                }
                else if (range > maxRange)
                {
                    range = startRange;
                    detectionObjectId++;
                }
                else if (ChangeObjectID)
                {
                    detectionObjectId++;
                }

                // Zodiac additional fields
                detection.affiliation = new DetectionReportAffiliation()
                {
                    type = "Unknown"
                };

                detection.detectionSensorID = 1;
                detection.detectionSensorIDSpecified = true;

                var xmlstring = ConfigXMLParser.Serialize(detection);

                bool retval = MessageSender.Send(messenger, xmlstring);

                if (retval)
                {
                    MessageCount++;
                    // Added to pass SAPIENT_Test_Harness_Build_Note-O
                    form.UpdateOutputWindow("Detection Sent");
                }
                else
                {
                    Log.ErrorFormat("Send Detection Failed {0}", MessageCount);
                    // Added to pass SAPIENT_Test_Harness_Build_Note-O
                    form.UpdateOutputWindow("Send Detection Failed");
                }

                if (LoopMessages)
                {
                    Thread.Sleep(LoopTime);
                }

            } while (LoopMessages);
        }

        /// <summary>
        /// send out detection messages, will loop until latitude is greater than 52.69
        /// there is a wait of 100ms between each send
        /// </summary>
        /// <param name="comms_connection">IConnection messenger object used to send messages over</param>
        /// <param name="form">main windows form</param>
        public void GenerateDetections(object comms_connection) //, ClientForm form)
        {
            var messenger = (IConnection)comms_connection;
            var sub = new subClass
            {
                level = 1,
                levelSpecified = true,
                type = "Vehicle Class",
                value = "4 Wheeled",
                confidence = 0.7f,
                confidenceSpecified = true,
                subClass1 = new[]
                      {
                    new subClass
                    {
                        level = 2, levelSpecified = true, type = "Size", value = "Heavy", confidence = 0.6f, confidenceSpecified = true,
                        subClass1 = new[]
                        {
                            new subClass {level = 3, levelSpecified = true, type = "Vehicle Type", value = "Truck"},
                            new subClass {level = 3, levelSpecified = true, type = "Vehicle Type", value = "Bus"}
                        }
                    }
                }
            };

            var detection = new DetectionReport
            {
                timestamp = DateTime.UtcNow,
                sourceID = ASMId,
                reportID = MessageCount,
                objectID = detectionObjectId,
                taskID = 0,
                state = "lostInView",
                detectionConfidence = 1.0f,
                detectionConfidenceSpecified = true,
                trackInfo = new[]
                                      {
                                    new DetectionReportTrackInfo {type = "confidence", value = 0.9f, e = 0.01f, eSpecified = true},
                                    new DetectionReportTrackInfo {type = "speed", value = 2.0f},
                                },
                objectInfo = new[]
                                      {
                                    new DetectionReportObjectInfo {type = "height", value = 1.8f, e = 0.1f, eSpecified = true},
                                    new DetectionReportObjectInfo {type = "majorLength", value = 1.2f},
                                },
                colour = "red",
                @class = new[]
                                   {
                                    new DetectionReportClass { type = "Human", confidence = 0.9f, confidenceSpecified = true },
                                    new DetectionReportClass { type = "Vehicle", confidence = 0.1f, confidenceSpecified = true, subClass = new[] { sub } }
                             },
                behaviour = new[]
                                   {
                                    new DetectionReportBehaviour { type = "Walking", confidence = 0.6f, confidenceSpecified = true },
                                    new DetectionReportBehaviour { type = "Running", confidence = 0.4f, confidenceSpecified = true }
                             },
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

            do
            {
                detection.reportID = MessageCount;
                detection.objectID = this.detectionObjectId;
                detection.timestamp = DateTime.UtcNow;

                {
                    detection.location = new location { X = longitude, Y = latitude };

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
                    detectionObjectId++;
                }
                else if (range > maxRange)
                {
                    range = startRange;
                    detectionObjectId++;
                }
                else if (ChangeObjectID)
                {
                    detectionObjectId++;
                }

                // Zodiac additional fields
                detection.affiliation = new DetectionReportAffiliation()
                {
                    type = "Unknown"
                };

                detection.detectionSensorID = ASMId;
                detection.detectionSensorIDSpecified = true;

                var xmlstring = ConfigXMLParser.Serialize(detection);

                bool retval = MessageSender.Send(messenger, xmlstring);

                if (retval)
                {
                    MessageCount++;
                }
                else
                {
                    Log.ErrorFormat("Send Detection Failed {0}", MessageCount);
                }

                if (LoopMessages)
                {
                    Thread.Sleep(LoopTime);
                }

            } while (LoopMessages);
        }

        #endregion

    }
}

// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: RegistrationGenerator.cs$
// <copyright file="RegistrationGenerator.cs" company="QinetiQ">
// Crown-Owned Copyright (c) supplied by QinetiQ. See Release/Supply Conditions
// </copyright>

namespace SapientASMsimulator
{
    using System;
    using log4net;
    using SapientServices;
    using SapientServices.Communication;
    using SapientServices.Data;

    /// <summary>
    /// Generate Registration messages
    /// </summary>
    public class RegistrationGenerator
    {
        /// <summary>
        /// Log4net logger
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Gets or sets the fixed sensor identifier to use in all messages
        /// </summary>
        public static int AsmId { get; set; }

        /// <summary>
        /// Gets or sets the Time that the last registration message was sent 
        /// </summary>
        public static DateTime SendRegistrationTime { get; private set; }

        /// <summary>
        /// sends registration message out over the object passed
        /// </summary>
        /// <param name="messenger">object used to send messages over</param>
        /// <param name="form">main windows form</param>
        /// <param name="logger">file data logger</param>  
        public static void GenerateRegistration(IConnection messenger, IGUIInterface form, SapientLogger logger)
        {
            var reg_def = new SensorRegistrationModeDefinitionTaskDefinitionRegionDefinition
            {
                regionType = new[]
                {
                    new SensorRegistrationModeDefinitionTaskDefinitionRegionDefinitionRegionType { Value = "Area of Interest" },
                    new SensorRegistrationModeDefinitionTaskDefinitionRegionDefinitionRegionType { Value = "Ignore" }
                },
                locationType = new locationType
                {
                    units = "decimal degrees-metres",
                    datum = "WGS84",
                    zone = UTMZone(),
                    north = "Grid",
                    Value = "UTM"
                },

                // TBD behaviour filter definition
                classFilterDefinition = new[]
                {
                    new SensorRegistrationModeDefinitionTaskDefinitionRegionDefinitionClassFilterDefinition
                    {
                        type = "All",
                        filterParameter = new[]
                        {
                            new filterParameter
                            {
                                name = "confidence",
                                operators = "All"
                            }
                        }
                    },
                    new SensorRegistrationModeDefinitionTaskDefinitionRegionDefinitionClassFilterDefinition
                    {
                        type = "Human",
                        filterParameter = new[]
                        {
                            new filterParameter { name = "confidence", operators = "All" }
                        },
                        subClassFilterDefinition = new[]
                        {
                            new subClassFilterDefinition
                            {
                                level = 1,
                                type = "PersonID",
                                filterParameter = new[]
                                {
                                    new filterParameter { name = "confidence", operators = "All" }
                                }
                            }
                        }
                    }
                },
            };

            var task_def2 = new SensorRegistrationModeDefinitionTaskDefinition
            {
                concurrentTasks = 1,
                concurrentTasksSpecified = true,
                regionDefinition = reg_def,
                command = new[]
                {
                    new SensorRegistrationModeDefinitionTaskDefinitionCommand
                    {
                        name = "LookAt", units = "rangeBearingCone", completionTime = 10, completionTimeUnits = "seconds"
                    }
                }
            };

            var det_def = new SensorRegistrationModeDefinitionDetectionDefinition
            {
                detectionClassDefinition = new SensorRegistrationModeDefinitionDetectionDefinitionDetectionClassDefinition
                {
                    classDefinition = new SensorRegistrationModeDefinitionDetectionDefinitionDetectionClassDefinitionClassDefinition[1],
                    classPerformance = new SensorRegistrationModeDefinitionDetectionDefinitionDetectionClassDefinitionClassPerformance[1],
                    confidenceDefinition = "Single Class",
                }
            };

            det_def.detectionClassDefinition.classDefinition[0] =
                new SensorRegistrationModeDefinitionDetectionDefinitionDetectionClassDefinitionClassDefinition
                {
                    confidence = new confidence { units = "probability" },
                    type = "Human",
                    subClassDefinition = new subClassDefinition[1]
                };

            det_def.detectionClassDefinition.classDefinition[0].subClassDefinition = new[]
            {
                new subClassDefinition
                {
                    level = 1,
                    type = "Activity",
                    values = "Walking,Running",
                    subClassDefinition1 = new[]
                    {
                        new subClassDefinition { level = 2, type = "Threat", values = "Active, Passive" }
                    }
                }
            };

            new SensorRegistrationModeDefinitionDetectionDefinitionDetectionClassDefinitionClassDefinition
            {
                confidence = new confidence { units = "probability" },
                type = "Vehicle"
            };

            det_def.detectionClassDefinition.classPerformance[0] =
                new SensorRegistrationModeDefinitionDetectionDefinitionDetectionClassDefinitionClassPerformance
                {
                    type = "FAR",
                    units = "Per Period",
                    unitValue = "1",
                    variationType = "Linear with Range",
                    performanceValue = new[]
                    {
                        new performanceValue { type = "eRmin", value = "0.1" },
                        new performanceValue { type = "eRmax", value = "0.5" }
                    }
                };

            det_def.detectionReport = new SensorRegistrationModeDefinitionDetectionDefinitionDetectionReport[3];

            for (int i = 0; i < det_def.detectionReport.Length; i++)
            {
                det_def.detectionReport[i] = new SensorRegistrationModeDefinitionDetectionDefinitionDetectionReport();
            }

            det_def.detectionReport[0].category = "detection";
            det_def.detectionReport[0].type = "confidence";
            det_def.detectionReport[0].units = "probability";
            det_def.detectionReport[1].category = "track";
            det_def.detectionReport[1].type = "confidence";
            det_def.detectionReport[1].units = "probability";
            det_def.detectionReport[2].category = "track";
            det_def.detectionReport[2].type = "confidence";
            det_def.detectionReport[2].units = "probability";
            det_def.detectionReport[2].onChange = true;
            det_def.detectionReport[2].onChangeSpecified = true;

            det_def.locationType = new locationType
            {
                datum = "WGS84",
                units = "metres",
                Value = "UTM",
                zone = UTMZone()
            };

            var sensor_registration = new SensorRegistration
            {
                heartbeatDefinition =
                    new SensorRegistrationHeartbeatDefinition
                    {
                        sensorLocationDefinition =
                            new SensorRegistrationHeartbeatDefinitionSensorLocationDefinition
                            {
                                locationType = det_def.locationType
                            },
                        heartbeatInterval =
                            new SensorRegistrationHeartbeatDefinitionHeartbeatInterval
                            {
                                units = "seconds",
                                value = 5
                            },
                        heartbeatReport = new[]
                        {
                            new SensorRegistrationHeartbeatDefinitionHeartbeatReport
                            {
                                category = "sensor", type = "sensorLocation", units = string.Empty, onChange = true, onChangeSpecified = true
                            }
                        }
                    },
                modeDefinition = new[]
                {
                    new SensorRegistrationModeDefinition
                    {
                        detectionDefinition = det_def,
                        duration = new SensorRegistrationModeDefinitionDuration { units = "units", value = 1, },
                        modeDescription = "mode",
                        modeName = "Default",
                        scanType = ScanType(),
                        settleTime = new settleTime { units = "seconds", value = 10 },
                        maximumLatency = new SensorRegistrationModeDefinitionMaximumLatency { units = "seconds", value = 5 },
                        taskDefinition = task_def2,
                        trackingType = "Track",
                        type = "Permanent"
                    }
                },
                sensorType = GetSensorType(),
                platformType = GetPlatformType(),
                timestamp = DateTime.UtcNow,
            };

            if (AsmId > 0)
            {
                sensor_registration.sensorID = AsmId;
                sensor_registration.sensorIDSpecified = true;
            }

            string xmlstring = ConfigXMLParser.Serialize(sensor_registration);
            bool retval = MessageSender.Send(messenger, xmlstring, logger);

            if (retval)
            {
                SendRegistrationTime = DateTime.UtcNow;
                form.UpdateOutputWindow("SensorRegistration Sent");
                Log.Info("Send Registration Succeeded");
            }
            else
            {
                form.UpdateOutputWindow("SensorRegistration Send failed");
                Log.Error("Send Registration Failed");
            }
        }

        /// <summary>
        /// Gets the sensor type to use in registration messages from the application configuration.
        /// </summary>
        /// <returns>The sensor type.</returns>
        private static string GetSensorType()
        {
            return Properties.Settings.Default.SensorType;
        }

        /// <summary>
        /// Gets the platform type to use in registration messages from the application configuration.
        /// </summary>
        /// <returns>The sensor type.</returns>
        private static string GetPlatformType()
        {
            return Properties.Settings.Default.PlatformType;
        }

        private static string UTMZone()
        {
            return Properties.Settings.Default.UTMzone;
        }

        private static string ScanType()
        {
            return Properties.Settings.Default.ScanType;
        }
    }
}

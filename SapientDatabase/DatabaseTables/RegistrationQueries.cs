// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: RegistrationQueries.cs$
// Crown-Owned Copyright (c) supplied by QinetiQ. See Release/Supply Conditions
// $NoKeywords$
namespace SapientDatabase.DatabaseTables
{
    using System;
    using System.Reflection;
    using log4net;
    using Npgsql;
    using SapientServices.Data;

    /// <summary>
    /// Database methods relating to Registration messages
    /// </summary>
    public class RegistrationQueries
    {
        /// <summary>
        /// Log4net logger.
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Test writing to Registration database table
        /// </summary>
        /// <param name="host">database server address</param>
        /// <param name="port">database port</param>
        /// <param name="user">database user name</param>
        /// <param name="password">database password</param>
        /// <param name="name">database name</param>
        public static void Test(string host, string port, string user, string password, string name)
        {
            object thisLock = new object();
            string db_server;
            string db_port;
            string db_user;
            string db_password;
            string db_name;
            NpgsqlConnection connection;
            db_server = host;
            db_port = port;
            db_user = user;
            db_password = password;
            db_name = name;
            connection = new NpgsqlConnection("Server=" + db_server + ";Port=" + db_port + ";User Id=" + db_user + ";Password=" + db_password + ";Database=" + db_name + ";");
            connection.Open();

            SensorRegistration registration = GenerateTestRegistrationMessage();
            string xmlString = ConfigXMLParser.Serialize(registration);
            string sqlString = GenerateInsertQueryString(registration, xmlString);
            Database.SendToDatabase(sqlString, connection, thisLock);
        }

        /// <summary>
        ///  Method to write registration message to database
        /// </summary>
        /// <param name="registration">registration object</param>
        /// <param name="xml">string of entire registration message xml</param>
        /// <returns>SQL string</returns>
        public static string GenerateInsertQueryString(SensorRegistration registration, string xml)
        {
            DateTime updateTime = DateTime.UtcNow;
            string messageTimeStamp = registration.timestamp.ToString(DatabaseUtil.DateTimeFormat);
            string updateTimeStamp = updateTime.ToString(DatabaseUtil.DateTimeFormat);
            string platformType = registration.platformType;

            if (platformType == string.Empty)
            {
                platformType = "Static";
            }

            var message = xml.Replace("\r\n", string.Empty);
            string sqlString = string.Format($@"INSERT INTO sensor_registration_v3 VALUES(
                  '{messageTimeStamp}',
                  '{updateTimeStamp}',
                  {registration.sensorID},
                  '{registration.sensorType}',
                  '{message}',
                  '{platformType}');");

            return sqlString;
        }

        private static SensorRegistration GenerateTestRegistrationMessage()
        {
            var reg_def = new SensorRegistrationModeDefinitionTaskDefinitionRegionDefinition
            {
                regionType = new[]
                {
                    new SensorRegistrationModeDefinitionTaskDefinitionRegionDefinitionRegionType { Value = "Area of Interest" },
                    new SensorRegistrationModeDefinitionTaskDefinitionRegionDefinitionRegionType { Value = "Ignore" },
                },
                locationType = new locationType
                {
                    units = "decimal degrees-metres",
                    datum = "WGS84",
                    zone = "30U",
                    north = "Grid",
                    Value = "UTM",
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
                                operators = "All",
                            },
                        },
                    },
                    new SensorRegistrationModeDefinitionTaskDefinitionRegionDefinitionClassFilterDefinition
                    {
                        type = "Human",
                        filterParameter = new[]
                        {
                            new filterParameter { name = "confidence", operators = "All" },
                        },
                        subClassFilterDefinition = new[]
                        {
                            new subClassFilterDefinition
                            {
                                level = 1,
                                type = "PersonID",
                                filterParameter = new[]
                                {
                                    new filterParameter { name = "confidence", operators = "All" },
                                },
                            },
                        },
                    },
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
                        name = "LookAt", units = "rangeBearingCone", completionTime = 10, completionTimeUnits = "seconds",
                    },
                },
            };

            var det_def = new SensorRegistrationModeDefinitionDetectionDefinition
            {
                detectionClassDefinition = new SensorRegistrationModeDefinitionDetectionDefinitionDetectionClassDefinition
                {
                    classDefinition = new SensorRegistrationModeDefinitionDetectionDefinitionDetectionClassDefinitionClassDefinition[1],
                    classPerformance = new SensorRegistrationModeDefinitionDetectionDefinitionDetectionClassDefinitionClassPerformance[1],
                    confidenceDefinition = "Single Class",
                },
            };

            det_def.detectionClassDefinition.classDefinition[0] =
                new SensorRegistrationModeDefinitionDetectionDefinitionDetectionClassDefinitionClassDefinition
                {
                    confidence = new confidence { units = "probability" },
                    type = "Human",
                    subClassDefinition = new subClassDefinition[1],
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
                        new subClassDefinition { level = 2, type = "Threat", values = "Active, Passive" },
                    },
                },
            };

            new SensorRegistrationModeDefinitionDetectionDefinitionDetectionClassDefinitionClassDefinition
            {
                confidence = new confidence { units = "probability" },
                type = "Vehicle",
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
                        new performanceValue { type = "eRmax", value = "0.5" },
                    },
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
                zone = "30U",
            };

            var sensor_registration = new SensorRegistration
            {
                heartbeatDefinition =
                    new SensorRegistrationHeartbeatDefinition
                    {
                        sensorLocationDefinition =
                            new SensorRegistrationHeartbeatDefinitionSensorLocationDefinition
                            {
                                locationType = det_def.locationType,
                            },
                        heartbeatInterval =
                            new SensorRegistrationHeartbeatDefinitionHeartbeatInterval
                            {
                                units = "seconds",
                                value = 5,
                            },
                        heartbeatReport = new[]
                        {
                            new SensorRegistrationHeartbeatDefinitionHeartbeatReport
                            {
                                category = "sensor", type = "sensorLocation", units = string.Empty, onChange = true, onChangeSpecified = true,
                            },
                        },
                    },
                modeDefinition = new[]
                {
                    new SensorRegistrationModeDefinition
                    {
                        detectionDefinition = det_def,
                        duration = new SensorRegistrationModeDefinitionDuration { units = "units", value = 1, },
                        modeDescription = "mode",
                        modeName = "Default",
                        scanType = "Fixed",
                        settleTime = new settleTime { units = "seconds", value = 10 },
                        maximumLatency = new SensorRegistrationModeDefinitionMaximumLatency { units = "seconds", value = 5 },
                        taskDefinition = task_def2,
                        trackingType = "Track",
                        type = "Permanent",
                    },
                },
                sensorType = "Test ASM",
                platformType = "Static",
                timestamp = DateTime.UtcNow,
            };

            return sensor_registration;
        }
    }
}

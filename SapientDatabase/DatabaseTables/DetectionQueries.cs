// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: DetectionQueries.cs$
// Crown-Owned Copyright (c) supplied by QinetiQ. See Release/Supply Conditions
// $NoKeywords$
namespace SapientDatabase.DatabaseTables
{
    using System;
    using System.Reflection;
    using System.Text;
    using log4net;
    using Npgsql;
    using SapientServices.Data;

    /// <summary>
    /// Database methods relating to DetectionReports
    /// </summary>
    public static class DetectionQueries
    {
        /// <summary>
        /// Log4net logger.
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Test writing to detection to database table
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

            DetectionReport det = new DetectionReport()
            {
                sourceID = 1,
                reportID = 1,
                objectID = 1,
                timestamp = DateTime.UtcNow,
                state = "Test",
            };

            location loc = new location()
            {
                X = -2.325,
                Y = 52.101,
            };

            det.location = loc;

            det.associatedFile = new[]
            {
                new DetectionReportAssociatedFile
                {
                    type = "image",
                    url = "testfilename.jpg",
                },
            };

            string sqlString = GenerateInsertQueryString(det, false);
            Database.SendToDatabase(sqlString, connection, thisLock);
            det.sourceID = 0;
            sqlString = GenerateInsertQueryString(det, true);
            Database.SendToDatabase(sqlString, connection, thisLock);
        }

        /// <summary>
        /// Generate SQL string for inserting all data from this detection report into the database
        /// </summary>
        /// <param name="detectionReport">DetectionReport object</param>
        /// <param name="hldmm">whether HLDMM or not</param>
        /// <returns>SQL string</returns>
        public static string GenerateInsertQueryString(DetectionReport detectionReport, bool hldmm)
        {
            DateTime updateTime = DateTime.UtcNow;
            StringBuilder sb = new StringBuilder();
            string tablePrefix = "detection_report";

            if (hldmm)
            {
                tablePrefix = "hl_detection_report";
            }

            string commonFieldString = GenerateCommonFieldString(detectionReport, updateTime);

            Velocity vel = GenerateVelocity(detectionReport);

            GenerateLocationInsertString(detectionReport, tablePrefix, commonFieldString, ref sb, vel);
            GenerateObjectInfoInsertString(detectionReport, tablePrefix, commonFieldString, ref sb);
            GenerateClassInsertString(detectionReport, tablePrefix, commonFieldString, ref sb);
            GenerateBehaviourInsertString(detectionReport, tablePrefix, commonFieldString, ref sb);
            GenerateAssociatedFileInsertString(detectionReport, tablePrefix, commonFieldString, ref sb);

            return sb.ToString();
        }

        /// <summary>
        /// Generate Velocity based on difference between next predicted location and current detected location
        /// </summary>
        /// <param name="detectionReport">detection report object</param>
        /// <returns>velocity object</returns>
        private static Velocity GenerateVelocity(DetectionReport detectionReport)
        {
            Velocity vel = new Velocity();

            if (detectionReport.location != null && detectionReport.predictedLocation != null && detectionReport.predictedLocation.location != null)
            {
                vel.X = detectionReport.predictedLocation.location.X - detectionReport.location.X;
                vel.Y = detectionReport.predictedLocation.location.Y - detectionReport.location.Y;
                vel.Heading = 123;
                vel.Speed = 10.2;
                vel.Valid = true;
            }

            return vel;
        }

        /// <summary>
        /// Generate string of fields common to all detection tables
        /// </summary>
        /// <param name="detectionReport">DetectionReport object</param>
        /// <param name="updateTime">database update time - typically time now</param>
        /// <returns>string containing database fields common to all detection tables</returns>
        private static string GenerateCommonFieldString(DetectionReport detectionReport, DateTime updateTime)
        {
            string detectionTimeStamp = detectionReport.timestamp.ToString(DatabaseUtil.DateTimeFormat);
            string updateTimeStamp = updateTime.ToString(DatabaseUtil.DateTimeFormat);

            string sqlString = string.Format($@"
                                        {detectionReport.sourceID},
                                        '{detectionTimeStamp}',
                                        '{updateTimeStamp}',
                                        {detectionReport.reportID},
                                        {detectionReport.objectID} ");
            return sqlString;
        }

        /// <summary>
        /// Generate SQL string for adding location and predicted location sections of DetectionReport to database
        /// </summary>
        /// <param name="detectionReport">DetectionReport object</param>
        /// <param name="tablePrefix">database table name prefix</param>
        /// <param name="commonFieldString">string of values common to all tables</param>
        /// <param name="sb">string builder to add SQL to</param>
        private static void GenerateLocationInsertString(DetectionReport detectionReport, string tablePrefix, string commonFieldString, ref StringBuilder sb, Velocity velocity)
        {
            bool predicted = false;
            string predictedTimestamp = "NULL";
            GenerateLocation(detectionReport, detectionReport.location, tablePrefix, commonFieldString, ref sb, predicted, predictedTimestamp, velocity);
            GenerateRangeBearing(detectionReport, detectionReport.rangeBearing, tablePrefix, commonFieldString, ref sb, predicted, predictedTimestamp);

            if (detectionReport.predictedLocation != null)
            {
                predicted = true;
                predictedTimestamp = DatabaseUtil.Nullable(detectionReport.predictedLocation.predictionTimestamp, detectionReport.predictedLocation.predictionTimestampSpecified);

                GenerateLocation(detectionReport, detectionReport.predictedLocation.location, tablePrefix, commonFieldString, ref sb, predicted, predictedTimestamp, velocity);
                GenerateRangeBearing(detectionReport, detectionReport.predictedLocation.rangeBearing, tablePrefix, commonFieldString, ref sb, predicted, predictedTimestamp);
            }
        }

        /// <summary>
        /// Generate SQL string for adding cartesian location sections of DetectionReport to database
        /// </summary>
        /// <param name="detectionReport">DetectionReport object</param>
        /// <param name="loc">cartesian location object</param>
        /// <param name="tablePrefix">database table name prefix</param>
        /// <param name="commonFieldString">string of values common to all tables</param>
        /// <param name="sb">string builder to add SQL to</param>
        /// <param name="predicted">whether actual or predicted location</param>
        /// <param name="predictedTimestamp">predicted location timestamp</param>
        private static void GenerateLocation(DetectionReport detectionReport, location loc, string tablePrefix, string commonFieldString, ref StringBuilder sb, bool predicted, string predictedTimestamp, Velocity velocity)
        {
            if (loc != null)
            {
                var t_id = detectionReport.taskID;
                var state = detectionReport.state;
                var detection_confidence = DatabaseUtil.Nullable(detectionReport.detectionConfidence, detectionReport.detectionConfidenceSpecified);
                var colour = detectionReport.colour;

                var x = loc.X.ToString("F7");
                var y = loc.Y.ToString("F7");
                var z = DatabaseUtil.Nullable(loc.Z, loc.ZSpecified);
                var dx = DatabaseUtil.Nullable(loc.eX, loc.eXSpecified);
                var dy = DatabaseUtil.Nullable(loc.eY, loc.eYSpecified);
                var dz = DatabaseUtil.Nullable(loc.eZ, loc.eZSpecified);

                // Zodiac fields 2020
                string affiliation = string.Empty;
                if (detectionReport.affiliation != null)
                {
                    affiliation = detectionReport.affiliation.type;
                }

                var detSensorId = DatabaseUtil.Nullable(detectionReport.detectionSensorID, detectionReport.detectionSensorIDSpecified);
                var vel_x = DatabaseUtil.Nullable(velocity.X, velocity.Valid);
                var vel_y = DatabaseUtil.Nullable(velocity.Y, velocity.Valid);
                var speed = DatabaseUtil.Nullable(velocity.Speed, velocity.Valid);
                var heading = DatabaseUtil.Nullable(velocity.Heading, velocity.Valid);

                string sql1 = string.Format($@"INSERT INTO {tablePrefix}_location_v3 VALUES(
                    {commonFieldString},
                    {t_id},
                    '{state}',
                    {detection_confidence},
                    '{colour}',
                    {predicted},
                    {predictedTimestamp},
                    {x}, 
                    {y},
                    {z}, 
                    {dx},
                    {dy},
                    {dz},
                    '{affiliation}',
                    {detSensorId},
                    {vel_x},
                    {vel_y},
                    {speed},
                    {heading}
                    );");
                sb.Append(sql1);
            }
        }

        /// <summary>
        /// Generate SQL string for adding spherical location sections of DetectionReport to database
        /// </summary>
        /// <param name="detectionReport">DetectionReport object</param>
        /// <param name="rb">rangeBearing, spherical location object</param>
        /// <param name="tablePrefix">database table name prefix</param>
        /// <param name="commonFieldString">string of values common to all tables</param>
        /// <param name="sb">string builder to add SQL to</param>
        /// <param name="predicted">whether actual or predicted location</param>
        /// <param name="predictedTimestamp">predicted location timestamp</param>
        private static void GenerateRangeBearing(DetectionReport detectionReport, rangeBearing rb, string tablePrefix, string commonFieldString, ref StringBuilder sb, bool predicted, string predictedTimestamp)
        {
            if (rb != null)
            {
                var t_id = detectionReport.taskID;
                var state = detectionReport.state;
                var detection_confidence = DatabaseUtil.Nullable(detectionReport.detectionConfidence, detectionReport.detectionConfidenceSpecified);
                var colour = detectionReport.colour;

                var az = rb.Az.ToString("F7");
                var r = rb.R.ToString("F7");
                var ele = DatabaseUtil.Nullable(rb.Ele, rb.EleSpecified);
                var z = DatabaseUtil.Nullable(rb.Z, rb.ZSpecified);
                var daz = DatabaseUtil.Nullable(rb.eAz, rb.eAzSpecified);
                var dr = DatabaseUtil.Nullable(rb.eR, rb.eRSpecified);
                var dele = DatabaseUtil.Nullable(rb.eEle, rb.eEleSpecified);
                var dz = DatabaseUtil.Nullable(rb.eZ, rb.eZSpecified);

                // Zodiac fields 2020
                var affiliation = string.Empty;
                if (detectionReport.affiliation != null)
                {
                    affiliation = detectionReport.affiliation.type;
                }

                var detSensorId = DatabaseUtil.Nullable(detectionReport.detectionSensorID, detectionReport.detectionSensorIDSpecified);
                string sql1 = string.Format($@"INSERT INTO {tablePrefix}_range_bearing_v3 VALUES(
                    {commonFieldString},
                    {t_id},
                    '{state}',
                    {detection_confidence},
                    '{colour}',
                    {predicted},
                    {predictedTimestamp},
                    {r}, 
                    {az},
                    {ele},
                    {z},
                    {dr},
                    {daz},
                    {dele},
                    {dz},
                    '{affiliation}',
                    {detSensorId}
                    );");
                sb.Append(sql1);
            }
        }

        /// <summary>
        /// Generate SQL string for adding ObjectInfo and TrackInfo sections of DetectionReport to database
        /// </summary>
        /// <param name="detectionReport">DetectionReport object</param>
        /// <param name="tablePrefix">database table name prefix</param>
        /// <param name="commonFieldString">string of values common to all tables</param>
        /// <param name="sb">string builder to add SQL to</param>
        private static void GenerateObjectInfoInsertString(DetectionReport detectionReport, string tablePrefix, string commonFieldString, ref StringBuilder sb)
        {
            if (detectionReport.trackInfo != null)
            {
                foreach (var track_info in detectionReport.trackInfo)
                {
                    var e = DatabaseUtil.Nullable(track_info.e, track_info.eSpecified);
                    string sql1 = string.Format($@"INSERT INTO {tablePrefix}_track_info_v3 VALUES(
                        {commonFieldString},
                        'trackInfo',
                        '{track_info.type}',
                        {track_info.value},
                        {e} );");
                    sb.Append(sql1);
                }
            }

            if (detectionReport.objectInfo != null)
            {
                foreach (var object_info in detectionReport.objectInfo)
                {
                    var value = object_info.value.ToString("F7");
                    var e = DatabaseUtil.Nullable(object_info.e, object_info.eSpecified);

                    string sql1 = string.Format($@"INSERT INTO {tablePrefix}_track_info_v3 VALUES(
                        {commonFieldString},
                        'objectInfo',
                        '{object_info.type}',
                        {value},
                        {e} );");
                    sb.Append(sql1);
                }
            }
        }

        /// <summary>
        /// Generate SQL string for adding class section of DetectionReport to database
        /// </summary>
        /// <param name="detectionReport">DetectionReport object</param>
        /// <param name="tablePrefix">database table name prefix</param>
        /// <param name="commonFieldString">string of values common to all tables</param>
        /// <param name="sb">string builder to add SQL to</param>
        private static void GenerateClassInsertString(DetectionReport detectionReport, string tablePrefix, string commonFieldString, ref StringBuilder sb)
        {
            if (detectionReport.@class != null)
            {
                foreach (var cl in detectionReport.@class)
                {
                    var confidence = DatabaseUtil.Nullable(cl.confidence, cl.confidenceSpecified);

                    string sql1 = string.Format($@"INSERT INTO {tablePrefix}_class_v3 VALUES(
                        {commonFieldString},
                        '{cl.type}',
                        {confidence} );");

                    sb.Append(sql1);

                    StringBuilder subclassSb = new StringBuilder();
                    if (cl.subClass != null)
                    {
                        int parentSubClassId = 0;
                        int nextSubClassId = 1;
                        foreach (var subclass in cl.subClass)
                        {
                            nextSubClassId = AddSubclass(commonFieldString, cl, subclass, parentSubClassId, nextSubClassId, tablePrefix, ref subclassSb);
                        }
                    }

                    sb.Append(subclassSb.ToString()); // append sub class sql to main sql
                }
            }
        }

        /// <summary>
        /// Generate SQL string for adding behaviour section of DetectionReport to database
        /// </summary>
        /// <param name="detectionReport">DetectionReport object</param>
        /// <param name="tablePrefix">database table name prefix</param>
        /// <param name="commonFieldString">string of values common to all tables</param>
        /// <param name="sb">string builder to add SQL to</param>
        private static void GenerateBehaviourInsertString(DetectionReport detectionReport, string tablePrefix, string commonFieldString, ref StringBuilder sb)
        {
            if (detectionReport.behaviour != null)
            {
                foreach (var behaviour in detectionReport.behaviour)
                {
                    var confidence = DatabaseUtil.Nullable(behaviour.confidence, behaviour.confidenceSpecified);

                    string sql1 = string.Format($@"INSERT INTO {tablePrefix}_behaviour_v3 VALUES(
                        {commonFieldString},
                        '{behaviour.type}',
                        {confidence} );");

                    sb.Append(sql1);
                }
            }
        }

        /// <summary>
        /// Generate SQL string for adding associated file section of DetectionReport to database
        /// </summary>
        /// <param name="detectionReport">DetectionReport object</param>
        /// <param name="tablePrefix">database table name prefix</param>
        /// <param name="commonFieldString">string of values common to all tables</param>
        /// <param name="sb">string builder to add SQL to</param>
        private static void GenerateAssociatedFileInsertString(DetectionReport detectionReport, string tablePrefix, string commonFieldString, ref StringBuilder sb)
        {
            if (detectionReport.associatedFile != null)
            {
                foreach (var aft in detectionReport.associatedFile)
                {
                    string sql1 = string.Format($@"INSERT INTO {tablePrefix}_assoc_file_v3 VALUES(
                        {commonFieldString},
                        '{aft.type}', 
                        '{aft.url}' );");

                    sb.Append(sql1);
                }
            }
        }

        /// <summary>
        /// Generate SQL string for adding sub class section of DetectionReport to database
        /// </summary>
        /// <param name="commonFieldString">string of values common to all tables</param>
        /// <param name="cl">parent class object</param>
        /// <param name="subclass">sub class to add</param>
        /// <param name="parentSubClassId">identifier of parent class/sub class</param>
        /// <param name="nextSubClassId">identifier of this sub class object</param>
        /// <param name="tablePrefix">database table name prefix</param>
        /// <param name="sb">string builder to add SQL to</param>
        /// <returns>next sub class identifier to use</returns>
        private static int AddSubclass(string commonFieldString, DetectionReportClass cl, subClass subclass, int parentSubClassId, int nextSubClassId, string tablePrefix, ref StringBuilder sb)
        {
            int thisSubClassId = nextSubClassId;
            nextSubClassId++;

            var level = DatabaseUtil.Nullable(subclass.level, subclass.levelSpecified);
            var confidence = DatabaseUtil.Nullable(subclass.confidence, subclass.confidenceSpecified);

            string sql1 = string.Format($@"INSERT INTO {tablePrefix}_subclass_v3 VALUES(
                {commonFieldString},
                '{cl.type}',
                {level},
                '{subclass.type}', 
                '{subclass.value}',
                {confidence}, 
                {thisSubClassId}, 
                {parentSubClassId} );");

            sb.Append(sql1);

            if (subclass.subClass1 != null)
            {
                foreach (var sub in subclass.subClass1)
                {
                    nextSubClassId = AddSubclass(commonFieldString, cl, sub, thisSubClassId, nextSubClassId, tablePrefix, ref sb);
                }
            }

            return nextSubClassId;
        }

        private class Velocity
        {
            public double X { get; set; }

            public double Y { get; set; }

            public double Speed { get; set; }

            public double Heading { get; set; }

            public bool Valid { get; set; }
        }
    }
}
// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: AlertQueries.cs$
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
    /// Database methods relating to Alerts
    /// </summary>
    public static class AlertQueries
    {
        /// <summary>
        /// Log4net logger.
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Test writing to alert database table
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

            double sensorX = -2.325;
            double sensorY = 52.101;

            Alert alert = new Alert()
            {
                sourceID = 1,
                alertID = 1,
                description = "Test Alert",
                timestamp = DateTime.UtcNow,
                alertType = "Test",
            };

            location loc = new location()
            {
                X = sensorX,
                Y = sensorY,
            };

            alert.location = loc;
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
                    description = "test location description",
                },
            };

            alert.associatedFile = new[]
            {
                new AlertAssociatedFile
                {
                    type = "image",
                    url = "testfilename.jpg",
                },
            };

            string sqlString = GenerateInsertQueryString(alert, false);
            Database.SendToDatabase(sqlString, connection, thisLock);
            alert.sourceID = 0;
            sqlString = GenerateInsertQueryString(alert, true);
            Database.SendToDatabase(sqlString, connection, thisLock);
        }

        /// <summary>
        /// method to Populate Alert Tables
        /// </summary>
        /// <param name="alert">alert object</param>
        /// <param name="hldmm">whether high level message</param>
        /// <returns>SQL string</returns>
        public static string GenerateInsertQueryString(Alert alert, bool hldmm)
        {
            DateTime updateTime = DateTime.UtcNow;
            StringBuilder sb = new StringBuilder();

            if (hldmm)
            {
                sb.Append(GenerateAlertInsertQueryString(alert, AlertConstants.HLAlert.Table, updateTime));
                sb.Append(GenerateAlertLocationInsertQueryString(alert, AlertConstants.HLAlertLocation.Table, updateTime));

                // No range bearing data from HLDMM
                GenerateAlertAssociatedFileInsertQueryString(alert, AlertConstants.HLAlertAssocFile.Table, updateTime, ref sb);
                GenerateAlertAssociatedDetectionInsertQueryString(alert, AlertConstants.HLAlertAssocDetection.Table, updateTime, ref sb);
                GenerateAlertAssociatedDetectionLocationInsertQueryString(alert, AlertConstants.HLAlertLocation.Table, updateTime, ref sb);
            }
            else
            {
                sb.Append(GenerateAlertInsertQueryString(alert, AlertConstants.Alert.Table, updateTime));
                sb.Append(GenerateAlertLocationInsertQueryString(alert, AlertConstants.AlertLocation.Table, updateTime));
                sb.Append(GenerateAlertRangeBearingInsertQueryString(alert, AlertConstants.AlertRangeBearing.Table, updateTime));
                GenerateAlertAssociatedFileInsertQueryString(alert, AlertConstants.AlertAssocFile.Table, updateTime, ref sb);
                GenerateAlertAssociatedDetectionInsertQueryString(alert, AlertConstants.AlertAssocDetection.Table, updateTime, ref sb);
                GenerateAlertAssociatedDetectionLocationInsertQueryString(alert, AlertConstants.AlertLocation.Table, updateTime, ref sb);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Generate SQL string for inserting all data from this aletr message into the database
        /// </summary>
        /// <param name="alert">alert object</param>
        /// <param name="tableName">database table name</param>
        /// <param name="updateTime">dtaabase update time</param>
        /// <returns>SQL string</returns>
        private static string GenerateAlertInsertQueryString(Alert alert, string tableName, DateTime updateTime)
        {
            string alertTimeStamp = alert.timestamp.ToString(DatabaseUtil.DateTimeFormat);
            string updateTimeStamp = updateTime.ToString(DatabaseUtil.DateTimeFormat);
            string additionalFields = string.Empty;

            if ((alert.priority != string.Empty) && alert.rankingSpecified)
            {
                // added extra fields for sequence alerts for SAPIENT Phase 4 May 16
                additionalFields = string.Format($@",
                    '{alert.priority}',
                    {alert.ranking},
                    {alert.regionID},
                    {alert.confidence},
                    '{alert.debugText}'
                    ");
            }

            string sqlString = string.Format($@"INSERT INTO  {tableName} VALUES(
                    {alert.sourceID},
                    '{alertTimeStamp}',
                    '{updateTimeStamp}',
                    {alert.alertID},
                    '{alert.alertType}',
                    '{alert.status}',
                    '{alert.description}'
                    {additionalFields}
                    );");

            return sqlString;
        }

        private static string GenerateAlertLocationInsertQueryString(Alert alert, string tableName, DateTime updateTime)
        {
            string sqlString = string.Empty;

            if (alert.location != null)
            {
                string alertTimeStamp = alert.timestamp.ToString(DatabaseUtil.DateTimeFormat);
                string updateTimeStamp = updateTime.ToString(DatabaseUtil.DateTimeFormat);

                var x = alert.location.X.ToString("F7");
                var y = alert.location.Y.ToString("F7");
                var z = DatabaseUtil.Nullable(alert.location.Z, alert.location.ZSpecified);
                var dx = DatabaseUtil.Nullable(alert.location.eX, alert.location.eXSpecified);
                var dy = DatabaseUtil.Nullable(alert.location.eY, alert.location.eYSpecified);
                var dz = DatabaseUtil.Nullable(alert.location.eZ, alert.location.eZSpecified);

                sqlString = string.Format($@"INSERT INTO  {tableName} VALUES(
                    {alert.sourceID},
                    '{alertTimeStamp}',
                    '{updateTimeStamp}',
                    {alert.alertID},
                    {x}, 
                    {y},
                    {z}, 
                    {dx},
                    {dy},
                    {dz}
                    );");
            }

            return sqlString;
        }

        private static string GenerateAlertRangeBearingInsertQueryString(Alert alert, string tableName, DateTime updateTime)
        {
            string sqlString = string.Empty;

            if (alert.rangeBearing != null)
            {
                string alertTimeStamp = alert.timestamp.ToString(DatabaseUtil.DateTimeFormat);
                string updateTimeStamp = updateTime.ToString(DatabaseUtil.DateTimeFormat);

                var az = alert.rangeBearing.Az;
                var r = alert.rangeBearing.R;
                var ele = DatabaseUtil.Nullable(alert.rangeBearing.Ele, alert.rangeBearing.EleSpecified);
                var z = DatabaseUtil.Nullable(alert.rangeBearing.Z, alert.rangeBearing.ZSpecified);
                var daz = DatabaseUtil.Nullable(alert.rangeBearing.eAz, alert.rangeBearing.eAzSpecified);
                var dr = DatabaseUtil.Nullable(alert.rangeBearing.eR, alert.rangeBearing.eRSpecified);
                var dele = DatabaseUtil.Nullable(alert.rangeBearing.eEle, alert.rangeBearing.eEleSpecified);
                var dz = DatabaseUtil.Nullable(alert.rangeBearing.eZ, alert.rangeBearing.eZSpecified);

                sqlString = string.Format($@"INSERT INTO  {tableName} VALUES(
                    {alert.sourceID},
                    '{alertTimeStamp}',
                    '{updateTimeStamp}',
                    {alert.alertID},
                    {r}, 
                    {az},
                    {ele},
                    {z},
                    {dr},
                    {daz},
                    {dele},
                    {dz}
                    );");
            }

            return sqlString;
        }

        private static void GenerateAlertAssociatedFileInsertQueryString(Alert alert, string tableName, DateTime updateTime, ref StringBuilder sb)
        {
            if (alert.associatedFile != null)
            {
                string alertTimeStamp = alert.timestamp.ToString(DatabaseUtil.DateTimeFormat);
                string updateTimeStamp = updateTime.ToString(DatabaseUtil.DateTimeFormat);

                foreach (AlertAssociatedFile assocFile in alert.associatedFile)
                {
                    sb.AppendFormat($@"INSERT INTO  {tableName} VALUES(
                    {alert.sourceID},
                    '{alertTimeStamp}',
                    '{updateTimeStamp}',
                    {alert.alertID},
                    '{assocFile.type}',
                    '{assocFile.url}'
                    );");
                }
            }
        }

        private static void GenerateAlertAssociatedDetectionInsertQueryString(Alert alert, string tableName, DateTime updateTime, ref StringBuilder sb)
        {
            if (alert.associatedDetection != null)
            {
                string alertTimeStamp = alert.timestamp.ToString(DatabaseUtil.DateTimeFormat);
                string updateTimeStamp = updateTime.ToString(DatabaseUtil.DateTimeFormat);

                foreach (AlertAssociatedDetection assocDetection in alert.associatedDetection)
                {
                    sb.AppendFormat($@"INSERT INTO  {tableName} VALUES(
                    {alert.sourceID},
                    '{alertTimeStamp}',
                    '{updateTimeStamp}',
                    {alert.alertID},
                    {assocDetection.objectID}
                    );");
                }
            }
        }

        private static void GenerateAlertAssociatedDetectionLocationInsertQueryString(Alert alert, string tableName, DateTime updateTime, ref StringBuilder sb)
        {
            if (alert.associatedDetection != null)
            {
                string alertTimeStamp = alert.timestamp.ToString(DatabaseUtil.DateTimeFormat);
                string updateTimeStamp = updateTime.ToString(DatabaseUtil.DateTimeFormat);

                foreach (AlertAssociatedDetection assocDetection in alert.associatedDetection)
                {
                    // added associated detection locations for sequence alerts for SAPIENT Phase 4 May 16
                    if (assocDetection.location != null)
                    {
                        var x = assocDetection.location.X.ToString("F7");
                        var y = assocDetection.location.Y.ToString("F7");
                        var z = DatabaseUtil.Nullable(assocDetection.location.Z, assocDetection.location.ZSpecified);
                        var dx = DatabaseUtil.Nullable(assocDetection.location.eX, assocDetection.location.eXSpecified);
                        var dy = DatabaseUtil.Nullable(assocDetection.location.eY, assocDetection.location.eYSpecified);
                        var dz = DatabaseUtil.Nullable(assocDetection.location.eZ, assocDetection.location.eZSpecified);

                        sb.AppendFormat($@"INSERT INTO  {tableName} VALUES(
                            {alert.sourceID},
                            '{alertTimeStamp}',
                            '{updateTimeStamp}',
                            {alert.alertID},
                            {x}, 
                            {y},
                            {z}, 
                            {dx},
                            {dy},
                            {dz},
                            '{assocDetection.description}',
                            {assocDetection.objectID}
                            );");
                    }
                }
            }
        }
    }
}
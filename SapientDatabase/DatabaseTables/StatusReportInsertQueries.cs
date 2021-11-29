// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: StatusReportInsertQueries.cs$
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
    /// Database methods relating to Status Report messages
    /// </summary>
    public class StatusReportInsertQueries
    {
        /// <summary>
        /// Log4net logger.
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Test writing to status report database table
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

            // test range bearing field of view
            StatusReport statusReport = GeneratePTZStatusReport(true);
            string sqlString = GenerateInsertQueryString(statusReport);
            Database.SendToDatabase(sqlString, connection, thisLock);

            // test region coverage
            statusReport = GeneratePTZStatusReport(false);
            sqlString = GenerateInsertQueryString(statusReport);
            Database.SendToDatabase(sqlString, connection, thisLock);

            // test High Level region
            sqlString = GenerateHLInsertQueryString(statusReport);
            Database.SendToDatabase(sqlString, connection, thisLock);
        }

        /// <summary>
        /// Method to Populate Status Report Tables
        /// </summary>
        /// <param name="report">status report object</param>
        /// <returns>SQL string</returns>
        public static string GenerateInsertQueryString(StatusReport report)
        {
            try
            {
                DateTime updateTime = DateTime.UtcNow;
                string commonFieldString = GenerateCommonFieldString(report, updateTime);
                StringBuilder sb = new StringBuilder();

                GenerateTopLevelStatus(report, commonFieldString, ref sb);
                GenerateStatusMessages(report, commonFieldString, ref sb);
                GenerateRangeBearingRegions(report, commonFieldString, ref sb);
                GenerateCartesianRegions(report, commonFieldString, ref sb);

                return sb.ToString();
            }
            catch (Exception ex)
            {
                return string.Empty;
                throw ex;
            }
        }

        /// <summary>
        /// Method to Populate HL Status Report Tables
        /// </summary>
        /// <param name="report">status report object</param>
        /// <returns>SQL string</returns>
        public static string GenerateHLInsertQueryString(StatusReport report)
        {
            DateTime updateTime = DateTime.UtcNow;
            StringBuilder sb = new StringBuilder();

            // status regions for HLDMM
            if (report.statusRegion != null)
            {
                foreach (var region in report.statusRegion)
                {
                    if (region.locationList != null)
                    {
                        string sql = InitStatusReportHLLocationList(report, region.locationList, region.type, region.regionID, updateTime, region.regionName, region.regionStatus, region.description);
                        sb.Append(sql);
                    }
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// method to Initialize Status Report Location List
        /// </summary>
        /// <param name="report">report</param>
        /// <param name="location_list">location list</param>
        /// <param name="type">type</param>
        /// <param name="regionId">region Identifier</param>
        /// <param name="updateTime">time now of last update of record</param>
        private static string InitStatusReportHLLocationList(StatusReport report, locationList location_list, string type, int regionId, DateTime updateTime, string regionName, string regionStatus, string description)
        {
            string sql = string.Empty;

            if (location_list.location != null)
            {
                StringBuilder polygon_builder = new StringBuilder();
                polygon_builder.Append("( ");

                foreach (var location in location_list.location)
                {
                    polygon_builder.Append(string.Format("( {0:F7}, {1:F7} ), ", location.X, location.Y));
                }

                polygon_builder.Append(string.Format("( {0:F7}, {1:F7} ) )", location_list.location[0].X, location_list.location[0].Y));

                StringBuilder e_polygon_builder = polygon_builder; // for now just copy main polygon for error

                const string tableName = HLStatusReportRegionConstants.Table;
                string commonFieldString = GenerateCommonFieldString(report, updateTime);
                sql = string.Format($@"INSERT INTO {tableName} VALUES(
                            {commonFieldString},
                            '{type}',
                            '{polygon_builder}',
                            {regionId},
                            '{e_polygon_builder}',
                            '{regionName}',
                            '{regionStatus}',
                            '{description}'
                            );");
            }

            return sql;
        }

        /// <summary>
        /// Generate SQL string for adding top level status fields of StatusReport to database
        /// </summary>
        /// <param name="report">status report object</param>
        /// <param name="commonFieldString">string of values common to all tables</param>
        /// <param name="sb">string builder to add SQL to</param>
        private static void GenerateTopLevelStatus(StatusReport report, string commonFieldString, ref StringBuilder sb)
        {
            string sql = string.Empty;
            const string mainTableName = StatusReportConstants.Table;

            var power_source = DatabaseUtil.Nullable(report.power, ptr => ptr.source);
            var power_status = DatabaseUtil.Nullable(report.power, ptr => ptr.status);
            var power_level = DatabaseUtil.Nullable(report.power, ptr => ptr.level, ptr => ptr.levelSpecified);

            if (report.sensorLocation != null)
            {
                var loc = report.sensorLocation.location;
                var x = loc.X.ToString("F7");
                var y = loc.Y.ToString("F7");
                var z = DatabaseUtil.Nullable(loc.Z, loc.ZSpecified);
                var dx = DatabaseUtil.Nullable(loc.eX, loc.eXSpecified);
                var dy = DatabaseUtil.Nullable(loc.eY, loc.eYSpecified);
                var dz = DatabaseUtil.Nullable(loc.eZ, loc.eZSpecified);

                sql = string.Format($@"INSERT INTO {mainTableName} VALUES(
                            {commonFieldString},
                            '{report.system}',
                            '{report.info}',
                            '{report.activeTaskID}',
                            '{report.mode}',
                            {power_source},
                            {power_status},
                            {power_level},
                            {x}, 
                            {y},
                            {z}, 
                            {dx},
                            {dy},
                            {dz}
                            );");
            }
            else
            {
                sql = string.Format($@"INSERT INTO {mainTableName} VALUES(
                            {commonFieldString},
                            '{report.system}',
                            '{report.info}',
                            '{report.activeTaskID}',
                            '{report.mode}',
                            {power_source},
                            {power_status},
                            {power_level}
                            );");
            }

            sb.Append(sql);
        }

        /// <summary>
        /// Generate SQL string for adding status message fields of StatusReport to database
        /// </summary>
        /// <param name="report">status report object</param>
        /// <param name="commonFieldString">string of values common to all tables</param>
        /// <param name="sb">string builder to add SQL to</param>
        private static void GenerateStatusMessages(StatusReport report, string commonFieldString, ref StringBuilder sb)
        {
            if (report.status != null)
            {
                foreach (var status in report.status)
                {
                    const string tableName = StatusReportMessagesConstants.Table;
                    string sql = string.Format($@"INSERT INTO {tableName} VALUES(
                            {commonFieldString},
                            '{status.level}',
                            '{status.type}',
                            '{status.value}'
                            );");

                    sb.Append(sql);
                }
            }
        }

        /// <summary>
        /// Generate SQL string for adding range bearing regions, coverages and field of views of StatusReport to database
        /// </summary>
        /// <param name="report">status report object</param>
        /// <param name="commonFieldString">string of values common to all tables</param>
        /// <param name="sb">string builder to add SQL to</param>
        private static void GenerateRangeBearingRegions(StatusReport report, string commonFieldString, ref StringBuilder sb)
        {
            if (report.fieldOfView != null && report.fieldOfView.rangeBearingCone != null)
            {
                string sql = InitStatusReportRangeBearingCone(report, report.fieldOfView.rangeBearingCone, "FieldOfView", commonFieldString);
                sb.Append(sql);
            }

            if (report.coverage != null && report.coverage.rangeBearingCone != null)
            {
                string sql = InitStatusReportRangeBearingCone(report, report.coverage.rangeBearingCone, "Coverage", commonFieldString);
                sb.Append(sql);
            }

            if (report.obscuration != null)
            {
                foreach (var obscuration in report.obscuration)
                {
                    if (obscuration.rangeBearingCone != null)
                    {
                        string sql = InitStatusReportRangeBearingCone(report, obscuration.rangeBearingCone, "Obscuration", commonFieldString);
                        sb.Append(sql);
                    }
                }
            }
        }

        /// <summary>
        /// Generate SQL string for adding cartesian regions, coverages and field of views of StatusReport to database
        /// </summary>
        /// <param name="report">status report object</param>
        /// <param name="commonFieldString">string of values common to all tables</param>
        /// <param name="sb">string builder to add SQL to</param>
        private static void GenerateCartesianRegions(StatusReport report, string commonFieldString, ref StringBuilder sb)
        {
            int regionID = 1;
            if (report.fieldOfView != null && report.fieldOfView.locationList != null)
            {
                string sql = InitStatusReportLocationList(report, report.fieldOfView.locationList, "FieldOfView", regionID++, commonFieldString);
                sb.Append(sql);
            }

            if (report.coverage != null && report.coverage.locationList != null)
            {
                string sql = InitStatusReportLocationList(report, report.coverage.locationList, "Coverage", regionID++, commonFieldString);
                sb.Append(sql);
            }

            if (report.obscuration != null)
            {
                foreach (var obscuration in report.obscuration)
                {
                    if (obscuration.locationList != null)
                    {
                        string sql = InitStatusReportLocationList(report, obscuration.locationList, "Obscuration", regionID++, commonFieldString);
                        sb.Append(sql);
                    }
                }
            }

            GenerateStatusRegions(report, commonFieldString, ref sb, ref regionID);
        }

        /// <summary>
        /// Generate SQL string for adding miscellaneous status regions of StatusReport to database
        /// </summary>
        /// <param name="report">status report object</param>
        /// <param name="commonFieldString">string of values common to all tables</param>
        /// <param name="sb">string builder to add SQL to</param>
        /// <param name="regionID">next region ID to use</param>
        private static void GenerateStatusRegions(StatusReport report, string commonFieldString, ref StringBuilder sb, ref int regionID)
        {
            if (report.statusRegion != null)
            {
                foreach (var region in report.statusRegion)
                {
                    if (region.locationList != null)
                    {
                        string sql = InitStatusReportLocationList(report, region.locationList, region.type, regionID++, commonFieldString);
                        sb.Append(sql);
                    }
                }
            }
        }

        /// <summary>
        /// method to Initialize Status Report Location List
        /// </summary>
        /// <param name="report">report</param>
        /// <param name="location_list">location list</param>
        /// <param name="type">type</param>
        /// <param name="index">index</param>
        /// <param name="commonFieldString">string of values common to all tables</param>
        private static string InitStatusReportLocationList(StatusReport report, locationList location_list, string type, int index, string commonFieldString)
        {
            string sql = string.Empty;
            if (location_list.location != null)
            {
                StringBuilder polygon_builder = new StringBuilder();
                polygon_builder.Append("( ");

                foreach (var location in location_list.location)
                {
                    polygon_builder.Append(string.Format("( {0:F7}, {1:F7} ), ", location.X, location.Y));
                }

                polygon_builder.Append(string.Format("( {0:F7}, {1:F7} ) )", location_list.location[0].X, location_list.location[0].Y));

                const string tableName = StatusReportRegionConstants.Table;
                sql = string.Format($@"INSERT INTO {tableName} VALUES(
                            {commonFieldString},
                            '{type}',
                            '{polygon_builder}',
                            {index}
                            );");
            }

            return sql;
        }

        /// <summary>
        /// method to Initialize Status Report Range Bearing Cone
        /// </summary>
        /// <param name="report">report</param>
        /// <param name="range_bearing_cone">range bearing cone</param>
        /// <param name="type">type</param>
        /// <param name="updateTime">time now of last update of record</param>
        private static string InitStatusReportRangeBearingCone(StatusReport report, rangeBearingCone range_bearing_cone, string type, string commonFieldString)
        {
            var r = range_bearing_cone.R.ToString("F7");
            var az = range_bearing_cone.Az.ToString("F7");
            var ele = DatabaseUtil.Nullable(range_bearing_cone.Ele, range_bearing_cone.EleSpecified);

            var vExtent = DatabaseUtil.Nullable(range_bearing_cone.vExtent, range_bearing_cone.vExtentSpecified);

            var dr = DatabaseUtil.Nullable(range_bearing_cone.eR, range_bearing_cone.eRSpecified);
            var daz = DatabaseUtil.Nullable(range_bearing_cone.eAz, range_bearing_cone.eAzSpecified);
            var dele = DatabaseUtil.Nullable(range_bearing_cone.eEle, range_bearing_cone.eEleSpecified);

            var ehExtent = DatabaseUtil.Nullable(range_bearing_cone.ehExtent, range_bearing_cone.ehExtentSpecified);
            var evExtent = DatabaseUtil.Nullable(range_bearing_cone.evExtent, range_bearing_cone.evExtentSpecified);

            const string tableName = StatusReportRangeBearingConstants.Table;
            string sql = string.Format($@"INSERT INTO {tableName} VALUES(
                            {commonFieldString},
                            '{type}',
                            {r}, 
                            {az},
                            {ele},
                            {range_bearing_cone.hExtent},
                            {vExtent},
                            {dr},
                            {daz},
                            {dele},
                            {ehExtent},
                            {evExtent}
                            );");
            return sql;
        }

        /// <summary>
        /// Generate string of fields common to all status report tables
        /// </summary>
        /// <param name="report">StatusReport object</param>
        /// <param name="updateTime">database update time - typically time now</param>
        /// <returns>string containing database fields common to all detection tables</returns>
        private static string GenerateCommonFieldString(StatusReport report, DateTime updateTime)
        {
            string messageTimeStamp = report.timestamp.ToString(DatabaseUtil.DateTimeFormat);
            string updateTimeStamp = updateTime.ToString(DatabaseUtil.DateTimeFormat);

            string sqlString = string.Format($@"
                                        {report.sourceID},
                                        '{messageTimeStamp}',
                                        '{updateTimeStamp}',
                                        {report.reportID} ");
            return sqlString;
        }

        /// <summary>
        /// Populate status report with PTZ like values
        /// </summary>
        /// <returns>populated StatusReport object</returns>
        private static StatusReport GeneratePTZStatusReport(bool useRB)
        {
            double sensorX = 545636;
            double sensorY = 5771236;

            StatusReport status = new StatusReport
            {
                timestamp = DateTime.UtcNow,
                sourceID = 1,
                reportID = 2,
                system = "OK",
                info = "New",
                activeTaskID = 0,
                mode = "Test",
                power = new StatusReportPower { source = "Mains", status = "OK" },
                sensorLocation = new StatusReportSensorLocation { location = new location { X = sensorX, Y = sensorY, Z = 2.0, ZSpecified = true } },
                fieldOfView = new StatusReportFieldOfView(),
            };

            if (useRB)
            {
                status.fieldOfView.rangeBearingCone = GetFieldOfViewCone();
            }
            else
            {
                status.coverage = new StatusReportCoverage()
                {
                    locationList = GenerateCoverage(sensorX, sensorY),
                };
            }

            return status;
        }

        private static locationList GenerateCoverage(double sensorX, double sensorY)
        {
            const double CoverageX = 30;
            const double CoverageY = 30;

            locationList sensorCoverage = new locationList
            {
                location = new[]
                {
                    new location { X = sensorX - CoverageX, Y = sensorY - CoverageY },
                    new location { X = sensorX - CoverageX, Y = sensorY + CoverageY },
                    new location { X = sensorX + CoverageX, Y = sensorY + CoverageY },
                    new location { X = sensorX + CoverageX, Y = sensorY - CoverageY },
                    new location { X = sensorX - CoverageX, Y = sensorY - CoverageY },
                },
            };

            return sensorCoverage;
        }

        /// <summary>
        /// Return a field of cone of the current range, azimuth and elevation
        /// </summary>
        /// <returns>populated rangeBearingCone object</returns>
        private static rangeBearingCone GetFieldOfViewCone()
        {
            rangeBearingCone cone = new rangeBearingCone();
            cone.Az = 127;
            cone.R = 100;
            cone.hExtent = 30;
            cone.Ele = 5;
            cone.EleSpecified = true;
            return cone;
        }
    }
}

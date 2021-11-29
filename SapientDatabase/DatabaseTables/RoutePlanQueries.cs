// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: RoutePlanQueries.cs$
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
    /// Database methods relating to Route Plans
    /// </summary>
    public static class RoutePlanQueries
    {
        /// <summary>
        /// Log4net logger.
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Test writing to route plan database table
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

            RoutePlan routePlan = new RoutePlan()
            {
                sensorID = 1,
                objectiveID = 10,
                objectiveIDSpecified = true,
                eta = DateTime.UtcNow.AddSeconds(30),
                etaSpecified = true,
                taskID = 2000,
                taskIDSpecified = true,
                routeDescription = "Test Route Description",
                routeName = "Test Route Name",
                status = "Approved",
                timestamp = DateTime.UtcNow,
            };

            locationList loc = new locationList()
            {
                location = new[]
                {
                   new location
                   {
                       X = 1,
                       Y = 2,
                       Z = 0,
                       ZSpecified = true,
                   },
                   new location
                   {
                       X = 2,
                       Y = 3,
                       Z = 0,
                       ZSpecified = true,
                   },
                },
            };

            routePlan.locationList = loc;
            routePlan.fieldOfView = new RoutePlanFieldOfView()
            {
                rangeBearingCone = GetTestFieldOfViewCone(),
            };

            WriteRoutePlan(routePlan, connection, thisLock);
        }

        /// <summary>
        /// method to Populate Route Plan Tables
        /// </summary>
        /// <param name="routePlan">route plan objects</param>
        /// <param name="connection">database connection</param>
        /// <param name="lockObject">lock object</param>
        public static void WriteRoutePlan(RoutePlan routePlan, NpgsqlConnection connection, object lockObject)
        {
            string sqlString = GenerateInsertQueryString(routePlan);
            Database.SendToDatabase(sqlString, connection, lockObject);
        }

        /// <summary>
        /// Generate database insert query string
        /// </summary>
        /// <param name="routePlan">route plan objects</param>
        /// <returns>SQL string</returns>
        private static string GenerateInsertQueryString(RoutePlan routePlan)
        {
            DateTime updateTime = DateTime.UtcNow;
            string commonFieldString = GenerateCommonFieldString(routePlan, updateTime);
            StringBuilder sb = new StringBuilder();

            sb.Append(GenerateRoutePlanInsertQueryString(routePlan,
                                                        RoutePlanConstants.Table,
                                                        commonFieldString));

            if (routePlan.fieldOfView!=null && routePlan.fieldOfView.rangeBearingCone != null)
            {
                sb.Append(GenerateRangeBearing(routePlan.fieldOfView.rangeBearingCone,
                                                RoutePlanRangeBearingConstants.Table,
                                                commonFieldString,
                                                routePlan.objectiveID));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Generate SQL string for main route plan table
        /// </summary>
        /// <param name="routePlan">route plan object</param>
        /// <param name="tableName">database table name</param>
        /// <param name="commonFieldString">string of values common to all tables</param>
        /// <returns>SQL string</returns>
        private static string GenerateRoutePlanInsertQueryString(RoutePlan routePlan, string tableName, string commonFieldString)
        {
            string location = LocationListToPolygonString(routePlan.locationList, false);
            string eta = routePlan.eta.ToString(DatabaseUtil.DateTimeFormat);
            string additionalFields = string.Empty;

            if (routePlan.fieldOfView!=null && routePlan.fieldOfView.locationList != null)
            {
                string xy_fov = LocationListToPolygonString(routePlan.fieldOfView.locationList, true);
                additionalFields = string.Format($@",'{xy_fov}'");
            }

            string sql = string.Format($@"INSERT INTO {tableName} VALUES(
              {commonFieldString},
              {routePlan.sensorID},
              {routePlan.taskID},
              {routePlan.objectiveID},
              '{routePlan.routeName}',
              '{routePlan.status}',
              '{location}',
              '{eta}',
              '{routePlan.status}'
              {additionalFields}
               );");
            Log.Debug(sql);
            return sql;
        }

        /// <summary>
        /// Generate SQL string for route plan table for field of view Range Bearing Cone
        /// </summary>
        /// <param name="range_bearing_cone">range bearing cone</param>
        /// <param name="tableName">range bearing table name</param>
        /// <param name="commonFieldString">string of values common to all tables</param>
        /// <param name="reportID">objective ID</param>
        /// <returns>SQL string</returns>
        private static string GenerateRangeBearing(rangeBearingCone range_bearing_cone, string tableName, string commonFieldString, int reportID)
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
            string type = "FieldOfView";

            string sql = string.Format($@"INSERT INTO {tableName} VALUES(
                            {commonFieldString},
                            {reportID},
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
        /// Generate string of fields common to all route plan tables
        /// </summary>
        /// <param name="routePlan">Route Plan object</param>
        /// <param name="updateTime">database update time - typically time now</param>
        /// <returns>string containing database fields common to all detection tables</returns>
        private static string GenerateCommonFieldString(RoutePlan routePlan, DateTime updateTime)
        {
            string messageTimeStamp = routePlan.timestamp.ToString(DatabaseUtil.DateTimeFormat);
            string updateTimeStamp = updateTime.ToString(DatabaseUtil.DateTimeFormat);
            int sourceID = routePlan.sensorID;

            string sqlString = string.Format($@"{sourceID},
                                        '{messageTimeStamp}',
                                        '{updateTimeStamp}'
                                        ");
            return sqlString;
        }

        /// <summary>
        /// Create a polygon string from a  Location List
        /// </summary>
        /// <param name="location_list">location list</param>
        /// <param name="type">type</param>
        /// <param name="index">index</param>
        /// <param name="polygon">if polygon</param>
        /// <param name="updateTime">time now of last update of record</param>
        private static string LocationListToPolygonString(locationList location_list, bool polygon)
        {
            string sql = string.Empty;
            if ((location_list.location != null) && (location_list.location.Length > 0))
            {
                StringBuilder polygon_builder = new StringBuilder();
                if (polygon)
                {
                    polygon_builder.Append("( ");
                }
                else
                {
                    polygon_builder.Append("[ ");
                }

                foreach (var location in location_list.location)
                {
                    polygon_builder.Append(string.Format("( {0:F7}, {1:F7} ), ", location.X, location.Y));
                }

                // if polygon complete the loop with the first point again
                if (polygon)
                {
                    polygon_builder.Append(string.Format("( {0:F7}, {1:F7} )", location_list.location[0].X, location_list.location[0].Y));
                }
                else
                {
                    // a bit of a hack - repeat the last point
                    polygon_builder.Append(string.Format("( {0:F7}, {1:F7} )", location_list.location[location_list.location.Length-1].X, location_list.location[location_list.location.Length-1].Y));
                }

                if (polygon)
                {
                    polygon_builder.Append(")");
                }
                else
                {
                    polygon_builder.Append("]");
                }
                sql = polygon_builder.ToString();
            }

            return sql;
        }

        /// <summary>
        /// Return a field of cone of the current range, azimuth and elevation
        /// </summary>
        /// <returns>populated rangeBearingCone object</returns>
        private static rangeBearingCone GetTestFieldOfViewCone()
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
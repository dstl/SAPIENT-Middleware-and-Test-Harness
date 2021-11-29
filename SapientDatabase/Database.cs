// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: Database.cs$
// Crown-Owned Copyright (c) supplied by QinetiQ. See Release/Supply Conditions

namespace SapientDatabase
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using log4net;
    using Npgsql;
    using SapientServices.Data;

    /// <summary>
    /// class to handle interface to and from Sapient database
    /// </summary>
    public class Database
    {
        /// <summary>
        /// Log4net logger.
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Lock object
        /// </summary>
        private object thisLock = new object();

        private readonly string db_server;
        private readonly string db_port;
        private readonly string db_user;
        private readonly string db_password;
        private readonly string db_name;
        public readonly string DbConnectionError;
        private readonly NpgsqlConnection connection;

        /// <summary>
        /// Initializes a new instance of the <see cref="Database" /> class.
        /// Configure the database connection
        /// </summary>
        /// <param name="host">server on which to connect</param>
        /// <param name="port">port to access database</param>
        /// <param name="user">user name for the database</param>
        /// <param name="password">password to access the database</param>
        /// <param name="name">name of the dataset</param>
        public Database(string host, string port, string user, string password, string name)
        {
            if (name != string.Empty)
            {
                try
                {
                    db_server = host;
                    db_port = port;
                    db_user = user;
                    db_password = password;
                    db_name = name;
                    connection = new NpgsqlConnection("Server=" + db_server + ";Port=" + db_port + ";User Id=" + db_user + ";Password=" + db_password + ";Database=" + db_name + ";");
                    connection.Open();
                    DatabaseCreator.CreateTables(connection);
                    Connected = true;
                }
                catch (NpgsqlException nex)
                {
                    connection = null;
                    Connected = false;
                    DbConnectionError = nex.ToString();

                    if ((uint)nex.ErrorCode == 0x80004005)
                    {
                        Log.ErrorFormat("Cannot connect to database server on Host:{0} Port:{1}. Check these settings and then check that the database server is installed and running.", host, port);
                    }
                    else
                    {
                        Log.Error("SQL Error checking database existence");
                        Log.Error(nex);
                    }
                }
                catch (Exception ex)
                {
                    connection = null;
                    Connected = false;
                    DbConnectionError = ex.ToString();
                }
            }
            else
            {
                connection = null;
            }
        }

        /// <summary>
        /// Gets or sets a flag as to whether connected to database
        /// </summary>
        public static bool Connected { get; private set; }

        /// <summary>
        /// Send SQL string to database
        /// </summary>
        /// <param name="sqlString">SQL string</param>
        /// <param name="connection">connection</param>
        /// <param name="lockObject">lock object</param>
        public static void SendToDatabase(string sqlString, NpgsqlConnection connection, object lockObject)
        {
            if (connection != null)
            {
                using (NpgsqlTransaction pg_transaction = connection.BeginTransaction())
                {
                    try
                    {
                        lock (lockObject)
                        {
                            NpgsqlCommand command = new NpgsqlCommand(sqlString, connection, pg_transaction);
                            command.ExecuteNonQuery();

                            // No exceptions encountered
                            pg_transaction.Commit();
                        }
                    }
                    catch (Exception ex)
                    {
                        // Transaction rolled back to the original state
                        pg_transaction.Rollback();
                        throw ex;
                    }
                }
            }
        }

        /// <summary>
        /// method to close the connection to the database
        /// </summary>
        public void Close()
        {
            Connected = false;

            if (connection != null)
            {
                connection.Close();
            }
        }

        /// <summary>
        /// method to Populate High Level Status Report Tables
        /// </summary>
        /// <param name="report">report</param>
        public void DbHLStatus(StatusReport report)
        {
            string sqlString = DatabaseTables.StatusReportInsertQueries.GenerateHLInsertQueryString(report);
            Database.SendToDatabase(sqlString, connection, thisLock);
        }

        /// <summary>
        /// Method to write registration data to database
        /// </summary>
        /// <param name="registration">registration object</param>
        /// <param name="xml">string of entire registration message xml</param>
        public void DbRegistration(SensorRegistration registration, string xml)
        {
            string sqlString = DatabaseTables.RegistrationQueries.GenerateInsertQueryString(registration, xml);
            Database.SendToDatabase(sqlString, connection, thisLock);
        }

        /// <summary>
        /// Method to write status report data to database
        /// </summary>
        /// <param name="report">Object containing all the health information</param>
        public void DbStatus(StatusReport report)
        {
            string sqlString = DatabaseTables.StatusReportInsertQueries.GenerateInsertQueryString(report);
            Database.SendToDatabase(sqlString, connection, thisLock);
        }

        /// <summary>
        /// Method to write Detection data to database
        /// </summary>
        /// <param name="detectionReport">Object containing all the Detection information</param>
        public void DbDetection(DetectionReport detectionReport, bool hldmm)
        {
            string sqlString = DatabaseTables.DetectionQueries.GenerateInsertQueryString(detectionReport, hldmm);
            Database.SendToDatabase(sqlString, connection, thisLock);
        }

        /// <summary>
        /// Method to write task acknowledgement message to database
        /// </summary>
        /// <param name="taskAck">task acknowledgement object</param>
        /// <param name="tableName">database table to write to</param>
        public void DbAcknowledgeTask(SensorTaskACK taskAck, string tableName)
        {
            string sqlString = DatabaseTables.TaskAckQueries.GenerateInsertQueryString(taskAck, tableName);
            Database.SendToDatabase(sqlString, connection, thisLock);
        }

        /// <summary>
        /// Method to write task message to database
        /// </summary>
        /// <param name="task">Object containing all the task information</param>
        /// <param name="task_message">string of entire task message xml</param>
        public void DbTask(SensorTask task, string task_message, bool hldmm)
        {
            string sqlString = DatabaseTables.TaskQueries.GenerateInsertQueryString(task, task_message, hldmm);
            Database.SendToDatabase(sqlString, connection, thisLock);
        }

        /// <summary>
        /// Method to update alert data to database
        /// </summary>
        /// <param name="alert">Object containing the alert</param>
        public void DbAlert(Alert alert, bool hldmm)
        {
            string sqlString = DatabaseTables.AlertQueries.GenerateInsertQueryString(alert, hldmm);
            Database.SendToDatabase(sqlString, connection, thisLock);
        }

        /// <summary>
        /// Method to update alert information in database if response received
        /// </summary>
        /// <param name="response">Object containing the response</param>
        public void DbAcknowledgeAlert(AlertResponse response)
        {
            // currently no alert ack table in database to write to
        }

        /// <summary>
        /// Method to update objective information in database
        /// </summary>
        /// <param name="objective">Object containing the objective</param>
        public void DbObjective(Objective objective)
        {
            string sqlString = DatabaseTables.ObjectiveQueries.GenerateInsertQueryString(objective, DatabaseTables.ObjectiveConstants.Objective.Table);
            Database.SendToDatabase(sqlString, connection, thisLock);
        }

        /// <summary>
        /// Method to update objective approval information in database
        /// </summary>
        /// <param name="objectiveID"></param>
        /// <param name="sensorID"></param>
        /// <param name="objectiveStatus"></param>
        public void DbApproval(int objectiveID, int sensorID, string objectiveStatus)
        {
            DatabaseTables.ObjectiveQueries.WriteApproval(objectiveID, sensorID, objectiveStatus, connection, thisLock);
        }

        /// <summary>
        /// Method to update route plan information in database
        /// </summary>
        /// <param name="routePlan">Object containing the route plan</param>
        public void DbRoutePlan(RoutePlan routePlan)
        {
            DatabaseTables.RoutePlanQueries.WriteRoutePlan(routePlan, connection, thisLock);
        }

        /// <summary>
        /// return sensor location offset data from database. ID 0 will return all sensor offset data
        /// </summary>
        /// <param name="sensorID"></param>
        /// <returns></returns>
        public Dictionary<long, Dictionary<string, double>> GetSensorOffsetFromDB(long sensorID = 0)
        {
            var offsets = new Dictionary<long, Dictionary<string, double>>();
            NpgsqlCommand command = new NpgsqlCommand();
            command.Connection = this.connection;
            string sql;
            if (sensorID == 0)
            {
                sql = "select t.sensor_id,t.x_offset,t.y_offset,t.z_offset,t.az_offset,t.ele_offset,r.MAXTIME from (SELECT sensor_id, MAX(update_time) as MAXTIME FROM sensor_location_offset group by sensor_id) r inner join sensor_location_offset t on t.sensor_id=r.sensor_id and t.update_time=r.MAXTIME";
            }
            else
            {
                sql = string.Format("select sensor_id,x_offset,y_offset,z_offset,az_offset,ele_offset from sensor_location_offset where sensor_id={0}", sensorID);
            }

            command.CommandText = sql;
            var response = command.ExecuteReader();
            while (response.Read())
            {
                offsets.Add((long)response[0], new Dictionary<string, double>()
                {
                    { "x_offset", (double)response[1] },
                    { "y_offset", (double)response[2] },
                    { "z_offset", (double)response[3] },
                    { "az_offset", (double)response[4] },
                    { "ele_offset", (double)response[5] },
                });
            }

            return offsets;
        }

        /// <summary>
        /// update sensor location offset data in database
        /// </summary>
        /// <param name="sensorID">sensor ID to apply offset to</param>
        /// <param name="xOffset">x coordinate offset</param>
        /// <param name="yOffset">y coordinate offset</param>
        /// <param name="zOffset">z coordinate offset</param>
        /// <param name="azOffset">azimuth angle offset</param>
        /// <param name="eleOffset">elevation angle offset</param>
        public void UpdateSensorOffsetInDB(long sensorID, double xOffset, double yOffset, double zOffset, double azOffset, double eleOffset)
        {
            var offsets = new Dictionary<long, Dictionary<string, double>>();
            NpgsqlCommand command = new NpgsqlCommand();
            command.Connection = this.connection;
            string sql = string.Format("Insert INTO sensor_location_offset Values ('{0}',{1},{2},{3},{4},{5},{6}) ON CONFLICT (sensor_id) DO UPDATE SET update_time='{0}', x_offset = {2}, y_offset={3}, z_offset={4},az_offset={5},ele_offset={6};", DateTime.UtcNow.ToString(DatabaseUtil.DateTimeFormat), sensorID, xOffset, yOffset, zOffset, azOffset, eleOffset);
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// return objective information data from database
        /// </summary>
        /// <param name="approval"></param>
        /// <returns></returns>
        public string GetObjectiveInformation(Approval approval)
        {
            string retVal=string.Empty;
            NpgsqlCommand command = new NpgsqlCommand();
            command.Connection = this.connection;
            string sql = string.Format("Select information from {0} where objective_id={1} and sensor_id={2} and information<>'' order by key_id DESC limit 1;", SapientDatabase.DatabaseTables.ObjectiveConstants.Objective.Table, approval.objectiveID, approval.sensorID);
            command.CommandText = sql;
            var response = command.ExecuteReader();
            while (response.Read())
            {
                retVal = response[0].ToString();
            }
            return retVal;
        }
    }
}

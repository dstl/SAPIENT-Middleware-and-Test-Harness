// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: TaskQueries.cs$
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
    /// Database methods relating to Task messages
    /// </summary>
    public class TaskQueries
    {
        /// <summary>
        /// Log4net logger.
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Test writing to task database table
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

            SensorTask task = new SensorTask()
            {
                sensorID = 1,
                taskID = 123,
                timestamp = DateTime.UtcNow,
            };

            task.command = new SensorTaskCommand()
            {
                request = "Registration",
            };

            string xmlString = ConfigXMLParser.Serialize(task);
            string sqlString = GenerateInsertQueryString(task, xmlString, false);
            Database.SendToDatabase(sqlString, connection, thisLock);
        }

        /// <summary>
        /// Method to write task message to database
        /// </summary>
        /// <param name="task">Object containing all the task information</param>
        /// <param name="task_message">string of entire task message xml</param>
        /// <param name="hldmm">if high level database table</param>
        /// <returns>SQL string</returns>
        public static string GenerateInsertQueryString(SensorTask task, string task_message, bool hldmm)
        {
            bool deleteRegionTask = false;

            if ((task.region != null) && task.region[0].type.ToLowerInvariant().Equals("delete"))
            {
                deleteRegionTask = true;
            }

            string tableName = TaskConstants.SensorTask.Table;

            if (hldmm)
            {
                tableName = TaskConstants.HLTask.Table;
            }

            string taskType = string.Empty;

            if ((task.taskName != null) && task.taskName.ToLowerInvariant().Contains("manual task"))
            {
                taskType = "manual task";
            }

            string sqlString = string.Empty;

            if (!deleteRegionTask)
            {
                DateTime updateTime = DateTime.UtcNow;
                string messageTimeStamp = task.timestamp.ToString(DatabaseUtil.DateTimeFormat);
                string updateTimeStamp = updateTime.ToString(DatabaseUtil.DateTimeFormat);
                string t2 = task_message.Replace("\r\n", string.Empty);

                sqlString = string.Format($@"INSERT INTO  {tableName} VALUES(
                {task.sensorID},
                '{messageTimeStamp}',
                '{updateTimeStamp}', 
                {task.taskID},
                '{t2}',
                '{taskType}');");
            }

            if (task.region != null)
            {
                sqlString += DbRegion(task);
            }

            Log.Debug(sqlString);
            return sqlString;
        }

        private static string DbRegion(SensorTask task)
        {
            string sqlString = string.Empty;

            if (task.region[0].type != "Delete")
            {
                sqlString = AddRegions(task);
            }
            else
            {
                sqlString = DeleteRegion(task.region[0].regionID);
            }

            return sqlString;
        }

        private static string AddRegions(SensorTask task)
        {
            string sqlString = string.Empty;

            if ((task.region != null) && (task.region.Length > 0))
            {
                for (int i = 0; i < task.region.Length; i++)
                {
                    sqlString += AddRegion(task, i);
                }
            }

            return sqlString;
        }

        private static string AddRegion(SensorTask task, int j)
        {
            string sqlString = string.Empty;

            if ((task.region != null) && (task.region[j].locationList != null))
            {
                DateTime updateTime = DateTime.UtcNow;
                string messageTimeStamp = task.timestamp.ToString(DatabaseUtil.DateTimeFormat);
                string updateTimeStamp = updateTime.ToString(DatabaseUtil.DateTimeFormat);
                int task_id = task.taskID;

                string type = task.region[j].type;

                string tableName = TaskConstants.HLTaskRegion.Table;

                // Build Location string
                string polygon = "(";
                for (int i = 0; i < task.region[j].locationList.location.Length; i++)
                {
                    polygon += "(" + task.region[j].locationList.location[i].X + "," + task.region[j].locationList.location[i].Y + "),";
                }

                polygon = polygon.Substring(0, polygon.Length - 1) + ")";

                string region_id = task.region[j].regionID.ToString();

                sqlString = string.Format($@"INSERT INTO {tableName} VALUES(
                {task.sensorID},
                '{messageTimeStamp}',
                '{updateTimeStamp}', 
                {task.taskID},
                '{type}', 
                '{polygon}',
                '{region_id}');");
            }

            return sqlString;
        }

        private static string DeleteRegion(long regionID)
        {
            string sqlString = DeleteRegionFromTaskTable(regionID);
            sqlString += DeleteRegionFromRegionTable(regionID);
            return sqlString;
        }

        private static string DeleteRegionFromRegionTable(long regionID)
        {
            // Delete from hl_task_region_v3
            string sqlString = "DELETE FROM hl_task_region_v3 WHERE region_id=" + regionID.ToString();
            return sqlString;
        }

        private static string DeleteRegionFromTaskTable(long regionID)
        {
            // Delete from hl_task FIRST
            string deleteString = "DELETE FROM hl_task_v3 WHERE task_id IN (SELECT task_id FROM hl_task_region_v3 WHERE region_id=" + regionID.ToString() + ")";
            return deleteString;
        }
    }
}
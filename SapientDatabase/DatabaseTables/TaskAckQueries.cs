// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: TaskAckQueries.cs$
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
    /// Database methods relating to Task Acknowledgement messages
    /// </summary>
    public class TaskAckQueries
    {
        /// <summary>
        /// Log4net logger.
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Test writing to Task Acknowledgement database table
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

            SensorTaskACK taskAck = new SensorTaskACK()
            {
                sensorID = 1,
                taskID = 123,
                timestamp = DateTime.UtcNow,
            };

            string tableName = TaskConstants.HLTaskAck.Table;
            string sqlString = GenerateInsertQueryString(taskAck, tableName);
            Database.SendToDatabase(sqlString, connection, thisLock);

            taskAck.taskID++;
            taskAck.associatedFile = new[]
            {
                new SensorTaskACKAssociatedFile { type = "image", url = "testfilename.jpg" },
            };
            sqlString = GenerateInsertQueryString(taskAck, tableName);
            Database.SendToDatabase(sqlString, connection, thisLock);

            tableName = TaskConstants.SensorTaskAck.Table;
            sqlString = GenerateInsertQueryString(taskAck, tableName);
            Database.SendToDatabase(sqlString, connection, thisLock);

            tableName = TaskConstants.GUITaskAck.Table;
            sqlString = GenerateInsertQueryString(taskAck, tableName);
            Database.SendToDatabase(sqlString, connection, thisLock);
        }

        /// <summary>
        /// Method to write task acknowledgement message to database
        /// </summary>
        /// <param name="taskAck">task acknowledgement object</param>
        /// <param name="tableName">database table to write to</param>
        /// <returns>SQL string</returns>
        public static string GenerateInsertQueryString(SensorTaskACK taskAck, string tableName)
        {
            string sqlString = string.Empty;
            DateTime updateTime = DateTime.UtcNow;
            string messageTimeStamp = taskAck.timestamp.ToString(DatabaseUtil.DateTimeFormat);
            string updateTimeStamp = updateTime.ToString(DatabaseUtil.DateTimeFormat);
            int task_key_id = 0;

            if (taskAck.associatedFile != null)
            {
                if (taskAck.associatedFile[0] != null)
                {
                    string fileType = taskAck.associatedFile[0].type;
                    string fileUrl = taskAck.associatedFile[0].url;

                    sqlString = string.Format($@"INSERT INTO  {tableName} VALUES(
                    {taskAck.sensorID},
                    '{messageTimeStamp}',
                    '{updateTimeStamp}', 
                    {taskAck.taskID},
                    '{taskAck.status}',
                    '{taskAck.reason}',
                    {task_key_id},
                    '{fileType}',
                    '{fileUrl}'
                    );");
                }
            }
            else
            {
                sqlString = string.Format($@"INSERT INTO  {tableName} VALUES(
                    {taskAck.sensorID},
                    '{messageTimeStamp}',
                    '{updateTimeStamp}', 
                    {taskAck.taskID},
                    '{taskAck.status}',
                    '{taskAck.reason}',
                    {task_key_id});");
            }

            return sqlString;
        }
    }
}
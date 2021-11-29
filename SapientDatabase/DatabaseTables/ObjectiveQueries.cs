// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: ObjectiveQueries.cs$
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
    /// Database methods relating to Objectives
    /// </summary>
    public class ObjectiveQueries
    {
        /// <summary>
        /// Log4net logger.
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Test writing to objective database table
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

            Objective objective = new Objective()
            {
                sensorID = 1,
                sourceID = 0,
                objectiveID = 1,
                objectiveType = "objective type",
                description = "Go to location",
            };

            location loc = new location()
            {
                X = -2.325,
                Y = 52.101,
            };

            objective.location = loc;

            string sqlString = GenerateInsertQueryString(objective, ObjectiveConstants.Objective.Table);
            Database.SendToDatabase(sqlString, connection, thisLock);
        }

        /// <summary>
        /// Generate SQL string for inserting all data from this objective message into the database
        /// </summary>
        /// <param name="objective">objective object</param>
        /// <param name="tableName">database table to write to</param>
        /// <returns>SQL string</returns>
        public static string GenerateInsertQueryString(Objective objective, string tableName)
        {
            DateTime updateTime = DateTime.UtcNow;

            var x = objective.location.X;
            var y = objective.location.Y;
            var z = DatabaseUtil.Nullable(objective.location.Z, objective.location.ZSpecified);

            string objectiveTimeStamp = objective.timestamp.ToString(DatabaseUtil.DateTimeFormat);
            string updateTimeStamp = updateTime.ToString(DatabaseUtil.DateTimeFormat);

            string sql = string.Format($@"INSERT INTO  {tableName} VALUES(
            '{objectiveTimeStamp}',
            '{updateTimeStamp}', 
            {objective.sourceID},
            {objective.objectiveID},
            {objective.sensorID}, 
            '{objective.objectiveType}', 
            '{objective.status}', 
            '{objective.description}',
            {objective.location.X}, 
            {objective.location.Y}, 
            {objective.location.Z},
            '{objective.priority}',
            {objective.ranking}, 
            '{objective.information}',
            {objective.objectID},
            {objective.regionID});");

            Log.Debug(sql);
            return sql;
        }

        public static void WriteApproval(int objectiveID, int sensorID, string status, NpgsqlConnection connection, object lockObject)
        {
            string sqlString = GenerateApprovalInsertQueryString(objectiveID, sensorID, status, ObjectiveConstants.Approval.Table);
            Database.SendToDatabase(sqlString, connection, lockObject);
        }

        private static string GenerateApprovalInsertQueryString(int objectiveID, int sensorID, string status, string tableName)
        {
            DateTime updateTime = DateTime.UtcNow;
            string updateTimeStamp = updateTime.ToString(DatabaseUtil.DateTimeFormat);

            string sql = string.Format($@"INSERT INTO  {tableName} VALUES(
            '{updateTimeStamp}', 
            {sensorID},
            {objectiveID},
            '{status}');");
            Log.Debug(sql);
            return sql;
        }
    }
}

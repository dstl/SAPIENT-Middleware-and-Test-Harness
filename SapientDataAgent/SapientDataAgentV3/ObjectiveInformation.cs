// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: ObjectiveInformation.cs$
// Crown-Owned Copyright (c) supplied by QinetiQ. See Release/Supply Conditions
// $NoKeywords$

namespace SapientMiddleware
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Timers;
    using log4net;
    using SapientDatabase;
    using SapientServices.Data;

    /// <summary>
    /// Class to handle the objective information store
    /// </summary>
    public static class ObjectiveInformation
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // concurrent dictionary to handle reading and writing to objective list
        private static ConcurrentDictionary<Objective, string> dictObjectiveInternalInfo = new ConcurrentDictionary<Objective, string>();

        // timer for printing objective list info every 30 seconds
        private static System.Timers.Timer printListTimer = new System.Timers.Timer(TimeSpan.FromMinutes(0.5).TotalMilliseconds);

        /// <summary>
        /// Initializes static members of the <see cref="ObjectiveInformation" /> class.
        /// Sets the parameters for the list all timer
        /// </summary>
        static ObjectiveInformation()
        {
            printListTimer.AutoReset = true;
            printListTimer.Elapsed += new System.Timers.ElapsedEventHandler(ListAll);
        }

        /// <summary>
        /// Add objectives to the objective dictionary and starts the list all timer
        /// </summary>
        /// <param name="objective">objective message object</param>
        public static void AddObjective(Objective objective)
        {
            dictObjectiveInternalInfo.TryAdd(objective, "No acknowledgement");
            printListTimer.Start();
        }

        /// <summary>
        /// Update objective status based on sensorTask Acknowledgement
        /// </summary>
        /// <param name="objectiveID">objective ID</param>
        /// <param name="sensorID">sensor ID</param>
        /// <param name="status">status text</param>
        /// <param name="database">database object</param>
        public static void ApproveObjective(int objectiveID, int sensorID, string status, Database database)
        {
            try
            {
                dictObjectiveInternalInfo[dictObjectiveInternalInfo.Where(e => e.Key.objectiveID == objectiveID && e.Key.sensorID == sensorID).Single().Key] = status;
            }
            catch
            {
                Log.Debug($@"Acknowledgement Task ID: {objectiveID} does not belong to an objective");
            }

            try
            {
                database.DbApproval(objectiveID, sensorID, status);
            }
            catch (Exception e)
            {
                Log.Error("Error writing approval status to DB: " + e.Message);
            }
        }

        /// <summary>
        /// Remove old objectives from the objective list and pause timer if empty
        /// </summary>
        private static void RemoveOldObjectives()
        {
            string valueout;

            foreach (var key in (dictObjectiveInternalInfo.Where(e => e.Key.timestamp < (DateTime.UtcNow.AddHours(-1))).Select(f => f.Key)))
            {
                dictObjectiveInternalInfo.TryRemove(key, out valueout);
            }

            if (dictObjectiveInternalInfo.Count == 0)
            {
                printListTimer.Stop();
            }
        }

        /// <summary>
        /// purge old objectives and print pertinent information
        /// </summary>
        /// <param name="sender">not used</param>
        /// <param name="e">not used</param>
        public static void ListAll(object sender, ElapsedEventArgs e)
        {
            if (dictObjectiveInternalInfo.Count > 0)
            {
                RemoveOldObjectives();
                foreach (var pair in (dictObjectiveInternalInfo.OrderByDescending(a => a.Key.timestamp).GroupBy(b => b.Key.sensorID).Select(c => c.First())))
                {
                    Log.Info($@"SensorID: {pair.Key.sensorID}, ObjectiveID: {pair.Key.objectiveID}, Objective Information: {pair.Key.description}, Objective timestamp: {pair.Key.timestamp}, Objective Status: {pair.Value}");
                }
            }
        }
    }
}

// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: TaskConstants.cs$
// Crown-Owned Copyright (c) supplied by QinetiQ. See Release/Supply Conditions
// $NoKeywords$
namespace SapientDatabase.DatabaseTables
{
    /// <summary>
    /// The names of tasking tables and sequences.
    /// </summary>
    public static class TaskConstants
    {
        public static class Objective
        {
            public const string Table = "hl_objective_v3";
            public const string Pkey = "hl_objective_pkey";
        }

        public static class SensorTask
        {
            public const string Table = "sensor_task_v3";
            public const string Seq = "sensor_task_id_seq";
            public const string Pkey = "sensor_task_pkey";
        }

        public static class HLTask
        {
            public const string Table = "hl_task_v3";
            public const string Seq = "hl_task_id_seq";
            public const string Pkey = "hl_task_pkey";
        }

        public static class SensorTaskAck
        {
            public const string Table = "sensor_taskack_v3";
            public const string Seq = "sensor_taskack_v3_key_id_seq";
            public const string Pkey = "sensor_taskack_pkey";
        }

        public static class HLTaskAck
        {
            public const string Table = "hl_taskack_v3";
            public const string Seq = "hl_taskack_v3_key_id_seq";
            public const string Pkey = "hl_taskack_pkey";
        }

        public static class GUITaskAck
        {
            public const string Table = "gui_taskack_v3";
            public const string Seq = "gui_taskack_v3_key_id_seq";
            public const string Pkey = "gui_taskack_pkey";
        }

        public static class SensorTaskRegion
        {
            public const string Table = "sensor_task_region_v3";
            public const string Seq = "sensor_task_region_v3_key_id_seq";
            public const string Pkey = "sensor_task_region_v3_pkey";
            public const string TaskKeyIdSeq = SensorTask.Seq;
        }

        public static class HLTaskRegion
        {
            public const string Table = "hl_task_region_v3";
            public const string Seq = "hl_task_region_v3_key_id_seq";
            public const string Pkey = "hl_task_region_v3_pkey";
        }

        public static class HLTaskApproval
        {
            public const string Table = "hl_task_approval_v3";
            public const string Pkey = "hl_task_approval_pkey";
            public const string Accepted = "Accepted";
            public const string Rejected = "Rejected";
        }
    }
}

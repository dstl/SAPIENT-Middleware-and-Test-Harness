// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: ObjectiveConstants.cs$
// Crown-Owned Copyright (c) supplied by QinetiQ. See Release/Supply Conditions
// $NoKeywords$
namespace SapientDatabase.DatabaseTables
{
    /// <summary>
    /// The names of objective tables and sequences.
    /// </summary>
    public static class ObjectiveConstants
    {
        public static class Objective
        {
            public const string Table = "hl_objective_v3";
            public const string Pkey = "hl_objective_pkey";
        }

        public static class Approval
        {
            public const string Table = "hl_task_approval_v3";
            public const string Pkey = "hl_task_approval_pkey";
        }
    }
}
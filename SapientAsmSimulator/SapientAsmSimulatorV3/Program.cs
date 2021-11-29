// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: Program.cs$
// <copyright file="Program.cs" company="QinetiQ">
// Crown-Owned Copyright (c) supplied by QinetiQ. See Release/Supply Conditions
// </copyright>

using System;
using System.Windows.Forms;
using log4net;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace SapientASMsimulator
{
    /// <summary>
    /// Main Program Class
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Log4net logger
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args">command line argument - optionally specify fixed sensor identifier</param>
        [STAThread]
        public static void Main(string[] args)
        {
            const int ExeName = 0;
            const int ExeVersion = 2;

            // Output the assembly name and version number for configuration purposes
            string[] assemblyDetails = System.Reflection.Assembly.GetExecutingAssembly().FullName.Split(',', '=');

            Log.Info(Environment.NewLine);
            Log.Info(assemblyDetails[ExeName] + " - Version " + assemblyDetails[ExeVersion]);

            if (args.Length > 0)
            {
                ASMMainProcess.AsmId = int.Parse(args[0]);
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ClientForm());
        }
    }
}
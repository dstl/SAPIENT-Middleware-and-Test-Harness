// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: Program.cs$
// Crown-Owned Copyright (c) supplied by QinetiQ. See Release/Supply Conditions

using System;
using System.Windows.Forms;
using log4net;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace SapientHldmmSimulator
{
    class Program
    {
        /// <summary>
        /// log4net logger
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [STAThread]
        static void Main()
        {
            const int ExeName = 0;
            const int ExeVersion = 2;

            // Output the assembly name and version number for configuration purposes
            string[] assemblyDetails = System.Reflection.Assembly.GetExecutingAssembly().FullName.Split(',', '=');

            Log.Info(Environment.NewLine);
            Log.Info(assemblyDetails[ExeName] + " - Version " + assemblyDetails[ExeVersion]);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TaskForm());
        }
    }
}

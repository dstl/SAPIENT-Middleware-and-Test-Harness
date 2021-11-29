// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: CSVDetection.cs$
// <copyright file="CSVDetection.cs" company="QinetiQ">
// Crown-Owned Copyright (c) supplied by QinetiQ. See Release/Supply Conditions
// </copyright>

namespace SapientASMsimulator
{
    using System;
    using System.IO;
    using log4net;
    using ScriptReader;
    class CSVDetection
    {
        /// <summary>
        /// Log4net logger
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private string directory;

        /// <summary>
        /// used for passing filename from single file click to process thread
        /// </summary>
        private string currentFilename;

        ////private ISender messenger;

        ////private Dictionary<string, string> expectedErrors;

       /// private List<string> xmlList = new List<string>();

        private string output;

        public void LoadData(string filename)
        {
            CSVFileParser parser = new CSVFileParser(filename);
            directory = Path.GetDirectoryName(filename);
            output += "Directory:" + directory + "\r\n";

            parser.Open();

            while (parser.Parse())
            {
                string[] linedata;

                // Parse a line of data
                parser.ParseLineString(out linedata);
                if (linedata[0] != "")
                {
                    ////this.listBox1.Items.Add(linedata[0]);
                    ////xmlList.Add(directory + "\\" + linedata[0]);
                }
            }
            ////this.listBox1.Items.Add("END");
            ////this.listBox1.SelectedIndex = 0;
            parser.Close();
        }


    }
}

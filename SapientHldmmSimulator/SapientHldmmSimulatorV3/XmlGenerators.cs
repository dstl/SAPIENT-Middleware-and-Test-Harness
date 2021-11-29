// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: XmlGenerators.cs$
// Crown-Owned Copyright (c) supplied by QinetiQ. See Release/Supply Conditions

namespace SapientHldmmSimulator
{
    using log4net;

    /// <summary>
    /// Base class for  XML generation 
    /// </summary>
    public class XmlGenerators
    {
        #region data members

        /// <summary>
        /// log4net logger
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected static double start_longitude = Properties.Settings.Default.startLongitude;
        protected static double start_latitude = Properties.Settings.Default.startLatitude;
        protected static bool isUTM = ((start_longitude > 180) || (start_longitude < -180) || (start_latitude > 90) || (start_latitude < -90));

        #endregion

        #region Properties

        public static int ASMId { get; set; }

        public long MessageCount { get; protected set; }

        public bool LoopMessages { get; set; }

        public int LoopTime { get; set; }

        #endregion

        #region public methods

        #endregion

    }
}

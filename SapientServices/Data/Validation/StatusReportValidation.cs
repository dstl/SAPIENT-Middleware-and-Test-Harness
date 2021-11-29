namespace SapientServices.Data.Validation
{
    public class StatusReportValidation
    {
        /// <summary>
        /// validate a Deserialized Status Report Message
        /// </summary>
        /// <param name="status_report">the Status Report Message</param>
        /// <returns>empty string or validation error message</returns>
        public static string ValidateStatusReport(StatusReport status_report)
        {
            string errorMessage = string.Empty;

            if (!ValidSystem(status_report.system))
            {
                errorMessage = "StatusReport Invalid system";
            }

            if ((errorMessage == string.Empty) && !ValidInfo(status_report.info))
            {
                errorMessage = "StatusReport Invalid info";
            }

            if (errorMessage == string.Empty)
            {
                errorMessage = ValidatePower(status_report);
            }

            if (errorMessage == string.Empty)
            {
                errorMessage = ValidateStatus(status_report);
            }

            if (errorMessage == string.Empty)
            {
                errorMessage = ValidateRegions(status_report);
            }

            return errorMessage;
        }

        /// <summary>
        /// Whether this is a valid system tag
        /// </summary>
        /// <param name="tag">request string to validate</param>
        /// <returns>true if valid</returns>
        private static bool ValidSystem(string tag)
        {
            var retval = false;
            switch (tag.ToLowerInvariant())
            {
                case "ok":
                case "error":
                case "tamper":
                case "goodbye":
                    retval = true;
                    break;
            }

            return retval;
        }

        /// <summary>
        /// Whether this is a valid info tag
        /// </summary>
        /// <param name="tag">request string to validate</param>
        /// <returns>true if valid</returns>
        private static bool ValidInfo(string tag)
        {
            var retval = false;
            switch (tag.ToLowerInvariant())
            {
                case "unchanged":
                case "new":
                case "additional":
                    retval = true;
                    break;
            }

            return retval;
        }

        /// <summary>
        /// validate a Deserialized Status Report Message power section
        /// </summary>
        /// <param name="status_report">the Status Report Message</param>
        /// <returns>empty string or validation error message</returns>
        private static string ValidatePower(StatusReport status_report)
        {
            if (status_report.power != null)
            {
                if (status_report.power.source != "Mains" && status_report.power.source != "Battery")
                {
                    return "StatusReport Invalid power source";
                }

                if (status_report.power.status != "OK" && status_report.power.status != "Fault")
                {
                    return "StatusReport Invalid power status";
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// validate a Deserialized Status Report Message status section
        /// </summary>
        /// <param name="status_report">the Status Report Message</param>
        /// <returns>empty string or validation error message</returns>
        private static string ValidateStatus(StatusReport status_report)
        {
            if (status_report.status != null)
            {
                foreach (var status in status_report.status)
                {
                    if (status.level != "Error" && status.level != "Warning" && status.level != "Information" && status.level != "Sensor")
                    {
                        return "StatusReport Invalid status level";
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// validate a Deserialized Status Report Message regions section
        /// </summary>
        /// <param name="status_report">the Status Report Message</param>
        /// <returns>empty string or validation error message</returns>
        private static string ValidateRegions(StatusReport status_report)
        {
            string errorMessage = string.Empty;

            if (status_report.fieldOfView != null)
            {
                errorMessage = ValidateLocationList(status_report.fieldOfView.locationList, status_report.fieldOfView.rangeBearingCone, "fieldOfView");
            }

            if ((status_report.coverage != null) && (errorMessage == string.Empty))
            {
                errorMessage = ValidateLocationList(status_report.coverage.locationList, status_report.coverage.rangeBearingCone, "coverage");
            }

            if (status_report.obscuration != null)
            {
                foreach (var obs in status_report.obscuration)
                {
                    if(errorMessage == string.Empty)
                    {
                        errorMessage = ValidateLocationList(obs.locationList, obs.rangeBearingCone, "obscuration");
                    }
                }
            }

            return errorMessage;
        }

        /// <summary>
        /// validate a Deserialized Status Report Message regions section
        /// </summary>
        /// <param name="status_report">the Status Report Message</param>
        /// <returns>empty string or validation error message</returns>
        private static string ValidateLocationList(locationList locations, rangeBearingCone rbc, string tag)
        {
            if (locations == null && rbc == null)
            {
                return string.Format($@"StatusReport Invalid {tag} has neither locationList or rangeBearingCone");
            }

            if (locations != null && rbc != null)
            {
                return string.Format($@"StatusReport Invalid {tag} has both locationList and rangeBearingCone");
            }

            return string.Empty;
        }
    }
}

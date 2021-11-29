namespace SapientServices.Data.Validation
{
    public class SensorRegistrationValidation
    {
        /// <summary>
        /// validate a Deserialized Sensor Registration Message
        /// </summary>
        /// <param name="registration">the Sensor Registration Message</param>
        /// <returns>empty string or validation error message</returns>
        public static string ValidateRegistration(SensorRegistration registration)
        {
            string errorMessage = string.Empty;

            if (string.IsNullOrEmpty(registration.sensorType))
            {
                errorMessage = "SensorRegistration Invalid no sensorType specified";
            }

            if (errorMessage == string.Empty)
            {
                errorMessage = ValidateDefaultMode(registration);
            }

            if (errorMessage == string.Empty)
            {
                foreach (var mode in registration.modeDefinition)
                {
                    if (errorMessage == string.Empty)
                    {
                        errorMessage = ValidateModeDefinition(mode);
                    }
                }
            }

            if (errorMessage == string.Empty)
            {
                errorMessage = ValidateHeartbeatDefinition(registration);
            }

            return errorMessage;
        }

        /// <summary>
        /// validate a Deserialized Sensor Registration Message Default Mode Definition
        /// </summary>
        /// <param name="registration">the Sensor Registration Message</param>
        /// <returns>empty string or validation error message</returns>
        private static string ValidateDefaultMode(SensorRegistration registration)
        {
            if (registration.modeDefinition == null || registration.modeDefinition.Length < 1)
            {
                return "SensorRegistration Invalid no modes defined";
            }

            if (registration.modeDefinition[0].type != "Permanent")
            {
                return "SensorRegistration Invalid modeDefinition[0] type must be Permanent";
            }

            if (registration.modeDefinition[0].modeName != "Default")
            {
                return "SensorRegistration Invalid modeDefinition[0] modeName must be Default";
            }

            return string.Empty;
        }

        /// <summary>
        /// validate a Deserialized Sensor Registration Message Mode Definition
        /// </summary>
        /// <param name="mode">mode section of the Sensor Registration Message</param>
        /// <returns>empty string or validation error message</returns>
        private static string ValidateModeDefinition(SensorRegistrationModeDefinition mode)
        {
            string errorMessage = string.Empty;

            if(!ValidScanType(mode.scanType))
            {
                errorMessage = "SensorRegistration Invalid modeDefinition scanType";
            }

            if ((errorMessage == string.Empty) && (string.IsNullOrEmpty(mode.trackingType) == false))
            {
                if (!ValidTrackingType(mode.trackingType))
                {
                    errorMessage = "SensorRegistration Invalid modeDefinition trackingType";
                }
            }

            if (errorMessage == string.Empty)
            {
                errorMessage = ValidateModeDetectionDefinition(mode);
            }

            if (errorMessage == string.Empty)
            {
                errorMessage = ValidateModeTaskDefinition(mode);
            }

            return errorMessage;
        }

        /// <summary>
        /// validate a Deserialized Sensor Registration Message Mode Detection Definition
        /// </summary>
        /// <param name="mode">mode section of the Sensor Registration Message</param>
        /// <returns>empty string or validation error message</returns>
        private static string ValidateModeDetectionDefinition(SensorRegistrationModeDefinition mode)
        {
            if (mode.detectionDefinition.locationType.Value != "UTM" && mode.detectionDefinition.locationType.Value != "GPS" && mode.detectionDefinition.locationType.Value != "RangeBearing")
            {
                return "SensorRegistration Invalid modeDefinition detectionDefinition locationType";
            }

            if (mode.detectionDefinition.detectionClassDefinition != null)
            {
                if (string.IsNullOrEmpty(mode.detectionDefinition.detectionClassDefinition.confidenceDefinition) == false)
                {
                    if (mode.detectionDefinition.detectionClassDefinition.confidenceDefinition != "Single Class" &&
                        mode.detectionDefinition.detectionClassDefinition.confidenceDefinition != "Multiple Class")
                    {
                        return "SensorRegistration Invalid modeDefinition detectionDefinition detectionClassDefinition confidenceDefinition";
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// validate a Deserialized Sensor Registration Message Mode Task Definition
        /// </summary>
        /// <param name="mode">mode section of the Sensor Registration Message</param>
        /// <returns>empty string or validation error message</returns>
        private static string ValidateModeTaskDefinition(SensorRegistrationModeDefinition mode)
        {
            if (mode.taskDefinition.regionDefinition != null)
            {
                var region = mode.taskDefinition.regionDefinition;
                foreach (var rt in region.regionType)
                {
                    if(!ValidRegionType(rt.Value))
                    {
                        return "SensorRegistration Invalid modeDefinition taskDefinition regionDefinition regionType";
                    }
                }

                if(!ValidLocationType(region.locationType))
                {
                    return "SensorRegistration Invalid modeDefinition taskDefinition regionDefinition locationType";
                }
            }

            if (mode.taskDefinition.command != null)
            {
                foreach (var command in mode.taskDefinition.command)
                {
                    if(!ValidCommandName(command.name))
                    {
                        return "SensorRegistration Invalid modeDefinition taskDefinition command name";
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// validate a Deserialized Sensor Registration Message Heartbeat Definition
        /// </summary>
        /// <param name="registration">the Sensor Registration Message</param>
        /// <returns>empty string or validation error message</returns>
        private static string ValidateHeartbeatDefinition(SensorRegistration registration)
        {
            if (registration.heartbeatDefinition == null)
            {
                return "SensorRegistration Invalid heartbeatDefinition";
            }

            if (registration.heartbeatDefinition.fieldOfViewDefinition != null)
            {
                var fov = registration.heartbeatDefinition.fieldOfViewDefinition.locationType;
                if (!ValidLocationType(fov))
                {
                    return "SensorRegistration Invalid heartbeatDefinition fieldOfViewDefinition locationType";
                }
            }

            if (registration.heartbeatDefinition.obscurationDefinition != null)
            {
                var obs = registration.heartbeatDefinition.obscurationDefinition.locationType;
                if (!ValidLocationType(obs))
                {
                    return "SensorRegistration Invalid heartbeatDefinition obscurationDefinition locationType";
                }
            }

            if (registration.heartbeatDefinition.coverageDefinition != null)
            {
                var obs = registration.heartbeatDefinition.coverageDefinition.locationType;
                if (!ValidLocationType(obs))
                {
                    return "SensorRegistration Invalid heartbeatDefinition coverageDefinition locationType";
                }
            }

            if (registration.heartbeatDefinition.sensorLocationDefinition != null)
            {
                var obs = registration.heartbeatDefinition.sensorLocationDefinition.locationType;
                if (!ValidLocationType(obs))
                {
                    return "SensorRegistration Invalid heartbeatDefinition sensorLocationDefinition locationType";
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Whether this is a valid scan type
        /// </summary>
        /// <param name="tag">string to validate</param>
        /// <returns>true if valid</returns>
        private static bool ValidScanType(string tag)
        {
            bool retval = string.IsNullOrEmpty(tag); // valid to have empty scan type

            if (!retval)
            {
                switch (tag)
                {
                    case "Fixed":
                    case "Scanning":
                    case "Steerable":
                        retval = true;
                        break;
                }
            }

            return retval;
        }

        /// <summary>
        /// Whether this is a valid tracking type
        /// </summary>
        /// <param name="tag">string to validate</param>
        /// <returns>true if valid</returns>
        private static bool ValidTrackingType(string tag)
        {
            bool retval = string.IsNullOrEmpty(tag); // valid to have empty tracking type

            if (!retval)
            {
                switch (tag)
                {
                    case "None":
                    case "Tracklet":
                    case "Track":
                    case "TrackWithReID":
                        retval = true;
                        break;
                }
            }

            return retval;
        }

        /// <summary>
        /// Whether this is a valid region type
        /// </summary>
        /// <param name="tag">string to validate</param>
        /// <returns>true if valid</returns>
        private static bool ValidRegionType(string tag)
        {
            var retval = false;
            switch (tag.ToLowerInvariant())
            {
                case "area of interest":
                case "ignore":
                case "boundary":
                    retval = true;
                    break;
            }

            return retval;
        }

        /// <summary>
        /// Whether this is a valid location type
        /// </summary>
        /// <param name="locationType">location type to validate</param>
        /// <returns>true if valid</returns>
        private static bool ValidLocationType(locationType locationType)
        {
            bool retval = false;

            if (locationType != null)
            {         
                switch (locationType.Value)
                {
                    case "UTM":
                    case "GPS":
                    case "RangeBearing":
                        retval = true;
                        break;
                }
            }

            return retval;
        }

        /// <summary>
        /// Whether this is a valid command name
        /// </summary>
        /// <param name="tag">string to validate</param>
        /// <returns>true if valid</returns>
        private static bool ValidCommandName(string tag)
        {
            var retval = false;
            switch (tag)
            {
                case "Request":
                case "DetectionThreshold":
                case "DetectionReportRate":
                case "ClassificationThreshold":
                case "Mode":
                case "LookAt":
                case "LookFor":
                    retval = true;
                    break;
            }

            return retval;
        }
    }
}

namespace SapientServices.Data.Validation
{
    public class SensorTaskValidation
    {
        /// <summary>
        /// validate a Deserialized Sensor Task ACK Message
        /// </summary>
        /// <param name="sensor_task_ack">the Sensor Task ACK Message</param>
        /// <returns>empty string or validation error message</returns>
        public static string ValidateSensorTaskAck(SensorTaskACK sensor_task_ack)
        {
            string retval = string.Empty;
            if (!ValidApprovalStatus(sensor_task_ack.status))
            {
                retval = "SensorTaskACK Invalid Status";
            }

            return retval;
        }

        /// <summary>
        /// validate a Deserialized Sensor Task Message
        /// </summary>
        /// <param name="task">the Sensor Task Message</param>
        /// <returns>empty string or validation error message</returns>
        public static string ValidateTask(SensorTask task)
        {
            bool hasRegionOrCommand = HasRegionOrCommand(task);

            string errorMessage = ValidControlStatus(task.control, hasRegionOrCommand);

            // removed for Zodiac manual tasking July 20
            ////if (task.region != null && task.command != null)
            ////{
            ////    errorMessage = "SensorTask has both region and command";
            ////}

            if (errorMessage == string.Empty)
            {
                errorMessage = ValidateRegions(task);
            }

            if (errorMessage == string.Empty)
            {
                errorMessage = ValidateCommandTask(task);
            }

            return errorMessage;
        }

        /// <summary>
        /// Whether this is a valid control status
        /// </summary>
        /// <param name="tag">request string to validate</param>
        /// <param name="hasRegionOrCommand">whether has region or command sections</param>
        /// <returns>empty string if valid otherwise error message</returns>
        private static string ValidControlStatus(string tag, bool hasRegionOrCommand)
        {
            string errorMessage = string.Empty;
            switch (tag)
            {
                case "Stop":
                case "Pause":
                    if (hasRegionOrCommand)
                    {
                        errorMessage = "SensorTask Stop/Pause task should not have a region or command";
                    }
                    break;
                case "Start":
                case "Default":
                    // Start or Default control should have either a region or a command or a rule
                    if (!hasRegionOrCommand)
                    {
                        errorMessage = "SensorTask has neither region nor command";
                    }
                    break;
                default:
                    errorMessage = "SensorTask Invalid control";
                    break;
            }

            return errorMessage;
        }

        /// <summary>
        /// Whether this is a valid request command
        /// </summary>
        /// <param name="tag">request string to validate</param>
        /// <returns>true if valid</returns>
        public static bool ValidRequest(string tag)
        {
            var retval = false;
            switch (tag)
            {
                case "Registration":
                case "Heartbeat":
                case "Reset":
                case "Sensor Time":
                case "Stop":
                case "Start":
                case "Take Snapshot":
                case "Follow Track":
                case "Stop All":
                case "Home":
                case "Cancel Look At":
                case "Take Control":
                case "Release Control":
                case "Get Route Plan":
                    retval = true;
                    break;
            }

            // non ICD messages supported by HLDMM simulator
            if (!retval)
            {
                retval = ValidAuxiliaryRequest(tag);
            }

            return retval;
        }

        /// <summary>
        /// Whether this is a valid request command (not included in ICD)
        /// </summary>
        /// <param name="tag">request string to validate</param>
        /// <returns>true if valid</returns>
        private static bool ValidAuxiliaryRequest(string tag)
        {
            // non ICD messages supported by HLDMM simulator
            if (tag.Contains("Follow Track") || tag.Contains("Start Video") || tag.Contains("Stop Video") || tag.Contains("Play Video"))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Whether this task has a region or command section
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        private static bool HasRegionOrCommand(SensorTask task)
        {
            return task.region != null || task.command != null;
        }

        /// <summary>
        /// validate a Deserialized Sensor Task Message Region section
        /// </summary>
        /// <param name="task">the Sensor Task Message</param>
        /// <returns>empty string or validation error message</returns>
        private static string ValidateRegions(SensorTask task)
        {
            if (task.region != null)
            {
                var regions = task.region;
                foreach (var region in regions)
                {
                    string errorMessage = ValidateRegion(region);
                    if (errorMessage != string.Empty)
                    {
                        return errorMessage;
                    }
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// validate a region section of a task message
        /// </summary>
        /// <param name="region">the Region object</param>
        /// <returns>empty string or validation error message</returns>
        private static string ValidateRegion(SensorTaskRegion region)
        {
            if (!ValidRegionType(region.type))
            {
                return "SensorTask Invalid region type";
            }

            if (region.locationList == null && region.rangeBearingCone == null)
            {
                return "SensorTask Invalid region has neither locationList nor rangeBearingCone";
            }

            if (region.locationList != null && (region.locationList.location == null || region.locationList.location.Length < 1))
            {
                return "SensorTask Invalid region locationList has no coordinates";
            }

            if (region.locationList != null && region.rangeBearingCone != null)
            {
                return "SensorTask Invalid region has both locationList and rangeBearingCone";
            }

            return string.Empty;
        }

        /// <summary>
        /// validate a Deserialized Sensor Task Message Command section
        /// </summary>
        /// <param name="task">the Sensor Task Message</param>
        /// <returns>empty string or validation error message</returns>
        private static string ValidateCommandTask(SensorTask task)
        {
            string errorMessage = string.Empty;

            if (task.command != null)
            {
                int commandCount = 0;

                errorMessage = ValidateRequestCommandTask(task, ref commandCount);

                if (errorMessage == string.Empty)
                {
                    errorMessage = ValidateThresholdCommandTask(task, ref commandCount);
                }

                // Removed constraint on mode text
                if (string.IsNullOrEmpty(task.command.mode) == false)
                {
                    ++commandCount;
                }

                if (errorMessage == string.Empty)
                {
                    errorMessage = ValidateLookAtCommandTask(task.command.lookAt, ref commandCount);
                }

                if (task.command.lookFor != null)
                {
                    ++commandCount;
                }

                if (errorMessage == string.Empty && commandCount == 0)
                {
                    errorMessage = "SensorTask Invalid command has no commands specified";
                }

                if (commandCount > 1)
                {
                    errorMessage = "SensorTask Invalid command has multiple commands specified";
                }
            }

            return errorMessage;
        }

        /// <summary>
        /// validate a Deserialized Sensor Task Message Request Command section
        /// </summary>
        /// <param name="task">the Sensor Task Message</param>
        /// <param name="commandCount">return number of commands</param>
        /// <returns>empty string or validation error message</returns>
        private static string ValidateRequestCommandTask(SensorTask task, ref int commandCount)
        {
            if (string.IsNullOrEmpty(task.command.request) == false)
            {
                // check for valid request string
                if (ValidRequest(task.command.request))
                {
                    ++commandCount;
                }
                else
                {
                    return "SensorTask Invalid command request";
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// validate a Deserialized Sensor Task Message Threshold Command section
        /// </summary>
        /// <param name="task">the Sensor Task Message</param>
        /// <param name="commandCount">return number of commands</param>
        /// <returns>empty string or validation error message</returns>
        private static string ValidateThresholdCommandTask(SensorTask task, ref int commandCount)
        {
            if (string.IsNullOrEmpty(task.command.detectionThreshold) == false)
            {
                if (ValidCommand(task.command.detectionThreshold))
                {
                    ++commandCount;
                }
                else
                {
                    return "SensorTask Invalid command detectionThreshold";
                }
            }

            if (string.IsNullOrEmpty(task.command.detectionReportRate) == false)
            {
                if (ValidCommand(task.command.detectionReportRate))
                {
                    ++commandCount;
                }
                else
                {
                    return "SensorTask Invalid command detectionReportRate";
                }
            }

            if (string.IsNullOrEmpty(task.command.classificationThreshold) == false)
            {
                if (ValidCommand(task.command.classificationThreshold))
                {
                    ++commandCount;
                }
                else
                {
                    return "SensorTask Invalid command classificationThreshold";
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// validate a Deserialized Sensor Task Message LookAt Command section
        /// </summary>
        /// <param name="lookAt">the Sensor Task Message Look At command section</param>
        /// <param name="commandCount">return number of commands</param>
        /// <returns>empty string or validation error message</returns>
        private static string ValidateLookAtCommandTask(SensorTaskCommandLookAt lookAt, ref int commandCount)
        {
            if (lookAt != null)
            {
                if (lookAt.locationList == null && lookAt.rangeBearingCone == null)
                {
                    return "SensorTask Invalid command lookAt neither location nor RangeBearing specified";
                }

                if (lookAt.locationList != null && lookAt.rangeBearingCone != null)
                {
                    return "SensorTask Invalid command lookAt both location and RangeBearing specified";
                }

                if (lookAt.locationList != null && (lookAt.locationList.location == null || lookAt.locationList.location.Length < 1))
                {
                    return "SensorTask Invalid region lookAt locationList has no coordinates";
                }

                ++commandCount;
            }

            return string.Empty;
        }

        /// <summary>
        /// validate a command threshold string
        /// </summary>
        /// <param name="threshold">threshold to be validated</param>
        /// <returns>true if valid</returns>
        private static bool ValidCommand(string threshold)
        {
            var retval = false;
            switch (threshold.ToLowerInvariant())
            {
                case "auto":
                case "low":
                case "medium":
                case "high":
                case "lower":
                case "higher":
                    retval = true;
                    break;
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
                case "delete":
                case "priority":
                    retval = true;
                    break;
            }

            return retval;
        }

        /// <summary>
        /// Whether this is a valid request command
        /// </summary>
        /// <param name="tag">request string to validate</param>
        /// <returns>true if valid</returns>
        private static bool ValidApprovalStatus(string tag)
        {
            var retval = false;
            switch (tag)
            {
                case "Accepted":
                case "Approved":
                case "Rejected":
                case "Complete":
                    retval = true;
                    break;
            }

            return retval;
        }
    }
}

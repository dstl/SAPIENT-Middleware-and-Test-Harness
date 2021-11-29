namespace SapientServices.Data.Validation
{
    public class AlertValidation
    {
        /// <summary>
        /// validate a Deserialized Alert Message
        /// </summary>
        /// <param name="alert">the Alert Message</param>
        /// <returns>empty string or validation error message</returns>
        public static string ValidateAlert(Alert alert)
        {
            string retval = string.Empty;
            if (string.IsNullOrEmpty(alert.status) == false)
            {
                if (!ValidAlertStatus(alert.status))
                {
                    retval = "Alert Invalid status";
                }
            }

            if (alert.location != null && alert.rangeBearing != null)
            {
                retval = "Alert Invalid has both location and rangeBearing";
            }

            return retval;
        }

        /// <summary>
        /// validate a Deserialized Alert Response Message
        /// </summary>
        /// <param name="response">the Alert Response Message</param>
        /// <returns>empty string or validation error message</returns>
        public static string ValidateAlertResponse(AlertResponse response)
        {
            string retval = string.Empty;
            if (!ValidAlertStatus(response.status))
            {
                retval = "AlertResponse Invalid status";
            }

            return retval;
        }

        /// <summary>
        /// Whether this is a valid request command
        /// </summary>
        /// <param name="tag">request string to validate</param>
        /// <returns>true if valid</returns>
        private static bool ValidAlertStatus(string tag)
        {
            var retval = false;
            switch (tag)
            {
                case "Active":
                case "Acknowledge":
                case "Reject":
                case "Ignore":
                case "Clear":
                    retval = true;
                    break;
            }

            return retval;
        }
    }
}

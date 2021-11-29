namespace SapientServices.Data.Validation
{
    public class DetectionValidation
    {
        /// <summary>
        /// validate a Deserialized Detection Report Message
        /// </summary>
        /// <param name="detection_report">the Detection Report Message</param>
        /// <returns>empty string or validation error message</returns>
        public static string ValidateDetection(DetectionReport detection_report)
        {
            // if detection is not lost then it needs either a location or a range-bearing
            if (detection_report.location == null && detection_report.rangeBearing == null)
            {
                return "DetectionReport Invalid has neither location nor rangeBearing";
            }

            return ValidatePredictedLocation(detection_report);
        }

        /// <summary>
        /// validate Predicted Location element of detection message
        /// </summary>
        /// <param name="detection_report">the Detection Report Message</param>
        /// <returns>empty string or validation error message</returns>
        private static string ValidatePredictedLocation(DetectionReport detection_report)
        {
            // if predictedLocation is present then it needs either a location or a range-bearing
            if (detection_report.predictedLocation != null)
            {
                if (detection_report.predictedLocation.location == null && detection_report.predictedLocation.rangeBearing == null)
                {
                    return "DetectionReport Invalid predicted Location has neither Location nor rangeBearing";
                }

                if (detection_report.predictedLocation.location != null && detection_report.predictedLocation.rangeBearing != null)
                {
                    return "DetectionReport Invalid predictedLocation has both location and rangeBearing";
                }
            }

            return string.Empty;
        }
    }
}

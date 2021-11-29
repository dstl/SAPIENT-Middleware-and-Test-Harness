// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: SapientMessageValidator.cs$
// Crown-Owned Copyright (c) supplied by QinetiQ. See Release/Supply Conditions
// $NoKeywords$

namespace SapientMiddleware
{
    using System;
    using log4net;
    using SapientServices;
    using SapientServices.Data;
    using SapientServices.Data.Validation;

    public class SapientMessageValidator
    {
        private static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Parse SAPIENT Registration Message and return success or otherwise.
        /// </summary>
        /// <param name="message">XML message string.</param>
        /// <param name="registration">parsed message object.</param>
        /// <param name="errorString">error message text.</param>
        /// <returns>Parsed message type or failure mode for further action.</returns>
        public static SapientMessageType ParseRegistration(string message, out SensorRegistration registration, out string errorString)
        {
            errorString = string.Empty;
            registration = null;

            try
            {
                var schema_error = ConfigXMLParser.GetSchemaErrors(message, ConfigXMLParser.RegistrationXmlSchemaSet);
                if (schema_error == string.Empty)
                {
                    SapientMessageType output_error;
                    var deserialize_error = string.Empty;
                    registration = (SensorRegistration)SapientProtocol.Deserialize(ConfigXMLParser.SensorRegistration, message, ref deserialize_error);

                    if (registration != null)
                    {
                        var validate = SensorRegistrationValidation.ValidateRegistration(registration);
                        if (validate == string.Empty)
                        {
                            output_error = SapientMessageType.Registration;
                        }
                        else
                        {
                            errorString = "Validation: " + validate;
                            output_error = SapientMessageType.InvalidClient;
                        }
                    }
                    else
                    {
                        errorString = "Deserialize: " + deserialize_error;
                        output_error = SapientMessageType.InvalidClient;
                    }

                    return output_error;
                }

                errorString = "Schema: " + schema_error;
                return SapientMessageType.InvalidClient;
            }
            catch (Exception e)
            {
                errorString = "Internal:SensorRegistration:" + e.Message;
                return SapientMessageType.InternalError;
            }
        }

        /// <summary>
        /// Parse SAPIENT Detection Message and return success or otherwise.
        /// </summary>
        /// <param name="message">XML message string.</param>
        /// <param name="detection_report">parsed message object.</param>
        /// <param name="errorString">error message text.</param>
        /// <returns>Parsed message type or failure mode for further action.</returns>
        public static SapientMessageType ParseDetection(string message, out DetectionReport detection_report, out string errorString)
        {
            errorString = string.Empty;
            detection_report = null;
            try
            {
                var schema_error = ConfigXMLParser.GetSchemaErrors(message, ConfigXMLParser.DetectionXmlSchemaSet);
                if (schema_error == string.Empty)
                {
                    var deserialize_error = string.Empty;
                    detection_report = (DetectionReport)SapientProtocol.Deserialize(ConfigXMLParser.DetectionReport, message, ref deserialize_error);
                    if (detection_report != null)
                    {
                        var validate = DetectionValidation.ValidateDetection(detection_report);
                        if (validate == string.Empty)
                        {
                            return SapientMessageType.Detection;
                        }

                        errorString = "Validation: " + validate;
                        return SapientMessageType.InvalidClient;
                    }

                    errorString = "Deserialize: " + deserialize_error;
                    return SapientMessageType.InvalidClient;
                }

                errorString = "Schema: " + schema_error;
                return SapientMessageType.InvalidClient;
            }
            catch (Exception e)
            {
                Log.Error("Exception parsing Detection message", e);
                errorString = "Internal:DetectionReport:" + e.Message;
                return SapientMessageType.InternalError;
            }
        }

        /// <summary>
        /// Parse SAPIENT Status Report Message and return success or otherwise.
        /// </summary>
        /// <param name="message">XML message string.</param>
        /// <param name="status_report">parsed message object.</param>
        /// <param name="errorString">error message text.</param>
        /// <returns>Parsed message type or failure mode for further action.</returns>
        public static SapientMessageType ParseStatusReport(string message, out StatusReport status_report, out string errorString)
        {
            errorString = string.Empty;
            status_report = null;
            try
            {
                var schema_error = ConfigXMLParser.GetSchemaErrors(message, ConfigXMLParser.StatusXmlSchemaSet);

                if (schema_error == string.Empty)
                {
                    var deserialize_error = string.Empty;
                    status_report = (StatusReport)SapientProtocol.Deserialize(ConfigXMLParser.StatusReport, message, ref deserialize_error);

                    if (status_report != null)
                    {
                        var validate = StatusReportValidation.ValidateStatusReport(status_report);

                        if (validate == string.Empty)
                        {
                            return SapientMessageType.Status;
                        }

                        errorString = "Validation: " + validate;
                        return SapientMessageType.InvalidClient;
                    }

                    errorString = "Deserialize: " + deserialize_error;
                    return SapientMessageType.InvalidClient;
                }

                errorString = "Schema: " + schema_error;
                return SapientMessageType.InvalidClient;
            }
            catch (Exception e)
            {
                Log.Error("Exception parsing Status message", e);
                errorString = "Internal:StatusReport:" + e.Message;
                return SapientMessageType.InternalError;
            }
        }

        /// <summary>
        /// Parse SAPIENT Alert Message and return success or otherwise.
        /// </summary>
        /// <param name="message">XML message string.</param>
        /// <param name="alert">parsed message object.</param>
        /// <param name="errorString">error message text.</param>
        /// <returns>Parsed message type or failure mode for further action.</returns>
        public static SapientMessageType ParseAlert(string message, out Alert alert, out string errorString)
        {
            errorString = string.Empty;
            alert = null;
            try
            {
                var schema_error = ConfigXMLParser.GetSchemaErrors(message, ConfigXMLParser.AlertXmlSchemaSet);

                if (schema_error == string.Empty)
                {
                    var deserialize_error = string.Empty;
                    alert = (Alert)SapientProtocol.Deserialize(ConfigXMLParser.Alert, message, ref deserialize_error);

                    if (alert != null)
                    {
                        var validate = AlertValidation.ValidateAlert(alert);

                        if (validate == string.Empty)
                        {
                            return SapientMessageType.Alert;
                        }

                        errorString = "Validation: " + validate;
                        return SapientMessageType.InvalidClient;
                    }

                    errorString = "Deserialize: " + deserialize_error;
                    return SapientMessageType.InvalidClient;
                }

                errorString = "Schema: " + schema_error;
                return SapientMessageType.InvalidClient;
            }
            catch (Exception e)
            {
                Log.Error("Exception parsing Alert message", e);
                errorString = "Internal:Alert:" + e.Message;
                return SapientMessageType.InternalError;
            }
        }

        /// <summary>
        /// Parse SAPIENT SensorTask Message and return success or otherwise.
        /// </summary>
        /// <param name="message">XML message string.</param>
        /// <param name="task">parsed message object.</param>
        /// <param name="errorString">error message text.</param>
        /// <returns>Parsed message type or failure mode for further action.</returns>
        public static SapientMessageType ParseSensorTask(string message, out SensorTask task, out string errorString)
        {
            errorString = string.Empty;
            task = null;
            try
            {
                var schema_error = ConfigXMLParser.GetSchemaErrors(message, ConfigXMLParser.TaskXmlSchemaSet);

                if (schema_error == string.Empty)
                {
                    var deserialize_error = string.Empty;
                    task = (SensorTask)SapientProtocol.Deserialize(ConfigXMLParser.SensorTask, message, ref deserialize_error);

                    if (task != null)
                    {
                        var validate = SensorTaskValidation.ValidateTask(task);

                        if (validate == string.Empty)
                        {
                            return SapientMessageType.Task;
                        }
                        else
                        {
                            errorString = "Validation: " + validate;
                            return SapientMessageType.InvalidTasking;
                        }
                    }
                    else
                    {
                        errorString = "Deserialize: " + deserialize_error;
                        return SapientMessageType.InvalidTasking;
                    }
                }
                else
                {
                    errorString = "Schema: " + schema_error;
                    return SapientMessageType.InvalidTasking;
                }
            }
            catch (Exception e)
            {
                Log.Error("Exception parsing SensorTask message", e);
                errorString = "Internal:SensorTask:" + e.Message;
                return SapientMessageType.InternalError;
            }
        }

        /// <summary>
        /// Parse SAPIENT Task acknowledgement Message and return success or otherwise.
        /// </summary>
        /// <param name="message">XML message string.</param>
        /// <param name="sensor_task_ack">parsed message object.</param>
        /// <param name="errorString">error message text.</param>
        /// <returns>Parsed message type or failure mode for further action.</returns>
        public static SapientMessageType ParseSensorTaskAck(string message, out SensorTaskACK sensor_task_ack, out string errorString)
        {
            errorString = string.Empty;
            sensor_task_ack = null;
            try
            {
                var schema_error = ConfigXMLParser.GetSchemaErrors(message, ConfigXMLParser.TaskAckXmlSchemaSet);
                if (schema_error == string.Empty)
                {
                    var deserialize_error = string.Empty;
                    sensor_task_ack = (SensorTaskACK)SapientProtocol.Deserialize(ConfigXMLParser.SensorTaskACK, message, ref deserialize_error);
                    if (sensor_task_ack != null)
                    {
                        var validate = SensorTaskValidation.ValidateSensorTaskAck(sensor_task_ack);
                        if (validate == string.Empty)
                        {
                            return SapientMessageType.TaskACK;
                        }

                        errorString = "Validation: " + validate;
                        return SapientMessageType.InvalidClient;
                    }

                    errorString = "Deserialize: " + deserialize_error;
                    return SapientMessageType.InvalidClient;
                }

                errorString = "Schema: " + schema_error;
                return SapientMessageType.InvalidClient;
            }
            catch (Exception e)
            {
                Log.Error("Exception parsing SensorTaskACK message", e);
                errorString = "Internal:SensorTaskACK:" + e.Message;
                return SapientMessageType.InternalError;
            }
        }

        public static SapientMessageType ParseApproval(string message, out Approval approval, out string errorString)
        {
            errorString = string.Empty;
            approval = null;
            try
            {
                var schema_error = ConfigXMLParser.GetSchemaErrors(message, ConfigXMLParser.ApprovalXmlSchemaSet);
                if (schema_error == string.Empty)
                {
                    var deserialize_error = string.Empty;
                    approval = (Approval)SapientProtocol.Deserialize(ConfigXMLParser.Approval, message, ref deserialize_error);
                    if (approval != null)
                    {
                        var validate = ApprovalValidation.ValidateApproval(approval);
                        if (validate == string.Empty)
                        {
                            return SapientMessageType.Approval;
                        }

                        errorString = "Validation: " + validate;
                        return SapientMessageType.InvalidClient;
                    }

                    errorString = "Deserialize: " + deserialize_error;
                    return SapientMessageType.InvalidClient;
                }

                errorString = "Schema: " + schema_error;
                return SapientMessageType.InvalidClient;
            }
            catch (Exception e)
            {
                Log.Error("Exception parsing Approval message", e);
                errorString = "Internal:Approval:" + e.Message;
                return SapientMessageType.InternalError;
            }

        }

        /// <summary>
        /// Parse SAPIENT Alert Response Message and return success or otherwise.
        /// </summary>
        /// <param name="message">XML message string.</param>
        /// <param name="alertResponse">parsed message object.</param>
        /// <param name="errorString">error message text.</param>
        /// <returns>Parsed message type or failure mode for further action.</returns>
        public static SapientMessageType ParseAlertResponse(string message, out AlertResponse alertResponse, out string errorString)
        {
            errorString = string.Empty;
            alertResponse = null;
            try
            {
                var schema_error = ConfigXMLParser.GetSchemaErrors(message, ConfigXMLParser.AlertResponseXmlSchemaSet);
                if (schema_error == string.Empty)
                {
                    var deserialize_error = string.Empty;
                    alertResponse = (AlertResponse)SapientProtocol.Deserialize(ConfigXMLParser.AlertResponse, message, ref deserialize_error);
                    if (alertResponse != null)
                    {
                        var validate = AlertValidation.ValidateAlertResponse(alertResponse);
                        if (validate == string.Empty)
                        {
                            return SapientMessageType.AlertResponse;
                        }
                        else
                        {
                            errorString = "Validation: " + validate;
                            return SapientMessageType.InvalidTasking;
                        }
                    }
                    else
                    {
                        errorString = "Deserialize: " + deserialize_error;
                        return SapientMessageType.InvalidTasking;
                    }
                }
                else
                {
                    errorString = "Schema: " + schema_error;
                    return SapientMessageType.InvalidTasking;
                }
            }
            catch (Exception e)
            {
                Log.Error("Exception parsing AlertResponse message", e);
                errorString = "Internal:AlertResponse: " + e.Message;
                return SapientMessageType.InternalError;
            }
        }

        /// <summary>
        /// Parse SAPIENT Route Plan Message and return success or otherwise.
        /// </summary>
        /// <param name="message">XML message string.</param>
        /// <param name="routePlan">parsed message object.</param>
        /// <param name="errorString">error message text.</param>
        /// <returns>Parsed message type or failure mode for further action.</returns>
        public static SapientMessageType ParseRoutePlan(string message, out RoutePlan routePlan, out string errorString)
        {
            errorString = string.Empty;
            routePlan = null;
            try
            {
                var schema_error = ConfigXMLParser.GetSchemaErrors(message, ConfigXMLParser.RoutePlanXmlSchemaSet);
                if (schema_error == string.Empty)
                {
                    var deserialize_error = string.Empty;
                    routePlan = (RoutePlan)SapientProtocol.Deserialize(ConfigXMLParser.RoutePlan, message, ref deserialize_error);
                    if (routePlan != null)
                    {
                        var validate = ConfigXMLParser.ValidateRoutePlan(routePlan);
                        if (validate == string.Empty)
                        {
                            return SapientMessageType.RoutePlan;
                        }
                        else
                        {
                            errorString = "Validation: " + validate;
                            return SapientMessageType.InvalidClient;
                        }
                    }
                    else
                    {
                        errorString = "Deserialize: " + deserialize_error;
                        return SapientMessageType.InvalidClient;
                    }
                }
                else
                {
                    errorString = "Schema: " + schema_error;
                    return SapientMessageType.InvalidClient;
                }
            }
            catch (Exception e)
            {
                Log.Error("Exception parsing RoutePlan message", e);
                errorString = "Internal:RoutePlan: " + e.Message;
                return SapientMessageType.InternalError;
            }
        }

        /// <summary>
        /// Parse SAPIENT Objective Message and return success or otherwise.
        /// </summary>
        /// <param name="message">XML message string.</param>
        /// <param name="objective">parsed message object.</param>
        /// <param name="errorString">error message text.</param>
        /// <returns>Parsed message type or failure mode for further action.</returns>
        public static SapientMessageType ParseObjective(string message, out Objective objective, out string errorString)
        {
            errorString = string.Empty;
            objective = null;
            try
            {
                var schema_error = ConfigXMLParser.GetSchemaErrors(message, ConfigXMLParser.ObjectiveXmlSchemaSet);
                if (schema_error == string.Empty)
                {
                    var deserialize_error = string.Empty;
                    objective = (Objective)SapientProtocol.Deserialize(ConfigXMLParser.Objective, message, ref deserialize_error);
                    if (objective != null)
                    {
                        var validate = ConfigXMLParser.ValidateObjective(objective);
                        if (validate == string.Empty)
                        {
                            return SapientMessageType.Objective;
                        }
                        else
                        {
                            errorString = "Validation: " + validate;
                            return SapientMessageType.InvalidTasking;
                        }
                    }
                    else
                    {
                        errorString = "Deserialize: " + deserialize_error;
                        return SapientMessageType.InvalidTasking;
                    }
                }
                else
                {
                    errorString = "Schema: " + schema_error;
                    return SapientMessageType.InvalidTasking;
                }
            }
            catch (Exception e)
            {
                Log.Error("Exception parsing Objective message", e);
                errorString = "Internal:Objective: " + e.Message;
                return SapientMessageType.InternalError;
            }
        }
    }
}

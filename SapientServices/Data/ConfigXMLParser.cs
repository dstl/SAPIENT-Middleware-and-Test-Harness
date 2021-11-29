// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: ConfigXMLParser.cs$
// Crown-Owned Copyright (c) supplied by QinetiQ. See Release/Supply Conditions
namespace SapientServices.Data
{
    using System;
    using System.IO;
    using System.Text;
    using System.Xml.Linq;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    /// <summary>
    /// Class for converting to and from xml
    /// </summary>
    public static class ConfigXMLParser
    {
        public static XmlSchemaSet StatusXmlSchemaSet = new XmlSchemaSet();
        public static XmlSchemaSet RegistrationXmlSchemaSet = new XmlSchemaSet();
        public static XmlSchemaSet TaskXmlSchemaSet = new XmlSchemaSet();
        public static XmlSchemaSet TaskAckXmlSchemaSet = new XmlSchemaSet();
        public static XmlSchemaSet RegistrationAckXmlSchemaSet = new XmlSchemaSet();
        public static XmlSchemaSet DetectionXmlSchemaSet = new XmlSchemaSet();
        public static XmlSchemaSet AlertXmlSchemaSet = new XmlSchemaSet();
        public static XmlSchemaSet AlertResponseXmlSchemaSet = new XmlSchemaSet();
        public static XmlSchemaSet ObjectiveXmlSchemaSet = new XmlSchemaSet();
        public static XmlSchemaSet RoutePlanXmlSchemaSet = new XmlSchemaSet();
        public static XmlSchemaSet ApprovalXmlSchemaSet = new XmlSchemaSet();

        public static XmlSerializer SensorRegistration;
        public static XmlSerializer SensorRegistrationACK;
        public static XmlSerializer SensorTask;
        public static XmlSerializer SensorTaskACK;
        public static XmlSerializer StatusReport;
        public static XmlSerializer DetectionReport;
        public static XmlSerializer Error;
        public static XmlSerializer Alert;
        public static XmlSerializer AlertResponse;
        public static XmlSerializer Objective;
        public static XmlSerializer RoutePlan;
        public static XmlSerializer Approval;

        /// <summary>
        /// Initial schemas for all message types
        /// </summary>
        /// <param name="schema_path">path to directory containing XSD files</param>
        public static void InitSchemas(string schema_path)
        {
            StatusXmlSchemaSet.Add(string.Empty, schema_path + @"\locationV3.xsd");
            StatusXmlSchemaSet.Add(string.Empty, schema_path + @"\heartbeatV3.xsd");

            TaskXmlSchemaSet.Add(string.Empty, schema_path + @"\locationV3.xsd");
            TaskXmlSchemaSet.Add(string.Empty, schema_path + @"\TaskV3.xsd");

            TaskAckXmlSchemaSet.Add(string.Empty, schema_path + @"\SensorTaskACK.xsd");

            RegistrationXmlSchemaSet.Add(string.Empty, schema_path + @"\locationV3.xsd");
            RegistrationXmlSchemaSet.Add(string.Empty, schema_path + @"\registrationV3.xsd");
            RegistrationAckXmlSchemaSet.Add(string.Empty, schema_path + @"\SensorRegistrationACK.xsd");

            DetectionXmlSchemaSet.Add(string.Empty, schema_path + @"\locationV3.xsd");
            DetectionXmlSchemaSet.Add(string.Empty, schema_path + @"\detectionV3.xsd");

            AlertXmlSchemaSet.Add(string.Empty, schema_path + @"\locationV3.xsd");
            AlertXmlSchemaSet.Add(string.Empty, schema_path + @"\alert.xsd");

            AlertResponseXmlSchemaSet.Add(string.Empty, schema_path + @"\alertResponse.xsd");

            RoutePlanXmlSchemaSet.Add(string.Empty, schema_path + @"\routePlan.xsd");
            ObjectiveXmlSchemaSet.Add(string.Empty, schema_path + @"\objective.xsd");

            ApprovalXmlSchemaSet.Add(string.Empty, schema_path + @"\Approval.xsd");

            InitialiseSerializers();
        }

        /// <summary>
        /// Determine if xml message contains any schema errors
        /// </summary>
        /// <param name="message">xml message</param>
        /// <param name="xml_schema_set">the messages schema</param>
        /// <returns>string containing any schema errors</returns>
        public static string GetSchemaErrors(string message, XmlSchemaSet xml_schema_set)
        {
            var doc = XDocument.Parse(message);
            var errors = false;
            var validate1 = new StringBuilder();
            doc.Validate(
                xml_schema_set,
                (o, e) =>
                {
                    if (errors)
                    {
                        validate1.Append("\n");
                    }

                    validate1.Append(e.Message);
                    errors = true;
                });
            return validate1.ToString();
        }

        /// <summary>
        /// convert an class to a xml string
        /// </summary>
        /// <param name="object_to_serialize"> class to convert to xml string</param>
        /// <returns>xml string</returns>
        public static string Serialize(object object_to_serialize)
        {
            // Add an empty namespace and empty value
            var ns = new XmlSerializerNamespaces();
            ns.Add(string.Empty, string.Empty);

            var mem = new MemoryStream();
            var ser = new XmlSerializer(object_to_serialize.GetType());
            ser.Serialize(mem, object_to_serialize, ns);
            var utf8 = new UTF8Encoding();
            return utf8.GetString(mem.ToArray());
        }

        /// <summary>
        /// convert an xml string to an object
        /// </summary>
        /// <param name="type_to_deserialize"> type of object to convert to</param>
        /// <param name="xml_string">xml string of the target class type</param>
        /// <returns>deserialized object</returns>
        public static object Deserialize(Type type_to_deserialize, string xml_string)
        {
            var bytes = Encoding.UTF8.GetBytes(xml_string);
            var mem = new MemoryStream(bytes);
            var ser = new XmlSerializer(type_to_deserialize);
            return ser.Deserialize(mem);
        }

        /// <summary>
        /// Whether this is a valid ICD1400 tag
        /// </summary>
        /// <param name="tag">tag to validate</param>
        /// <returns>true if valid</returns>
        public static bool ValidTag(string tag)
        {
            var retval = false;
            switch (tag)
            {
                case "SensorRegistration":
                case "SensorRegistrationACK":
                case "StatusReport":
                case "DetectionReport":
                case "SensorTask":
                case "SensorTaskACK":
                case "Alert":
                case "AlertResponse":
                case "Objective":
                case "RoutePlan":
                case "Approval":
                    retval = true;
                    break;
            }

            return retval;
        }

        /// <summary>
        /// validate a Deserialized Route Plan Message
        /// </summary>
        /// <param name="routePlan">the Route Plan Message</param>
        /// <returns>empty string or validation error message</returns>
        public static string ValidateRoutePlan(RoutePlan routePlan)
        {
            return string.Empty;
        }

        /// <summary>
        /// validate a Deserialized Objective Message
        /// </summary>
        /// <param name="objective">the Objective Message</param>
        /// <returns>empty string or validation error message</returns>
        public static string ValidateObjective(Objective objective)
        {
            return string.Empty;
        }

        /// <summary>
        /// Initialise serialiser objects
        /// </summary>
        private static void InitialiseSerializers()
        {
            SensorRegistration = new XmlSerializer(typeof(SensorRegistration));
            SensorRegistrationACK = new XmlSerializer(typeof(SensorRegistrationACK));
            SensorTaskACK = new XmlSerializer(typeof(SensorTaskACK));
            StatusReport = new XmlSerializer(typeof(StatusReport));
            DetectionReport = new XmlSerializer(typeof(DetectionReport));
            SensorTask = new XmlSerializer(typeof(SensorTask));
            Error = new XmlSerializer(typeof(Error));
            Alert = new XmlSerializer(typeof(Alert));
            AlertResponse = new XmlSerializer(typeof(AlertResponse));
            Objective = new XmlSerializer(typeof(Objective));
            RoutePlan = new XmlSerializer(typeof(RoutePlan));
            Approval = new XmlSerializer(typeof(Approval));
        }
    }
}
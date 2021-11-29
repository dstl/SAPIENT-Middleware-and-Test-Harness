// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: DetectionConstants.cs$
// Crown-Owned Copyright (c) supplied by QinetiQ. See Release/Supply Conditions
// $NoKeywords$
namespace SapientDatabase.DatabaseTables
{
    /// <summary>
    /// Constants for detection report tables.
    /// </summary>
    public static class DetectionConstants
    {
        public static class DetectionReportClass
        {
            public const string Table = "detection_report_class_v3";
            public const string Seq = "detection_report_id_seq";
            public const string Pkey = "detection_report_class_pkey";
        }

        public static class DetectionReportAssocDetection
        {
            public const string Table = "detection_report_assoc_detection_v3";
            public const string DetectionIdSeq = DetectionReportClass.Seq;
            public const string Pkey = "detection_report_assoc_detection_pkey";
        }

        public static class DetectionReportAssocFile
        {
            public const string Table = "detection_report_assoc_file_v3";
            public const string DetectionIdSeq = DetectionReportClass.Seq;
            public const string Pkey = "detection_report_assoc_file_pkey";
        }

        public static class DetectionReportBehaviour
        {
            public const string Table = "detection_report_behaviour_v3";
            public const string DetectionIdSeq = DetectionReportClass.Seq;
            public const string Pkey = "detection_report_behaviour_pkey";
        }

        public static class DetectionReportLocation
        {
            public const string Table = "detection_report_location_v3";
            public const string DetectionIdSeq = DetectionReportClass.Seq;
            public const string Pkey = "detection_report_location_pkey";
        }

        public static class DetectionReportRangeBearing
        {
            public const string Table = "detection_report_range_bearing_v3";
            public const string DetectionIdSeq = DetectionReportClass.Seq;
            public const string Pkey = "detection_report_range_bearing_pkey";
        }

        public static class DetectionReportSignal
        {
            public const string Table = "detection_report_signal_v3";
            public const string DetectionIdSeq = DetectionReportClass.Seq;
            public const string Pkey = "detection_report_signal_pkey";
        }

        public static class DetectionReportSubclass
        {
            public const string Table = "detection_report_subclass_v3";
            public const string DetectionIdSeq = DetectionReportClass.Seq;
            public const string Pkey = "detection_report_subclass_pkey";
        }

        public static class DetectionReportTrackInfo
        {
            public const string Table = "detection_report_track_info_v3";
            public const string DetectionIdSeq = DetectionReportClass.Seq;
            public const string Pkey = "detection_report_track_info_pkey";
        }

        public static class HLDetectionReportClass
        {
            public const string Table = "hl_detection_report_class_v3";
            public const string Seq = "hl_detection_report_id_seq";
            public const string Pkey = "hl_detection_report_class_pkey";
        }

        public static class HLDetectionReportAssocDetection
        {
            public const string Table = "hl_detection_report_assoc_detection_v3";
            public const string DetectionIdSeq = HLDetectionReportClass.Seq;
            public const string Pkey = "hl_detection_report_assoc_detection_pkey";
        }

        public static class HLDetectionReportAssocFile
        {
            public const string Table = "hl_detection_report_assoc_file_v3";
            public const string DetectionIdSeq = HLDetectionReportClass.Seq;
            public const string Pkey = "hl_detection_report_assoc_file_pkey";
        }

        public static class HLDetectionReportBehaviour
        {
            public const string Table = "hl_detection_report_behaviour_v3";
            public const string DetectionIdSeq = HLDetectionReportClass.Seq;
            public const string Pkey = "hl_detection_report_behaviour_pkey";
        }

        public static class HLDetectionReportLocation
        {
            public const string Table = "hl_detection_report_location_v3";
            public const string DetectionIdSeq = HLDetectionReportClass.Seq;
            public const string Pkey = "hl_detection_report_location_pkey";
        }

        public static class HLDetectionReportRangeBearing
        {
            public const string Table = "hl_detection_report_range_bearing_v3";
            public const string DetectionIdSeq = HLDetectionReportClass.Seq;
            public const string Pkey = "hl_detection_report_range_bearing_pkey";
        }

        public static class HLDetectionReportSignal
        {
            public const string Table = "hl_detection_report_signal_v3";
            public const string DetectionIdSeq = HLDetectionReportClass.Seq;
            public const string Pkey = "hl_detection_report_signal_pkey";
        }

        public static class HLDetectionReportSubclass
        {
            public const string Table = "hl_detection_report_subclass_v3";
            public const string DetectionIdSeq = HLDetectionReportClass.Seq;
            public const string Pkey = "hl_detection_report_subclass_pkey";
        }

        public static class HLDetectionReportTrackInfo
        {
            public const string Table = "hl_detection_report_track_info_v3";
            public const string DetectionIdSeq = HLDetectionReportClass.Seq;
            public const string Pkey = "hl_detection_report_track_info_pkey";
        }
    }
}

// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: TaskForm.Designer.cs$
// Crown-Owned Copyright (c) supplied by QinetiQ. See Release/Supply Conditions

namespace SapientHldmmSimulator
{
    partial class TaskForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Send_task = new System.Windows.Forms.Button();
            this.Output_box = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.Sensor_input = new System.Windows.Forms.TextBox();
            this.Read_file = new System.Windows.Forms.Button();
            this.open_file_dialog = new System.Windows.Forms.OpenFileDialog();
            this.clear = new System.Windows.Forms.Button();
            this.openLogFile = new System.Windows.Forms.OpenFileDialog();
            this.send_detection = new System.Windows.Forms.Button();
            this.sendPTZTask = new System.Windows.Forms.Button();
            this.alertResponse = new System.Windows.Forms.Button();
            this.alertID = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.HLHeartbeatButton = new System.Windows.Forms.Button();
            this.HLAlertButton = new System.Windows.Forms.Button();
            this.fileScriptButton = new System.Windows.Forms.Button();
            this.loopTasksCheckBox = new System.Windows.Forms.CheckBox();
            this.loopDetectionCheckBox = new System.Windows.Forms.CheckBox();
            this.sendObjectiveButton = new System.Windows.Forms.Button();
            this.filenameTextBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.sendToHLDMMcheckBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // Send_task
            // 
            this.Send_task.Location = new System.Drawing.Point(7, 74);
            this.Send_task.Name = "Send_task";
            this.Send_task.Size = new System.Drawing.Size(106, 25);
            this.Send_task.TabIndex = 0;
            this.Send_task.Text = "Send Task";
            this.Send_task.UseVisualStyleBackColor = true;
            this.Send_task.Click += new System.EventHandler(this.SendTaskClick);
            // 
            // Output_box
            // 
            this.Output_box.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Output_box.Location = new System.Drawing.Point(151, 9);
            this.Output_box.Multiline = true;
            this.Output_box.Name = "Output_box";
            this.Output_box.Size = new System.Drawing.Size(338, 619);
            this.Output_box.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(25, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "SensorID:";
            // 
            // Sensor_input
            // 
            this.Sensor_input.Location = new System.Drawing.Point(82, 9);
            this.Sensor_input.Name = "Sensor_input";
            this.Sensor_input.Size = new System.Drawing.Size(31, 20);
            this.Sensor_input.TabIndex = 3;
            this.Sensor_input.Text = "16";
            // 
            // Read_file
            // 
            this.Read_file.Location = new System.Drawing.Point(7, 396);
            this.Read_file.Name = "Read_file";
            this.Read_file.Size = new System.Drawing.Size(106, 36);
            this.Read_file.TabIndex = 9;
            this.Read_file.Text = "Send File";
            this.Read_file.UseVisualStyleBackColor = true;
            this.Read_file.Click += new System.EventHandler(this.ReadFileClick);
            // 
            // open_file_dialog
            // 
            this.open_file_dialog.FileName = "openFileDialog1";
            // 
            // clear
            // 
            this.clear.Location = new System.Drawing.Point(7, 531);
            this.clear.Name = "clear";
            this.clear.Size = new System.Drawing.Size(106, 36);
            this.clear.TabIndex = 11;
            this.clear.Text = "Clear";
            this.clear.UseVisualStyleBackColor = true;
            this.clear.Click += new System.EventHandler(this.ClearClick);
            // 
            // openLogFile
            // 
            this.openLogFile.FileName = "*.log";
            // 
            // send_detection
            // 
            this.send_detection.Location = new System.Drawing.Point(7, 239);
            this.send_detection.Name = "send_detection";
            this.send_detection.Size = new System.Drawing.Size(106, 38);
            this.send_detection.TabIndex = 12;
            this.send_detection.Text = "Send Detection";
            this.send_detection.UseVisualStyleBackColor = true;
            this.send_detection.Click += new System.EventHandler(this.SendDetectionClick);
            // 
            // sendPTZTask
            // 
            this.sendPTZTask.Location = new System.Drawing.Point(7, 123);
            this.sendPTZTask.Name = "sendPTZTask";
            this.sendPTZTask.Size = new System.Drawing.Size(106, 38);
            this.sendPTZTask.TabIndex = 13;
            this.sendPTZTask.Text = "Send PTZ Task";
            this.sendPTZTask.UseVisualStyleBackColor = true;
            this.sendPTZTask.Click += new System.EventHandler(this.SendPTZTaskClick);
            // 
            // alertResponse
            // 
            this.alertResponse.Location = new System.Drawing.Point(7, 195);
            this.alertResponse.Name = "alertResponse";
            this.alertResponse.Size = new System.Drawing.Size(106, 38);
            this.alertResponse.TabIndex = 14;
            this.alertResponse.Text = "Send Alert Response";
            this.alertResponse.UseVisualStyleBackColor = true;
            this.alertResponse.Click += new System.EventHandler(this.AlertResponseClick);
            // 
            // alertID
            // 
            this.alertID.Location = new System.Drawing.Point(77, 169);
            this.alertID.Name = "alertID";
            this.alertID.Size = new System.Drawing.Size(31, 20);
            this.alertID.TabIndex = 16;
            this.alertID.Text = "0";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(20, 172);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(42, 13);
            this.label2.TabIndex = 15;
            this.label2.Text = "Alert ID";
            // 
            // HLHeartbeatButton
            // 
            this.HLHeartbeatButton.Location = new System.Drawing.Point(7, 351);
            this.HLHeartbeatButton.Name = "HLHeartbeatButton";
            this.HLHeartbeatButton.Size = new System.Drawing.Size(106, 38);
            this.HLHeartbeatButton.TabIndex = 17;
            this.HLHeartbeatButton.Text = "Send HL Status Report";
            this.HLHeartbeatButton.UseVisualStyleBackColor = true;
            this.HLHeartbeatButton.Click += new System.EventHandler(this.HLHeartbeatButton_Click);
            // 
            // HLAlertButton
            // 
            this.HLAlertButton.Location = new System.Drawing.Point(7, 306);
            this.HLAlertButton.Name = "HLAlertButton";
            this.HLAlertButton.Size = new System.Drawing.Size(106, 36);
            this.HLAlertButton.TabIndex = 18;
            this.HLAlertButton.Text = "Send HL Alert";
            this.HLAlertButton.UseVisualStyleBackColor = true;
            this.HLAlertButton.Click += new System.EventHandler(this.HLAlertButton_Click);
            // 
            // fileScriptButton
            // 
            this.fileScriptButton.Location = new System.Drawing.Point(7, 441);
            this.fileScriptButton.Name = "fileScriptButton";
            this.fileScriptButton.Size = new System.Drawing.Size(106, 36);
            this.fileScriptButton.TabIndex = 19;
            this.fileScriptButton.Text = "Send List of Files";
            this.fileScriptButton.UseVisualStyleBackColor = true;
            this.fileScriptButton.Click += new System.EventHandler(this.fileScriptButton_Click);
            // 
            // loopTasksCheckBox
            // 
            this.loopTasksCheckBox.AutoSize = true;
            this.loopTasksCheckBox.Location = new System.Drawing.Point(12, 105);
            this.loopTasksCheckBox.Name = "loopTasksCheckBox";
            this.loopTasksCheckBox.Size = new System.Drawing.Size(82, 17);
            this.loopTasksCheckBox.TabIndex = 22;
            this.loopTasksCheckBox.Text = "Loop Tasks";
            this.loopTasksCheckBox.UseVisualStyleBackColor = true;
            this.loopTasksCheckBox.CheckedChanged += new System.EventHandler(this.loopTasksCheckBox_CheckedChanged);
            // 
            // loopDetectionCheckBox
            // 
            this.loopDetectionCheckBox.AutoSize = true;
            this.loopDetectionCheckBox.Location = new System.Drawing.Point(12, 283);
            this.loopDetectionCheckBox.Name = "loopDetectionCheckBox";
            this.loopDetectionCheckBox.Size = new System.Drawing.Size(104, 17);
            this.loopDetectionCheckBox.TabIndex = 22;
            this.loopDetectionCheckBox.Text = "Loop Detections";
            this.loopDetectionCheckBox.UseVisualStyleBackColor = true;
            this.loopDetectionCheckBox.CheckedChanged += new System.EventHandler(this.loopDetectionCheckBox_CheckedChanged);
            // 
            // sendObjectiveButton
            // 
            this.sendObjectiveButton.Location = new System.Drawing.Point(7, 483);
            this.sendObjectiveButton.Name = "sendObjectiveButton";
            this.sendObjectiveButton.Size = new System.Drawing.Size(106, 36);
            this.sendObjectiveButton.TabIndex = 23;
            this.sendObjectiveButton.Text = "Send Objective";
            this.sendObjectiveButton.UseVisualStyleBackColor = true;
            this.sendObjectiveButton.Click += new System.EventHandler(this.sendObjectiveButton_Click);
            // 
            // filenameTextBox
            // 
            this.filenameTextBox.Location = new System.Drawing.Point(7, 608);
            this.filenameTextBox.Name = "filenameTextBox";
            this.filenameTextBox.Size = new System.Drawing.Size(127, 20);
            this.filenameTextBox.TabIndex = 28;
            this.filenameTextBox.Text = "testfile.jpg";
            this.filenameTextBox.TextChanged += new System.EventHandler(this.filenameTextBox_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(4, 592);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(107, 13);
            this.label6.TabIndex = 29;
            this.label6.Text = "Associated Filename:";
            // 
            // sendToHLDMMcheckBox
            // 
            this.sendToHLDMMcheckBox.AutoSize = true;
            this.sendToHLDMMcheckBox.Location = new System.Drawing.Point(12, 35);
            this.sendToHLDMMcheckBox.Name = "sendToHLDMMcheckBox";
            this.sendToHLDMMcheckBox.Size = new System.Drawing.Size(110, 17);
            this.sendToHLDMMcheckBox.TabIndex = 30;
            this.sendToHLDMMcheckBox.Text = "Send To HLDMM";
            this.sendToHLDMMcheckBox.UseVisualStyleBackColor = true;
            this.sendToHLDMMcheckBox.CheckedChanged += new System.EventHandler(this.sendToHLDMMcheckBox_CheckedChanged);
            // 
            // TaskForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(499, 640);
            this.Controls.Add(this.sendToHLDMMcheckBox);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.filenameTextBox);
            this.Controls.Add(this.sendObjectiveButton);
            this.Controls.Add(this.loopDetectionCheckBox);
            this.Controls.Add(this.loopTasksCheckBox);
            this.Controls.Add(this.fileScriptButton);
            this.Controls.Add(this.HLAlertButton);
            this.Controls.Add(this.HLHeartbeatButton);
            this.Controls.Add(this.alertID);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.alertResponse);
            this.Controls.Add(this.sendPTZTask);
            this.Controls.Add(this.send_detection);
            this.Controls.Add(this.clear);
            this.Controls.Add(this.Read_file);
            this.Controls.Add(this.Sensor_input);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Output_box);
            this.Controls.Add(this.Send_task);
            this.Name = "TaskForm";
            this.Text = "HLDMM Simulator";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.TaskFormFormClosed);
            this.Load += new System.EventHandler(this.TaskFormLoad);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Send_task;
        private System.Windows.Forms.TextBox Output_box;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox Sensor_input;
        private System.Windows.Forms.Button Read_file;
        private System.Windows.Forms.OpenFileDialog open_file_dialog;
        private System.Windows.Forms.Button clear;
        private System.Windows.Forms.OpenFileDialog openLogFile;
        private System.Windows.Forms.Button send_detection;
        private System.Windows.Forms.Button sendPTZTask;
        private System.Windows.Forms.Button alertResponse;
        private System.Windows.Forms.TextBox alertID;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button HLHeartbeatButton;
        private System.Windows.Forms.Button HLAlertButton;
        private System.Windows.Forms.Button fileScriptButton;
        private System.Windows.Forms.CheckBox loopTasksCheckBox;
        private System.Windows.Forms.CheckBox loopDetectionCheckBox;
        private System.Windows.Forms.Button sendObjectiveButton;
        public System.Windows.Forms.TextBox filenameTextBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox sendToHLDMMcheckBox;
    }
}
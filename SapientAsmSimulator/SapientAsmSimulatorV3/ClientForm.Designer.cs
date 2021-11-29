// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: ClientForm.Designer.cs$
// <copyright file="ClientForm.Designer.cs" company="QinetiQ">
// Crown-Owned Copyright (c) supplied by QinetiQ. See Release/Supply Conditions
// </copyright>

namespace SapientASMsimulator
{
    /// <summary>
    /// class to display the ASM Client Form
    /// </summary>
    partial class ClientForm
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
            this.components = new System.ComponentModel.Container();
            this.Send_Reg = new System.Windows.Forms.Button();
            this.Send_heart = new System.Windows.Forms.Button();
            this.Send_dec = new System.Windows.Forms.Button();
            this.OutputWindow = new System.Windows.Forms.TextBox();
            this.Read_file = new System.Windows.Forms.Button();
            this.LoopDetection = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.HeartbeatTime = new System.Windows.Forms.TextBox();
            this.Heartbeat = new System.Windows.Forms.CheckBox();
            this.openXmlFile = new System.Windows.Forms.OpenFileDialog();
            this.ClearButton = new System.Windows.Forms.Button();
            this.openLogFile = new System.Windows.Forms.OpenFileDialog();
            this.DetectionTime = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.ASMText = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.sendAlert = new System.Windows.Forms.Button();
            this.GUITimer = new System.Windows.Forms.Timer(this.components);
            this.detectionCountTextBox = new System.Windows.Forms.TextBox();
            this.detectionsSentLabel = new System.Windows.Forms.Label();
            this.heartbeatCountTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.LoopAlerts = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.alertCountTextBox = new System.Windows.Forms.TextBox();
            this.fileScriptButton = new System.Windows.Forms.Button();
            this.senedRoutePlanButton = new System.Windows.Forms.Button();
            this.filenameTextBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // Send_Reg
            // 
            this.Send_Reg.Location = new System.Drawing.Point(13, 25);
            this.Send_Reg.Name = "Send_Reg";
            this.Send_Reg.Size = new System.Drawing.Size(99, 36);
            this.Send_Reg.TabIndex = 0;
            this.Send_Reg.Text = "Send Registration";
            this.Send_Reg.UseVisualStyleBackColor = true;
            this.Send_Reg.Click += new System.EventHandler(this.SendRegistrationClick);
            // 
            // Send_heart
            // 
            this.Send_heart.Location = new System.Drawing.Point(12, 94);
            this.Send_heart.Name = "Send_heart";
            this.Send_heart.Size = new System.Drawing.Size(99, 36);
            this.Send_heart.TabIndex = 1;
            this.Send_heart.Text = "Send Heartbeat";
            this.Send_heart.UseVisualStyleBackColor = true;
            this.Send_heart.Click += new System.EventHandler(this.SendHeartbeatClick);
            // 
            // Send_dec
            // 
            this.Send_dec.Location = new System.Drawing.Point(12, 136);
            this.Send_dec.Name = "Send_dec";
            this.Send_dec.Size = new System.Drawing.Size(99, 36);
            this.Send_dec.TabIndex = 2;
            this.Send_dec.Text = "Send Detection";
            this.Send_dec.UseVisualStyleBackColor = true;
            this.Send_dec.Click += new System.EventHandler(this.SendDetectionClick);
            // 
            // OutputWindow
            // 
            this.OutputWindow.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.OutputWindow.Location = new System.Drawing.Point(118, 25);
            this.OutputWindow.Multiline = true;
            this.OutputWindow.Name = "OutputWindow";
            this.OutputWindow.Size = new System.Drawing.Size(319, 363);
            this.OutputWindow.TabIndex = 4;
            // 
            // Read_file
            // 
            this.Read_file.Location = new System.Drawing.Point(11, 220);
            this.Read_file.Name = "Read_file";
            this.Read_file.Size = new System.Drawing.Size(99, 36);
            this.Read_file.TabIndex = 7;
            this.Read_file.Text = "Send File";
            this.Read_file.UseVisualStyleBackColor = true;
            this.Read_file.Click += new System.EventHandler(this.ReadFileClick);
            // 
            // LoopDetection
            // 
            this.LoopDetection.AutoSize = true;
            this.LoopDetection.Location = new System.Drawing.Point(11, 431);
            this.LoopDetection.Name = "LoopDetection";
            this.LoopDetection.Size = new System.Drawing.Size(104, 17);
            this.LoopDetection.TabIndex = 8;
            this.LoopDetection.Text = "Loop Detections";
            this.LoopDetection.UseVisualStyleBackColor = true;
            this.LoopDetection.CheckedChanged += new System.EventHandler(this.LoopDetectionCheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(178, 397);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Interval (s)";
            // 
            // HeartbeatTime
            // 
            this.HeartbeatTime.Location = new System.Drawing.Point(132, 394);
            this.HeartbeatTime.Name = "HeartbeatTime";
            this.HeartbeatTime.Size = new System.Drawing.Size(42, 20);
            this.HeartbeatTime.TabIndex = 10;
            this.HeartbeatTime.Text = "5";
            this.HeartbeatTime.TextChanged += new System.EventHandler(this.HeartbeatTime_TextChanged);
            // 
            // Heartbeat
            // 
            this.Heartbeat.AutoSize = true;
            this.Heartbeat.Location = new System.Drawing.Point(11, 394);
            this.Heartbeat.Name = "Heartbeat";
            this.Heartbeat.Size = new System.Drawing.Size(115, 17);
            this.Heartbeat.TabIndex = 11;
            this.Heartbeat.Text = "Loop StatusReport";
            this.Heartbeat.UseVisualStyleBackColor = true;
            this.Heartbeat.CheckedChanged += new System.EventHandler(this.LoopHeartbeatCheckedChanged);
            // 
            // openXmlFile
            // 
            this.openXmlFile.FileName = "openFileDialog1";
            // 
            // ClearButton
            // 
            this.ClearButton.Location = new System.Drawing.Point(11, 346);
            this.ClearButton.Name = "ClearButton";
            this.ClearButton.Size = new System.Drawing.Size(99, 36);
            this.ClearButton.TabIndex = 13;
            this.ClearButton.Text = "Clear";
            this.ClearButton.UseVisualStyleBackColor = true;
            this.ClearButton.Click += new System.EventHandler(this.ClearClick);
            // 
            // openLogFile
            // 
            this.openLogFile.FileName = "openFileDialog2";
            // 
            // DetectionTime
            // 
            this.DetectionTime.Location = new System.Drawing.Point(132, 429);
            this.DetectionTime.Name = "DetectionTime";
            this.DetectionTime.Size = new System.Drawing.Size(42, 20);
            this.DetectionTime.TabIndex = 14;
            this.DetectionTime.Text = "100";
            this.DetectionTime.TextChanged += new System.EventHandler(this.DetectionTime_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(178, 430);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 13);
            this.label2.TabIndex = 15;
            this.label2.Text = "Interval  (ms)";
            // 
            // ASMText
            // 
            this.ASMText.Location = new System.Drawing.Point(73, 65);
            this.ASMText.Name = "ASMText";
            this.ASMText.Size = new System.Drawing.Size(39, 20);
            this.ASMText.TabIndex = 17;
            this.ASMText.TextChanged += new System.EventHandler(this.ASMText_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(19, 68);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(47, 13);
            this.label3.TabIndex = 16;
            this.label3.Text = "ASM ID:";
            // 
            // sendAlert
            // 
            this.sendAlert.Location = new System.Drawing.Point(11, 178);
            this.sendAlert.Name = "sendAlert";
            this.sendAlert.Size = new System.Drawing.Size(99, 36);
            this.sendAlert.TabIndex = 18;
            this.sendAlert.Text = "Send Alert";
            this.sendAlert.UseVisualStyleBackColor = true;
            this.sendAlert.Click += new System.EventHandler(this.SendAlertClick);
            // 
            // GUITimer
            // 
            this.GUITimer.Enabled = true;
            this.GUITimer.Tick += new System.EventHandler(this.GUITimer_Tick);
            // 
            // detectionCountTextBox
            // 
            this.detectionCountTextBox.Location = new System.Drawing.Point(343, 427);
            this.detectionCountTextBox.Name = "detectionCountTextBox";
            this.detectionCountTextBox.ReadOnly = true;
            this.detectionCountTextBox.Size = new System.Drawing.Size(100, 20);
            this.detectionCountTextBox.TabIndex = 18;
            // 
            // detectionsSentLabel
            // 
            this.detectionsSentLabel.AutoSize = true;
            this.detectionsSentLabel.Location = new System.Drawing.Point(251, 432);
            this.detectionsSentLabel.Name = "detectionsSentLabel";
            this.detectionsSentLabel.Size = new System.Drawing.Size(86, 13);
            this.detectionsSentLabel.TabIndex = 19;
            this.detectionsSentLabel.Text = "Detections Sent:";
            // 
            // heartbeatCountTextBox
            // 
            this.heartbeatCountTextBox.Location = new System.Drawing.Point(343, 397);
            this.heartbeatCountTextBox.Name = "heartbeatCountTextBox";
            this.heartbeatCountTextBox.ReadOnly = true;
            this.heartbeatCountTextBox.Size = new System.Drawing.Size(100, 20);
            this.heartbeatCountTextBox.TabIndex = 20;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(251, 400);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(87, 13);
            this.label4.TabIndex = 21;
            this.label4.Text = "Heartbeats Sent:";
            // 
            // LoopAlerts
            // 
            this.LoopAlerts.AutoSize = true;
            this.LoopAlerts.Location = new System.Drawing.Point(11, 462);
            this.LoopAlerts.Name = "LoopAlerts";
            this.LoopAlerts.Size = new System.Drawing.Size(79, 17);
            this.LoopAlerts.TabIndex = 22;
            this.LoopAlerts.Text = "Loop Alerts";
            this.LoopAlerts.UseVisualStyleBackColor = true;
            this.LoopAlerts.CheckedChanged += new System.EventHandler(this.LoopAlerts_CheckedChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(251, 464);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(61, 13);
            this.label5.TabIndex = 24;
            this.label5.Text = "Alerts Sent:";
            // 
            // alertCountTextBox
            // 
            this.alertCountTextBox.Location = new System.Drawing.Point(343, 459);
            this.alertCountTextBox.Name = "alertCountTextBox";
            this.alertCountTextBox.ReadOnly = true;
            this.alertCountTextBox.Size = new System.Drawing.Size(100, 20);
            this.alertCountTextBox.TabIndex = 23;
            // 
            // fileScriptButton
            // 
            this.fileScriptButton.Location = new System.Drawing.Point(11, 262);
            this.fileScriptButton.Name = "fileScriptButton";
            this.fileScriptButton.Size = new System.Drawing.Size(99, 36);
            this.fileScriptButton.TabIndex = 25;
            this.fileScriptButton.Text = "Send List of Files";
            this.fileScriptButton.UseVisualStyleBackColor = true;
            this.fileScriptButton.Click += new System.EventHandler(this.fileScriptButton_Click);
            // 
            // senedRoutePlanButton
            // 
            this.senedRoutePlanButton.Location = new System.Drawing.Point(13, 304);
            this.senedRoutePlanButton.Name = "senedRoutePlanButton";
            this.senedRoutePlanButton.Size = new System.Drawing.Size(99, 36);
            this.senedRoutePlanButton.TabIndex = 26;
            this.senedRoutePlanButton.Text = "Send Route Plan";
            this.senedRoutePlanButton.UseVisualStyleBackColor = true;
            this.senedRoutePlanButton.Click += new System.EventHandler(this.senedRoutePlanButton_Click);
            // 
            // filenameTextBox
            // 
            this.filenameTextBox.Location = new System.Drawing.Point(294, 485);
            this.filenameTextBox.Name = "filenameTextBox";
            this.filenameTextBox.Size = new System.Drawing.Size(149, 20);
            this.filenameTextBox.TabIndex = 27;
            this.filenameTextBox.Text = "testfile.jpg";
            this.filenameTextBox.TextChanged += new System.EventHandler(this.filenameTextBox_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(181, 485);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(107, 13);
            this.label6.TabIndex = 28;
            this.label6.Text = "Associated Filename:";
            // 
            // ClientForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(449, 526);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.filenameTextBox);
            this.Controls.Add(this.senedRoutePlanButton);
            this.Controls.Add(this.fileScriptButton);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.alertCountTextBox);
            this.Controls.Add(this.LoopAlerts);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.heartbeatCountTextBox);
            this.Controls.Add(this.detectionsSentLabel);
            this.Controls.Add(this.detectionCountTextBox);
            this.Controls.Add(this.sendAlert);
            this.Controls.Add(this.ASMText);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.DetectionTime);
            this.Controls.Add(this.ClearButton);
            this.Controls.Add(this.Heartbeat);
            this.Controls.Add(this.HeartbeatTime);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.LoopDetection);
            this.Controls.Add(this.Read_file);
            this.Controls.Add(this.OutputWindow);
            this.Controls.Add(this.Send_dec);
            this.Controls.Add(this.Send_heart);
            this.Controls.Add(this.Send_Reg);
            this.Name = "ClientForm";
            this.Text = "Sapient ASM Simulator";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ClientForm_FormClosed);
            this.Load += new System.EventHandler(this.Form1Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Send_Reg;
        private System.Windows.Forms.Button Send_heart;
        private System.Windows.Forms.Button Send_dec;
        private System.Windows.Forms.TextBox OutputWindow;
        private System.Windows.Forms.Button Read_file;
        private System.Windows.Forms.CheckBox LoopDetection;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox HeartbeatTime;
        private System.Windows.Forms.CheckBox Heartbeat;
        private System.Windows.Forms.OpenFileDialog openXmlFile;
        private System.Windows.Forms.Button ClearButton;
        private System.Windows.Forms.OpenFileDialog openLogFile;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.TextBox DetectionTime;
        public System.Windows.Forms.TextBox ASMText;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button sendAlert;
        private System.Windows.Forms.Timer GUITimer;
        private System.Windows.Forms.TextBox detectionCountTextBox;
        private System.Windows.Forms.Label detectionsSentLabel;
        private System.Windows.Forms.TextBox heartbeatCountTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox LoopAlerts;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox alertCountTextBox;
        private System.Windows.Forms.Button fileScriptButton;
        private System.Windows.Forms.Button senedRoutePlanButton;
        public System.Windows.Forms.TextBox filenameTextBox;
        private System.Windows.Forms.Label label6;
    }
}
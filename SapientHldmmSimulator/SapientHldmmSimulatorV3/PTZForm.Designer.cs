// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: PTZForm.Designer.cs$
// Crown-Owned Copyright (c) supplied by QinetiQ. See Release/Supply Conditions

namespace SapientHldmmSimulator
{
    partial class PTZForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.Azimuth_txt = new System.Windows.Forms.TextBox();
            this.Elevation_txt = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.command = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.SendAsPTZ = new System.Windows.Forms.CheckBox();
            this.Zoom_txt = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(22, 50);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Azimuth";
            // 
            // Azimuth_txt
            // 
            this.Azimuth_txt.Location = new System.Drawing.Point(173, 47);
            this.Azimuth_txt.Name = "Azimuth_txt";
            this.Azimuth_txt.Size = new System.Drawing.Size(100, 20);
            this.Azimuth_txt.TabIndex = 1;
            // 
            // Elevation_txt
            // 
            this.Elevation_txt.Location = new System.Drawing.Point(173, 73);
            this.Elevation_txt.Name = "Elevation_txt";
            this.Elevation_txt.Size = new System.Drawing.Size(100, 20);
            this.Elevation_txt.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(22, 76);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(51, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Elevation";
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Location = new System.Drawing.Point(198, 149);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 6;
            this.button1.Text = "OK";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.OnOk);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(12, 149);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 7;
            this.button2.Text = "Randomize";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.RandomizeClick);
            // 
            // command
            // 
            this.command.FormattingEnabled = true;
            this.command.Items.AddRange(new object[] {
            "lookAt",
            "request Registration",
            "request Reset",
            "request Heartbeat",
            "request Stop",
            "request Start",
            "request Take Snapshot",
            "request Start Video 0,1",
            "request Stop Video 0,1",
            "request Play Video 0,1",
            "request Follow Track 0,1",
            "detectionThreshold Low",
            "detectionThreshold Medium",
            "detectionThreshold High",
            "detectionReportRate Low",
            "detectionReportRate Medium",
            "detectionReportRate High",
            "mode Default",
            "mode Follow",
            "Take Control",
            "Release Control"});
            this.command.Location = new System.Drawing.Point(76, 12);
            this.command.Name = "command";
            this.command.Size = new System.Drawing.Size(197, 21);
            this.command.TabIndex = 9;
            this.command.Text = "lookAt";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(22, 15);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(54, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Command";
            // 
            // SendAsPTZ
            // 
            this.SendAsPTZ.AutoSize = true;
            this.SendAsPTZ.Location = new System.Drawing.Point(183, 126);
            this.SendAsPTZ.Name = "SendAsPTZ";
            this.SendAsPTZ.Size = new System.Drawing.Size(64, 17);
            this.SendAsPTZ.TabIndex = 11;
            this.SendAsPTZ.Text = "HExtent";
            this.SendAsPTZ.UseVisualStyleBackColor = true;
            // 
            // Zoom_txt
            // 
            this.Zoom_txt.Location = new System.Drawing.Point(173, 99);
            this.Zoom_txt.Name = "Zoom_txt";
            this.Zoom_txt.Size = new System.Drawing.Size(100, 20);
            this.Zoom_txt.TabIndex = 12;
            this.Zoom_txt.Text = "0";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(22, 106);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(85, 13);
            this.label4.TabIndex = 13;
            this.label4.Text = "Range/ HExtent";
            // 
            // PTZForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(285, 181);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.Zoom_txt);
            this.Controls.Add(this.SendAsPTZ);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.command);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.Elevation_txt);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.Azimuth_txt);
            this.Controls.Add(this.label1);
            this.Name = "PTZForm";
            this.Text = "Azimuth";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.TextBox Azimuth_txt;
        public System.Windows.Forms.TextBox Elevation_txt;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label3;
        public System.Windows.Forms.ComboBox command;
        public System.Windows.Forms.TextBox Zoom_txt;
        private System.Windows.Forms.Label label4;
        public System.Windows.Forms.CheckBox SendAsPTZ;
    }
}
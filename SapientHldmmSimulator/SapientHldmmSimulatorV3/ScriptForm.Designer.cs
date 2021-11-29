// Project:           $Project: 00010855-Sapient$
// File:              $Workfile: ScriptForm.Designer.cs$
// Crown-Owned Copyright (c) supplied by QinetiQ. See Release/Supply Conditions

namespace ScriptReader
{
  partial class ScriptForm
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
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.NextButton = new System.Windows.Forms.Button();
            this.outputWindow = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.LoadXMLList = new System.Windows.Forms.Button();
            this.SendXMLList = new System.Windows.Forms.Button();
            this.SendAllXML = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Location = new System.Drawing.Point(12, 41);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(559, 264);
            this.listBox1.TabIndex = 0;
            // 
            // NextButton
            // 
            this.NextButton.Location = new System.Drawing.Point(71, 12);
            this.NextButton.Name = "NextButton";
            this.NextButton.Size = new System.Drawing.Size(75, 23);
            this.NextButton.TabIndex = 1;
            this.NextButton.Text = "Send";
            this.NextButton.UseVisualStyleBackColor = true;
            this.NextButton.Click += new System.EventHandler(this.NextButton_Click);
            // 
            // outputWindow
            // 
            this.outputWindow.Location = new System.Drawing.Point(12, 328);
            this.outputWindow.Multiline = true;
            this.outputWindow.Name = "outputWindow";
            this.outputWindow.ReadOnly = true;
            this.outputWindow.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.outputWindow.Size = new System.Drawing.Size(559, 316);
            this.outputWindow.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "XML Files";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 309);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(39, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Output";
            // 
            // LoadXMLList
            // 
            this.LoadXMLList.Location = new System.Drawing.Point(161, 12);
            this.LoadXMLList.Name = "LoadXMLList";
            this.LoadXMLList.Size = new System.Drawing.Size(93, 23);
            this.LoadXMLList.TabIndex = 5;
            this.LoadXMLList.Text = "Load XML List";
            this.LoadXMLList.UseVisualStyleBackColor = true;
            this.LoadXMLList.Click += new System.EventHandler(this.LoadXMLList_Click);
            // 
            // SendXMLList
            // 
            this.SendXMLList.Location = new System.Drawing.Point(260, 12);
            this.SendXMLList.Name = "SendXMLList";
            this.SendXMLList.Size = new System.Drawing.Size(87, 23);
            this.SendXMLList.TabIndex = 6;
            this.SendXMLList.Text = "Send XML List";
            this.SendXMLList.UseVisualStyleBackColor = true;
            this.SendXMLList.Click += new System.EventHandler(this.SendXMLList_Click);
            // 
            // SendAllXML
            // 
            this.SendAllXML.Location = new System.Drawing.Point(353, 12);
            this.SendAllXML.Name = "SendAllXML";
            this.SendAllXML.Size = new System.Drawing.Size(89, 23);
            this.SendAllXML.TabIndex = 7;
            this.SendAllXML.Text = "Send All XML";
            this.SendAllXML.UseVisualStyleBackColor = true;
            this.SendAllXML.Click += new System.EventHandler(this.SendAllXML_Click);
            // 
            // ScriptForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(583, 648);
            this.Controls.Add(this.SendAllXML);
            this.Controls.Add(this.SendXMLList);
            this.Controls.Add(this.LoadXMLList);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.outputWindow);
            this.Controls.Add(this.NextButton);
            this.Controls.Add(this.listBox1);
            this.Name = "ScriptForm";
            this.Text = "Read File List Script";
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.ListBox listBox1;
    private System.Windows.Forms.Button NextButton;
    private System.Windows.Forms.TextBox outputWindow;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button LoadXMLList;
        private System.Windows.Forms.Button SendXMLList;
        private System.Windows.Forms.Button SendAllXML;
    }
}
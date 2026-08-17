namespace MQTTSubscriber
{
    partial class FrmMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblStatus = new Label();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            SuspendLayout();
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(70, 29);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(49, 20);
            lblStatus.TabIndex = 0;
            lblStatus.Text = "Status";
            // 
            // label1
            // 
            label1.BorderStyle = BorderStyle.FixedSingle;
            label1.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(530, 103);
            label1.Name = "label1";
            label1.Size = new Size(126, 109);
            label1.TabIndex = 1;
            label1.Text = "label1";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            label1.UseCompatibleTextRendering = true;
            // 
            // label2
            // 
            label2.BorderStyle = BorderStyle.FixedSingle;
            label2.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.Location = new Point(134, 103);
            label2.Name = "label2";
            label2.Size = new Size(126, 109);
            label2.TabIndex = 2;
            label2.Text = "label2";
            label2.TextAlign = ContentAlignment.MiddleCenter;
            label2.UseCompatibleTextRendering = true;
            // 
            // label3
            // 
            label3.BorderStyle = BorderStyle.FixedSingle;
            label3.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label3.Location = new Point(266, 103);
            label3.Name = "label3";
            label3.Size = new Size(126, 109);
            label3.TabIndex = 3;
            label3.Text = "label3";
            label3.TextAlign = ContentAlignment.MiddleCenter;
            label3.UseCompatibleTextRendering = true;
            // 
            // label4
            // 
            label4.BorderStyle = BorderStyle.FixedSingle;
            label4.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label4.Location = new Point(398, 103);
            label4.Name = "label4";
            label4.Size = new Size(126, 109);
            label4.TabIndex = 4;
            label4.Text = "label4";
            label4.TextAlign = ContentAlignment.MiddleCenter;
            label4.UseCompatibleTextRendering = true;
            // 
            // FrmMain
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(lblStatus);
            Name = "FrmMain";
            Text = "Form1";
            Load += FrmMain_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblStatus;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
    }
}

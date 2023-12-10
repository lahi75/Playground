
namespace WebcamApp
{
    partial class MainForm
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
            camView = new System.Windows.Forms.PictureBox();
            camSelectionBox = new System.Windows.Forms.ComboBox();
            startButton = new System.Windows.Forms.Button();
            stopButton = new System.Windows.Forms.Button();
            fpsLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)camView).BeginInit();
            SuspendLayout();
            // 
            // camView
            // 
            camView.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            camView.BackColor = System.Drawing.Color.Black;
            camView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            camView.Location = new System.Drawing.Point(12, 0);
            camView.Name = "camView";
            camView.Size = new System.Drawing.Size(1514, 733);
            camView.TabIndex = 0;
            camView.TabStop = false;
            // 
            // camSelectionBox
            // 
            camSelectionBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            camSelectionBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            camSelectionBox.FormattingEnabled = true;
            camSelectionBox.Location = new System.Drawing.Point(391, 795);
            camSelectionBox.Name = "camSelectionBox";
            camSelectionBox.Size = new System.Drawing.Size(457, 40);
            camSelectionBox.TabIndex = 1;
            // 
            // startButton
            // 
            startButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            startButton.Location = new System.Drawing.Point(12, 789);
            startButton.Name = "startButton";
            startButton.Size = new System.Drawing.Size(150, 46);
            startButton.TabIndex = 2;
            startButton.Text = "Start";
            startButton.UseVisualStyleBackColor = true;
            startButton.Click += startButton_Click;
            // 
            // stopButton
            // 
            stopButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            stopButton.Location = new System.Drawing.Point(201, 789);
            stopButton.Name = "stopButton";
            stopButton.Size = new System.Drawing.Size(150, 46);
            stopButton.TabIndex = 3;
            stopButton.Text = "Stop";
            stopButton.UseVisualStyleBackColor = true;
            stopButton.Click += stopButton_Click;
            // 
            // fpsLabel
            // 
            fpsLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            fpsLabel.AutoSize = true;
            fpsLabel.Location = new System.Drawing.Point(880, 803);
            fpsLabel.Name = "fpsLabel";
            fpsLabel.Size = new System.Drawing.Size(52, 32);
            fpsLabel.TabIndex = 4;
            fpsLabel.Text = "FPS";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1538, 881);
            Controls.Add(fpsLabel);
            Controls.Add(stopButton);
            Controls.Add(startButton);
            Controls.Add(camSelectionBox);
            Controls.Add(camView);
            MinimumSize = new System.Drawing.Size(1000, 600);
            Name = "MainForm";
            Text = "WebcamApp";
            FormClosing += MainForm_FormClosing;
            Load += MainForm_Load;
            ((System.ComponentModel.ISupportInitialize)camView).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.PictureBox camView;
        private System.Windows.Forms.ComboBox camSelectionBox;
        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.Button stopButton;
        private System.Windows.Forms.Label fpsLabel;
    }
}


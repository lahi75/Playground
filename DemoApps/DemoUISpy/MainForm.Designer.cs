
namespace CaptureDemo
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
            this.components = new System.ComponentModel.Container();
            this.screenBox = new System.Windows.Forms.PictureBox();
            this.captureTimer = new System.Windows.Forms.Timer(this.components);
            this.keyLabel = new System.Windows.Forms.Label();
            this.mouseLabel = new System.Windows.Forms.Label();
            this.touchLabel = new System.Windows.Forms.Label();
            this.uiElementLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.screenBox)).BeginInit();
            this.SuspendLayout();
            // 
            // screenBox
            // 
            this.screenBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.screenBox.BackColor = System.Drawing.Color.Black;
            this.screenBox.Location = new System.Drawing.Point(0, -1);
            this.screenBox.Name = "screenBox";
            this.screenBox.Size = new System.Drawing.Size(1273, 566);
            this.screenBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.screenBox.TabIndex = 0;
            this.screenBox.TabStop = false;
            this.screenBox.Click += new System.EventHandler(this.ScreenBox_Click);
            // 
            // keyLabel
            // 
            this.keyLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.keyLabel.AutoSize = true;
            this.keyLabel.Location = new System.Drawing.Point(27, 613);
            this.keyLabel.Name = "keyLabel";
            this.keyLabel.Size = new System.Drawing.Size(113, 32);
            this.keyLabel.TabIndex = 1;
            this.keyLabel.Text = "Last Key: ";
            // 
            // mouseLabel
            // 
            this.mouseLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.mouseLabel.AutoSize = true;
            this.mouseLabel.Location = new System.Drawing.Point(489, 613);
            this.mouseLabel.Name = "mouseLabel";
            this.mouseLabel.Size = new System.Drawing.Size(142, 32);
            this.mouseLabel.TabIndex = 2;
            this.mouseLabel.Text = "Mouse Pos: ";
            // 
            // touchLabel
            // 
            this.touchLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.touchLabel.AutoSize = true;
            this.touchLabel.Location = new System.Drawing.Point(489, 689);
            this.touchLabel.Name = "touchLabel";
            this.touchLabel.Size = new System.Drawing.Size(126, 32);
            this.touchLabel.TabIndex = 3;
            this.touchLabel.Text = "Touch Pos:";
            // 
            // uiElementLabel
            // 
            this.uiElementLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.uiElementLabel.AutoSize = true;
            this.uiElementLabel.Location = new System.Drawing.Point(794, 613);
            this.uiElementLabel.Name = "uiElementLabel";
            this.uiElementLabel.Size = new System.Drawing.Size(106, 32);
            this.uiElementLabel.TabIndex = 4;
            this.uiElementLabel.Text = "Element:";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1274, 784);
            this.Controls.Add(this.uiElementLabel);
            this.Controls.Add(this.touchLabel);
            this.Controls.Add(this.mouseLabel);
            this.Controls.Add(this.keyLabel);
            this.Controls.Add(this.screenBox);
            this.MinimumSize = new System.Drawing.Size(1300, 350);
            this.Name = "MainForm";
            this.Text = "DemoCapture";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.screenBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox screenBox;
        private System.Windows.Forms.Timer captureTimer;
        private System.Windows.Forms.Label keyLabel;
        private System.Windows.Forms.Label mouseLabel;
        private System.Windows.Forms.Label touchLabel;
        private System.Windows.Forms.Label uiElementLabel;
    }
}


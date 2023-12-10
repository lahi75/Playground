
namespace DemoNetwork
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
            components = new System.ComponentModel.Container();
            remoteIPLabel = new System.Windows.Forms.Label();
            open1Btn = new System.Windows.Forms.Button();
            messageBox = new System.Windows.Forms.ListBox();
            chatBox = new System.Windows.Forms.TextBox();
            open2Btn = new System.Windows.Forms.Button();
            pingTimer = new System.Windows.Forms.Timer(components);
            sendImgBtn = new System.Windows.Forms.Button();
            pictureBox = new System.Windows.Forms.PictureBox();
            progressBar = new System.Windows.Forms.ProgressBar();
            ((System.ComponentModel.ISupportInitialize)pictureBox).BeginInit();
            SuspendLayout();
            // 
            // remoteIPLabel
            // 
            remoteIPLabel.AutoSize = true;
            remoteIPLabel.BackColor = System.Drawing.SystemColors.ActiveBorder;
            remoteIPLabel.ForeColor = System.Drawing.Color.White;
            remoteIPLabel.Location = new System.Drawing.Point(210, 23);
            remoteIPLabel.MinimumSize = new System.Drawing.Size(160, 46);
            remoteIPLabel.Name = "remoteIPLabel";
            remoteIPLabel.Size = new System.Drawing.Size(160, 46);
            remoteIPLabel.TabIndex = 1;
            remoteIPLabel.Text = "--";
            remoteIPLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // open1Btn
            // 
            open1Btn.Location = new System.Drawing.Point(30, 23);
            open1Btn.Name = "open1Btn";
            open1Btn.Size = new System.Drawing.Size(150, 46);
            open1Btn.TabIndex = 3;
            open1Btn.Text = "Open 1";
            open1Btn.UseVisualStyleBackColor = true;
            open1Btn.Click += open1Btn_Click;
            // 
            // messageBox
            // 
            messageBox.FormattingEnabled = true;
            messageBox.Location = new System.Drawing.Point(30, 84);
            messageBox.Name = "messageBox";
            messageBox.Size = new System.Drawing.Size(524, 516);
            messageBox.TabIndex = 5;
            // 
            // chatBox
            // 
            chatBox.Location = new System.Drawing.Point(30, 616);
            chatBox.Name = "chatBox";
            chatBox.Size = new System.Drawing.Size(524, 39);
            chatBox.TabIndex = 7;
            chatBox.KeyDown += chatBox_KeyDown;
            // 
            // open2Btn
            // 
            open2Btn.Location = new System.Drawing.Point(404, 23);
            open2Btn.Name = "open2Btn";
            open2Btn.Size = new System.Drawing.Size(150, 46);
            open2Btn.TabIndex = 8;
            open2Btn.Text = "Open 2";
            open2Btn.UseVisualStyleBackColor = true;
            open2Btn.Click += open2Btn_Click;
            // 
            // pingTimer
            // 
            pingTimer.Interval = 1000;
            pingTimer.Tick += pingTimer_Tick;
            // 
            // sendImgBtn
            // 
            sendImgBtn.Location = new System.Drawing.Point(30, 735);
            sendImgBtn.Name = "sendImgBtn";
            sendImgBtn.Size = new System.Drawing.Size(150, 46);
            sendImgBtn.TabIndex = 9;
            sendImgBtn.Text = "Image";
            sendImgBtn.UseVisualStyleBackColor = true;
            sendImgBtn.Click += sendImgBtn_Click;
            // 
            // pictureBox
            // 
            pictureBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            pictureBox.Location = new System.Drawing.Point(186, 672);
            pictureBox.Name = "pictureBox";
            pictureBox.Size = new System.Drawing.Size(368, 199);
            pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            pictureBox.TabIndex = 10;
            pictureBox.TabStop = false;
            // 
            // progressBar
            // 
            progressBar.Location = new System.Drawing.Point(30, 882);
            progressBar.Name = "progressBar";
            progressBar.Size = new System.Drawing.Size(524, 18);
            progressBar.TabIndex = 12;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(589, 915);
            Controls.Add(progressBar);
            Controls.Add(pictureBox);
            Controls.Add(sendImgBtn);
            Controls.Add(open2Btn);
            Controls.Add(chatBox);
            Controls.Add(messageBox);
            Controls.Add(open1Btn);
            Controls.Add(remoteIPLabel);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "MainForm";
            Text = "ChatApp";
            FormClosed += MainForm_FormClosed;
            ((System.ComponentModel.ISupportInitialize)pictureBox).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.Label remoteIPLabel;
        private System.Windows.Forms.Button open1Btn;
        private System.Windows.Forms.ListBox messageBox;
        private System.Windows.Forms.TextBox chatBox;
        private System.Windows.Forms.Button open2Btn;
        private System.Windows.Forms.Timer pingTimer;
        private System.Windows.Forms.Button sendImgBtn;
        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.ProgressBar progressBar;
    }
}


using Phoebit.Vision.Camera;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace WebcamApp
{
    public partial class MainForm : Form
    {
        private CameraFrameSource _frameSource;
        private int _latestFPS;
        private static Bitmap _latestFrame;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                // Refresh the list of available cameras
                camSelectionBox.Items.Clear();
                foreach (Camera cam in CameraService.AvailableCameras)
                    camSelectionBox.Items.Add(cam);

                if (camSelectionBox.Items.Count > 0)
                    camSelectionBox.SelectedIndex = 0;
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            thrashOldCamera();
        }

        private void drawLatestImage(object sender, PaintEventArgs e)
        {
            if (_latestFrame != null)
            {
                //calculate the ratio to maintain the aspect ration of the original image
                double dbl = (double)_latestFrame.Width / (double)_latestFrame.Height;
                int w, h;                
                if ((int)((double)camView.Height * dbl) <= camView.Width)
                {
                    w = (int)((double)camView.Height * dbl);
                    h = camView.Height;                    
                }
                else
                {
                    w = camView.Width;
                    h = (int)((double)camView.Width / dbl);                    
                }

                // center image in image box
                int x = camView.Width / 2 - w / 2;

                // Draw the latest image from the active camera
                e.Graphics.DrawImage(_latestFrame, x, 0, w, h);
            }

            fpsLabel.Text = _latestFPS.ToString() + " FPS";
        }

        public void OnImageCaptured(Phoebit.Vision.Contracts.IFrameSource frameSource, Phoebit.Vision.Contracts.Frame frame, double fps)
        {
            _latestFrame = frame.Image;            
            _latestFPS = (int)Math.Round(fps);

            camView.Invalidate();            
        }

        private void setFrameSource(CameraFrameSource cameraFrameSource)
        {
            if (_frameSource == cameraFrameSource)
                return;

            _frameSource = cameraFrameSource;
        }

        private void startCapturing()
        {
            try
            {
                Camera c = (Camera)camSelectionBox.SelectedItem;
                setFrameSource(new CameraFrameSource(c));
                _frameSource.Camera.CaptureWidth = 640;
                _frameSource.Camera.CaptureHeight = 480;
                _frameSource.Camera.Fps = 30;
                _frameSource.NewFrame += OnImageCaptured;

                camView.Paint += new PaintEventHandler(drawLatestImage);
                _frameSource.StartFrameCapture();
            }
            catch (Exception ex)
            {
                camSelectionBox.Text = "Select A Camera";
                MessageBox.Show(ex.Message);
            }
        }

        private void thrashOldCamera()
        {
            // Trash the old camera
            if (_frameSource != null)
            {
                _frameSource.NewFrame -= OnImageCaptured;
                _frameSource.Camera.Dispose();
                setFrameSource(null);
                camView.Paint -= new PaintEventHandler(drawLatestImage);
            }

            fpsLabel.Text = "FPS";
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            // Early return if we've selected the current camera
            if (_frameSource != null && _frameSource.Camera == camSelectionBox.SelectedItem)
                return;

            thrashOldCamera();
            startCapturing();
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            thrashOldCamera();
        }
    }
}

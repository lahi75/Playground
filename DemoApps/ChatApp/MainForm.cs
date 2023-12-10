using Phoebit.Chat.Protocol;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DemoNetwork
{
    public partial class MainForm : Form
    {
        ChatClient _udpClient = new ChatClient();
        int _port1 = 36231;
        int _port2 = 36232;

        Stopwatch _pingWatch = new Stopwatch();
        String _connectedClient = "";

        public MainForm()
        {
            InitializeComponent();
        }       
        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            CloseUDP();   
        }

        private void udpClient_ChatEvent(object sender, ChatClient.ChatEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new EventHandler<ChatClient.ChatEventArgs>(udpClient_ChatEvent), new object[] { sender, e });
            }
            else
            {
                // only deal with packages that are interesting for us
                switch (e.Packet.Header.PayloadType)
                {
                    case Header.PayloadTypeEnum.Name:
                        _connectedClient = e.Packet.Name.PCName;                        
                        System.Net.IPAddress address = new System.Net.IPAddress(0);                        
                        address = _udpClient.RemoteAddress;
                        remoteIPLabel.Text = address.ToString();
                        remoteIPLabel.BackColor = Color.Green;
                        _pingWatch.Restart();
                        break;
                    case Header.PayloadTypeEnum.Text:
                        AddMessage(_connectedClient + ": " + e.Packet.ChatText.Message);
                        break;
                    case Header.PayloadTypeEnum.Image:
                        MemoryStream outstream = new MemoryStream(e.Packet.ChatImage.ImageData);                        
                        pictureBox.Image = Image.FromStream(outstream);
                        // reset progress when image was received
                        progressBar.Value = 0;
                        break;
                    default:
                        break;
                }
            }
        }

        private void AddMessage(String s)
        {
            messageBox.Items.Add(s);

            // scroll to bottomn
            messageBox.SelectedIndex = messageBox.Items.Count - 1;
            messageBox.SelectedIndex = -1;
        }

        private void chatBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                _udpClient.SendText(chatBox.Text);
                AddMessage(Environment.MachineName.ToString() + ": " + chatBox.Text);
                chatBox.Clear();
                e.SuppressKeyPress = true;
            }
        }

        private void open1Btn_Click(object sender, EventArgs e)
        {
            OpenUDP(_port1, _port2);
        }

        private void open2Btn_Click(object sender, EventArgs e)
        {
            OpenUDP(_port2, _port1);
        }

        private void OpenUDP(int send, int listen)
        {
            CloseUDP();

            if (_udpClient.Start(send, listen))
            {
                System.Diagnostics.Debug.WriteLine("Open Socket: Port (" + send + "," + listen + ")");
                _udpClient.ChatEvent += udpClient_ChatEvent;
                pingTimer.Start();
                _pingWatch.Restart();
                _udpClient.TransferProgress += udpClient_TransferProgress;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Open Error");                
            }
        }

        private void udpClient_TransferProgress(object sender, int e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new EventHandler<int>(udpClient_TransferProgress), new object[] { sender, e });
            }
            else
            {
                progressBar.Value = e;
            }
        }

        private void CloseUDP()
        {
            if (_udpClient.IsOpen())
            {
                _pingWatch.Stop();
                pingTimer.Stop();                
                _udpClient.Stop();
                _udpClient.ChatEvent -= udpClient_ChatEvent;
                _udpClient.TransferProgress -= udpClient_TransferProgress;
                System.Diagnostics.Debug.WriteLine("Close Socket");
            }
        }

        private void pingTimer_Tick(object sender, EventArgs e)
        {            
            System.Net.IPAddress address = new System.Net.IPAddress(0);

            if (_udpClient.IsOpen())
            {
                _udpClient.SendPing();

                // check if contact ist lost to remote client
                if (_pingWatch.Elapsed.TotalMilliseconds > 2500)
                {                    
                    remoteIPLabel.Text = "--";
                    remoteIPLabel.BackColor = Color.Red;
                }
            }
        }

        private void sendImgBtn_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();

            dialog.Title = "Open Image";
            dialog.Filter = "jpg files(*.jpg)| *.jpg";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Bitmap bm = new Bitmap(dialog.FileName);

                _udpClient.SendImage(bm);

                // reset progress when done
                progressBar.Value = 0;
            }
            dialog.Dispose();
        }
    }
}

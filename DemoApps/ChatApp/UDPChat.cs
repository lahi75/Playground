
// ----------------------------------------------------------------------------

using Phoebit.Chat.Protocol;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

// ----------------------------------------------------------------------------

namespace DemoNetwork
{
    /// <summary>
    /// class implementing the communication
    /// </summary>
    class ChatClient
    {      
        // socket for UDP communication
        UdpSockets _socket = new UdpSockets();
                       
        int _sendPort = 0;
        int _percent = 0;
        
        volatile Header.ErrorTypeEnum _lastError = Header.ErrorTypeEnum.NoError;


        /// <summary>
        /// other data sinks may subscribe to this event and handle specific data packages
        /// this class only handles the version message which is used by clients to sniff out the server
        /// </summary>
        public event EventHandler<ChatEventArgs> ChatEvent;

        /// <summary>
        /// transfer progress event (reports in percent)
        /// </summary>
        public event EventHandler<int> TransferProgress;

        /// <summary>
        /// start the protocol
        /// </summary>        
        public Boolean Start(int sendPort, int listenPort)
        {
            if (IsOpen())
                Stop();          

            _sendPort = sendPort;

            if (_socket.Open(listenPort))
            {
                _socket.PacketReceived += socket_PacketReceived;
                _socket.TransferProgress += socket_TransferProgress;
                return true;
            }          

            return false;
        }       

        /// <summary>
        /// stop the protocol
        /// </summary>      
        public void Stop()
        {
            if (IsOpen())
            {
                _socket.PacketReceived -= socket_PacketReceived;
                _socket.TransferProgress -= socket_TransferProgress;
                _socket.Close();
            }
        }

        /// <summary>
        /// checks if the socket is open
        /// </summary>        
        public Boolean IsOpen()
        {
            return _socket.IsOpen();
        }

        /// <summary>
        /// get the last error after sending a protocol command
        /// </summary>
        public Header.ErrorTypeEnum LastError
        {
            get { return _lastError; }
        }

        /// <summary>
        /// search for a remote instance on LAN networks
        /// </summary>        
        public Boolean SendPing()
        {
            if (_socket.IsOpen() == false)
                return false;

            if (InProgress)
            {
                System.Diagnostics.Debug.WriteLine("Transfer in progress");
                return false;
            }

            _lastError = Header.ErrorTypeEnum.NoError;

            // get all useful network interfaces        
            IPAddress[] Adresses2 = GetAllUnicastAddresses();

            lock (this)
            {                
                foreach (IPAddress Adres in Adresses2)
                {
                    if (IPAddress.IsLoopback(Adres))
                        continue;

                    Phoebit.Network.IPNetwork ipn = Phoebit.Network.IPNetwork.Parse(Adres.ToString());
                    Phoebit.Network.IPAddressCollection ips = Phoebit.Network.IPNetwork.ListIPAddress(ipn);

                    // broadcast version request to interfaces
                    IPEndPoint endPoint = new IPEndPoint(ipn.Broadcast, _sendPort);

                    DataPacket p = new DataPacket();
                    p.CreatePayload(Header.PayloadTypeEnum.Name);
                    p.Header.PacketType = Header.PacketTypeEnum.Data;
                    p.Header.PayloadType = Header.PayloadTypeEnum.Name;

                    p.Name.PCName = Environment.MachineName.ToString();

                    _socket.SendPacket(p, endPoint);
                }               
            }
            
            return true;
        }

        /// <summary>
        /// send a text packet
        /// </summary>        
        public Boolean SendText(String s)
        {
            if (_socket.IsOpen() == false)
                return false;

            if (RemoteAddress == null)
                return false;

            if (InProgress)            
                return false;            

            IPEndPoint endPoint = new IPEndPoint(RemoteAddress, _sendPort);

            _lastError = Header.ErrorTypeEnum.NoError;

            DataPacket p = new DataPacket();
            p.CreatePayload(Header.PayloadTypeEnum.Text);
            p.Header.PacketType = Header.PacketTypeEnum.Data;
            p.Header.PayloadType = Header.PayloadTypeEnum.Text;
            p.ChatText.Message = s.Length > 1024 ? s.Substring(0, 1024) : s; // limit to 1024 bytes

            lock (this)
            {              
                if (_socket.SendPacket(p, endPoint) == false)
                {
                    Debug.WriteLine("Send Error: Text");
                    return false;
                }             
            }

            if (_lastError != Header.ErrorTypeEnum.NoError)
                return false;

            return true;
        }
        
        /// <summary>
        /// sends the given image to the remote chat client
        /// </summary>        
        public Boolean SendImage(Bitmap image)
        {
            if (_socket.IsOpen() == false)
                return false;

            if (RemoteAddress == null)
                return false;

            if (InProgress)                         
                return false;           

            IPEndPoint endPoint = new IPEndPoint(RemoteAddress, _sendPort);

            _lastError = Header.ErrorTypeEnum.NoError;

            ImageCodecInfo imageCodecInfo = GetEncoderInfo("image/jpeg");
            // Get an ImageCodecInfo object that represents the JPEG codec.            
            EncoderParameters encoderParameters;
            Encoder encoder;
            EncoderParameter encoderParameter;
            encoder = Encoder.Quality;
            encoderParameters = new EncoderParameters(1);            
            encoderParameter = new EncoderParameter(encoder, 90L);
            encoderParameters.Param[0] = encoderParameter;

            // convert image into memory stream
            MemoryStream ms = new MemoryStream();
            image.Save(ms, imageCodecInfo, encoderParameters);
            
            DataPacket p = new DataPacket();
            p.CreatePayload(Header.PayloadTypeEnum.Image, (uint)ms.Length + 4); // reserve 4 addional bytes for the size
            p.Header.PacketType = Header.PacketTypeEnum.Data;
            p.Header.PayloadType = Header.PayloadTypeEnum.Image;

            p.ChatImage.ImageData = ms.ToArray();

            lock (this)
            {                
                if (_socket.SendPacket(p, endPoint) == false)
                {
                    Debug.WriteLine("Send Error: Image");
                    return false;
                }               
            }
            
            if (_lastError != Header.ErrorTypeEnum.NoError)
                return false;

            return true;
        }       
        
        /// <summary>
        /// helper function to enumerate encoder options for jpeg compression
        /// </summary>        
        private static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }

        /// <summary>
        /// get the address of a reachable remote instance in the network if available
        /// </summary>
        public IPAddress RemoteAddress
        {
            get;
            set;
        }

        /// <summary>
        /// call upon reception of a packet
        /// </summary>        
        private void socket_PacketReceived(object sender, ChatReceivedArgs e)
        {           
            if (e.Packet.Header.PacketType == Header.PacketTypeEnum.Ack)
            {
                // ACK packet
                switch (e.Packet.Header.PayloadType)
                {
                    case Header.PayloadTypeEnum.Name:
                        //System.Diagnostics.Debug.WriteLine("ACK from: " + e.EndPoint.Address.ToString() + " DATA: " + e.Packet.Name.PCName);

                        if (RemoteAddress != null)
                        {
                            // don't overwrite the loopback interface if available
                            if (IPAddress.IsLoopback(RemoteAddress) == false)
                                RemoteAddress = e.EndPoint.Address;
                        }
                        else
                            RemoteAddress = e.EndPoint.Address;                    
                        break;
                    case Header.PayloadTypeEnum.Text:                        
                        break;
                    case Header.PayloadTypeEnum.Image:                        
                        break;
                }
            }
            else if (e.Packet.Header.PacketType == Header.PacketTypeEnum.Error)
            {
                // TODO: Error reporting does't work well !!!!! synchronization problem
                _lastError = e.Packet.Header.ErrorType;                
                Console.WriteLine("Errorpacket received");
            }
            else if (e.Packet.Header.PacketType == Header.PacketTypeEnum.Data)
            {
                switch (e.Packet.Header.PayloadType)
                {
                    case Header.PayloadTypeEnum.Name:
                        //System.Diagnostics.Debug.WriteLine("Answer from: " + e.EndPoint.Address.ToString() + " DATA: " + e.Packet.Name.PCName);

                        if (RemoteAddress != null)
                        {
                            // don't overwrite the loopback interface if available
                            if (IPAddress.IsLoopback(RemoteAddress) == false)
                                RemoteAddress = e.EndPoint.Address;
                        }
                        else
                            RemoteAddress = e.EndPoint.Address;                        

                        e.Packet.Header.PacketType = Header.PacketTypeEnum.Ack;

                        // temporarily save pc name
                        String name = e.Packet.Name.PCName;

                        // now send the answer package back to the remote 
                        // inser our own PC name
                        e.Packet.Name.PCName = Environment.MachineName.ToString();
                        _socket.SendPacket(e.Packet, e.EndPoint);

                        // restore received pc name
                        e.Packet.Name.PCName = name;
                        break;
                    case Header.PayloadTypeEnum.Text:                        
                        break;
                    case Header.PayloadTypeEnum.Image:                        
                        break;
                }

                // send the packet to all subscribers, they will handle the package and may return data in the package
                ChatEvent?.Invoke(this, new ChatEventArgs(e.Packet));
            }
        }

        private void socket_TransferProgress(object sender, ProgressArgs e)
        {
            _percent = e.Percent;
            TransferProgress?.Invoke(this, e.Percent);
        }

        private bool InProgress
        {
            get { return _percent > 0 && _percent < 100; }
        }

        /// <summary>
        /// get all available network interfaces on this machine, exclude IPv6, VPNs and tunnels
        /// </summary>        
        private IPAddress[] GetAllUnicastAddresses()
        {
            // This works on both Mono and .NET , but there is a difference: it also
            // includes the LocalLoopBack so we need to filter that one out
            List<IPAddress> Addresses = new List<IPAddress>();
            // Obtain a reference to all network interfaces in the machine
            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in adapters)
            {                
                if (adapter.Name == "Local Area Connection" || adapter.Name == "WLAN" || adapter.Name == "Ethernet" || adapter.Name == "Ethernet0" || adapter.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                {
                    IPInterfaceProperties properties = adapter.GetIPProperties();
                    foreach (IPAddressInformation uniCast in properties.UnicastAddresses)
                    {
                        // Ignore loop-back addresses & IPv6
                        //if (!IPAddress.IsLoopback(uniCast.Address) && uniCast.Address.AddressFamily != AddressFamily.InterNetworkV6)
                        //    Addresses.Add(uniCast.Address);

                        // ignore IPv6
                        if (uniCast.Address.AddressFamily != AddressFamily.InterNetworkV6)
                            Addresses.Add(uniCast.Address);
                    }
                }

            }
            return Addresses.ToArray();
        }

        public class ChatEventArgs : EventArgs
        {
            /// <summary>
            /// ctor
            /// </summary>        
            public ChatEventArgs(DataPacket dataPacket)
            {
                this.Packet = dataPacket;
            }

            /// <summary>
            /// access the data packet
            /// </summary>
            public DataPacket Packet { get; }
        }
    }
}

// ----------------------------------------------------------------------------
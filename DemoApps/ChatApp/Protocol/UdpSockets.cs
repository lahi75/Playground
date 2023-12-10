// ----------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Threading;

// ----------------------------------------------------------------------------

namespace Phoebit.Chat.Protocol
{
    public class UdpSockets
    {
        UdpClient _receiveSocket = null;
        UdpClient _sendSocket = null;

        int _udpMaxSize = 1300; // don't exceed the max UDP packet (1300 best for WLAN)
        byte[] _udpPacket;
        int _port = 0;

        BackgroundWorker _readThread = null;
        volatile Boolean _running = false;
        volatile Boolean _killed = false;

        private Mutex   _sendMutex = new Mutex();

        /// <summary>
        /// delegate prototype of the packet received method
        /// </summary>            
        public delegate void ChatEventHandler(Object sender,ChatReceivedArgs e);

        /// <summary>
        /// event which is triggered when a packet is received
        /// </summary>        
        public event ChatEventHandler PacketReceived;

        /// <summary>
        /// event handler to report progress when sending huge data packets
        /// </summary>        
        public delegate void ProgressEventHandler(Object sender, ProgressArgs e);

        /// <summary>
        /// event which is triggeret with a transfer progress update
        /// </summary>
        public event ProgressEventHandler TransferProgress;
        
        /// <summary>
        /// dtor
        /// </summary>
        ~UdpSockets()
        {         
            Close();
        }

        /// <summary>
        /// open the sockets at the given port
        /// </summary>        
        public Boolean Open(int port)
        {
            //IPEndPoint object will allow us to read datagrams sent from any source.
            IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, port);

            _port = port;

            try
            {
                // bind to any ip address
                _receiveSocket = new UdpClient(remoteIpEndPoint);                

                // don't bind, since we don't know where we will send the answer to
                _sendSocket = new UdpClient();
                _sendSocket.DontFragment = true;                
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.ToString());                
                _receiveSocket = null;
                _sendSocket = null;

                return false;
            }

            _udpPacket = new byte[_udpMaxSize]; // create send buffer

            _readThread = new BackgroundWorker();
            _running = true;
            _killed = false;

            _readThread.DoWork += new DoWorkEventHandler(backgroundWorker_DoRead);
            _readThread.RunWorkerAsync();

            return true;
        }

        /// <summary>
        /// check if the sockets are open
        /// </summary>        
        public bool IsOpen()
        {
            return _receiveSocket != null && _sendSocket != null;           
        }

        /// <summary>
        /// close the sockets and clean up
        /// </summary>
        public void Close()
        {
            if (IsOpen() == false)
                return;

            _running = false;

            try
            {
                _receiveSocket.Close();
                _sendSocket.Close();
            }
            catch (System.Net.Sockets.SocketException s)
            {
                Console.WriteLine(s.Message);
            }

            // wait until the background reader has quit
            while (_killed == false)
                System.Threading.Thread.Sleep(10);
            
            _receiveSocket = null;
            _sendSocket = null;
        }

        /// <summary>
        /// send a data packet to the specified endpoint
        /// </summary>        
        public Boolean SendPacket(DataPacket packet, IPEndPoint endPoint)
        {
            if( IsOpen() == false )
                return false;

            try
            {
                if (packet.Header.DataBytes.Length == 0) // no header, -> error
                {
                    return false;
                }

                _sendMutex.WaitOne();

                // send the header
                if (packet.Header.DataBytes.Length > 0)
                {
                    if (Send(packet.Header.DataBytes, packet.Header.DataBytes.Length, endPoint) == false)
                    {
                        _sendMutex.ReleaseMutex();
                        return false;
                    }
                }

                if (packet.Payload.DataBytes.Length == 0) // payload is optional
                {
                    _sendMutex.ReleaseMutex();
                    return true;
                }

                int bytes_to_send = packet.Header.PayloadLength;
                int offset = 0;

                int max_bytes = bytes_to_send;

                // send the payload in chunks of maxSize
                while (bytes_to_send > 0)
                {                    
                    int p_size;

                    if (bytes_to_send > _udpMaxSize)
                        p_size = _udpMaxSize;
                    else
                        p_size = bytes_to_send;

                    Array.Copy(packet.Payload.DataBytes, offset, _udpPacket, 0, p_size);

                    offset += p_size;

                    // now send the payload chunk
                    if (Send(_udpPacket, p_size, endPoint) == false)
                    {
                        _sendMutex.ReleaseMutex();
                        return false;
                    }

                    bytes_to_send -= p_size;

                    // calculate percentage of transmission
                    int percent = ((max_bytes - bytes_to_send) * 100) / max_bytes;

                    // send progress to subscribers if sending images
                    if(packet.Header.PayloadType == Header.PayloadTypeEnum.Image)
                        TransferProgress?.Invoke(this, new ProgressArgs(percent));

                    if (bytes_to_send > 0)
                        System.Threading.Thread.Sleep(1);
                }
            }
            catch (System.ObjectDisposedException e)
            {
                Console.WriteLine("Chat Socket: " + e.Message);
                return false;
            }
            catch (System.Net.Sockets.SocketException e)
            {
                Console.WriteLine("Chat Socket: " + e.Message);
                return false;
            }
            catch (System.AccessViolationException e)
            {
                Console.WriteLine("Chat Socket: " + e.Message);
                return false;
            }
            catch (System.NullReferenceException e)
            {
                Console.WriteLine("Chat Socket: " + e.Message);
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine("Chat Socket: " + e.Message);
                return false;
            }
            finally
            {
                _sendMutex.ReleaseMutex();
            }

            return true;
        }

        /// <summary>
        /// read a packet from the udp socket
        /// </summary>        
        private DataPacket ReadPacket(ref IPEndPoint endPoint)
        {
            if ( IsOpen() == false )
                return null;            

            DataPacket packet = new DataPacket();

            try
            {
                // try to receive a packet header
                packet.Header.DataBytes = _receiveSocket.Receive(ref endPoint);                
                
                // return if it's not a valid packet header
                if (packet.Header.CheckHeaderFormat() == false)
                {
                    Console.WriteLine("Chat Socket: bad header format");
                    return null;
                }

                if (packet.Header.PayloadType == Header.PayloadTypeEnum.Image)
                {
                    System.Diagnostics.Debug.WriteLine("Image");
                }

                // create the payload, return false if it's an unknown payload    
                if (packet.CreatePayload(packet.Header.PayloadType, (uint)packet.Header.PayloadLength) == false)
                {
                    Console.WriteLine("Chat Socket: unknown payload type");
                    return null;
                }

                // return immediatly if the packet doesn't have a payload
                if (packet.Header.PayloadLength == 0)
                    return packet;

                int bytes_to_read = packet.Header.PayloadLength;
                int offset = 0;
                int max_bytes = bytes_to_read;
                
                // try to receive payload chunks
                do
                {
                    byte[] d = _receiveSocket.Receive(ref endPoint);                                        

                    if (d.Length != _udpMaxSize && d.Length != bytes_to_read)
                    {
                        Console.WriteLine("Didn't receive expected data: " + d.Length + ":" + bytes_to_read );
                        return null;
                    }

                    d.CopyTo(packet.Payload.DataBytes, offset);
                    bytes_to_read -= d.Length;
                    offset += d.Length;

                    int percent = ((max_bytes - bytes_to_read) * 100) / max_bytes;

                    // send progress to subscribers for image transfers
                    if (packet.Header.PayloadType == Header.PayloadTypeEnum.Image)
                        TransferProgress?.Invoke(this, new ProgressArgs(percent));                    

                } while (bytes_to_read > 0);

                if (packet.Header.PayloadLength != packet.Payload.DataBytes.Length)
                {
                    Console.WriteLine("Chat Socket: wrong payload length");
                    return null;
                }
            }
            catch (System.Net.Sockets.SocketException e)
            {
                // client not reachable -> lock the sending of messages to prevent "port not reachable exceptions"
                // but only do this on the UDP server to prevent deadlocks
                //if (_server)
                //    _lockInterface = true;

                Console.WriteLine("Chat Socket: " + e.Message);
                return null;
            }
            catch (System.ObjectDisposedException e)
            {
                Console.WriteLine("Chat Socket: " + e.Message);
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine("Chat Socket: " + e.Message);
                return null;
            }

            // unlock the interface
            //_lockInterface = false;

            return packet;
        }

        /// <summary>
        /// send data through the send socket
        /// </summary>        
        private Boolean Send(Byte[] data, int length, IPEndPoint endPoint)
        {
            if (_sendSocket != null)
            {                
                //System.Threading.Thread.Sleep(10);                
                return _sendSocket.Send(data, length, endPoint) == length;
            }

            return false;
        }

        /// <summary>
        /// background worker reading data from the socket
        /// </summary>        
        private void backgroundWorker_DoRead(object sender, DoWorkEventArgs e)
        {
            System.Threading.Thread.CurrentThread.Priority = ThreadPriority.Normal;

            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, _port);
            
            while (_running == true)
            {
                // if there is no callback registered, we don't have to read
                if (PacketReceived == null)
                {
                    System.Threading.Thread.Sleep(50);
                    continue;
                }

                // try to fetch a full packet from the socket
                DataPacket p = ReadPacket(ref endPoint);                            

                endPoint.Port = _port;

                // send the packet to the delegate
                if (p != null)
                    PacketReceived(this, new ChatReceivedArgs(p,endPoint));
            }
            _killed = true;
        }       
    }

    /// <summary>
    /// event argument class for a chat packet received delegate
    /// </summary>    
    public class ChatReceivedArgs : EventArgs
    {
        /// <summary>
        /// ctor
        /// </summary>        
        public ChatReceivedArgs(DataPacket packet, IPEndPoint endPoint)
        {
            _packet = packet;
            _endPoint = endPoint;
        }

        /// <summary>
        /// access the packet
        /// </summary>
        public DataPacket Packet
        {
            get { return _packet; }
        }

        public IPEndPoint EndPoint
        {
            get { return _endPoint; }
        }

        DataPacket _packet;
        IPEndPoint _endPoint;            
    }

    /// <summary>
    /// arguments for the transfer progress event
    /// </summary>
    public class ProgressArgs : EventArgs
    {
        /// <summary>
        /// ctor
        /// </summary>        
        public ProgressArgs(int percent)
        {
            Percent = percent;
        }

        /// <summary>
        /// access the packet
        /// </summary>
        public int Percent { get; }
    }
}

// ----------------------------------------------------------------------------
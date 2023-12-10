// ----------------------------------------------------------------------------

using Phoebit.Data;
using System;

// ----------------------------------------------------------------------------

namespace Phoebit.Chat.Protocol
{
    /// <summary>
    /// header definition of a data packet used by the chat application
    /// </summary>
    public class Header : ByteUnion
    {
        /// <summary>
        /// packet types
        /// </summary>        
        public enum PacketTypeEnum
        {                        
            /// data packet            
            Data = 1,
            /// error answer            
            Error = 2,
            /// ack answer
            Ack = 3
        }

        /// <summary>
        /// different payload types
        /// </summary>        
        public enum PayloadTypeEnum
        {                   
            Name,
            Text,
            Image
        }

        public enum ErrorTypeEnum
        {
            NoError,
            NoCommand            
        }

        /// <summary>
        /// ctor
        /// </summary>
        public Header() 
            : base(16)
        {
            MagicNumber = 0x1263BC3873454A28;
        }

        /// <summary>
        /// get/set the packet type
        /// </summary>
        public PacketTypeEnum PacketType
        {
            get
            {
                return (PacketTypeEnum)ReadByte(8);
            }
            set
            {
                WriteByte(8, (Byte)value);
            }
        }

        /// <summary>
        /// get/set the error type
        /// </summary>
        public ErrorTypeEnum ErrorType
        {
            get
            {
                return (ErrorTypeEnum)ReadByte(9);
            }
            set
            {
                WriteByte(9, (Byte)value);
            }
        }

        /// <summary>
        /// get/set the payload type of the packet
        /// </summary>
        public PayloadTypeEnum PayloadType
        {
            get
            {
                return (PayloadTypeEnum)ReadInt16(10);
            }
            set
            {
                WriteInt16(10, (Int16)value);
            }
        }

        /// <summary>
        /// set/get the length of the payload
        /// </summary>
        public Int32 PayloadLength
        {
            get
            {
                return ReadInt32(12);
            }
            set
            {
                WriteInt32(12, value);
            }
        }

        /// <summary>
        /// checks packet structure for validity 
        /// </summary>
        /// <returns></returns>
        public bool CheckHeaderFormat()
        {
            if (_data.Length != 16)
                return false;
            if (MagicNumber != 0x1263BC3873454A28)
                return false;
                    
            return true;
        }      
      
        /// <summary>
        /// set/get the magic number
        /// this number idendifies an et packet
        /// </summary>
        private Int64 MagicNumber
        {
            get
            {
                return ReadInt64(0);
            }
            set
            {
                WriteInt64(0, value);
            }
        }
    }

    /// <summary>
    /// definition of a packet version payload
    /// </summary>  
    public class Name : ByteUnion
    {              
        /// <summary>
        /// ctor
        /// </summary>
        public Name() 
            : base(256)
        {
        }

        /// <summary>
        /// major build number of software
        /// </summary>
        public String PCName
        {
            get
            {                
                System.Text.UnicodeEncoding enc = new System.Text.UnicodeEncoding();
                Int32 length = ReadInt32(0);
                byte[] s = ReadBytes(4, (UInt32)length);
                return enc.GetString(s, 0, s.Length);

            }
            set
            {
                System.Text.UnicodeEncoding enc = new System.Text.UnicodeEncoding();
                WriteInt32(0, (Byte)enc.GetByteCount(value));
                WriteBytes(4, enc.GetBytes(value));
            }
        }             
    }    
      
    /// <summary>
    /// payload text data
    /// </summary>
    public class ChatText : ByteUnion
    {
        /// <summary>
        /// ctor
        /// </summary>
        public ChatText() 
            : base(1024)
        {        
        }

        /// <summary>
        /// plain text message
        /// </summary>
        public string Message
        {
            get
            {                
                System.Text.UnicodeEncoding enc = new System.Text.UnicodeEncoding();
                Int32 length = ReadInt32(0);
                byte[] s = ReadBytes(4, (UInt32)length);
                return enc.GetString(s, 0, s.Length);
            }
            set
            {
                System.Text.UnicodeEncoding enc = new System.Text.UnicodeEncoding();
                WriteInt32(0, (Byte)enc.GetByteCount(value));
                WriteBytes(4, enc.GetBytes(value));
            }
        }
    }
    
    /// <summary>
    /// send the an image
    /// </summary>
    public class ChatImage : ByteUnion
    {             
        public ChatImage(uint Size)
            : base(Size)
        {        
        }       

        public byte[] ImageData
        {
            get
            {
                Int32 length = ReadInt32(0);
                return ReadBytes(4, (UInt32)length);
            }
            set
            {                                
                WriteInt32(0, value.Length );
                WriteBytes(4, value);
            }
        }
    }
  
    /// <summary>
    /// class defining a packet
    /// </summary>    
    public class DataPacket
    {
        private Header _packetHeader = new Header();    // header is always there
        private ByteUnion _payload = null;              // payload is optional

        /// <summary>
        /// creates a payload for the given command
        /// </summary>        
        public Boolean CreatePayload(Header.PayloadTypeEnum command)
        {
            return CreatePayload(command, 0);
        }

        /// <summary>
        /// creates a payload for the given command
        /// </summary>        
        public Boolean CreatePayload(Header.PayloadTypeEnum command, uint size)
        {
            switch (command)
            {
                case Header.PayloadTypeEnum.Name:
                    _payload = new Name();
                    break;
                case Header.PayloadTypeEnum.Text:
                    _payload = new ChatText();
                    break;
                case Header.PayloadTypeEnum.Image:
                    _payload = new ChatImage(size);
                    break;                
                default:
                    return false;
            }

            _packetHeader.PayloadType = command;
            _packetHeader.PayloadLength = _payload.DataBytes.Length;

            return true;
        }

        /// <summary>
        /// access the packet header
        /// </summary>
        public Header Header
        {
            get{ return _packetHeader; }
        }

        /// <summary>
        /// access the payload
        /// can be null
        /// </summary>
        public ByteUnion Payload
        {
            get { return _payload; }
        }        

        /// <summary>
        /// access version payload
        /// can be null
        /// </summary>
        public Name Name
        {
            get { return _payload as Name; }
        }

        /// <summary>
        /// access chat text message
        /// </summary>
        public ChatText ChatText
        {
            get { return _payload as ChatText; }
        }

        /// <summary>
        /// access chat image message
        /// </summary>
        public ChatImage ChatImage
        {
            get { return _payload as ChatImage; }
        }        
    }
}

// ----------------------------------------------------------------------------
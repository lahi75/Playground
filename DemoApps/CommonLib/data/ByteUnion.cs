// ---------------------------------------------------------------------------- 

using System;

// ----------------------------------------------------------------------------

namespace Phoebit.Data
{
    /// <summary>
    /// class implementing a union of bytes
    /// </summary>    
    public class ByteUnion
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="size">pass the desired initial size of the byte array</param>
        public ByteUnion(UInt32 size)
        {
            _data = new byte[size];
        }
      
        /// <summary>
        /// writes a byte in the data storage
        /// </summary>        
        protected void WriteByte(UInt32 offset, Byte value)
        {
            _data[offset] = value;
        }

        /// <summary>
        /// reads a byte from the given position
        /// </summary>        
        protected Byte ReadByte(UInt32 offset)
        {
            return _data[offset];
        }

        /// <summary>
        /// writes a short integer in the data storage
        /// </summary>        
        protected void WriteInt16(UInt32 offset, Int16 value)
        {            
            _data[offset] = (byte)((value >> 8) & 0xff);
            _data[offset + 1] = (byte)((value) & 0xff);
        }

        /// <summary>
        /// read a short integer from the given data storage
        /// </summary>        
        protected Int16 ReadInt16(UInt32 offset)
        {
            Int16 v = 0;

            v |= (Int16)(_data[offset]);
            v = (Int16)(v << 8);
            v |= (Int16)_data[offset + 1];            

            return v;
        }

        /// <summary>
        /// write 32bit integer to the given position in the data array
        /// </summary>        
        protected void WriteInt32(UInt32 offset, Int32 value)
        {
            _data[offset] = (byte)((value >> 24) & 0xff);
            _data[offset+1] = (byte)((value >> 16) & 0xff);
            _data[offset+2] = (byte)((value >> 8) & 0xff);
            _data[offset+3] = (byte)((value) & 0xff);
        }

        /// <summary>
        /// reads a 32bit integer from the given position
        /// </summary>        
        protected Int32 ReadInt32(UInt32 offset)
        {
            Int32 v = 0;

            v |= _data[offset];
            v = v << 8;
            v |= _data[offset + 1];
            v = v << 8;
            v |= _data[offset + 2];
            v = v << 8;
            v |= _data[offset + 3];

            return v;
        }

        /// <summary>
        /// writes a 64bit integer to the given position
        /// </summary>        
        protected void WriteInt64(UInt32 offset, Int64 value)
        {
            _data[offset] = (byte)((value >> 56) & 0xff);
            _data[offset + 1] = (byte)((value >> 48) & 0xff);
            _data[offset + 2] = (byte)((value >> 40) & 0xff);
            _data[offset + 3] = (byte)((value >> 32) & 0xff);
            _data[offset + 4] = (byte)((value >> 24) & 0xff);
            _data[offset + 5] = (byte)((value >> 16) & 0xff);
            _data[offset + 6] = (byte)((value >> 8) & 0xff);
            _data[offset + 7] = (byte)((value) & 0xff);
        }

        /// <summary>
        /// reads a 64bit integer from the given position
        /// </summary>        
        protected Int64 ReadInt64(UInt32 offset)
        {
            Int64 v = 0;

            v |= _data[offset];
            v = v << 8;
            v |= _data[offset + 1];
            v = v << 8;
            v |= _data[offset + 2];
            v = v << 8;
            v |= _data[offset + 3];
            v = v << 8;
            v |= _data[offset + 4];
            v = v << 8;
            v |= _data[offset + 5];
            v = v << 8;
            v |= _data[offset + 6];
            v = v << 8;
            v |= _data[offset + 7];

            return v;
        }

        /// <summary>
        /// writes a sequence of bytes into the data buffer
        /// </summary>        
        protected void WriteBytes(UInt32 offset, byte[] value)
        {
            if (offset + value.Length > _data.Length)
                return;
            
            value.CopyTo(_data, offset);
        }

        /// <summary>
        /// read a sequence of bytes from the data storage
        /// </summary>        
        protected byte[] ReadBytes(UInt32 offset, UInt32 length)
        {
            byte[] b = new byte[length];

            if (offset + length > _data.Length)
                return b;

            Array.Copy( _data, offset, b, 0, length);
            
            return b;
        }

        /// <summary>
        /// raw access to the data storage
        /// </summary>
        public byte[] DataBytes
        {
            get { return _data; }
            set { _data = value; /* (byte[])value.Clone();*/ }
        }
      
        /// <summary>
        /// array of data bytes
        /// </summary>
        protected byte[] _data;
    }
}

// ----------------------------------------------------------------------------
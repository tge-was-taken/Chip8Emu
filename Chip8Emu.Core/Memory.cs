using System;

namespace Chip8Emu.Core
{
    public class Memory
    {
        public Memory()
        {
            WriteBytes( 0, SystemFont.Data );
        }

        private readonly byte[] mData = new byte[4096];

        public byte[] ReadBytes( ushort address, int count )
        {
            var destination = new byte[count];
            ReadBytes( address, count, destination, 0 );
            return destination;
        }

        public void ReadBytes( ushort address, int count, byte[] buffer, int index )
        {
            Array.Copy( mData, address, buffer, index, count );
        }

        public ushort ReadUShort( ushort address )
        {
            return ( ushort ) ( mData[ address ] << 8 | mData[ address + 1 ] );
        }

        public byte ReadByte( ushort address )
        {
            return mData[ address ];
        }

        public void WriteByte( ushort address, byte value )
        {
            mData[address] = value;
        }

        public void WriteBytes( ushort address, byte[] buffer )
        {
            Array.Copy( buffer, 0, mData, address, buffer.Length );
        }
    }
}
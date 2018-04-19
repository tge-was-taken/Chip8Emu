namespace Chip8Emu.Core.Utilities
{
    internal static class BitField
    {
        public static ushort Unpack( this ushort @this, int start, int count )
        {
            const ushort totalBitCount = sizeof( ushort ) * 8;
            return ( ushort ) ( ( @this >> start ) & ( ushort.MaxValue >> ( totalBitCount - count ) ) );
        }

        public static void Pack( ref ushort destination, int start, int count, ushort value )
        {
            const ushort totalBitCount = sizeof( ushort ) * 8;
            var mask = (ushort)( ushort.MaxValue >> ( totalBitCount - count ) );
            value &= mask;
            destination = ( ushort ) ( destination & ~( mask << start ) | ( value << start ) );
        }
    }
}

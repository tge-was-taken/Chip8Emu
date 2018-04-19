using System;
using Chip8Emu.Core.Utilities;

namespace Chip8Emu.Core.Cpu
{
    public struct Instruction
    {
        public const int SIZE = sizeof( ushort );

        public ushort Data { get; }

        // 12 ... 15 : 4
        public byte Op1 => ( byte ) Data.Unpack( 12, 4 );

        // 00 ... 03 : 4
        public byte Op2 => ( byte ) Data.Unpack( 0, 4 );

        // 00 ... 07 : 8
        public byte Op3 => ( byte ) Data.Unpack( 0, 8 );

        // 00 ... 11 : 12
        public ushort NNN => Data.Unpack( 0, 12 );

        // 00 ... 03 : 4
        public byte N => ( byte )Data.Unpack( 0, 4 );

        // 08 ... 11 : 4
        public byte X => ( byte ) Data.Unpack( 8, 4 );

        // 04 ... 07 : 4
        public byte Y => ( byte ) Data.Unpack( 4, 4 );

        // 00 ... 07 : 8
        public byte KK => ( byte )Data.Unpack( 0, 8 );

        public Instruction( ushort data )
        {
            Data = data;
        }

        public int GetIndex()
        {
            // 0
            if ( Op1 == 0 )
            {
                if ( Op3 == 0xE0 )
                {
                    return 1;
                }
                else if ( Op3 == 0xEE )
                {
                    return 2;
                }
                else
                {
                    return 0;
                }
            }

            // 1 ... 7
            if ( Op1 < 8 )
            {
                return Op1 + 2;
            }

            // 8
            if ( Op1 == 8 )
            {
                if ( Op2 != 0xE )
                {
                    return Op2 + 10;
                }
                else
                {
                    return 18;
                }
            }

            // 9 ... 0xD
            if ( Op1 < 0xE )
            {
                return Op1 + 10;
            }

            // 0xE
            if ( Op1 == 0xE )
            {
                if ( Op3 == 0x9E )
                {
                    return 24;
                }
                else
                {
                    return 25;
                }
            }

            // 0xF
            if ( Op1 == 0xF )
            {
                switch ( Op3 )
                {
                    case 0x07:
                        return 26;
                    case 0x0A:
                        return 27;
                    case 0x15:
                        return 28;
                    case 0x18:
                        return 29;
                    case 0x1E:
                        return 30;
                    case 0x29:
                        return 31;
                    case 0x33:
                        return 32;
                    case 0x55:
                        return 33;
                    case 0x65:
                        return 34;
                }
            }

            throw new NotImplementedException( "Unknown instruction" );
        }
    }
}

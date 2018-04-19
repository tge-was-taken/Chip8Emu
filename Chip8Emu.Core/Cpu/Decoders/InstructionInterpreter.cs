using System;
using System.Diagnostics;
using Chip8Emu.Core.Input;

namespace Chip8Emu.Core.Cpu.Decoders
{
    public class InstructionInterpreter : IInstructionDecoder
    {
        private static readonly Random sRandom = new Random();
        private delegate bool InstructionHandler( CpuCore context, Instruction instruction );
        private static readonly InstructionHandler[] sInstructionHandlers = new InstructionHandler[Constants.INSTRUCTION_COUNT]
        {
            /* 0 */ SYS, CLS, RET,
            /* 1 */ JP,
            /* 2 */ CALL,
            /* 3 */ SEIMM,
            /* 4 */ SNEIMM,
            /* 5 */ SE,
            /* 6 */ LDIMM,
            /* 7 */ ADDIMM,
            /* 8 */ LD, OR, AND, XOR, ADD, SUB, SHR, SUBN, SHL,
            /* 9 */ SNE,
            /* A */ LDI,
            /* B */ JPV0,
            /* C */ RND,
            /* D */ DRW,
            /* E */ SKP, SKNP,
            /* F */ LDGDT, LDK, LDSDT, LDSST, ADDI, LDSPR, LDBCD, LDRW, LDRS
        };

        public bool Decode( CpuCore context, Instruction instruction )
        {
            var index = instruction.GetIndex();
            return sInstructionHandlers[ index ]( context, instruction );
        }

        /// <summary>
        /// 0nnn - SYS addr
        /// Jump to a machine code routine at nnn.
        /// This instruction is only used on the old computers on which Chip-8 was originally implemented. It is ignored by modern interpreters.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="instruction"></param>
        /// <returns></returns>
        private static bool SYS( CpuCore context, Instruction instruction )
        {
            return true;
        }

        /// <summary>
        /// 00E0 - CLS
        /// Clear the display.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="instruction"></param>
        /// <returns></returns>
        private static bool CLS( CpuCore context, Instruction instruction )
        {
            context.Display.Clear();
            return true;
        }

        /// <summary>
        /// 00EE - RET
        ///Return from a subroutine.
        ///
        ///The interpreter sets the program counter to the address at the top of the stack, then subtracts 1 from the stack pointer.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="instruction"></param>
        /// <returns></returns>
        private static bool RET( CpuCore context, Instruction instruction )
        {
            if ( context.SP == 0 )
#if DEBUG
                throw new StackUnderflowException();
#else
                return true;
#endif

            context.PC = context.Stack[ context.SP-- ];
            return true;
        }

        /// <summary>
        /// 1nnn - JP addr
        ///Jump to location nnn.
        ///
        ///The interpreter sets the program counter to nnn.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="instruction"></param>
        /// <returns></returns>
        private static bool JP( CpuCore context, Instruction instruction )
        {
            if ( context.PC == instruction.NNN )
            {
                Debug.WriteLine( "JP: Branch to self" );
            }

            context.PC = instruction.NNN;
            return false;
        }

        /// <summary>
        /// 2nnn - CALL addr
        ///Call subroutine at nnn.
        ///
        ///The interpreter increments the stack pointer, then puts the current PC on the top of the stack. The PC is then set to nnn.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="instruction"></param>
        /// <returns></returns>
        private static bool CALL( CpuCore context, Instruction instruction )
        {
            if ( context.SP + 1 >= context.Stack.Length )
#if DEBUG
                throw new StackOverflowException();
#else
                return true;
#endif

            context.Stack[ ++context.SP ] = context.PC;
            context.PC = instruction.NNN;
            return false;
        }

        /// <summary>
        /// 3xkk - SE Vx, byte
        ///Skip next instruction if Vx = kk.
        ///
        ///The interpreter compares register Vx to kk, and if they are equal, increments the program counter by 2.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="instruction"></param>
        /// <returns></returns>
        private static bool SEIMM( CpuCore context, Instruction instruction )
        {
            if ( context.GPR[instruction.X] == instruction.KK )
                context.PC += Instruction.SIZE;

            return true;
        }

        /// <summary>
        /// 4xkk - SNE Vx, byte
        ///Skip next instruction if Vx != kk.
        ///
        ///The interpreter compares register Vx to kk, and if they are not equal, increments the program counter by 2.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="instruction"></param>
        /// <returns></returns>
        private static bool SNEIMM( CpuCore context, Instruction instruction )
        {
            if ( context.GPR[instruction.X] != instruction.KK )
                context.PC += Instruction.SIZE;

            return true;
        }

        /// <summary>
        /// 
        ///5xy0 - SE Vx, Vy
        ///Skip next instruction if Vx = Vy.
        ///
        ///The interpreter compares register Vx to register Vy, and if they are equal, increments the program counter by 2.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="instruction"></param>
        /// <returns></returns>
        private static bool SE( CpuCore context, Instruction instruction )
        {
            if ( context.GPR[instruction.X] == context.GPR[instruction.Y] )
                context.PC += Instruction.SIZE;

            return true;
        }

        /// <summary>
        /// 6xkk - LD Vx, byte
        ///Set Vx = kk.
        ///
        ///The interpreter puts the value kk into register Vx.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="instruction"></param>
        /// <returns></returns>
        private static bool LDIMM( CpuCore context, Instruction instruction )
        {
            context.GPR[instruction.X] = instruction.KK;
            return true;
        }

        /// <summary>
        /// 
        ///7xkk - ADD Vx, byte
        ///Set Vx = Vx + kk.
        ///
        ///Adds the value kk to the value of register Vx, then stores the result in Vx. 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="instruction"></param>
        /// <returns></returns>
        private static bool ADDIMM( CpuCore context, Instruction instruction )
        {
            context.GPR[instruction.X] += instruction.KK;
            return true;
        }

        /// <summary>
        /// 8xy0 - LD Vx, Vy
        ///Set Vx = Vy.
        ///
        ///Stores the value of register Vy in register Vx.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="instruction"></param>
        /// <returns></returns>
        private static bool LD( CpuCore context, Instruction instruction )
        {
            context.GPR[ instruction.X ] = context.GPR[ instruction.Y ];
            return true;
        }

        /// <summary>
        /// 8xy1 - OR Vx, Vy
        ///Set Vx = Vx OR Vy.
        ///
        /// Performs a bitwise OR on the values of Vx and Vy, then stores the result in Vx. 
        /// A bitwise OR compares the corrseponding bits from two values, and if either bit is 1, then the same bit in the result is also 1. Otherwise, it is 0. 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="instruction"></param>
        /// <returns></returns>
        private static bool OR( CpuCore context, Instruction instruction )
        {
            context.GPR[ instruction.X ] |= context.GPR[ instruction.Y ];
            return true;
        }

        /// <summary>
        /// 8xy2 - AND Vx, Vy
        ///Set Vx = Vx AND Vy.
        ///
        ///Performs a bitwise AND on the values of Vx and Vy, then stores the result in Vx. A bitwise AND compares the corrseponding bits from two values, and if both bits are 1, then the same bit in the result is also 1. Otherwise, it is 0. 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="instruction"></param>
        /// <returns></returns>
        private static bool AND( CpuCore context, Instruction instruction )
        {
            context.GPR[instruction.X] &= context.GPR[instruction.Y];
            return true;
        }

        /// <summary>
        /// 
        ///8xy3 - XOR Vx, Vy
        ///Set Vx = Vx XOR Vy.
        ///
        ///Performs a bitwise exclusive OR on the values of Vx and Vy, then stores the result in Vx. An exclusive OR compares the corrseponding bits from two values, and if the bits are not both the same, then the corresponding bit in the result is set to 1. Otherwise, it is 0. 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="instruction"></param>
        /// <returns></returns>
        private static bool XOR( CpuCore context, Instruction instruction )
        {
            context.GPR[instruction.X] ^= context.GPR[instruction.Y];
            return true;
        }

        /// <summary>
        /// 8xy4 - ADD Vx, Vy
        ///Set Vx = Vx + Vy, set VF = carry.
        ///
        ///The values of Vx and Vy are added together. If the result is greater than 8 bits (i.e., &gt; 255,) VF is set to 1, otherwise 0. Only the lowest 8 bits of the result are kept, and stored in Vx.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="instruction"></param>
        /// <returns></returns>
        private static bool ADD( CpuCore context, Instruction instruction )
        {
            var res = context.GPR[instruction.X] + context.GPR[instruction.Y];

            if ( res > byte.MaxValue )
                context.VF = 1;
            else
                context.VF = 0;

            context.GPR[instruction.X] = ( byte )res;
            return true;
        }

        /// <summary>
        /// 8xy5 - SUB Vx, Vy
        ///Set Vx = Vx - Vy, set VF = NOT borrow.
        ///
        ///If Vx &gt; Vy, then VF is set to 1, otherwise 0. Then Vy is subtracted from Vx, and the results stored in Vx.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="instruction"></param>
        /// <returns></returns>
        private static bool SUB( CpuCore context, Instruction instruction )
        {
            var x = instruction.X;
            var y = instruction.Y;

            if ( context.GPR[ x ] > context.GPR[ y ] )
            {
                context.VF = 1;
            }
            else
            {
                context.VF = 0;
            }

            context.GPR[ x ] -= context.GPR[ y ];
            return true;
        }

        /// <summary>
        /// 8xy6 - SHR Vx {, Vy}
        ///Set Vx = Vx SHR 1.
        ///
        ///If the least-significant bit of Vx is 1, then VF is set to 1, otherwise 0. Then Vx is divided by 2.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="instruction"></param>
        /// <returns></returns>
        private static bool SHR( CpuCore context, Instruction instruction )
        {
            var x = instruction.X;
            if ( ( context.GPR[ x ] & 1 ) == 1 )
            {
                context.VF = 1;
            }
            else
            {
                context.VF = 0;
            }

            context.GPR[ x ] /= 2;
            return true;
        }

        /// <summary>
        /// 8xy7 - SUBN Vx, Vy
        ///Set Vx = Vy - Vx, set VF = NOT borrow.
        ///
        ///If Vy &gt; Vx, then VF is set to 1, otherwise 0. Then Vx is subtracted from Vy, and the results stored in Vx.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="instruction"></param>
        /// <returns></returns>
        private static bool SUBN( CpuCore context, Instruction instruction )
        {
            var x = instruction.X;
            var y = instruction.Y;

            if ( context.GPR[ y ] > context.GPR[ x ] )
            {
                context.VF = 1;
            }
            else
            {
                context.VF = 0;
            }

            context.GPR[ x ] = ( byte ) ( context.GPR[ y ] - context.GPR[ x ] );
            return true;
        }

        /// <summary>
        /// 8xyE - SHL Vx {, Vy}
        ///Set Vx = Vx SHL 1.
        ///
        ///If the most-significant bit of Vx is 1, then VF is set to 1, otherwise to 0. Then Vx is multiplied by 2.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="instruction"></param>
        /// <returns></returns>
        private static bool SHL( CpuCore context, Instruction instruction )
        {
            var x = instruction.X;
            if ( ( context.GPR[x] & 0x80 ) == 0x80 )
            {
                context.VF = 1;
            }
            else
            {
                context.VF = 0;
            }

            context.GPR[x] *= 2;
            return true;
        }

        /// <summary>
        /// 9xy0 - SNE Vx, Vy
        ///Skip next instruction if Vx != Vy.
        ///
        ///The values of Vx and Vy are compared, and if they are not equal, the program counter is increased by 2.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="instruction"></param>
        /// <returns></returns>
        private static bool SNE( CpuCore context, Instruction instruction )
        {
            if ( context.GPR[instruction.X] != context.GPR[instruction.Y] )
                context.PC += Instruction.SIZE;

            return true;
        }

        /// <summary>
        /// Annn - LD I, addr
        ///Set I = nnn.
        ///
        ///The value of register I is set to nnn.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="instruction"></param>
        /// <returns></returns>
        private static bool LDI( CpuCore context, Instruction instruction )
        {
            context.I = instruction.NNN;
            return true;
        }

        /// <summary>
        /// Bnnn - JP V0, addr
        ///Jump to location nnn + V0.
        ///
        ///The program counter is set to nnn plus the value of V0.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="instruction"></param>
        /// <returns></returns>
        private static bool JPV0( CpuCore context, Instruction instruction )
        {
            var pc = ( ushort ) ( instruction.NNN + context.V0 );
            if ( context.PC == pc )
            {
                Debug.WriteLine( "JPV0: Branch to self" );
            }

            context.PC = pc;
            return false;
        }

        /// <summary>
        /// Cxkk - RND Vx, byte
        ///Set Vx = random byte AND kk.
        ///
        ///The interpreter generates a random number from 0 to 255, which is then ANDed with the value kk. The results are stored in Vx. See instruction 8xy2 for more information on AND.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="instruction"></param>
        /// <returns></returns>
        private static bool RND( CpuCore context, Instruction instruction )
        {
            context.GPR[ instruction.X ] = ( byte ) ( sRandom.Next( 0, 256 ) & instruction.KK );
            return true;
        }

        /// <summary>
        /// Dxyn - DRW Vx, Vy, nibble
        /// Display n-byte sprite starting at memory location I at (Vx, Vy), set VF = collision.
        ///
        /// The interpreter reads n bytes from memory, starting at the address stored in I. 
        /// These bytes are then displayed as sprites on screen at coordinates (Vx, Vy). Sprites are XORed onto the existing screen. 
        /// If this causes any pixels to be erased, VF is set to 1, otherwise it is set to 0. 
        /// If the sprite is positioned so part of it is outside the coordinates of the display, it wraps around to the opposite side of the screen. 
        /// See instruction 8xy3 for more information on XOR, and section 2.4, Display, for more information on the Chip-8 screen and sprites.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="instruction"></param>
        /// <returns></returns>
        private static bool DRW( CpuCore context, Instruction instruction )
        {
            var x = context.GPR[ instruction.X ];
            var y = context.GPR[ instruction.Y ];
            var bytes = context.Memory.ReadBytes( context.I, instruction.N );

            if ( context.Display.DrawSprite(x, y, bytes) )
            {
                context.VF = 1;
            }
            else
            {
                context.VF = 0;
            }

            return true;
        }

        /// <summary>
        /// 
        ///Ex9E - SKP Vx
        ///Skip next instruction if key with the value of Vx is pressed.
        ///
        ///Checks the keyboard, and if the key corresponding to the value of Vx is currently in the down position, PC is increased by 2.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="instruction"></param>
        /// <returns></returns>
        private static bool SKP( CpuCore context, Instruction instruction )
        {
            var key = context.GPR[ instruction.X ];
            if ( context.Input.IsKeyDown( ( Key ) key ) )
                context.PC += Instruction.SIZE;

            return true;
        }

        /// <summary>
        /// ExA1 - SKNP Vx
        ///Skip next instruction if key with the value of Vx is not pressed.
        ///
        ///Checks the keyboard, and if the key corresponding to the value of Vx is currently in the up position, PC is increased by 2.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="instruction"></param>
        /// <returns></returns>
        private static bool SKNP( CpuCore context, Instruction instruction )
        {
            var key = context.GPR[instruction.X];
            if ( !context.Input.IsKeyDown( ( Key )key ) )
                context.PC += Instruction.SIZE;

            return true;
        }

        /// <summary>
        /// Fx07 - LD Vx, DT
        ///Set Vx = delay timer value.
        ///
        ///The value of DT is placed into Vx.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="instruction"></param>
        /// <returns></returns>
        private static bool LDGDT( CpuCore context, Instruction instruction )
        {
            context.GPR[ instruction.X ] = context.DT;
            return true;
        }

        /// <summary>
        /// Fx0A - LD Vx, K
        ///Wait for a key press, store the value of the key in Vx.
        ///
        ///All execution stops until a key is pressed, then the value of that key is stored in Vx.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="instruction"></param>
        /// <returns></returns>
        private static bool LDK( CpuCore context, Instruction instruction )
        {
            var key = context.Input.WaitUntilKeyDown();
            context.GPR[instruction.X] = ( byte )key;
            return true;
        }

        /// <summary>
        /// Fx15 - LD DT, Vx
        ///Set delay timer = Vx.
        ///
        ///DT is set equal to the value of Vx.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="instruction"></param>
        /// <returns></returns>
        private static bool LDSDT( CpuCore context, Instruction instruction )
        {
            context.DT.Value = context.GPR[ instruction.X ];
            return true;
        }

        /// <summary>
        /// Fx18 - LD ST, Vx
        ///Set sound timer = Vx.
        ///
        ///ST is set equal to the value of Vx.
        ///
        /// </summary>
        /// <param name="context"></param>
        /// <param name="instruction"></param>
        /// <returns></returns>
        private static bool LDSST( CpuCore context, Instruction instruction )
        {
            context.ST.Value = context.GPR[instruction.X];
            return true;
        }

        /// <summary>
        /// Fx1E - ADD I, Vx
        ///Set I = I + Vx.
        ///
        ///The values of I and Vx are added, and the results are stored in I.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="instruction"></param>
        /// <returns></returns>
        private static bool ADDI( CpuCore context, Instruction instruction )
        {
            context.I += context.GPR[ instruction.X ];
            return true;
        }

        /// <summary>
        /// Fx29 - LD F, Vx
        ///Set I = location of sprite for digit Vx.
        ///
        ///The value of I is set to the location for the hexadecimal sprite corresponding to the value of Vx. See section 2.4, Display, for more information on the Chip-8 hexadecimal font.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="instruction"></param>
        /// <returns></returns>
        private static bool LDSPR( CpuCore context, Instruction instruction )
        {
            context.I = ( ushort ) ( context.GPR[ instruction.X ] * Constants.FONT_GLYPH_BYTE_SIZE );
            return true;
        }

        /// <summary>
        /// Fx33 - LD B, Vx
        ///Store BCD representation of Vx in memory locations I, I+1, and I+2.
        ///
        ///The interpreter takes the decimal value of Vx, and places the hundreds digit in memory at location in I, the tens digit at location I+1, and the ones digit at location I+2.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="instruction"></param>
        /// <returns></returns>
        private static bool LDBCD( CpuCore context, Instruction instruction )
        {
            var vx = context.GPR[ instruction.X ];
            var hundreds = ( byte )( ( vx / 100 ) % 10 );
            var tens = ( byte ) ( vx / 10 );
            var ones = ( byte ) ( vx % 10 );
            context.Memory.WriteByte( context.I, hundreds );
            context.Memory.WriteByte( (ushort)(context.I + 1), tens );
            context.Memory.WriteByte( (ushort)(context.I + 2), ones );
            return true;
        }

        /// <summary>
        /// Fx55 - LD [I], Vx
        ///Store registers V0 through Vx in memory starting at location I.
        ///
        ///The interpreter copies the values of registers V0 through Vx into memory, starting at the address in I.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="instruction"></param>
        /// <returns></returns>
        private static bool LDRW( CpuCore context, Instruction instruction )
        {
            var count = instruction.X + 1;
            for ( int i = 0; i < count; i++ )
                context.Memory.WriteByte( ( ushort ) ( context.I + i ), context.GPR[ i ] );

            return true;
        }

        /// <summary>
        /// Fx65 - LD Vx, [I]
        ///Read registers V0 through Vx from memory starting at location I.
        ///
        ///The interpreter reads values from memory starting at location I into registers V0 through Vx.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="instruction"></param>
        /// <returns></returns>
        private static bool LDRS( CpuCore context, Instruction instruction )
        {
            var count = instruction.X + 1;
            for ( int i = 0; i < count; i++ )
                context.GPR[ i ] = context.Memory.ReadByte( ( ushort ) ( context.I + i ) );

            return true;
        }
    }
}

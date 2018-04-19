using System;
using Chip8Emu.Core;

namespace Chip8Emu.CLI
{
    internal static class Program
    {
        [STAThread]
        private static void Main( string[] args )
        {
#if !DEBUG
            if ( args.Length == 0 )
            {
                Console.WriteLine( "Missing path to rom file to load." );
                return;
            }
#endif
                

            var emulator = new Emulator( new EmulatorConfig() );

#if DEBUG
            emulator.LoadProgram( @"D:\Users\smart\Downloads\c8games\INVADERS" );
#else
            emulator.LoadProgram( args[0] );
#endif

            emulator.Run();
        }
    }
}

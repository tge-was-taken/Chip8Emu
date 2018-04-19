using System;

namespace Chip8Emu.Core.Display.Backends.Console
{
    public class ConsoleDisplayDevice : ConsoleDisplayDeviceBase
    {
        public ConsoleDisplayDevice()
        {
            System.Console.SetWindowSize( Constants.WIDTH + 1, Constants.HEIGHT + 1 );
            System.Console.SetBufferSize( Constants.WIDTH + 1, Constants.HEIGHT + 1 );
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.CursorVisible = false;
        }

        /// <summary>
        /// Start of frame initialization code.
        /// </summary>
        public override void OnStartFrame()
        {
        }

        /// <summary>
        /// End of frame code.
        /// </summary>
        public override void OnFinishFrame()
        {
        }

        /// <summary>
        /// Clears the screen.
        /// </summary>
        public override void Clear()
        {
            System.Console.Clear();
            base.Clear();
        }

        protected override void DrawPixel( int x, int y, string character )
        {
            System.Console.SetCursorPosition( x, y );
            System.Console.Write( character );
        }
    }
}

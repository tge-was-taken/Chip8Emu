using System;

namespace Chip8Emu.Core.Display.Backends.Console
{
    public class ConsoleDisplayDeviceDoubleBuffered : ConsoleDisplayDeviceBase
    {
        private static readonly string sZeroData = new string( ' ', Constants.WIDTH * Constants.HEIGHT );
        private IntPtr mFrontBuffer;
        private IntPtr mBackBuffer;
        private bool mShouldFlip;

        public ConsoleDisplayDeviceDoubleBuffered()
        {
            mBackBuffer = InitializeBuffer();
            mFrontBuffer = InitializeBuffer();
        }

        /// <summary>
        /// Start of frame initialization code.
        /// </summary>
        public override void OnStartFrame()
        {
            ClearBuffer( mBackBuffer );
            mShouldFlip = false;
        }

        /// <summary>
        /// End of frame code.
        /// </summary>
        public override void OnFinishFrame()
        {
            if ( !mShouldFlip )
                return;

            // Flush contents to back buffer
            ClearBuffer( mBackBuffer );
            for ( int y = 0; y < PixelBits.Length; y++ )
            {
                for ( int x = 0; x < PixelBits[y].Length; x++ )
                {
                    if ( PixelBits[ y ][ x ] )
                        DrawPixel( x, y, "0" );
                }
            }

            // Set active buffer to the back buffer
            PInvoke.Kernel32.SetConsoleActiveScreenBuffer( mBackBuffer );

            // The back buffer is now the front buffer, so we swap them
            var temp = mFrontBuffer;
            mFrontBuffer = mBackBuffer;
            mBackBuffer = temp;
        }

        /// <summary>
        /// Clears the screen.
        /// </summary>
        public override void Clear()
        {
            ClearBuffer( mBackBuffer );
            base.Clear();
        }

        public override bool DrawSprite( byte baseX, byte baseY, byte[] data )
        {
            mShouldFlip = true;
            return base.DrawSprite( baseX, baseY, data );
        }

        private IntPtr InitializeBuffer()
        {
            var buffer = CreateConsoleScreenBuffer();
            PInvoke.Kernel32.SetConsoleActiveScreenBuffer( buffer );
            System.Console.SetWindowSize( Constants.WIDTH + 1, Constants.HEIGHT + 1 );
            System.Console.SetBufferSize( Constants.WIDTH + 1, Constants.HEIGHT + 1 );
            return buffer;
        }

        protected override void DrawPixel( int x, int y, string character )
        {
            DrawPixel( x, y, character, ConsoleColor.White );
        }

        private void DrawPixel( int x, int y, string character, ConsoleColor color )
        {
            SetConsoleForegroundColor( color );
            SetConsoleCursorPosition( x, y );
            WriteConsole( mBackBuffer, character );
            SetConsoleForegroundColor( ConsoleColor.Gray );
        }

        private void ClearBuffer( IntPtr buffer )
        {
            WriteConsole( buffer, sZeroData );
        }

        private bool SetConsoleForegroundColor( ConsoleColor color )
        {
            return PInvoke.Kernel32.SetConsoleTextAttribute( mBackBuffer, ( PInvoke.Kernel32.CharacterAttributesFlags )color );
        }

        private bool SetConsoleBackgroundColor( ConsoleColor color )
        {
            return PInvoke.Kernel32.SetConsoleTextAttribute( mBackBuffer, ( PInvoke.Kernel32.CharacterAttributesFlags )( ( int )color << 4 ) );
        }

        private bool SetConsoleCursorPosition( int x, int y )
        {
            return PInvoke.Kernel32.SetConsoleCursorPosition( mBackBuffer, new PInvoke.COORD { X = ( short )x, Y = ( short )y } );
        }

        private static IntPtr CreateConsoleScreenBuffer()
        {
            return PInvoke.Kernel32.CreateConsoleScreenBuffer(
                PInvoke.Kernel32.ACCESS_MASK.GenericRight.GENERIC_READ | PInvoke.Kernel32.ACCESS_MASK.GenericRight.GENERIC_WRITE,
                PInvoke.Kernel32.FileShare.FILE_SHARE_READ | PInvoke.Kernel32.FileShare.FILE_SHARE_WRITE,
                IntPtr.Zero,
                PInvoke.Kernel32.ConsoleScreenBufferFlag.CONSOLE_TEXTMODE_BUFFER,
                IntPtr.Zero );
        }

        private static unsafe bool WriteConsole( IntPtr buffer, string text )
        {
            bool returnValue;

            fixed ( void* lpBuffer = text )
            {
                int lpNumberOfCharsWritten;
                returnValue = PInvoke.Kernel32.WriteConsole( buffer, lpBuffer, text.Length, out lpNumberOfCharsWritten, IntPtr.Zero );
            }

            return returnValue;
        }
    }
}
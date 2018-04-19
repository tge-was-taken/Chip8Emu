namespace Chip8Emu.Core.Display.Backends.Console
{
    public abstract class ConsoleDisplayDeviceBase : IDisplayDevice
    {
        protected readonly bool[][] PixelBits;

        protected ConsoleDisplayDeviceBase()
        {
            PixelBits = new bool[Constants.HEIGHT][];
            for ( int i = 0; i < PixelBits.Length; i++ )
                PixelBits[i] = new bool[Constants.WIDTH];
        }

        /// <summary>
        /// Start of frame initialization code.
        /// </summary>
        public abstract void OnStartFrame();

        /// <summary>
        /// End of frame code.
        /// </summary>
        public abstract void OnFinishFrame();

        /// <summary>
        /// Clears the screen.
        /// </summary>
        public virtual void Clear()
        {
            // Reset pixel bits
            foreach ( var row in PixelBits )
            {
                for ( int x = 0; x < row.Length; x++ )
                    row[x] = false;
            }
        }

        /// <summary>
        /// Draw a sprite to the frame buffer.
        /// </summary>
        /// <param name="baseX"></param>
        /// <param name="baseY"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual bool DrawSprite( byte baseX, byte baseY, byte[] data )
        {
            var collision = false;
            for ( int yOffset = 0; yOffset < data.Length; yOffset++ )
            {
                var row = data[yOffset];

                for ( int xOffset = 0; xOffset < 8; ++xOffset )
                {
                    // Check if current pixel is set
                    var mask = 1 << ( 7 - xOffset );
                    var set = ( row & mask ) == mask;
                    if ( !set )
                        continue;

                    // Calculate effective x & y
                    var x = ( baseX + xOffset ) % Constants.WIDTH;
                    var y = ( baseY + yOffset ) % Constants.HEIGHT;

                    // If pixel is already set, that means it'll be cleared -> collision flag
                    var originalPixel = PixelBits[y][x];
                    if ( originalPixel )
                        collision = true;

                    // Toggle pixel bit
                    PixelBits[y][x] ^= true;

                    // Draw the pixel
                    DrawPixel( x, y, PixelBits[ y ][ x ] ? "0" : " " );
                }
            }

            return collision;
        }

        protected abstract void DrawPixel( int x, int y, string character );
    }
}
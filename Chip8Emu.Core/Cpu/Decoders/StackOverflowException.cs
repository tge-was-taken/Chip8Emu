using System;
using System.Runtime.Serialization;

namespace Chip8Emu.Core.Cpu.Decoders
{
    [Serializable]
    internal class StackOverflowException : Exception
    {
        public StackOverflowException()
        {
        }

        public StackOverflowException( string message ) : base( message )
        {
        }

        public StackOverflowException( string message, Exception innerException ) : base( message, innerException )
        {
        }

        protected StackOverflowException( SerializationInfo info, StreamingContext context ) : base( info, context )
        {
        }
    }
}
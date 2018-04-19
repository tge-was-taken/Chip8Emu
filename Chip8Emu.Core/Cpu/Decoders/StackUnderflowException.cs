using System;
using System.Runtime.Serialization;

namespace Chip8Emu.Core.Cpu.Decoders
{
    [Serializable]
    internal class StackUnderflowException : Exception
    {
        public StackUnderflowException()
        {
        }

        public StackUnderflowException( string message ) : base( message )
        {
        }

        public StackUnderflowException( string message, Exception innerException ) : base( message, innerException )
        {
        }

        protected StackUnderflowException( SerializationInfo info, StreamingContext context ) : base( info, context )
        {
        }
    }
}
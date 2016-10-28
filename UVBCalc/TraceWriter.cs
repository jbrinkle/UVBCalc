using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UVBCalc
{
    internal class TraceWriter : IDisposable
    {
        private readonly Stream _stream;
        private readonly StreamWriter _streamWriter;

        public enum TraceLevel
        {
            None,
            Errors,
            Information,
            Verbose
        }

        public TraceLevel Level { get; set; }

        public TraceWriter()
        {
            
        }

        public TraceWriter(Stream stream)
        {
            if (stream == null || !stream.CanWrite)
            {
                throw new ArgumentNullException();
            }

            _stream = stream;
            _streamWriter = new StreamWriter(stream)
            {
                AutoFlush = true
            };

            Level = TraceLevel.Errors;
        }

        public virtual void WriteError(string message)
        {
            if (Level < TraceLevel.Errors)
            {
                return;
            }

            _streamWriter?.WriteLine(message);
        }

        public virtual void WriteInformation(string message)
        {
            if (Level < TraceLevel.Information)
            {
                return;
            }

            _streamWriter?.WriteLine(message);
        }

        public virtual void WriteVerbose(string message)
        {
            if (Level < TraceLevel.Verbose)
            {
                return;
            }

            _streamWriter?.WriteLine(message);
        }

        public void Dispose()
        {
            _streamWriter?.Dispose();
            _stream?.Dispose();
        }
    }
}

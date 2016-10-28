using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using FluentAssertions;

namespace UVBCalc.Tests
{
    [TestClass]
    public class TraceWriterTests
    {
        [TestMethod]
        public void TW_LevelAtNoneWriteErrorDoesNotWrite()
        {
            // arrange
            var mem = new MemoryStream();
            var tw = new TraceWriter(mem)
            {
                Level = TraceWriter.TraceLevel.None
            };

            // act
            tw.WriteError("message");

            // assert
            mem.Length.Should().Be(0);

            tw.Dispose();
        }

        [TestMethod]
        public void TW_LevelAtErrorWriteVerboseDoesNotWrite()
        {
            // arrange
            var mem = new MemoryStream();
            var tw = new TraceWriter(mem);

            // act
            tw.WriteVerbose("message");

            // assert
            mem.Length.Should().Be(0);

            tw.Dispose();
        }

        [TestMethod]
        public void TW_LevelAtErrorWriteErrorWrites()
        {
            // arrange
            var mem = new MemoryStream();
            var tw = new TraceWriter(mem);

            // act
            tw.WriteError("message");

            // assert
            mem.Length.Should().Be(9, "message = 7, 2 more for newline");

            tw.Dispose();
        }
    }
}

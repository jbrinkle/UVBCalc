using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using Moq;

namespace UVBCalc.Tests
{
    [TestClass]
    public class UsNavalSolarAltitudeServiceTests
    {
        private Mock<IWebRequestHelper> _mockWebReqHelper;
        private Mock<TraceWriter> _mockTracer;
        private UsNavalSolarAltitudeService _subject;

        [TestInitialize]
        public void Init()
        {
            _mockWebReqHelper = new Mock<IWebRequestHelper>();
            _mockTracer = new Mock<TraceWriter>();

            _subject = new UsNavalSolarAltitudeService(_mockTracer.Object, _mockWebReqHelper.Object, testMode: true);
        }

        [TestMethod]
        public void GetOneDayWithOnePeriodSucceeds()
        {
            // arrange
            _mockWebReqHelper.Setup(h => h.GetResponseBodyForUri(It.IsAny<Uri>()))
                             .Returns(@"
                             05:00   48.6
                             06:00   48.8
                             07:00   49.0
                             08:00   49.2
                             09:00   49.4
                             10:00   49.6
                             11:00   49.8
                             12:00   50.0
                             13:00   49.8
                             14:00   49.6
                             15:00   49.4
                             16:00   49.2
                             17:00   49.0
                             18:00   48.8
                             19:00   48.6
                             ");

            // act
            var entry = _subject.GetOneDay(DateTime.Today, "monroe", "wa");

            // assert
            entry.Duration.Should().Be(60);
            entry.EntryDate.Should().Be(DateTime.Today);
            entry.BeginOffset.Should().Be(TimeSpan.FromHours(12));
            entry.EndOffset.Should().Be(TimeSpan.FromHours(13));
        }

        [TestMethod]
        public void GetOneDayWithMultiplePeriodsSucceeds()
        {
            // arrange
            _mockWebReqHelper.Setup(h => h.GetResponseBodyForUri(It.IsAny<Uri>()))
                             .Returns(@"
                             05:00   48.6
                             06:00   49.0
                             07:00   49.4
                             08:00   49.8
                             09:00   50.2
                             10:00   50.2
                             11:00   50.2
                             12:00   49.8
                             13:00   49.4
                             14:00   49.0
                             15:00   48.6
                             16:00   48.2
                             ");

            // act
            var entry = _subject.GetOneDay(DateTime.Today, "monroe", "wa");

            // assert
            entry.Duration.Should().Be(180);
            entry.EntryDate.Should().Be(DateTime.Today);
            entry.BeginOffset.Should().Be(TimeSpan.FromHours(9));
            entry.EndOffset.Should().Be(TimeSpan.FromHours(12));
        }

        [TestMethod]
        public void GetAYearOfEntriesSucceeds()
        {
            // arrange
            _mockWebReqHelper.Setup(h => h.GetResponseBodyForUri(It.IsAny<Uri>()))
                             .Returns(@"
                             05:00   48.6
                             06:00   49.0
                             07:00   49.4
                             08:00   49.8
                             09:00   50.2
                             10:00   50.2
                             11:00   50.2
                             12:00   49.8
                             13:00   49.4
                             14:00   49.0
                             15:00   48.6
                             16:00   48.2
                             ");

            // act
            var entries = _subject.GetOneYear(2016, "some", "where");

            //assert
            entries.Count.Should().Be(366);
        }
    }
}

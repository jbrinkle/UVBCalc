// #define RUN_NETWORK_WEBREQ_TESTS

using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

namespace UVBCalc.Tests
{
    [TestClass]
    public class WebRequestHelperTests
    {
        private MemoryStream _stream;
        private TraceWriter _tracer;
        private WebRequestHelper _subject;

        [TestInitialize]
        public void Init()
        {
            _stream = new MemoryStream();
            _tracer = new TraceWriter(_stream);
            _subject = new WebRequestHelper(_tracer);
        }

        [TestMethod]
        public void WRH_PartialUriThrows()
        {
            // arrange
            var uri = new Uri("../Path/to/somewhere/", UriKind.Relative);

            // act
            Action action = () =>
            {
                _subject.GetResponseBodyForUri(uri);
            };

            // assert
            action.ShouldThrow<ArgumentException>();
        }

#if RUN_NETWORK_WEBREQ_TESTS
        [TestMethod]
        public void WRH_BingcomSucceeds()
        {
            // arrange
            var uri = new Uri("http://www.bing.com/");
            _tracer.Level = TraceWriter.TraceLevel.Verbose;

            // act
            var body = _subject.GetResponseBodyForUri(uri);

            // assert
            body.Should().NotBeNullOrEmpty();
            _stream.Length.Should().Be(3 + 3 + uri.ToString().Length + 2);
        }

        [TestMethod]
        public void WRH_BingComWithExtraPathFails()
        {
            // arrange
            var uri = new Uri("http://www.bing.com/with/an/extra/nonexistent/path");

            // act
            var body = _subject.GetResponseBodyForUri(uri);

            // assert
            body.Should().BeNull();
            _stream.Length.Should().BeGreaterOrEqualTo(3 + 3 + uri.ToString().Length + 2);
        }

        [TestMethod]
        public void WRH_NotARealSitecomFails()
        {
            // arrange
            var uri = new Uri("http://www.NotARealSiteBlahBlahBlahBlah.com/");

            // act
            var body = _subject.GetResponseBodyForUri(uri);

            // assert
            body.Should().BeNull();
            _stream.Length.Should().BeGreaterOrEqualTo(3 + 3 + uri.ToString().Length + 2);
        }
#endif
    }
}

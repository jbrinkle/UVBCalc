using System;
using System.IO;
using System.Net;

namespace UVBCalc
{
    internal interface IWebRequestHelper
    {
        /// <summary>
        /// Issues a web request to <paramref name="uri"/> and returns the body of the resource
        /// </summary>
        /// <param name="uri">The Uri of the web resource</param>
        /// <returns>String containing response body, or null if anything went wrong</returns>
        string GetResponseBodyForUri(Uri uri);
    }

    internal class WebRequestHelper : IWebRequestHelper
    {
        private TraceWriter _tracer;

        public WebRequestHelper(TraceWriter traceWriter)
        {
            _tracer = traceWriter;
        }

        public string GetResponseBodyForUri(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            if (!uri.IsAbsoluteUri)
            {
                throw new ArgumentException("Absolute Uri's are required to send a request.");
            }

            try
            {
                var request = WebRequest.CreateHttp(uri);
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.143 Safari/537.36";
                request.Accept = "text/html";
                request.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-US,en");

                var response = (HttpWebResponse)request.GetResponse();

                _tracer.WriteVerbose($"{(int)response.StatusCode} - {uri}");

                using (var bodyStream = response.GetResponseStream())
                {
                    if (bodyStream == null)
                    {
                        var trace = (_tracer.Level == TraceWriter.TraceLevel.Verbose
                            ? string.Empty
                            : $"{uri}\r\n") + " -- no response body found";

                        _tracer.WriteError(trace);

                        return null;
                    }

                    var reader = new StreamReader(bodyStream);
                    return reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                _tracer.WriteError($"ERR - {uri}\r\n -- {ex.Message}");
                return null;
            }
        }
    }
}

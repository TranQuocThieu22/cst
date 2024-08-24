using Microsoft.AspNetCore.ResponseCompression;
using System.IO;
using System.IO.Compression;

namespace educlient.Utils
{
    public class GzipCompressionProvider : ICompressionProvider
    {
        public string EncodingName => "gzip";
        public bool SupportsFlush => true;

        public Stream CreateStream(Stream outputStream)
        {
            return new GZipStream(outputStream, CompressionLevel.Optimal);
        }
    }

}

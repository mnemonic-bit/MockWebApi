using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockWebApi.Extension
{
    public static class StreamExtensions
    {

        /// <summary>
        /// Reads the contents of a stream and creates a string from
        /// that contents.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string ReadString(this Stream stream, Encoding? encoding = null)
        {
            encoding ??= Encoding.UTF8;

            using StreamReader reader = new StreamReader(
                stream,
                encoding: encoding,
                detectEncodingFromByteOrderMarks: false,
                leaveOpen: true);

            string value = reader.ReadToEnd();

            return value;
        }

        /// <summary>
        /// Reads asynchronously the contents of a stream and creates
        /// a string from that contents.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static async Task<string> ReadStringAsync(this Stream stream, Encoding? encoding = null)
        {
            encoding ??= Encoding.UTF8;

            using StreamReader reader = new StreamReader(
                stream,
                encoding: encoding,
                detectEncodingFromByteOrderMarks: false,
                leaveOpen: true);

            string value = await reader.ReadToEndAsync();

            return value;
        }

        /// <summary>
        /// Creates a stream which applies the compression methods as given in
        /// the <code>contentEncoding</code>. which is a comma-separated list
        /// of encodings to be applied. All compressions are applied in
        /// the order given in that comma-separated string.
        /// </summary>
        /// <param name="stream">The destination stream which will be filled with the compressed contents</param>
        /// <param name="contentEncodings">The comma-separated list of encodings.</param>
        /// <returns>Returns a stream that represents the compressed encodings.</returns>
        public static Stream CreateCompressionStream(this Stream stream, string contentEncodings)
        {
            // Split the comma-separated list of encodings, and create
            // a compression stream for each, starting with the last one,
            // because the last compression mentioned in the encodings-list
            // will receive the compression result of its predecessor.
            Stream compressionStream = (contentEncodings ?? string.Empty)
                .Split(',')
                .Select(contentEncoding => contentEncoding.Trim())
                .Reverse()
                .Where(x => !string.IsNullOrEmpty(x))
                .Aggregate(stream, CreateSingleCompressionStream);

            return compressionStream;
        }

        /// <summary>
        /// Reads the whole contents of a stream and returns it
        /// as a byte array. This method never returns null. An
        /// exception is thrown if the stream throws an exception
        /// during reading. If not enough memory is available,
        /// an OutOfMemoryException is thrown by the runtime.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns>Returns the stream's contents as byte array.</returns>
        public static byte[] ReadToEnd(this Stream stream)
        {
            const int COPY_BUFFER_SIZE = 32 * 1024;
            byte[] buffer = new byte[COPY_BUFFER_SIZE];

            using var memoryStream = new MemoryStream(COPY_BUFFER_SIZE);

            int numberOfBytesRead = 0;
            while ((numberOfBytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                memoryStream.Write(buffer, 0, numberOfBytesRead);
            }

            return memoryStream.ToArray();
        }

        /// <summary>
        /// Reads asynchronous the whole contents of a stream and returns
        /// it as a byte array. This method never returns null. An exception
        /// is thrown if the stream throws an exception during reading.
        /// If not enough memory is available, an OutOfMemoryException
        /// is thrown by the runtime.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns>Returns the stream's contents as byte array.</returns>
        public static async Task<byte[]> ReadToEndAsync(this Stream stream)
        {
            const int COPY_BUFFER_SIZE = 32 * 1024;
            byte[] buffer = new byte[COPY_BUFFER_SIZE];

            using var memoryStream = new MemoryStream(COPY_BUFFER_SIZE);

            int numberOfBytesRead = 0;
            while ((numberOfBytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await memoryStream.WriteAsync(buffer, 0, numberOfBytesRead);
            }

            return memoryStream.ToArray();
        }

        private static Stream CreateSingleCompressionStream(Stream destStream, string contentEncoding)
        {
            // for content types and encodings, please refer to
            // https://learn.microsoft.com/en-us/aspnet/core/performance/response-compression?view=aspnetcore-7.0#response-compression

            switch (contentEncoding)
            {
                case "br":
                    // Brotli compression
                    destStream = new BrotliStream(destStream, CompressionMode.Compress, true);
                    break;
                case "chunked":
                    //TODO: implement a chunked-stream to wrap the contents stream into.
                    break;
                case "compress":
                    // LZW compression - not implemented, because is has not been in
                    // use until 2003 because of patent law reasons.
                    break;
                case "deflate":
                    // zlib structure with deflate compression
                    destStream = new DeflateStream(destStream, CompressionMode.Compress, true);
                    break;
                case "exi":
                    //TODO: is this a real-life encoding? Its listed in the URL above
                    break;
                case "gzip":
                    // LZ77 compression
                    destStream = new GZipStream(destStream, CompressionMode.Compress, true);
                    break;
                case "identity":
                    // just return destStream as it is, since identity means
                    // 'leave the content as it is'
                    break;
                default:
                    throw new Exception($"Compression '{contentEncoding}' not supported by this server");
            }

            return destStream;
        }

    }
}

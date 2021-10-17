using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MockWebApi.Extension
{
    public static class StreamExtensions
    {

        public static async Task<string> ReadString(this Stream stream, Encoding encoding)
        {
            using StreamReader reader = new StreamReader(
                stream,
                encoding: encoding,
                detectEncodingFromByteOrderMarks: false,
                leaveOpen: true);

            string value = await reader.ReadToEndAsync();

            return value;
        }

    }
}

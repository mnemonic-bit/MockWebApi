using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace MockWebApi.Model
{
    public class HttpContentType
    {

        public string ContentType { get; set; }

        public Encoding CharacterEncoding { get; set; }

        public HttpContentType(string contentType)
        {
            // example of the input parameter:
            //application/json;charset=utf-8

            if (string.IsNullOrEmpty(contentType))
            {
                ContentType = DefaultContentType;
                CharacterEncoding = DefaultCharacterEncoding;
                return;
            }

            CharacterEncoding = DefaultCharacterEncoding;

            string[] characterSet = contentType
                .Split(';')
                .Select(x => x.Trim())
                .ToArray();

            if (characterSet.Length < 1)
            {
                throw new ArgumentException($"Cannot parse given content type '{contentType}'", nameof(contentType));
            }

            ContentType = characterSet[0];

            if (characterSet.Length < 2)
            {
                return;
            }

            string[] encodingName = characterSet[1]
                .Split('=')
                .Select(x => x.Trim())
                .ToArray();

            if (encodingName.Length < 2 || encodingName[0] != "charset")
            {
                return;
            }

            if (TryGetEncoding(encodingName[1].Trim(), out Encoding? encoding))
            {
                CharacterEncoding = encoding;
            }
        }

        public override string ToString()
        {
            return CharacterEncoding.Equals(Encoding.Latin1) ? ContentType : $"{ContentType};charset={CharacterEncoding.HeaderName}";
        }


        private readonly string DefaultContentType = "text/plain";
        private readonly Encoding DefaultCharacterEncoding = Encoding.Latin1;


        private static bool TryGetEncoding(string encodingName, [NotNullWhen(true)] out Encoding? encoding)
        {
            encoding = default;

            try
            {
                encoding = Encoding.GetEncoding(encodingName);
                return true;
            }
            catch
            {
            }

            return false;
        }

    }
}

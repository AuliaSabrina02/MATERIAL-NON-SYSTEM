using System.Security.Cryptography;
using System.Text;

namespace SEMB_ERP.Function
{
    public class Authentication
    {
        public string GenerateCodeVerifier(int length = 43)
        {
            var randomBytes = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Base64UrlEncode(randomBytes);
            //if (length < 43 || length > 128)
            //{
            //    throw new ArgumentOutOfRangeException(nameof(length), "Length must be between 43 and 128");
            //}

            //// Create a byte array to hold the random bytes
            //byte[] randomBytes = new byte[length];
            //rng.GetBytes(randomBytes);

            //// Convert the random bytes to a URL-safe Base64 string and remove characters that aren't allowed
            //string base64String = Convert.ToBase64String(randomBytes)
            //    .TrimEnd('=')
            //    .Replace('+', '-')
            //    .Replace('/', '_');

            //// Ensure the verifier's length is correct; truncate if needed
            //return base64String.Length > length ? base64String.Substring(0, length) : base64String;
        }

        public string GenerateCodeChallenge(string codeVerifier)
        {
            using (var hasher = SHA256.Create())
            {
                var hashed = hasher.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
                return Base64UrlEncode(hashed);
            }
        }

        private string Base64UrlEncode(byte[] input)
        {
            // Replace + and / with - and _ and remove padding
            var base64 = Convert.ToBase64String(input)
                                .TrimEnd('=')
                                .Replace('+', '-')
                                .Replace('/', '_');
            return base64;
        }
        public string MD5Hash(string text)
        {
            MD5 md5 = new MD5CryptoServiceProvider();

            //compute hash from the bytes of text  
            md5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(text));

            //get hash result after compute it  
            byte[] result = md5.Hash;

            StringBuilder strBuilder = new StringBuilder();
            for (int i = 0; i < result.Length; i++)
            {
                //change it into 2 hexadecimal digits  
                //for each byte  
                strBuilder.Append(result[i].ToString("x2"));
            }

            return strBuilder.ToString();
        }
    }
}

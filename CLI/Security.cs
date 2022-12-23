using System.Security.Cryptography;

namespace CLI;

public class Security
{
        internal static byte[] DeriveKey(string password, byte[] salt)
        {
            using var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt, 100101, HashAlgorithmName.SHA256);
            return rfc2898DeriveBytes.GetBytes(32);
        }
        
        internal static byte[] GenerateRandomSalt()
        {
            // Generate a random salt
            var random = new Random();
            var salt = new byte[16];
            random.NextBytes(salt);
            return salt;
        }

        internal static byte[] Encrypt(string message, byte[] key, byte[] salt)
        {
            // Generate a random initialization vector
            var iv = GenerateRandomSalt();

            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            aes.Padding = PaddingMode.PKCS7;

            // Encrypt the message
            using var memoryStream = new MemoryStream();
            using var cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
            
            // for some reason I cannot turn this using statement into a using declaration.
            // I think it has something to do with de cryptoStream
            using (var streamWriter = new StreamWriter(cryptoStream))
            {
                streamWriter.Write(message);
            }

            var content = memoryStream.ToArray();

            // Return the encrypted message, initialization vector, and salt
            return Concatenate(content, iv, salt);
        }

        internal static string Decrypt(byte[] encryptedMessage, byte[] key)
        {
            // Extract the salt and IV from the encrypted message
            var salt = new byte[16];
            var iv = new byte[16];
            var subArray = new byte[encryptedMessage.Length - (salt.Length + iv.Length)];
            Array.Copy(encryptedMessage, encryptedMessage.Length - 16, salt, 0, 16);
            Array.Copy(encryptedMessage, encryptedMessage.Length - 32, iv, 0, 16);
            Array.Copy(encryptedMessage, 0, subArray, 0, subArray.Length);

            // Decrypt the message
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            aes.Padding = PaddingMode.PKCS7;

            using var memoryStream = new MemoryStream(subArray);
            using var cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using StreamReader srDecrypt = new StreamReader(cryptoStream);
            return srDecrypt.ReadToEnd();
        }
        
        
        /// <summary>
        /// Concatenate multiple byte arrays, this way we can store the arrays as a single string
        /// and retrieve the data from the string at a later date.
        /// </summary>
        /// <param name="arrays"></param>
        /// <returns></returns>
        private static byte[] Concatenate(params byte[][] arrays)
        {
            // Concatenate the arrays
            var result = new byte[arrays.Sum(a => a.Length)];
            var offset = 0;
            foreach (var array in arrays)
            {
                Buffer.BlockCopy(array, 0, result, offset, array.Length);
                offset += array.Length;
            }
            return result;
        }
}
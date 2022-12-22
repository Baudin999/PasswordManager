using System.Security.Cryptography;

namespace ConsoleApp
{
    
    class Program
    {

        private static string location;
        
        static void Main(string[] args)
        {
            Console.WriteLine("Read or Write? ");
            var response = Console.ReadLine()?.ToLower().Trim() ?? "";
            
            Console.WriteLine("What is the location of the encrypted files? ");
            location = Console.ReadLine() ?? throw new Exception("No location specified");
            if (!Directory.Exists(location)) throw new Exception("Location does not exist");
            
            
            if (response == "read")
            {
                ReadPassword();
            }
            else if (response == "write")
            {
                WriteNewPassword();
            }
        }

        static void ReadPassword()
        {
            Console.Write("Encryption Password: ");
            var encryptionPw = Console.ReadLine()?.Trim();
            if (encryptionPw is null || encryptionPw.Length < 3) throw new Exception("Invalid encryption password");

            Console.Write("Key: ");
            var key = Console.ReadLine()?.Trim();
            if (key is null || key.Length < 3) throw new Exception("Invalid key");

            var pw = ReadPasswordFromFile(encryptionPw, key);
            
            Console.WriteLine(pw);
        }

        static void WriteNewPassword()
        {
            Console.Write("Encryption Password: ");
            var encryptionPw = Console.ReadLine()?.Trim();
            if (encryptionPw is null || encryptionPw.Length < 3) throw new Exception("Invalid encryption password");

            Console.Write("Key: ");
            var key = Console.ReadLine()?.Trim();
            if (key is null || key.Length < 3) throw new Exception("Invalid key");
            
            Console.Write("Password: ");
            var password = Console.ReadLine()?.Trim();
            if (password is null || password.Length < 3) throw new Exception("Invalid password");

            var salt = GenerateRandomSalt();
            SavePasswordToFile(encryptionPw, key, password, salt);
        }

        static void SavePasswordToFile(string encryptionPassword, string key, string password, byte[] salt)
        {
            var volumePath = location;
            var fileName = key + ".da";
            var filePath = Path.Combine(volumePath, fileName);
            
            var encryptionKey = DeriveKey(encryptionPassword, salt);
            var encryptedPassword = Encrypt(password, encryptionKey, salt);
            
            // Save encrypted data to file
            using (FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate))
            {
                fileStream.Write(encryptedPassword, 0, encryptedPassword.Length);
                fileStream.Close();
            }
        }

        static string ReadPasswordFromFile(string encryptionPassword, string key)
        {
            var volumePath = location;
            var fileName = key + ".da";
            var filePath = Path.Combine(volumePath, fileName);
            var encryptedMessage = File.ReadAllBytes(filePath);
            
            var salt = new byte[16];
            Array.Copy(encryptedMessage, encryptedMessage.Length - 16, salt, 0, 16);

            var encryptionKey = DeriveKey(encryptionPassword, salt);
            var password = Decrypt(encryptedMessage, encryptionKey);

            return password;
        }

        static byte[] DeriveKey(string password, byte[] salt)
        {
            using (var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256))
            {
                return rfc2898DeriveBytes.GetBytes(32);
            }
        }
        
        static byte[] GenerateRandomSalt()
        {
            // Generate a random salt
            var random = new Random();
            var salt = new byte[16];
            random.NextBytes(salt);
            return salt;
        }

        static byte[] Encrypt(string message, byte[] key, byte[] salt)
        {
            // Generate a random initialization vector
            var iv = GenerateRandomSalt();

            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Padding = PaddingMode.PKCS7;

                // Encrypt the message
                using (var memoryStream = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        using (var streamWriter = new StreamWriter(cryptoStream))
                        {
                            streamWriter.Write(message);
                        }
                    }
                    var content = memoryStream.ToArray();

                    // Return the encrypted message, initialization vector, and salt
                    return Concatenate(content, iv, salt);
                }
            }
        }



        
        static string Decrypt(byte[] encryptedMessage, byte[] key)
        {
            // Extract the salt and IV from the encrypted message
            var salt = new byte[16];
            var iv = new byte[16];
            var subArray = new byte[encryptedMessage.Length - (salt.Length + iv.Length)];
            Array.Copy(encryptedMessage, encryptedMessage.Length - 16, salt, 0, 16);
            Array.Copy(encryptedMessage, encryptedMessage.Length - 32, iv, 0, 16);
            Array.Copy(encryptedMessage, 0, subArray, 0, subArray.Length);

            // Decrypt the message
            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Padding = PaddingMode.PKCS7;

                using (var memoryStream = new MemoryStream(subArray))
                {
                    using (var cryptoStream =
                           new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(cryptoStream))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
        
        static byte[] Concatenate(params byte[][] arrays)
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
}
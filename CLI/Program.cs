using System.Runtime.CompilerServices;
using System.Security.Cryptography;

[assembly: InternalsVisibleTo("Tests")]
namespace CLI
{
    
    internal static class Program
    {

        private static JsonConfig config = new JsonConfig();

        internal static void Main(string[] args)
        {
            // the volumePath is the location where the passwords are stored,
            // I personally prefer a USB drive, but it can easily be a 
            // encrypted part of your hard-drive.
            var volumePath = "";
            
            // load the configuration from the home directory
            config = Helpers.GetConfiguration();

            // if the config is null, or if the config does not contain a location
            // prompt the user to give the location of where the passwords are stored.
            if (string.IsNullOrEmpty(config.location.Trim()))
            {
                Console.WriteLine("What is the location of the encrypted files? ");
                volumePath = Console.ReadLine() ?? "/Volumes/CKPWTEST";
                if (volumePath.Length == 0) volumePath = "/Volumes/CKPWTEST";
            }
            else
            {
                volumePath = config.location ??
                             throw new Exception("Something went wrong reading the path form the configuration file");
            }
            // check if the volume path exists
            if (!Directory.Exists(volumePath)) throw new Exception("Location does not exist");

            // promt the user for what they want
            Console.WriteLine("Read or Write? ");
            var response = Console.ReadLine()?.ToLower().Trim() ?? "";

            switch (response)
            {
                case "read":
                    ReadPassword(volumePath);
                    break;
                case "write":
                    WriteNewPassword(volumePath);
                    break;
                case "keys":
                    RetrieveKeys(volumePath);
                    break;
            }
        }

        private static void RetrieveKeys(string location)
        {
            var fileNames = Directory.GetFiles(location);
            if (fileNames.Length == 0)
            {
                Console.WriteLine("No passwords stored!");
                return;
            }
            foreach (var filePath in fileNames)
            {
                var file = new FileInfo(filePath);
                if (file.Extension == ".da")
                {
                    Console.WriteLine(file.Name);
                }
            }
        }

        private static void ReadPassword(string location)
        {
            Console.Write("Encryption Password: ");
            var encryptionPw = Helpers.ReadPasswordFromPrompt();// Console.ReadLine()?.Trim();
            if (encryptionPw is null || encryptionPw.Length < 3) throw new Exception("Invalid encryption password");

            Console.Write("Key: ");
            var key = Console.ReadLine()?.Trim();
            if (key is null || key.Length < 3) throw new Exception("Invalid key");

            var pw = ReadPasswordFromFile(location, encryptionPw, key);
            
            Console.WriteLine(pw);
        }

        private static void WriteNewPassword(string location)
        {
            Console.Write("Encryption Password: ");
            var encryptionPw = Helpers.ReadPasswordFromPrompt(); //Console.ReadLine()?.Trim();
            if (encryptionPw is null || encryptionPw.Length < 3) throw new Exception("Invalid encryption password");

            Console.Write("Key: ");
            var key = Console.ReadLine()?.Trim();
            if (key is null || key.Length < 3) throw new Exception("Invalid key");


            var username = config.username ?? null;
            if (username is null)
            {
                Console.Write("username: ");
                username = Console.ReadLine()?.Trim();
                if (username is null || username.Length < 3) throw new Exception("Invalid username");
            }

            Console.Write("Password (leave empty to generate): ");
            var password = Helpers.ReadPasswordFromPrompt();
            if (password is null || password.Length < 3)
            {
                Console.WriteLine("Generating strong password");
                Random rnd = new Random();
                var length = rnd.Next(config.minPasswordLength, config.maxPasswordLength + 1);
                password = Security.GeneratePassword(length, true, true, true, true);
            }

            var salt = Security.GenerateRandomSalt();
            SavePasswordToFile(location, encryptionPw, key, password, salt);
        }

        private static void SavePasswordToFile(string location, string encryptionPassword, string key, string password, byte[] salt)
        {
            var volumePath = location;
            var fileName = key + ".da";
            var filePath = Path.Combine(volumePath, fileName);
            
            if (File.Exists(filePath)) File.Delete(filePath);
            
            var encryptionKey = Security.DeriveKey(encryptionPassword, salt);
            var encryptedPassword = Security.Encrypt(password, encryptionKey, salt);
            
            // Save encrypted data to file
            using (FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate))
            {
                fileStream.Write(encryptedPassword, 0, encryptedPassword.Length);
                fileStream.Close();
            }
        }

        private static string ReadPasswordFromFile(string location, string encryptionPassword, string key)
        {
            var volumePath = location;
            var fileName = key + ".da";
            var filePath = Path.Combine(volumePath, fileName);
            var encryptedMessage = File.ReadAllBytes(filePath);
            
            var salt = new byte[16];
            Array.Copy(encryptedMessage, encryptedMessage.Length - 16, salt, 0, 16);

            var encryptionKey = Security.DeriveKey(encryptionPassword, salt);
            var password = Security.Decrypt(encryptedMessage, encryptionKey);

            return password;
        }
        
       
    }
}
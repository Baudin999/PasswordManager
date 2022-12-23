using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Tests")]
namespace CLI
{
    
    internal class Program
    {

        internal static void Main(string[] args)
        {
            var _location = "";
            
            // load the configuration from the home directory
            var config = Helpers.GetConfiguration();

            // if the config is null, or if the config does not contain a location
            // prompt the user to give the location of where the passwords are stored.
            if (config is null || string.IsNullOrWhiteSpace(config?.location.Trim()))
            {
                Console.WriteLine("What is the location of the encrypted files? ");
                _location = Console.ReadLine() ?? "/Volumes/CKPWTEST";
                if (_location.Length == 0) _location = "/Volumes/CKPWTEST";
                if (!Directory.Exists(_location)) throw new Exception("Location does not exist");

            }
            else
            {
                _location = config?.location;
            }

            // promt the user for what they want
            Console.WriteLine("Read or Write? ");
            var response = Console.ReadLine()?.ToLower().Trim() ?? "";

            switch (response)
            {
                case "read":
                    ReadPassword(_location);
                    break;
                case "write":
                    WriteNewPassword(_location);
                    break;
                case "keys":
                    RetrieveKeys(_location);
                    break;
            }
        }

        internal static void RetrieveKeys(string location)
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
                if (file.Extension == "da")
                {
                    Console.WriteLine(file.Name);
                }
            }
        }

        internal static void ReadPassword(string location)
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

        internal static void WriteNewPassword(string location)
        {
            Console.Write("Encryption Password: ");
            var encryptionPw = Helpers.ReadPasswordFromPrompt(); //Console.ReadLine()?.Trim();
            if (encryptionPw is null || encryptionPw.Length < 3) throw new Exception("Invalid encryption password");

            Console.Write("Key: ");
            var key = Console.ReadLine()?.Trim();
            if (key is null || key.Length < 3) throw new Exception("Invalid key");
            
            Console.Write("username: ");
            var username = Console.ReadLine()?.Trim();
            if (username is null || username.Length < 3) throw new Exception("Invalid username");
            
            Console.Write("Password: ");
            var password = Helpers.ReadPasswordFromPrompt(); //Console.ReadLine()?.Trim();
            if (password is null || password.Length < 3) throw new Exception("Invalid password");

            var salt = Security.GenerateRandomSalt();
            SavePasswordToFile(location, encryptionPw, key, password, salt);
        }

        internal static void SavePasswordToFile(string location, string encryptionPassword, string key, string password, byte[] salt)
        {
            var volumePath = location;
            var fileName = key + ".da";
            var filePath = Path.Combine(volumePath, fileName);
            
            var encryptionKey = Security.DeriveKey(encryptionPassword, salt);
            var encryptedPassword = Security.Encrypt(password, encryptionKey, salt);
            
            // Save encrypted data to file
            using (FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate))
            {
                fileStream.Write(encryptedPassword, 0, encryptedPassword.Length);
                fileStream.Close();
            }
        }

        internal static string ReadPasswordFromFile(string location, string encryptionPassword, string key)
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
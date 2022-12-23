using System.Security.Cryptography;
using System.Text;

namespace CLI;

public static class Security
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


    internal static string GeneratePassword(int length, bool includeLowercase, bool includeUppercase,
        bool includeNumeric, bool includeSpecial)
    {
        const string lowercase = "abcdefghijklmnopqrstuvwxyz";
        const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string numeric = "0123456789";
        const string special = "!@#$%^&*()_+-=[]{}|;':,.<>?";

        // Use a random number generator to select a random character from each allowed character set
        RandomNumberGenerator rng = RandomNumberGenerator.Create();
        StringBuilder password = new StringBuilder();

        // Divide the length into the appropriate number of parts
        int[] parts = Helpers.DivideNumberIntoParts(length, includeLowercase, includeUppercase, includeNumeric,
            includeSpecial);
        
        // check if we have a correct sum of the parts
        if (length != parts.Sum())
            throw new Exception("The sum of the parts is not equal to the length op te password");
        
        int partsIndex = -1;
        // Add lowercase characters
        if (includeLowercase)
        {
            AddLowercaseCharacters(password, lowercase, parts[++partsIndex], rng);
        }

        // Add uppercase characters
        if (includeUppercase)
        {
            AddUppercaseCharacters(password, uppercase, parts[++partsIndex], rng);
        }

        // Add numeric characters
        if (includeNumeric)
        {
            AddNumericCharacters(password, numeric, parts[++partsIndex], rng);
        }

        // Add special characters
        if (includeSpecial)
        {
            AddSpecialCharacters(password, special, parts[++partsIndex], rng);
        }
        return password.ToString();
    }

    static void AddLowercaseCharacters(StringBuilder password, string lowercase, int count, RandomNumberGenerator rng)
    {
        int charCount = lowercase.Length;
        for (int i = 0; i < count; i++)
        {
            password.Append(GetRandomCharacter(lowercase, charCount, rng));
        }
    }

    static void AddUppercaseCharacters(StringBuilder password, string uppercase, int count, RandomNumberGenerator rng)
    {
        int charCount = uppercase.Length;
        for (int i = 0; i < count; i++)
        {
            password.Append(GetRandomCharacter(uppercase, charCount, rng));
        }
    }

    static void AddNumericCharacters(StringBuilder password, string numeric, int count, RandomNumberGenerator rng)
    {
        int charCount = numeric.Length;
        for (int i = 0; i < count; i++)
        {
            password.Append(GetRandomCharacter(numeric, charCount, rng));
        }
    }

    static void AddSpecialCharacters(StringBuilder password, string special, int count, RandomNumberGenerator rng)
    {
        int charCount = special.Length;
        for (int i = 0; i < count; i++)
        {
            password.Append(GetRandomCharacter(special, charCount, rng));
        }
    }

    static char GetRandomCharacter(string characterSet, int charCount, RandomNumberGenerator rng)
    {
        byte[] uintBuffer = new byte[4];
        rng.GetBytes(uintBuffer);
        uint num = BitConverter.ToUInt32(uintBuffer, 0);
        return characterSet[(int)(num % (uint)charCount)];
    }
}
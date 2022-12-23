using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CLI;

public class Helpers
{
    internal static JsonConfig GetConfiguration()
    {
        string fileName = ".ckpw";
        string homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string filePath = Path.Combine(homeDirectory, fileName);
        string jsonString;

        if (!File.Exists(filePath)) return new JsonConfig();

        try
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                jsonString = reader.ReadToEnd();
                return JsonSerializer.Deserialize<JsonConfig>(jsonString) ?? new JsonConfig();
            }
        }
        catch (Exception)
        {
            return new JsonConfig();
        }
    }

    internal static string? ReadPasswordFromPrompt()
    {
        // Create a buffer to store the typed characters
        StringBuilder buffer = new StringBuilder();

        while (true)
        {
            // Read a key press
            ConsoleKeyInfo key = Console.ReadKey(true);

            // Check if the key press is Enter or Esc
            if (key.Key == ConsoleKey.Enter)
            {
                // If Enter, break out of the loop
                break;
            }
            else if (key.Key == ConsoleKey.Escape)
            {
                // If Esc, clear the buffer and write a new line
                buffer.Clear();
                Console.WriteLine();
            }
            else if (key.Key == ConsoleKey.Backspace)
            {
                // If Backspace, remove the last character from the buffer
                if (buffer.Length > 0)
                {
                    buffer.Remove(buffer.Length - 1, 1);
                    Console.Write("\b \b");
                }
            }
            else
            {
                // If any other key, add it to the buffer and write an asterisk
                buffer.Append(key.KeyChar);
                Console.Write("*");
            }
        }
        
        Console.WriteLine();

        var result = buffer.ToString();
        if (result.Length == 0) return null;
        else return result;
    }
    
    
    internal static int[] DivideNumberIntoParts(int number, bool includeLowercase, bool includeUppercase, bool includeNumeric, bool includeSpecial)
    {
        if (!includeLowercase && !includeUppercase && !includeNumeric && !includeSpecial)
            throw new Exception("At least one of the inclusions should be true.");
        
        // Create a random number generator
        RandomNumberGenerator rng = RandomNumberGenerator.Create();

        // Determine the number of parts
        int partsCount = (includeLowercase ? 1 : 0) + (includeUppercase ? 1 : 0) + (includeNumeric ? 1 : 0) + (includeSpecial ? 1 : 0);

        // Create an array to hold the parts
        int[] parts = new int[partsCount];

        // Divide the number into the appropriate number of parts
        int remaining = number;
        for (int i = 0; i < partsCount - 1; ++i)
        {
            // Generate a random number between 1 and the remaining value
            int part = GenerateRandomNumber(rng, 1, remaining - (partsCount - i));

            // Assign the part to the array
            parts[i] = part;

            // Decrement the remaining value
            remaining -= part;
        }

        // Assign the remaining value to the last element in the array
        parts[partsCount - 1] = remaining;

        // Shuffle the array to randomize the order of the parts
        ShuffleArray(parts, rng);

        return parts;
    }


    static int GenerateRandomNumber(RandomNumberGenerator rng, int minValue, int maxValue)
    {
        // Generate a random number between minValue and maxValue
        byte[] uintBuffer = new byte[4];
        rng.GetBytes(uintBuffer);
        uint num = BitConverter.ToUInt32(uintBuffer, 0);
        return (int)(minValue + (num % (uint)(maxValue - minValue + 1)));
    }

    static void ShuffleArray(int[] array, RandomNumberGenerator rng)
    {
        // Shuffle the array using the Fisher-Yates shuffle algorithm
        for (int i = array.Length - 1; i > 0; i--)
        {
            // Generate a random index
            int j = GenerateRandomNumber(rng, 0, i);

            // Swap the elements at indices i and j
            (array[i], array[j]) = (array[j], array[i]);
        }
    }
    
   internal  static string ShuffleString(string str)
    {
        RandomNumberGenerator rng = RandomNumberGenerator.Create();
        var characters = str.ToCharArray();
        // Shuffle the array using the Fisher-Yates shuffle algorithm
        for (int i = str.Length - 1; i > 0; i--)
        {
            // Generate a random index
            int j = GenerateRandomNumber(rng, 0, i);

            // Swap the elements at indices i and j
            
            (characters[i], characters[j]) = (characters[j], characters[i]);
        }

        return new string(characters);
    }
    
}

public class JsonConfig
{
    public string location { get; set; } = default!;
    public string username { get; set; } = default!;

    public int minPasswordLength { get; set; } = 20;
    public int maxPasswordLength { get; set; } = 40;
}

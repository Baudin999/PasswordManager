using System.Text;
using System.Text.Json;

namespace CLI;

public class Helpers
{
    internal static JsonConfig? GetConfiguration()
    {
        string fileName = ".ckpw";
        string homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string filePath = Path.Combine(homeDirectory, fileName);
        string jsonString;

        if (!File.Exists(filePath)) return null;

        try
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                jsonString = reader.ReadToEnd();
                return JsonSerializer.Deserialize<JsonConfig>(jsonString);
            }
        }
        catch (Exception)
        {
            return null;
        }
    }

    internal static string ReadPasswordFromPrompt()
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

        return buffer.ToString();
    }
}

public struct JsonConfig
{
    public string location { get; set; }
    public string username { get; set; }
}

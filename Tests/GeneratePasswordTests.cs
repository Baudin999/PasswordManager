using System.Diagnostics;
using CLI;

namespace Tests;

public class GeneratePasswordTests
{
    [Fact]
    public void GeneratePassword_Length12_IncludesLowercase()
    {
        // Arrange
        int length = 12;
        bool includeLowercase = true;
        bool includeUppercase = false;
        bool includeNumeric = false;
        bool includeSpecial = false;

        // Act
        string password = CLI.Security.GeneratePassword(length, includeLowercase, includeUppercase, includeNumeric,
            includeSpecial);

        // Assert
        Assert.Equal(length, password.Length);
        Assert.Matches("[a-z]{12}", password);
    }

    [Fact]
    public void GeneratePassword_Length12_IncludesUppercase()
    {
        // Arrange
        int length = 23;
        bool includeLowercase = false;
        bool includeUppercase = true;
        bool includeNumeric = false;
        bool includeSpecial = false;

        // Act
        string password = CLI.Security.GeneratePassword(length, includeLowercase, includeUppercase, includeNumeric,
            includeSpecial);

        // Assert
        Assert.Equal(length, password.Length);
        Assert.Matches("[A-Z]{23}", password);
    }

    [Fact]
    public void GeneratePassword_Length12_IncludesNumeric()
    {
        // Arrange
        int length = 13;
        bool includeLowercase = false;
        bool includeUppercase = false;
        bool includeNumeric = true;
        bool includeSpecial = false;

        // Act
        string password = CLI.Security.GeneratePassword(length, includeLowercase, includeUppercase, includeNumeric,
            includeSpecial);

        // Assert
        Assert.Equal(length, password.Length);
        Assert.Matches("[0-9]{13}", password);
    }

    [Fact]
    public void GeneratePassword_Length12_IncludesSpecial()
    {
        // Arrange
        int length = 44;
        bool includeLowercase = false;
        bool includeUppercase = false;
        bool includeNumeric = false;
        bool includeSpecial = true;

        // Act
        string password = CLI.Security.GeneratePassword(length, includeLowercase, includeUppercase, includeNumeric,
            includeSpecial);

        // Assert
        Assert.Equal(length, password.Length);
        Assert.Matches(@"[!@#$%^&*()_+-=\[\]{}|;""':,.<>?]{12}", password);
    }

    [Fact]
    public void GeneratePassword_MultipleLengths()
    {
        var lowerCasePattern = "a-z";
        var upperCasePattern = "A-Z";
        var numbersPattern = "0-9";
        var specialsPattern = @"!@#$%^&*()_+-=\[\]{}|;""':,.<>?";

        var iteration = 0;
        Random rnd = new Random();
        for (int i = 0; i < 100; ++i)
        {
            int length = rnd.Next(10, 60);
            bool includeLowercase = rnd.Next(0, 2) == 1;
            bool includeUppercase = rnd.Next(0, 2) == 1;
            bool includeNumeric = rnd.Next(0, 2) == 1;
            bool includeSpecial = rnd.Next(0, 2) == 1;
            
            if (!includeLowercase && !includeUppercase && !includeNumeric && !includeSpecial)
                continue;

            
            // Act
            string password = CLI.Security.GeneratePassword(
                length, 
                includeLowercase, 
                includeUppercase, 
                includeNumeric,
                includeSpecial);

            
            string pattern = $@"[";
            if (includeLowercase) pattern += lowerCasePattern;
            if (includeUppercase) pattern += upperCasePattern;
            if (includeNumeric) pattern += numbersPattern;
            if (includeSpecial) pattern += specialsPattern;
            pattern += $"]{{{length}}}";

            Debug.WriteLine($"length: {length}, pw: {password}");
            
            // Assert
            Assert.Equal(length, password.Length);
            Assert.Matches(pattern, password);
            iteration++;
            
            
        }
    }
    
    [Fact]
    public void GeneratePassword_Length12_IncludesUppercaseAndLowercase()
    {
        // Arrange
        int length = 10;
        bool includeLowercase = true;
        bool includeUppercase = true;
        bool includeNumeric = false;
        bool includeSpecial = false;

        // Act
        string password = CLI.Security.GeneratePassword(length, includeLowercase, includeUppercase, includeNumeric,
            includeSpecial);

        // Assert
        Assert.Equal(length, password.Length);
        Assert.Matches("[A-Za-z]{10}", password);
    }


    [Fact]
    public void GeneratePassword_NumberSplitter_SplitANumberInParts()
    {
        // it is important that we divide the total length of the password in
        // random amounts from each character class (numbers, lowercase, uppercase and special characters)
        // This function tests that that happens.

        int number = 30;
        int[] parts = Helpers.DivideNumberIntoParts(number, true, true, true, true);

        Assert.Equal(4, parts.Length);
        Assert.Equal(number, parts.Sum());
    }


    [Fact]
    public void GeneratePassword_NumberSplitter_SplitANumberInParts2()
    {
        // This test just does the previous test, but a 100 times
        
        Random rnd = new Random();
        for (int i = 0; i < 100; ++i)
        {
            int number = rnd.Next(10, 60);
            bool includeLowerCase = rnd.Next(0, 2) == 1;
            bool includeUpperCase = rnd.Next(0, 2) == 1;
            bool includeNumeric = rnd.Next(0, 2) == 1;
            bool includeSpecial = rnd.Next(0, 2) == 1;
            
            if (!includeLowerCase && !includeUpperCase && !includeNumeric && !includeSpecial)
                continue;
            
            int[] parts = Helpers.DivideNumberIntoParts(
                number, includeLowerCase, includeUpperCase, includeNumeric, includeSpecial);

            var totalNumberOfParts =
                (includeLowerCase ? 1 : 0) +
                (includeUpperCase ? 1 : 0) +
                (includeNumeric ? 1 : 0) +
                (includeSpecial ? 1 : 0);

            Assert.Equal(totalNumberOfParts, parts.Length);
            Assert.Equal(number, parts.Sum());
        }
    }
}
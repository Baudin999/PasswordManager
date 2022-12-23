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
        string password = CLI.Security.GeneratePassword(length, includeLowercase, includeUppercase, includeNumeric, includeSpecial);

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
        string password = CLI.Security.GeneratePassword(length, includeLowercase, includeUppercase, includeNumeric, includeSpecial);

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
        string password = CLI.Security.GeneratePassword(length, includeLowercase, includeUppercase, includeNumeric, includeSpecial);

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
        string password = CLI.Security.GeneratePassword(length, includeLowercase, includeUppercase, includeNumeric, includeSpecial);

        // Assert
        Assert.Equal(length, password.Length);
        Assert.Matches(@"[!@#$%^&*()_+-=\[\]{}|;""':,.<>?]{12}", password);
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
        string password = CLI.Security.GeneratePassword(length, includeLowercase, includeUppercase, includeNumeric, includeSpecial);

        // Assert
        Assert.Equal(length * 2, password.Length);
        Assert.Matches("[A-Za-z]{20}", password);
    }
    
    
}
    
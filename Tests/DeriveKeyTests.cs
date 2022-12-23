namespace Tests;

public class DeriveKeyTests
{
    [Fact]
    public void DeriveKey_WithValidInput_ReturnsExpectedKey()
    {
        // Arrange
        string password = "password123";
        byte[] salt = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 };

        // Act
        byte[] key = CLI.Security.DeriveKey(password, salt);
        
        
        // Assert
        Assert.Equal(32, key.Length);
        Assert.Equal(new byte[] {
            0x8C, 0x47, 0xC7, 0x16, 0x85, 0xEF, 
            0x2E, 0x69, 0x82, 0x83, 0x03, 0x67, 
            0x2B, 0x73, 0x0E, 0xD8, 0x5B, 0x62, 
            0x20, 0xD6, 0x99, 0x6D, 0x66, 0x1C, 
            0x52, 0x18, 0x93, 0xA8, 0xF7, 0xC1, 
            0x23, 0xD7
        }, key);
    }
    
    /*
     A simple method of extracting the hex values from a byte array
     
     var stringList = key
            .Select(b => "0x" + b.ToString("X").PadLeft(2, '0'));
     var template = string.Join(", ", stringList);
      
     */
}
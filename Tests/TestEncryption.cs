
using System;
using Xunit;

namespace Tests
{
    public class EncryptionTests
    {
        [Fact]
        public void Encrypt_And_Decrypt_WithValidInput_ReturnsOriginalMessage()
        {
            // Arrange
            string password = "password123";
            byte[] salt = CLI.Security.GenerateRandomSalt();
            byte[] key = CLI.Security.DeriveKey(password, salt);
            string message = "This is a test message";

            // Act
            byte[] encryptedMessage = CLI.Security.Encrypt(message, key, salt);
            string decryptedMessage = CLI.Security.Decrypt(encryptedMessage, key);

            // Assert
            Assert.Equal(message, decryptedMessage);
        }
    }
}


using FluentAssertions;
using Open_lab.Services;

namespace Open_lab.Tests.Services
{
    public class PasswordSecurityTests
    {
        [Fact]
        public void GenerateSalt_Should_Return_NonEmpty_Base64_String()
        {
            // Function: X.X — To Be Determined
            // Act
            var salt = PasswordSecurity.GenerateSalt();

            // Assert
            salt.Should().NotBeNullOrWhiteSpace();
            Convert.FromBase64String(salt).Length.Should().Be(16);
        }

        [Fact]
        public void GenerateSalt_Should_Return_Different_Values_On_Multiple_Calls()
        {
            // Function: X.X — To Be Determined
            // Act
            var salt1 = PasswordSecurity.GenerateSalt();
            var salt2 = PasswordSecurity.GenerateSalt();

            // Assert
            salt1.Should().NotBe(salt2);
        }

        [Fact]
        public void ComputeSha256_With_Same_Input_And_Salt_Should_Return_Same_Hash()
        {
            // Function: X.X — To Be Determined
            // Arrange
            const string password = "mysecret";
            var salt = PasswordSecurity.GenerateSalt();

            // Act
            var hash1 = PasswordSecurity.ComputeSha256(password, salt);
            var hash2 = PasswordSecurity.ComputeSha256(password, salt);

            // Assert
            hash1.Should().Be(hash2);
            hash1.Should().NotBeNullOrWhiteSpace();
            hash1.Length.Should().Be(64); // SHA256 hex string
        }

        [Fact]
        public void ComputeSha256_With_Different_Salts_Should_Return_Different_Hashes()
        {
            // Function: X.X — To Be Determined
            // Arrange
            const string password = "mysecret";
            var salt1 = PasswordSecurity.GenerateSalt();
            var salt2 = PasswordSecurity.GenerateSalt();

            // Act
            var hash1 = PasswordSecurity.ComputeSha256(password, salt1);
            var hash2 = PasswordSecurity.ComputeSha256(password, salt2);

            // Assert
            hash1.Should().NotBe(hash2);
        }

        [Fact]
        public void ComputeSha256_With_Different_Passwords_Same_Salt_Should_Return_Different_Hashes()
        {
            // Function: X.X — To Be Determined
            // Arrange
            var salt = PasswordSecurity.GenerateSalt();

            // Act
            var hash1 = PasswordSecurity.ComputeSha256("password1", salt);
            var hash2 = PasswordSecurity.ComputeSha256("password2", salt);

            // Assert
            hash1.Should().NotBe(hash2);
        }

        [Fact]
        public void Verify_With_Correct_Password_Should_Return_True()
        {
            // Function: X.X — To Be Determined
            // Arrange
            const string password = "correctpassword";
            var salt = PasswordSecurity.GenerateSalt();
            var hash = PasswordSecurity.ComputeSha256(password, salt);

            // Act
            var result = PasswordSecurity.Verify(password, salt, hash);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void Verify_With_Wrong_Password_Should_Return_False()
        {
            // Function: X.X — To Be Determined
            // Arrange
            const string password = "correctpassword";
            var salt = PasswordSecurity.GenerateSalt();
            var hash = PasswordSecurity.ComputeSha256(password, salt);

            // Act
            var result = PasswordSecurity.Verify("wrongpassword", salt, hash);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Verify_With_Wrong_Salt_Should_Return_False()
        {
            // Function: X.X — To Be Determined
            // Arrange
            const string password = "mysecret";
            var salt1 = PasswordSecurity.GenerateSalt();
            var salt2 = PasswordSecurity.GenerateSalt();
            var hash = PasswordSecurity.ComputeSha256(password, salt1);

            // Act
            var result = PasswordSecurity.Verify(password, salt2, hash);

            // Assert
            result.Should().BeFalse();
        }
    }
}

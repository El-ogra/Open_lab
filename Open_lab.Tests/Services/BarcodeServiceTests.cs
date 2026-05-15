using FluentAssertions;
using Open_lab.Services;

namespace Open_lab.Tests.Services
{
    public class BarcodeServiceTests
    {
        [Fact]
        public void GenerateCode128_WithContent_ShouldReturnFrozenImageSource()
        {
            // Function: Barcode printing
            // Arrange
            var service = new BarcodeService();

            // Act
            var image = service.GenerateCode128("LAB-001");

            // Assert
            image.Should().NotBeNull();
            image.IsFrozen.Should().BeTrue();
        }

        [Fact]
        public void GenerateCode128_WithEmptyContent_ShouldThrow()
        {
            // Function: Barcode printing validation
            // Arrange
            var service = new BarcodeService();

            // Act
            Action act = () => service.GenerateCode128("");

            // Assert
            act.Should().Throw<ArgumentException>();
        }
    }
}

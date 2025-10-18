using FluentAssertions;
using Volur.Application.DTOs.Provider;
using Volur.Application.Mappers;
using Xunit;

namespace Volur.UnitTests.Application.Mappers;

public class SymbolMapperTests
{
    [Fact]
    public void ToDomain_ShouldMapEodhdSymbolDtoCorrectly()
    {
        // Arrange
        var dto = new EodhdSymbolDto(
            Code: "AAPL",
            Name: "Apple Inc.",
            Country: "USA",
            Exchange: "US",
            Currency: "USD",
            Type: "Common Stock",
            Isin: "US0378331005",
            IsDelisted: false
        );

        // Act
        var domain = dto.ToDomain("US");

        // Assert
        domain.Ticker.Should().Be("AAPL");
        domain.Name.Should().Be("Apple Inc.");
        domain.ExchangeCode.Should().Be("US");
        domain.ParentExchange.Should().Be("US");
        domain.Currency.Should().Be("USD");
        domain.Type.Should().Be("Common Stock");
        domain.Isin.Should().Be("US0378331005");
        domain.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(null, true)]
    public void ToDomain_ShouldMapIsActiveCorrectly(bool? isDelisted, bool expectedIsActive)
    {
        // Arrange
        var dto = new EodhdSymbolDto(
            Code: "TEST",
            Name: "Test Symbol",
            Country: "USA",
            Exchange: "US",
            Currency: "USD",
            Type: "Common Stock",
            Isin: null,
            IsDelisted: isDelisted
        );

        // Act
        var domain = dto.ToDomain("US");

        // Assert
        domain.IsActive.Should().Be(expectedIsActive);
    }
}


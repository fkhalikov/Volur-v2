using FluentAssertions;
using Volur.Application.DTOs.Provider;
using Volur.Application.Mappers;
using Xunit;

namespace Volur.UnitTests.Application.Mappers;

public class ExchangeMapperTests
{
    [Fact]
    public void ToDomain_ShouldMapEodhdExchangeDtoCorrectly()
    {
        // Arrange
        var dto = new EodhdExchangeDto(
            Code: "US",
            Name: "US Stocks",
            OperatingMic: "XNYS",
            Country: "United States",
            Currency: "USD"
        );

        // Act
        var domain = dto.ToDomain();

        // Assert
        domain.Code.Should().Be("US");
        domain.Name.Should().Be("US Stocks");
        domain.OperatingMic.Should().Be("XNYS");
        domain.Country.Should().Be("United States");
        domain.Currency.Should().Be("USD");
    }

    [Fact]
    public void ToDto_ShouldMapExchangeEntityCorrectly()
    {
        // Arrange
        var entity = new Domain.Entities.Exchange(
            Code: "LSE",
            Name: "London Stock Exchange",
            OperatingMic: "XLON",
            Country: "United Kingdom",
            Currency: "GBP"
        );

        // Act
        var dto = entity.ToDto();

        // Assert
        dto.Code.Should().Be("LSE");
        dto.Name.Should().Be("London Stock Exchange");
        dto.OperatingMic.Should().Be("XLON");
        dto.Country.Should().Be("United Kingdom");
        dto.Currency.Should().Be("GBP");
    }
}


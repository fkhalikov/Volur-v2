using FluentAssertions;
using Volur.Application.UseCases.GetSymbols;
using Volur.Application.Validators;
using Xunit;

namespace Volur.UnitTests.Application.Validators;

public class GetSymbolsQueryValidatorTests
{
    private readonly GetSymbolsQueryValidator _validator = new();

    [Fact]
    public async Task Validate_ValidQuery_ShouldPass()
    {
        // Arrange
        var query = new GetSymbolsQuery("US", 1, 50);

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptyExchangeCode_ShouldFail(string? code)
    {
        // Arrange
        var query = new GetSymbolsQuery(code!, 1, 50);

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ExchangeCode");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validate_InvalidPage_ShouldFail(int page)
    {
        // Arrange
        var query = new GetSymbolsQuery("US", page, 50);

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Page");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(501)]
    public async Task Validate_InvalidPageSize_ShouldFail(int pageSize)
    {
        // Arrange
        var query = new GetSymbolsQuery("US", 1, pageSize);

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PageSize");
    }
}


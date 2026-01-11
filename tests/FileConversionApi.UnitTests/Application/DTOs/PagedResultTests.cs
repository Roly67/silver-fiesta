// <copyright file="PagedResultTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System;
using System.Collections.Generic;

using FileConversionApi.Application.DTOs;

using FluentAssertions;
using Xunit;

namespace FileConversionApi.UnitTests.Application.DTOs;

/// <summary>
/// Unit tests for <see cref="PagedResult{T}"/>.
/// </summary>
public class PagedResultTests
{
    /// <summary>
    /// Tests that TotalPages is calculated correctly when items divide evenly.
    /// </summary>
    [Fact]
    public void TotalPages_WhenItemsDivideEvenly_ReturnsCorrectValue()
    {
        // Arrange
        var result = new PagedResult<string>
        {
            Items = new List<string>(),
            Page = 1,
            PageSize = 10,
            TotalCount = 100,
        };

        // Act & Assert
        result.TotalPages.Should().Be(10);
    }

    /// <summary>
    /// Tests that TotalPages rounds up when items do not divide evenly.
    /// </summary>
    [Fact]
    public void TotalPages_WhenItemsDoNotDivideEvenly_RoundsUp()
    {
        // Arrange
        var result = new PagedResult<string>
        {
            Items = new List<string>(),
            Page = 1,
            PageSize = 10,
            TotalCount = 95,
        };

        // Act & Assert
        result.TotalPages.Should().Be(10);
    }

    /// <summary>
    /// Tests that TotalPages is 1 when total count is less than page size.
    /// </summary>
    [Fact]
    public void TotalPages_WhenTotalCountIsLessThanPageSize_ReturnsOne()
    {
        // Arrange
        var result = new PagedResult<string>
        {
            Items = new List<string>(),
            Page = 1,
            PageSize = 10,
            TotalCount = 5,
        };

        // Act & Assert
        result.TotalPages.Should().Be(1);
    }

    /// <summary>
    /// Tests that TotalPages is 0 when total count is 0.
    /// </summary>
    [Fact]
    public void TotalPages_WhenTotalCountIsZero_ReturnsZero()
    {
        // Arrange
        var result = new PagedResult<string>
        {
            Items = new List<string>(),
            Page = 1,
            PageSize = 10,
            TotalCount = 0,
        };

        // Act & Assert
        result.TotalPages.Should().Be(0);
    }

    /// <summary>
    /// Tests that TotalPages handles large numbers correctly.
    /// </summary>
    [Fact]
    public void TotalPages_WhenLargeNumbers_CalculatesCorrectly()
    {
        // Arrange
        var result = new PagedResult<string>
        {
            Items = new List<string>(),
            Page = 1,
            PageSize = 25,
            TotalCount = 1000001,
        };

        // Act & Assert
        result.TotalPages.Should().Be(40001);
    }

    /// <summary>
    /// Tests that HasNextPage is true when current page is less than total pages.
    /// </summary>
    [Fact]
    public void HasNextPage_WhenPageIsLessThanTotalPages_ReturnsTrue()
    {
        // Arrange
        var result = new PagedResult<string>
        {
            Items = new List<string>(),
            Page = 1,
            PageSize = 10,
            TotalCount = 50,
        };

        // Act & Assert
        result.HasNextPage.Should().BeTrue();
    }

    /// <summary>
    /// Tests that HasNextPage is false when current page equals total pages.
    /// </summary>
    [Fact]
    public void HasNextPage_WhenPageEqualsTotalPages_ReturnsFalse()
    {
        // Arrange
        var result = new PagedResult<string>
        {
            Items = new List<string>(),
            Page = 5,
            PageSize = 10,
            TotalCount = 50,
        };

        // Act & Assert
        result.HasNextPage.Should().BeFalse();
    }

    /// <summary>
    /// Tests that HasNextPage is false when there is only one page.
    /// </summary>
    [Fact]
    public void HasNextPage_WhenOnlyOnePage_ReturnsFalse()
    {
        // Arrange
        var result = new PagedResult<string>
        {
            Items = new List<string>(),
            Page = 1,
            PageSize = 10,
            TotalCount = 5,
        };

        // Act & Assert
        result.HasNextPage.Should().BeFalse();
    }

    /// <summary>
    /// Tests that HasNextPage is false when total count is zero.
    /// </summary>
    [Fact]
    public void HasNextPage_WhenTotalCountIsZero_ReturnsFalse()
    {
        // Arrange
        var result = new PagedResult<string>
        {
            Items = new List<string>(),
            Page = 1,
            PageSize = 10,
            TotalCount = 0,
        };

        // Act & Assert
        result.HasNextPage.Should().BeFalse();
    }

    /// <summary>
    /// Tests that HasPreviousPage is true when current page is greater than 1.
    /// </summary>
    [Fact]
    public void HasPreviousPage_WhenPageIsGreaterThanOne_ReturnsTrue()
    {
        // Arrange
        var result = new PagedResult<string>
        {
            Items = new List<string>(),
            Page = 2,
            PageSize = 10,
            TotalCount = 50,
        };

        // Act & Assert
        result.HasPreviousPage.Should().BeTrue();
    }

    /// <summary>
    /// Tests that HasPreviousPage is false when current page is 1.
    /// </summary>
    [Fact]
    public void HasPreviousPage_WhenPageIsOne_ReturnsFalse()
    {
        // Arrange
        var result = new PagedResult<string>
        {
            Items = new List<string>(),
            Page = 1,
            PageSize = 10,
            TotalCount = 50,
        };

        // Act & Assert
        result.HasPreviousPage.Should().BeFalse();
    }

    /// <summary>
    /// Tests that HasPreviousPage is true on the last page when multiple pages exist.
    /// </summary>
    [Fact]
    public void HasPreviousPage_WhenOnLastPage_ReturnsTrue()
    {
        // Arrange
        var result = new PagedResult<string>
        {
            Items = new List<string>(),
            Page = 5,
            PageSize = 10,
            TotalCount = 50,
        };

        // Act & Assert
        result.HasPreviousPage.Should().BeTrue();
    }

    /// <summary>
    /// Tests that Items property holds the provided items.
    /// </summary>
    [Fact]
    public void Items_WhenSet_ReturnsCorrectItems()
    {
        // Arrange
        var items = new List<string> { "item1", "item2", "item3" };
        var result = new PagedResult<string>
        {
            Items = items,
            Page = 1,
            PageSize = 10,
            TotalCount = 3,
        };

        // Act & Assert
        result.Items.Should().BeEquivalentTo(items);
        result.Items.Should().HaveCount(3);
    }

    /// <summary>
    /// Tests that Page property returns the set value.
    /// </summary>
    [Fact]
    public void Page_WhenSet_ReturnsCorrectValue()
    {
        // Arrange
        var result = new PagedResult<string>
        {
            Items = new List<string>(),
            Page = 5,
            PageSize = 10,
            TotalCount = 100,
        };

        // Act & Assert
        result.Page.Should().Be(5);
    }

    /// <summary>
    /// Tests that PageSize property returns the set value.
    /// </summary>
    [Fact]
    public void PageSize_WhenSet_ReturnsCorrectValue()
    {
        // Arrange
        var result = new PagedResult<string>
        {
            Items = new List<string>(),
            Page = 1,
            PageSize = 25,
            TotalCount = 100,
        };

        // Act & Assert
        result.PageSize.Should().Be(25);
    }

    /// <summary>
    /// Tests that TotalCount property returns the set value.
    /// </summary>
    [Fact]
    public void TotalCount_WhenSet_ReturnsCorrectValue()
    {
        // Arrange
        var result = new PagedResult<string>
        {
            Items = new List<string>(),
            Page = 1,
            PageSize = 10,
            TotalCount = 150,
        };

        // Act & Assert
        result.TotalCount.Should().Be(150);
    }

    /// <summary>
    /// Tests PagedResult with different generic types.
    /// </summary>
    [Fact]
    public void PagedResult_WithIntegerType_WorksCorrectly()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3, 4, 5 };
        var result = new PagedResult<int>
        {
            Items = items,
            Page = 1,
            PageSize = 5,
            TotalCount = 100,
        };

        // Act & Assert
        result.Items.Should().BeEquivalentTo(items);
        result.TotalPages.Should().Be(20);
        result.HasNextPage.Should().BeTrue();
        result.HasPreviousPage.Should().BeFalse();
    }

    /// <summary>
    /// Tests PagedResult with complex type.
    /// </summary>
    [Fact]
    public void PagedResult_WithComplexType_WorksCorrectly()
    {
        // Arrange
        var items = new List<ConversionJobDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                SourceFormat = "html",
                TargetFormat = "pdf",
                Status = FileConversionApi.Domain.Enums.ConversionStatus.Pending,
                InputFileName = "test.html",
                CreatedAt = DateTimeOffset.UtcNow,
            },
        };

        var result = new PagedResult<ConversionJobDto>
        {
            Items = items,
            Page = 1,
            PageSize = 10,
            TotalCount = 1,
        };

        // Act & Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].SourceFormat.Should().Be("html");
        result.TotalPages.Should().Be(1);
    }

    /// <summary>
    /// Tests middle page scenario with correct navigation properties.
    /// </summary>
    [Fact]
    public void NavigationProperties_WhenOnMiddlePage_ReturnCorrectValues()
    {
        // Arrange
        var result = new PagedResult<string>
        {
            Items = new List<string> { "item1", "item2" },
            Page = 3,
            PageSize = 10,
            TotalCount = 50,
        };

        // Act & Assert
        result.Page.Should().Be(3);
        result.TotalPages.Should().Be(5);
        result.HasPreviousPage.Should().BeTrue();
        result.HasNextPage.Should().BeTrue();
    }

    /// <summary>
    /// Tests edge case where page size equals total count.
    /// </summary>
    [Fact]
    public void PagedResult_WhenPageSizeEqualsTotalCount_HasCorrectValues()
    {
        // Arrange
        var result = new PagedResult<string>
        {
            Items = new List<string> { "a", "b", "c", "d", "e" },
            Page = 1,
            PageSize = 5,
            TotalCount = 5,
        };

        // Act & Assert
        result.TotalPages.Should().Be(1);
        result.HasNextPage.Should().BeFalse();
        result.HasPreviousPage.Should().BeFalse();
    }

    /// <summary>
    /// Tests with page size of 1.
    /// </summary>
    [Fact]
    public void PagedResult_WhenPageSizeIsOne_CalculatesCorrectly()
    {
        // Arrange
        var result = new PagedResult<string>
        {
            Items = new List<string> { "item" },
            Page = 5,
            PageSize = 1,
            TotalCount = 10,
        };

        // Act & Assert
        result.TotalPages.Should().Be(10);
        result.HasNextPage.Should().BeTrue();
        result.HasPreviousPage.Should().BeTrue();
    }
}

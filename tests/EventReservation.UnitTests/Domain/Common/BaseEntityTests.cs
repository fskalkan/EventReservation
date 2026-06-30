using EventReservation.Domain.Common;
using FluentAssertions;

namespace EventReservation.UnitTests.Domain.Common;

public sealed class BaseEntityTests
{
    [Fact]
    public void Constructor_ShouldCreateEntity_WithIdAndCreatedAt()
    {
        // Act
        var entity = new TestEntity();

        // Assert
        entity.Id.Should().NotBeEmpty();
        entity.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        entity.UpdatedAt.Should().BeNull();
        entity.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void MarkAsUpdated_ShouldSetUpdatedAt()
    {
        // Arrange
        var entity = new TestEntity();

        // Act
        entity.MarkAsUpdated();

        // Assert
        entity.UpdatedAt.Should().NotBeNull();
        entity.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void SoftDelete_ShouldSetIsDeletedTrue_AndSetUpdatedAt()
    {
        // Arrange
        var entity = new TestEntity();

        // Act
        entity.SoftDelete();

        // Assert
        entity.IsDeleted.Should().BeTrue();
        entity.UpdatedAt.Should().NotBeNull();
    }

    private sealed class TestEntity : BaseEntity
    {
    }
}
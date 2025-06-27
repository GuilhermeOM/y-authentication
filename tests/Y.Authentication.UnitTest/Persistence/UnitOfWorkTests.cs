using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using Y.Authentication.Domain.Shared;
using Y.Authentication.Infrastructure.Persistence;

namespace Y.Authentication.UnitTest.Persistence;
public class UnitOfWorkTests
{
    private readonly Mock<AppDataContext> _appDataContextMock;
    private readonly Mock<IDbContextTransaction> _dbContextTransactionMock;

    private readonly UnitOfWork _unitOfWork;

    public UnitOfWorkTests()
    {
        _appDataContextMock = new Mock<AppDataContext>(new DbContextOptions<AppDataContext>());
        _dbContextTransactionMock = new Mock<IDbContextTransaction>();

        Mock<DatabaseFacade> databaseFacadeMock = new(_appDataContextMock.Object);

        _appDataContextMock
            .Setup(context => context.Database)
            .Returns(databaseFacadeMock.Object);

        databaseFacadeMock
            .Setup(database => database.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_dbContextTransactionMock.Object);

        _dbContextTransactionMock
            .Setup(transaction => transaction.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _dbContextTransactionMock
            .Setup(transaction => transaction.RollbackAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWork = new UnitOfWork(_appDataContextMock.Object);
    }

    [Fact]
    public async Task TransactionAsync_ShouldCommit_WhenActionSucceeds()
    {
        // Act
        var actualResult = await _unitOfWork.TransactionAsync(SuccessAction, CancellationToken.None);

        // Assert
        actualResult.IsSuccess.Should().BeTrue();

        _appDataContextMock.Verify(
            context => context.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        _dbContextTransactionMock.Verify(
            transaction => transaction.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        _dbContextTransactionMock.Verify(
            transaction => transaction.RollbackAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task TransactionAsync_ShouldRollback_WhenActionFails()
    {
        // Act
        var actualResult = await _unitOfWork.TransactionAsync(FailureAction, CancellationToken.None);

        // Assert
        actualResult.IsSuccess.Should().BeFalse();

        _appDataContextMock.Verify(
            context => context.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);

        _dbContextTransactionMock.Verify(
            transaction => transaction.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Never);

        _dbContextTransactionMock.Verify(
            transaction => transaction.RollbackAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task TransactionAsync_ShouldRollback_WhenExceptionIsThrown()
    {
        // Act
        var actualResult = await _unitOfWork.TransactionAsync(ExceptionAction, CancellationToken.None);

        // Assert
        actualResult.IsSuccess.Should().BeFalse();

        _appDataContextMock.Verify(
            context => context.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);

        _dbContextTransactionMock.Verify(
            transaction => transaction.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Never);

        _dbContextTransactionMock.Verify(
            transaction => transaction.RollbackAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static Task<Result> SuccessAction() => Task.FromResult(Result.Success());
    private static Task<Result> FailureAction() => Task.FromResult(Result.Failure(new Error(UnitOfWork.TransactionErrorCode, "error")));
    private static Task<Result> ExceptionAction() => throw new Exception();
}

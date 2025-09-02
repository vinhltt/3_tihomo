using AutoMapper;
using Bogus;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PlanningInvestment.Application.DTOs.Investment;
using PlanningInvestment.Application.Profiles;
using PlanningInvestment.Application.Services;
using PlanningInvestment.Domain.Entities;
using PlanningInvestment.Domain.Enums;
using PlanningInvestment.Domain.BaseRepositories;
using PlanningInvestment.Domain.UnitOfWorks;
using Xunit;

namespace PlanningInvestment.Tests.Services;

/// <summary>
///     Unit tests for InvestmentService. (EN)<br />
///     Các test đơn vị cho InvestmentService. (VI)
/// </summary>
public class InvestmentServiceTests : IDisposable
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IBaseRepository<Investment, Guid>> _repositoryMock;
    private readonly Mock<ILogger<InvestmentService>> _loggerMock;
    private readonly IMapper _mapper;
    private readonly InvestmentService _service;
    private readonly Faker<Investment> _investmentFaker;
    private readonly Faker<CreateInvestmentRequest> _createRequestFaker;

    /// <summary>
    ///     Initializes a new instance of the InvestmentServiceTests class. (EN)<br />
    ///     Khởi tạo một phiên bản mới của lớp InvestmentServiceTests. (VI)
    /// </summary>
    public InvestmentServiceTests()
    {
        // Setup mocks
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _repositoryMock = new Mock<IBaseRepository<Investment, Guid>>();
        _loggerMock = new Mock<ILogger<InvestmentService>>();

        // Setup AutoMapper
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<InvestmentProfile>();
        });
        _mapper = mapperConfig.CreateMapper();

        // Setup repository mock
        _unitOfWorkMock.Setup(x => x.Repository<Investment, Guid>())
                      .Returns(_repositoryMock.Object);

        // Create service
        _service = new InvestmentService(_mapper, _unitOfWorkMock.Object, _loggerMock.Object);

        // Setup Faker for Investment entities
        _investmentFaker = new Faker<Investment>()
            .RuleFor(x => x.Id, f => f.Random.Guid())
            .RuleFor(x => x.UserId, f => f.Random.Guid())
            .RuleFor(x => x.Symbol, f => f.Random.AlphaNumeric(5).ToUpper())
            .RuleFor(x => x.InvestmentType, f => f.PickRandom<InvestmentType>())
            .RuleFor(x => x.PurchasePrice, f => f.Random.Decimal(1, 1000))
            .RuleFor(x => x.Quantity, f => f.Random.Decimal(1, 100))
            .RuleFor(x => x.CurrentMarketPrice, f => f.Random.Decimal(1, 1000))
            .RuleFor(x => x.PurchaseDate, f => f.Date.Past())
            .RuleFor(x => x.Notes, f => f.Lorem.Sentence())
            .RuleFor(x => x.CreatedAt, f => f.Date.Past())
            .RuleFor(x => x.UpdatedAt, f => f.Date.Recent())
            .RuleFor(x => x.CreateBy, f => f.Internet.UserName())
            .RuleFor(x => x.UpdateBy, f => f.Internet.UserName());

        // Setup Faker for CreateInvestmentRequest
        _createRequestFaker = new Faker<CreateInvestmentRequest>()
            .RuleFor(x => x.UserId, f => f.Random.Guid())
            .RuleFor(x => x.Symbol, f => f.Random.AlphaNumeric(5).ToUpper())
            .RuleFor(x => x.InvestmentType, f => f.PickRandom<InvestmentType>())
            .RuleFor(x => x.PurchasePrice, f => f.Random.Decimal(1, 1000))
            .RuleFor(x => x.Quantity, f => f.Random.Decimal(1, 100))
            .RuleFor(x => x.CurrentMarketPrice, f => f.Random.Decimal(1, 1000))
            .RuleFor(x => x.PurchaseDate, f => f.Date.Past())
            .RuleFor(x => x.Notes, f => f.Lorem.Sentence());
    }

    /// <summary>
    ///     Test GetUserInvestmentsAsync returns investments for valid user. (EN)<br />
    ///     Test GetUserInvestmentsAsync trả về danh sách đầu tư cho người dùng hợp lệ. (VI)
    /// </summary>
    [Fact]
    public async Task GetUserInvestmentsAsync_ValidUserId_ReturnsInvestments()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var investments = _investmentFaker.Generate(3);
        investments.ForEach(i => i.UserId = userId);

        _repositoryMock.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<Investment, object>>[]>()))
                      .ReturnsAsync(investments);

        // Act
        var result = await _service.GetUserInvestmentsAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().AllSatisfy(x => x.UserId.Should().Be(userId));
    }

    /// <summary>
    ///     Test GetUserInvestmentsAsync returns empty collection for user with no investments. (EN)<br />
    ///     Test GetUserInvestmentsAsync trả về collection rỗng cho người dùng không có đầu tư. (VI)
    /// </summary>
    [Fact]
    public async Task GetUserInvestmentsAsync_UserWithNoInvestments_ReturnsEmptyCollection()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _repositoryMock.Setup(x => x.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Investment, bool>>>(), It.IsAny<System.Linq.Expressions.Expression<Func<Investment, object>>[]>()))
                      .ReturnsAsync(new List<Investment>());

        // Act
        var result = await _service.GetUserInvestmentsAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    /// <summary>
    ///     Test UpdateMarketPriceAsync updates investment market price successfully. (EN)<br />
    ///     Test UpdateMarketPriceAsync cập nhật giá thị trường đầu tư thành công. (VI)
    /// </summary>
    [Fact]
    public async Task UpdateMarketPriceAsync_ValidRequest_UpdatesMarketPrice()
    {
        // Arrange
        var investment = _investmentFaker.Generate();
        var newPrice = 150.75m;
        var request = new UpdateMarketPriceRequest { CurrentMarketPrice = newPrice };

        _repositoryMock.Setup(x => x.GetByIdAsync(investment.Id, It.IsAny<System.Linq.Expressions.Expression<Func<Investment, object>>[]>()))
                      .ReturnsAsync(investment);
        _repositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Investment>()))
                      .ReturnsAsync(1);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
                       .ReturnsAsync(1);

        // Act
        var result = await _service.UpdateMarketPriceAsync(investment.Id, request);

        // Assert
        result.Should().NotBeNull();
        result!.CurrentMarketPrice.Should().Be(newPrice);
        _repositoryMock.Verify(x => x.UpdateAsync(It.Is<Investment>(i => i.CurrentMarketPrice == newPrice)), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    /// <summary>
    ///     Test UpdateMarketPriceAsync returns null for non-existent investment. (EN)<br />
    ///     Test UpdateMarketPriceAsync trả về null cho đầu tư không tồn tại. (VI)
    /// </summary>
    [Fact]
    public async Task UpdateMarketPriceAsync_NonExistentInvestment_ReturnsNull()
    {
        // Arrange
        var investmentId = Guid.NewGuid();
        var request = new UpdateMarketPriceRequest { CurrentMarketPrice = 150.75m };

        _repositoryMock.Setup(x => x.GetByIdAsync(investmentId, It.IsAny<System.Linq.Expressions.Expression<Func<Investment, object>>[]>()))
                      .ReturnsAsync((Investment?)null);

        // Act
        var result = await _service.UpdateMarketPriceAsync(investmentId, request);

        // Assert
        result.Should().BeNull();
        _repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Investment>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    /// <summary>
    ///     Test GetPortfolioSummaryAsync calculates portfolio correctly. (EN)<br />
    ///     Test GetPortfolioSummaryAsync tính toán danh mục đầu tư đúng cách. (VI)
    /// </summary>
    [Fact]
    public async Task GetPortfolioSummaryAsync_WithInvestments_CalculatesCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var investments = new List<Investment>
        {
            new Investment
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Symbol = "AAPL",
                InvestmentType = InvestmentType.Stock,
                PurchasePrice = 100m,
                Quantity = 10m,
                CurrentMarketPrice = 120m,
                PurchaseDate = DateTime.UtcNow.AddDays(-30),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreateBy = "testuser",
                UpdateBy = "testuser"
            },
            new Investment
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Symbol = "MSFT",
                InvestmentType = InvestmentType.Stock,
                PurchasePrice = 200m,
                Quantity = 5m,
                CurrentMarketPrice = 180m,
                PurchaseDate = DateTime.UtcNow.AddDays(-60),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreateBy = "testuser",
                UpdateBy = "testuser"
            }
        };

        _repositoryMock.Setup(x => x.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Investment, bool>>>(), It.IsAny<System.Linq.Expressions.Expression<Func<Investment, object>>[]>()))
                      .ReturnsAsync(investments);

        // Act
        var result = await _service.GetPortfolioSummaryAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.TotalInvestedAmount.Should().Be(2000m); // (100*10) + (200*5)
        result.CurrentTotalValue.Should().Be(2100m); // (120*10) + (180*5)
        result.TotalProfitLoss.Should().Be(100m); // 2100 - 2000
        result.TotalProfitLossPercentage.Should().Be(5m); // (100/2000) * 100
        result.InvestmentCount.Should().Be(2);
        result.InvestmentTypeBreakdown.Should().HaveCount(1); // Only Stock type
        result.InvestmentTypeBreakdown.First().InvestmentType.Should().Be(InvestmentType.Stock);
        result.InvestmentTypeBreakdown.First().Count.Should().Be(2);
    }

    /// <summary>
    ///     Test GetPortfolioSummaryAsync returns empty summary for user with no investments. (EN)<br />
    ///     Test GetPortfolioSummaryAsync trả về tóm tắt rỗng cho người dùng không có đầu tư. (VI)
    /// </summary>
    [Fact]
    public async Task GetPortfolioSummaryAsync_NoInvestments_ReturnsEmptySummary()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _repositoryMock.Setup(x => x.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Investment, bool>>>(), It.IsAny<System.Linq.Expressions.Expression<Func<Investment, object>>[]>()))
                      .ReturnsAsync(new List<Investment>());

        // Act
        var result = await _service.GetPortfolioSummaryAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.TotalInvestedAmount.Should().Be(0);
        result.CurrentTotalValue.Should().Be(0);
        result.TotalProfitLoss.Should().Be(0);
        result.TotalProfitLossPercentage.Should().Be(0);
        result.InvestmentCount.Should().Be(0);
        result.InvestmentTypeBreakdown.Should().BeEmpty();
    }

    /// <summary>
    ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources. (EN)<br />
    ///     Thực hiện các tác vụ do ứng dụng định nghĩa liên quan đến việc giải phóng, phát hành hoặc đặt lại các tài nguyên không được quản lý. (VI)
    /// </summary>
    public void Dispose()
    {
        _unitOfWorkMock?.Reset();
        _repositoryMock?.Reset();
        _loggerMock?.Reset();
    }
}

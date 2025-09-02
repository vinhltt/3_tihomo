using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlanningInvestment.Application.DTOs.Investment;
using PlanningInvestment.Domain.Entities;
using PlanningInvestment.Domain.Enums;
using PlanningInvestment.Infrastructure;
using PlanningInvestment.Api;
using Xunit;

namespace PlanningInvestment.Tests.Controllers;

/// <summary>
///     Integration tests for InvestmentController. (EN)<br />
///     Các test tích hợp cho InvestmentController. (VI)
/// </summary>
public class InvestmentControllerTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly Faker<CreateInvestmentRequest> _createRequestFaker;
    private readonly Faker<UpdateMarketPriceRequest> _updatePriceFaker;
    private readonly string _testUserId = Guid.NewGuid().ToString();

    /// <summary>
    ///     Initializes a new instance of the InvestmentControllerTests class. (EN)<br />
    ///     Khởi tạo một phiên bản mới của lớp InvestmentControllerTests. (VI)
    /// </summary>
    public InvestmentControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                // Remove the real database context
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<PlanningInvestmentDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add in-memory database for testing
                services.AddDbContext<PlanningInvestmentDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDatabase");
                });

                // Add test authentication
                services.AddAuthentication("Test")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
                
                services.AddAuthorization();
            });
        });

        _client = _factory.CreateClient();

        // Setup Faker for CreateInvestmentRequest
        _createRequestFaker = new Faker<CreateInvestmentRequest>()
            .RuleFor(x => x.Symbol, f => f.Random.AlphaNumeric(5).ToUpper())
            .RuleFor(x => x.InvestmentType, f => f.PickRandom<InvestmentType>())
            .RuleFor(x => x.PurchasePrice, f => f.Random.Decimal(1, 1000))
            .RuleFor(x => x.Quantity, f => f.Random.Decimal(1, 100))
            .RuleFor(x => x.CurrentMarketPrice, f => f.Random.Decimal(1, 1000))
            .RuleFor(x => x.PurchaseDate, f => f.Date.Past())
            .RuleFor(x => x.Notes, f => f.Lorem.Sentence());

        // Setup Faker for UpdateMarketPriceRequest
        _updatePriceFaker = new Faker<UpdateMarketPriceRequest>()
            .RuleFor(x => x.CurrentMarketPrice, f => f.Random.Decimal(1, 1000));
    }

    /// <summary>
    ///     Test POST /api/investment creates investment successfully. (EN)<br />
    ///     Test POST /api/investment tạo đầu tư thành công. (VI)
    /// </summary>
    [Fact]
    public async Task CreateInvestment_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var request = _createRequestFaker.Generate();

        // Act
        var response = await _client.PostAsJsonAsync("/api/investment", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var investment = await response.Content.ReadFromJsonAsync<InvestmentViewModel>();
        investment.Should().NotBeNull();
        investment!.Symbol.Should().Be(request.Symbol);
        investment.InvestmentType.Should().Be(request.InvestmentType);
        investment.PurchasePrice.Should().Be(request.PurchasePrice);
        investment.Quantity.Should().Be(request.Quantity);
    }

    /// <summary>
    ///     Test GET /api/investment returns user investments. (EN)<br />
    ///     Test GET /api/investment trả về danh sách đầu tư của người dùng. (VI)
    /// </summary>
    [Fact]
    public async Task GetUserInvestments_WithExistingInvestments_ReturnsInvestments()
    {
        // Arrange - Create some test investments first
        var createRequests = _createRequestFaker.Generate(3);
        
        foreach (var request in createRequests)
        {
            await _client.PostAsJsonAsync("/api/investment", request);
        }

        // Act
        var response = await _client.GetAsync("/api/investment");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var investments = await response.Content.ReadFromJsonAsync<List<InvestmentViewModel>>();
        investments.Should().NotBeNull();
        investments!.Should().HaveCount(3);
        investments.Should().AllSatisfy(x => x.UserId.Should().Be(Guid.Parse(_testUserId)));
    }

    /// <summary>
    ///     Test GET /api/investment/portfolio/summary returns portfolio summary. (EN)<br />
    ///     Test GET /api/investment/portfolio/summary trả về tóm tắt danh mục đầu tư. (VI)
    /// </summary>
    [Fact]
    public async Task GetPortfolioSummary_WithInvestments_ReturnsValidSummary()
    {
        // Arrange - Create some test investments
        var createRequests = _createRequestFaker.Generate(2);
        var createdInvestments = new List<InvestmentViewModel>();

        foreach (var request in createRequests)
        {
            var createResponse = await _client.PostAsJsonAsync("/api/investment", request);
            var created = await createResponse.Content.ReadFromJsonAsync<InvestmentViewModel>();
            createdInvestments.Add(created!);
        }

        // Act
        var response = await _client.GetAsync("/api/investment/portfolio/summary");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var summary = await response.Content.ReadFromJsonAsync<PortfolioSummaryViewModel>();
        summary.Should().NotBeNull();
        summary!.InvestmentCount.Should().Be(2);
        summary.TotalInvestedAmount.Should().BeGreaterThan(0);
    }

    /// <summary>
    ///     Test PATCH /api/investment/{id}/market-price updates market price. (EN)<br />
    ///     Test PATCH /api/investment/{id}/market-price cập nhật giá thị trường. (VI)
    /// </summary>
    [Fact]
    public async Task UpdateMarketPrice_ValidRequest_ReturnsUpdatedInvestment()
    {
        // Arrange - Create an investment first
        var createRequest = _createRequestFaker.Generate();
        var createResponse = await _client.PostAsJsonAsync("/api/investment", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<InvestmentViewModel>();

        var updateRequest = _updatePriceFaker.Generate();

        // Act
        var response = await _client.PatchAsync($"/api/investment/{created!.Id}/market-price", 
                                               JsonContent.Create(updateRequest));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var updated = await response.Content.ReadFromJsonAsync<InvestmentViewModel>();
        updated.Should().NotBeNull();
        updated!.CurrentMarketPrice.Should().Be(updateRequest.CurrentMarketPrice);
    }

    /// <summary>
    ///     Test DELETE /api/investment/{id} deletes investment successfully. (EN)<br />
    ///     Test DELETE /api/investment/{id} xóa đầu tư thành công. (VI)
    /// </summary>
    [Fact]
    public async Task DeleteInvestment_ExistingInvestment_ReturnsNoContent()
    {
        // Arrange - Create an investment first
        var createRequest = _createRequestFaker.Generate();
        var createResponse = await _client.PostAsJsonAsync("/api/investment", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<InvestmentViewModel>();

        // Act
        var response = await _client.DeleteAsync($"/api/investment/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify the investment is deleted
        var getResponse = await _client.GetAsync($"/api/investment/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>
    ///     Test accessing non-existent investment returns NotFound. (EN)<br />
    ///     Test truy cập đầu tư không tồn tại trả về NotFound. (VI)
    /// </summary>
    [Fact]
    public async Task GetInvestment_NonExistentId_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/investment/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>
    ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources. (EN)<br />
    ///     Thực hiện các tác vụ do ứng dụng định nghĩa liên quan đến việc giải phóng, phát hành hoặc đặt lại các tài nguyên không được quản lý. (VI)
    /// </summary>
    public void Dispose()
    {
        _client?.Dispose();
    }
}

/// <summary>
///     Test authentication handler for integration tests. (EN)<br />
///     Handler xác thực test cho các test tích hợp. (VI)
/// </summary>
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly string _testUserId = Guid.NewGuid().ToString();

    public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
                          ILoggerFactory logger,
                          UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, _testUserId),
            new Claim(ClaimTypes.Name, "Test User"),
            new Claim("sub", _testUserId)
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

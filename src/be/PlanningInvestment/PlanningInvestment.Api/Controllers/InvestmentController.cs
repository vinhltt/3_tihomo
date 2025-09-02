using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlanningInvestment.Application.DTOs.Investment;
using PlanningInvestment.Application.Interfaces;
using System.Security.Claims;

namespace PlanningInvestment.API.Controllers;

/// <summary>
///     Controller for managing investment operations. (EN)<br />
///     Controller để quản lý các hoạt động đầu tư. (VI)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InvestmentController : ControllerBase
{
    private readonly IInvestmentService _investmentService;
    private readonly ILogger<InvestmentController> _logger;

    /// <summary>
    ///     Initializes a new instance of the InvestmentController class. (EN)<br />
    ///     Khởi tạo một phiên bản mới của lớp InvestmentController. (VI)
    /// </summary>
    /// <param name="investmentService">The investment service.</param>
    /// <param name="logger">The logger instance.</param>
    public InvestmentController(IInvestmentService investmentService, ILogger<InvestmentController> logger)
    {
        _investmentService = investmentService;
        _logger = logger;
    }

    /// <summary>
    ///     Gets all investments for the authenticated user. (EN)<br />
    ///     Lấy tất cả các khoản đầu tư cho người dùng đã xác thực. (VI)
    /// </summary>
    /// <returns>A list of user investments.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<InvestmentViewModel>>> GetUserInvestments()
    {
        try
        {
            var userId = GetUserId();
            _logger.LogTrace("Getting investments for user {userId}", userId);
            
            var investments = await _investmentService.GetUserInvestmentsAsync(userId);
            return Ok(investments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting user investments");
            return StatusCode(500, "An error occurred while retrieving investments");
        }
    }

    /// <summary>
    ///     Gets a specific investment by ID. (EN)<br />
    ///     Lấy một khoản đầu tư cụ thể theo ID. (VI)
    /// </summary>
    /// <param name="id">The investment identifier.</param>
    /// <returns>The investment details.</returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InvestmentViewModel>> GetInvestment(Guid id)
    {
        try
        {
            var userId = GetUserId();
            _logger.LogTrace("Getting investment {id} for user {userId}", id, userId);
            
            var investment = await _investmentService.GetByIdAsync(id);
            if (investment == null)
            {
                return NotFound($"Investment with ID {id} not found");
            }

            // Verify the investment belongs to the authenticated user
            // Xác minh khoản đầu tư thuộc về người dùng đã xác thực
            if (investment.UserId != userId)
            {
                return Forbid("You do not have permission to access this investment");
            }

            return Ok(investment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting investment {id}", id);
            return StatusCode(500, "An error occurred while retrieving the investment");
        }
    }

    /// <summary>
    ///     Creates a new investment. (EN)<br />
    ///     Tạo một khoản đầu tư mới. (VI)
    /// </summary>
    /// <param name="request">The investment creation request.</param>
    /// <returns>The created investment.</returns>
    [HttpPost]
    public async Task<ActionResult<InvestmentViewModel>> CreateInvestment([FromBody] CreateInvestmentRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetUserId();
            request.UserId = userId; // Ensure the investment is created for the authenticated user
            
            _logger.LogTrace("Creating investment for user {userId}", userId);
            
            var investment = await _investmentService.CreateAsync(request);
            return CreatedAtAction(nameof(GetInvestment), new { id = investment?.Id }, investment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating investment");
            return StatusCode(500, "An error occurred while creating the investment");
        }
    }

    /// <summary>
    ///     Updates an existing investment. (EN)<br />
    ///     Cập nhật một khoản đầu tư hiện có. (VI)
    /// </summary>
    /// <param name="id">The investment identifier.</param>
    /// <param name="request">The investment update request.</param>
    /// <returns>The updated investment.</returns>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<InvestmentViewModel>> UpdateInvestment(Guid id, [FromBody] UpdateInvestmentRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetUserId();
            _logger.LogTrace("Updating investment {id} for user {userId}", id, userId);
            
            // Verify the investment exists and belongs to the user
            // Xác minh khoản đầu tư tồn tại và thuộc về người dùng
            var existingInvestment = await _investmentService.GetByIdAsync(id);
            if (existingInvestment == null)
            {
                return NotFound($"Investment with ID {id} not found");
            }

            if (existingInvestment.UserId != userId)
            {
                return Forbid("You do not have permission to update this investment");
            }

            var investment = await _investmentService.UpdateAsync(id, request);
            return Ok(investment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating investment {id}", id);
            return StatusCode(500, "An error occurred while updating the investment");
        }
    }

    /// <summary>
    ///     Deletes an investment. (EN)<br />
    ///     Xóa một khoản đầu tư. (VI)
    /// </summary>
    /// <param name="id">The investment identifier.</param>
    /// <returns>No content result.</returns>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteInvestment(Guid id)
    {
        try
        {
            var userId = GetUserId();
            _logger.LogTrace("Deleting investment {id} for user {userId}", id, userId);
            
            // Verify the investment exists and belongs to the user
            // Xác minh khoản đầu tư tồn tại và thuộc về người dùng
            var existingInvestment = await _investmentService.GetByIdAsync(id);
            if (existingInvestment == null)
            {
                return NotFound($"Investment with ID {id} not found");
            }

            if (existingInvestment.UserId != userId)
            {
                return Forbid("You do not have permission to delete this investment");
            }

            await _investmentService.DeleteHardAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting investment {id}", id);
            return StatusCode(500, "An error occurred while deleting the investment");
        }
    }

    /// <summary>
    ///     Updates the market price of an investment. (EN)<br />
    ///     Cập nhật giá thị trường của một khoản đầu tư. (VI)
    /// </summary>
    /// <param name="id">The investment identifier.</param>
    /// <param name="request">The market price update request.</param>
    /// <returns>The updated investment.</returns>
    [HttpPatch("{id:guid}/market-price")]
    public async Task<ActionResult<InvestmentViewModel>> UpdateMarketPrice(Guid id, [FromBody] UpdateMarketPriceRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetUserId();
            _logger.LogTrace("Updating market price for investment {id} for user {userId}", id, userId);
            
            // Verify the investment exists and belongs to the user
            // Xác minh khoản đầu tư tồn tại và thuộc về người dùng
            var existingInvestment = await _investmentService.GetByIdAsync(id);
            if (existingInvestment == null)
            {
                return NotFound($"Investment with ID {id} not found");
            }

            if (existingInvestment.UserId != userId)
            {
                return Forbid("You do not have permission to update this investment");
            }

            var investment = await _investmentService.UpdateMarketPriceAsync(id, request);
            return Ok(investment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating market price for investment {id}", id);
            return StatusCode(500, "An error occurred while updating the market price");
        }
    }

    /// <summary>
    ///     Gets portfolio summary for the authenticated user. (EN)<br />
    ///     Lấy tóm tắt danh mục cho người dùng đã xác thực. (VI)
    /// </summary>
    /// <returns>The portfolio summary.</returns>
    [HttpGet("portfolio/summary")]
    public async Task<ActionResult<PortfolioSummaryViewModel>> GetPortfolioSummary()
    {
        try
        {
            var userId = GetUserId();
            _logger.LogTrace("Getting portfolio summary for user {userId}", userId);
            
            var summary = await _investmentService.GetPortfolioSummaryAsync(userId);
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting portfolio summary");
            return StatusCode(500, "An error occurred while retrieving portfolio summary");
        }
    }

    /// <summary>
    ///     Extracts the user ID from the authentication claims. (EN)<br />
    ///     Trích xuất ID người dùng từ các claims xác thực. (VI)
    /// </summary>
    /// <returns>The user identifier.</returns>
    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                         ?? User.FindFirst("sub")?.Value 
                         ?? User.FindFirst("user_id")?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Unable to extract user ID from authentication token");
        }

        return userId;
    }
}

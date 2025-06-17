using Identity.Application.Services.Roles;
using Identity.Contracts.Roles;
using Identity.Contracts.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Controllers;

/// <summary>
/// Controller for role management operations (EN)<br/>
/// Controller cho các thao tác quản lý vai trò (VI)
/// </summary>
/// <param name="roleService">
/// Service for role operations (EN)<br/>
/// Dịch vụ cho các thao tác vai trò (VI)
/// </param>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "RequireAdmin")]
public class RolesController(IRoleService roleService) : ControllerBase
{
    /// <summary>
    /// Get all roles
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<RoleResponse>>>> GetRoles()
    {
        var roles = await roleService.GetAllAsync();
        
        return Ok(new ApiResponse<List<RoleResponse>>
        {
            Success = true,
            Message = "Roles retrieved successfully",
            Data = roles.ToList()
        });
    }

    /// <summary>
    /// Get role by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<RoleResponse>>> GetRole(Guid id)
    {
        var role = await roleService.GetByIdAsync(id);
        
        if (role == null)
        {
            return NotFound(new ApiResponse<RoleResponse>
            {
                Success = false,
                Message = "Role not found"
            });
        }

        return Ok(new ApiResponse<RoleResponse>
        {
            Success = true,
            Message = "Role retrieved successfully",
            Data = role
        });
    }

    /// <summary>
    /// Create new role
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<RoleResponse>>> CreateRole(
        [FromBody] CreateRoleRequest request)
    {
        var role = await roleService.CreateAsync(request);
        
        return CreatedAtAction(
            nameof(GetRole),
            new { id = role.Id },
            new ApiResponse<RoleResponse>
            {
                Success = true,
                Message = "Role created successfully",
                Data = role
            });
    }

    /// <summary>
    /// Update role
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<RoleResponse>>> UpdateRole(
        Guid id,
        [FromBody] UpdateRoleRequest request)
    {
        var role = await roleService.UpdateAsync(id, request);
        
        return Ok(new ApiResponse<RoleResponse>
        {
            Success = true,
            Message = "Role updated successfully",
            Data = role
        });
    }

    /// <summary>
    /// Delete role
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteRole(Guid id)
    {
        await roleService.DeleteAsync(id);
        
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Role deleted successfully"
        });
    }

    /// <summary>
    /// Assign role to user
    /// </summary>
    [HttpPost("{roleId:guid}/users/{userId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> AssignRoleToUser(
        Guid roleId,
        Guid userId)    {
        await roleService.AssignRoleToUserAsync(roleId, userId);
        
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Role assigned to user successfully"
        });
    }

    /// <summary>
    /// Remove role from user
    /// </summary>
    [HttpDelete("{roleId:guid}/users/{userId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> RemoveRoleFromUser(
        Guid roleId,
        Guid userId)    {
        await roleService.RemoveRoleFromUserAsync(roleId, userId);
        
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Role removed from user successfully"
        });
    }
}

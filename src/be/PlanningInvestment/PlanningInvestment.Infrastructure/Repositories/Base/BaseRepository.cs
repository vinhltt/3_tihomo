using System.Linq.Expressions;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PlanningInvestment.Domain.BaseRepositories;
using PlanningInvestment.Infrastructure;
using Shared.EntityFramework.BaseEfModels;

namespace PlanningInvestment.Infrastructure.Repositories.Base;

/// <summary>
///     Base class for repositories (EN)<br />
///     Lớp cơ sở cho các repository (VI)
/// </summary>
public class BaseRepository<TEntity, TKey>(
    PlanningInvestmentDbContext context,
    IHttpContextAccessor? httpContextAccessor
)
    : IBaseRepository<TEntity, TKey>
    where TEntity : BaseEntity<TKey>
{
    #region ctor

    public BaseRepository(PlanningInvestmentDbContext context) : this(context, null)
    {
    }

    #endregion ctor

    #region props

    private DbSet<TEntity>? EntitiesDbSet { get; set; }

    #endregion props

    #region public

    /// <summary>
    ///     Retrieves entities without tracking, including specified navigation properties (EN)<br />
    ///     Truy xuất các thực thể không theo dõi, bao gồm các thuộc tính điều hướng được chỉ định (VI)
    /// </summary>
    /// <param name="includes">Navigation properties to include (EN)<br />Các thuộc tính điều hướng cần bao gồm (VI)</param>
    /// <returns>An IQueryable of entities</returns>
    public IQueryable<TEntity> GetNoTrackingEntities(
        params Expression<Func<TEntity, object>>[] includes)
    {
        var query = Entities.AsNoTracking();
        query = includes.Aggregate(query, (current, include) => current.Include(include));
        query = query.Where(e => e.CreateBy == GetUserNameInHttpContext());
        return query;
    }

    /// <summary>
    ///     Creates a list of new entities asynchronously. (EN)<br />
    ///     Tạo một danh sách các thực thể mới một cách bất đồng bộ. (VI)
    /// </summary>
    /// <param name="entities">The list of entities to create.</param>
    /// <returns>A task representing the asynchronous operation, containing the number of state entries written to the database.</returns>
    public async Task<int> CreateAsync(List<TEntity> entities)
    {
        var currentUser = GetUserNameInHttpContext();
        var utcNow = DateTime.UtcNow;

        foreach (var entity in entities)
        {
            entity.CreatedAt = utcNow;
            entity.UpdatedAt = utcNow;
            entity.CreateBy = currentUser;
            entity.UpdateBy = currentUser;
        }

        await Entities.AddRangeAsync(entities);
        return await context.SaveChangesAsync();
    }

    /// <summary>
    ///     Creates a new entity asynchronously. (EN)<br />
    ///     Tạo một thực thể mới một cách bất đồng bộ. (VI)
    /// </summary>
    /// <param name="entity">The entity to create.</param>
    /// <returns>A task representing the asynchronous operation, containing the number of state entries written to the database.</returns>
    public async Task<int> CreateAsync(TEntity entity)
    {
        var currentUser = GetUserNameInHttpContext();
        var utcNow = DateTime.UtcNow;

        entity.CreatedAt = utcNow;
        entity.UpdatedAt = utcNow;
        entity.CreateBy = currentUser;
        entity.UpdateBy = currentUser;

        await Entities.AddAsync(entity);
        return await context.SaveChangesAsync();
    }

    /// <summary>
    ///     Updates an existing entity asynchronously. (EN)<br />
    ///     Cập nhật một thực thể hiện có một cách bất đồng bộ. (VI)
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <returns>A task representing the asynchronous operation, containing the number of state entries written to the database.</returns>
    public async Task<int> UpdateAsync(TEntity entity)
    {
        var currentUser = GetUserNameInHttpContext();
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdateBy = currentUser;

        Entities.Update(entity);
        return await context.SaveChangesAsync();
    }

    /// <summary>
    ///     Deletes entities by their identifiers asynchronously. (EN)<br />
    ///     Xóa các thực thể theo định danh của chúng một cách bất đồng bộ. (VI)
    /// </summary>
    /// <param name="ids">The identifiers of the entities to delete.</param>
    /// <returns>A task representing the asynchronous operation, containing the number of state entries written to the database.</returns>
    public async Task<int> DeleteAsync(IEnumerable<TKey> ids)
    {
        var currentUser = GetUserNameInHttpContext();
        var entities = await Entities.Where(e => ids.Contains(e.Id) && e.CreateBy == currentUser).ToListAsync();
        
        if (entities.Count == 0)
            return 0;

        Entities.RemoveRange(entities);
        return await context.SaveChangesAsync();
    }

    /// <summary>
    ///     Deletes an entity by its identifier asynchronously. (EN)<br />
    ///     Xóa một thực thể theo định danh của nó một cách bất đồng bộ. (VI)
    /// </summary>
    /// <param name="id">The identifier of the entity to delete.</param>
    /// <returns>A task representing the asynchronous operation, containing the number of state entries written to the database.</returns>
    public async Task<int> DeleteAsync(TKey id)
    {
        var currentUser = GetUserNameInHttpContext();
        var entity = await Entities.FirstOrDefaultAsync(e => e.Id!.Equals(id) && e.CreateBy == currentUser);
        
        if (entity == null)
            return 0;

        Entities.Remove(entity);
        return await context.SaveChangesAsync();
    }

    /// <summary>
    ///     Gets an entity by its identifier asynchronously. (EN)<br />
    ///     Lấy một thực thể theo định danh của nó một cách bất đồng bộ. (VI)
    /// </summary>
    /// <param name="id">The identifier of the entity.</param>
    /// <param name="includes">Navigation properties to include.</param>
    /// <returns>A task representing the asynchronous operation, containing the entity if found.</returns>
    public async Task<TEntity?> GetByIdAsync(TKey id, params Expression<Func<TEntity, object>>[] includes)
    {
        var query = Entities.AsQueryable();
        query = includes.Aggregate(query, (current, include) => current.Include(include));
        
        var currentUser = GetUserNameInHttpContext();
        return await query.FirstOrDefaultAsync(e => e.Id!.Equals(id) && e.CreateBy == currentUser);
    }

    /// <summary>
    ///     Gets all entities asynchronously. (EN)<br />
    ///     Lấy tất cả các thực thể một cách bất đồng bộ. (VI)
    /// </summary>
    /// <param name="includes">Navigation properties to include.</param>
    /// <returns>A task representing the asynchronous operation, containing the collection of entities.</returns>
    public async Task<List<TEntity>> GetAllAsync(params Expression<Func<TEntity, object>>[] includes)
    {
        var query = Entities.AsQueryable();
        query = includes.Aggregate(query, (current, include) => current.Include(include));
        
        var currentUser = GetUserNameInHttpContext();
        return await query.Where(e => e.CreateBy == currentUser).ToListAsync();
    }

    /// <summary>
    ///     Gets entities based on a predicate asynchronously. (EN)<br />
    ///     Lấy các thực thể dựa trên một điều kiện một cách bất đồng bộ. (VI)
    /// </summary>
    /// <param name="predicate">The predicate to filter entities.</param>
    /// <param name="includes">Navigation properties to include.</param>
    /// <returns>A task representing the asynchronous operation, containing the collection of entities.</returns>
    public async Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes)
    {
        var query = Entities.AsQueryable();
        query = includes.Aggregate(query, (current, include) => current.Include(include));
        
        var currentUser = GetUserNameInHttpContext();
        return await query.Where(predicate).Where(e => e.CreateBy == currentUser).ToListAsync();
    }

    /// <summary>
    ///     Gets entities with pagination asynchronously. (EN)<br />
    ///     Lấy các thực thể với phân trang một cách bất đồng bộ. (VI)
    /// </summary>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The number of entities per page.</param>
    /// <param name="includes">Navigation properties to include.</param>
    /// <returns>A task representing the asynchronous operation, containing the collection of entities.</returns>
    public async Task<IEnumerable<TEntity>> GetPagedAsync(int pageNumber, int pageSize, params Expression<Func<TEntity, object>>[] includes)
    {
        var query = Entities.AsQueryable();
        query = includes.Aggregate(query, (current, include) => current.Include(include));
        
        var currentUser = GetUserNameInHttpContext();
        return await query.Where(e => e.CreateBy == currentUser)
                          .Skip((pageNumber - 1) * pageSize)
                          .Take(pageSize)
                          .ToListAsync();
    }

    /// <summary>
    ///     Counts entities based on a predicate asynchronously. (EN)<br />
    ///     Đếm các thực thể dựa trên một điều kiện một cách bất đồng bộ. (VI)
    /// </summary>
    /// <param name="predicate">The predicate to filter entities.</param>
    /// <returns>A task representing the asynchronous operation, containing the count of entities.</returns>
    public async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate)
    {
        var currentUser = GetUserNameInHttpContext();
        return await Entities.Where(predicate).Where(e => e.CreateBy == currentUser).CountAsync();
    }

    /// <summary>
    ///     Counts all entities asynchronously. (EN)<br />
    ///     Đếm tất cả các thực thể một cách bất đồng bộ. (VI)
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing the count of entities.</returns>
    public async Task<int> CountAsync()
    {
        var currentUser = GetUserNameInHttpContext();
        return await Entities.Where(e => e.CreateBy == currentUser).CountAsync();
    }

    /// <summary>
    ///     Updates a collection of existing entities asynchronously. (EN)<br />
    ///     Cập nhật một tập hợp các thực thể hiện có một cách bất đồng bộ. (VI)
    /// </summary>
    /// <param name="entities">The collection of entities to update.</param>
    /// <returns>A task representing the asynchronous operation, containing the number of state entries written to the database.</returns>
    public async Task<int> UpdateAsync(IEnumerable<TEntity> entities)
    {
        var currentUser = GetUserNameInHttpContext();
        var utcNow = DateTime.UtcNow;

        foreach (var entity in entities)
        {
            entity.UpdatedAt = utcNow;
            entity.UpdateBy = currentUser;
        }

        Entities.UpdateRange(entities);
        return await context.SaveChangesAsync();
    }

    /// <summary>
    ///     Deletes an entity permanently from the database asynchronously based on its key values. (EN)<br />
    ///     Xóa vĩnh viễn một thực thể khỏi cơ sở dữ liệu một cách bất đồng bộ dựa trên giá trị khóa của nó. (VI)
    /// </summary>
    /// <param name="keyValues">The key values of the entity to delete.</param>
    /// <returns>A task representing the asynchronous operation, containing the number of state entries written to the database.</returns>
    public async Task<int> DeleteHardAsync(params object[] keyValues)
    {
        var entity = await Entities.FindAsync(keyValues);
        if (entity == null)
            return 0;

        var currentUser = GetUserNameInHttpContext();
        if (entity.CreateBy != currentUser)
            return 0; // Don't allow deleting entities not owned by current user

        Entities.Remove(entity);
        return await context.SaveChangesAsync();
    }

    /// <summary>
    ///     Deletes an entity permanently from the database. (EN)<br />
    ///     Xóa vĩnh viễn một thực thể khỏi cơ sở dữ liệu. (VI)
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    public void DeleteHard(TEntity entity)
    {
        var currentUser = GetUserNameInHttpContext();
        if (entity.CreateBy == currentUser)
        {
            Entities.Remove(entity);
        }
    }

    /// <summary>
    ///     Soft deletes an entity asynchronously based on its key values by marking it as deleted. (EN)<br />
    ///     Xóa mềm một thực thể một cách bất đồng bộ dựa trên giá trị khóa của nó bằng cách đánh dấu nó đã bị xóa. (VI)
    /// </summary>
    /// <param name="keyValues">The key values of the entity to soft delete.</param>
    /// <returns>A task representing the asynchronous operation, containing the number of state entries written to the database.</returns>
    public async Task<int> DeleteSoftAsync(params object[] keyValues)
    {
        var entity = await Entities.FindAsync(keyValues);
        if (entity == null)
            return 0;

        var currentUser = GetUserNameInHttpContext();
        if (entity.CreateBy != currentUser)
            return 0; // Don't allow deleting entities not owned by current user

        // For Investment entities, we don't have soft delete property, so we'll use hard delete
        // In a real scenario, you might want to add IsDeleted property to BaseEntity
        Entities.Remove(entity);
        return await context.SaveChangesAsync();
    }

    /// <summary>
    ///     Soft deletes an entity asynchronously by marking it as deleted. (EN)<br />
    ///     Xóa mềm một thực thể một cách bất đồng bộ bằng cách đánh dấu nó đã bị xóa. (VI)
    /// </summary>
    /// <param name="entity">The entity to soft delete.</param>
    /// <returns>A task representing the asynchronous operation, containing the number of state entries written to the database.</returns>
    public async Task<int> DeleteSoftAsync(TEntity entity)
    {
        var currentUser = GetUserNameInHttpContext();
        if (entity.CreateBy != currentUser)
            return 0; // Don't allow deleting entities not owned by current user

        // For Investment entities, we don't have soft delete property, so we'll use hard delete
        // In a real scenario, you might want to add IsDeleted property to BaseEntity
        Entities.Remove(entity);
        return await context.SaveChangesAsync();
    }

    /// <summary>
    ///     Gets an entity by its identifier without tracking asynchronously. (EN)<br />
    ///     Lấy một thực thể theo định danh của nó mà không theo dõi một cách bất đồng bộ. (VI)
    /// </summary>
    /// <param name="id">The identifier of the entity.</param>
    /// <param name="includes">Navigation properties to include.</param>
    /// <returns>A task representing the asynchronous operation, containing the entity if found.</returns>
    public async Task<TEntity?> GetByIdNoTrackingAsync(TKey id, params Expression<Func<TEntity, object>>[] includes)
    {
        var query = Entities.AsNoTracking().AsQueryable();
        query = includes.Aggregate(query, (current, include) => current.Include(include));
        
        var currentUser = GetUserNameInHttpContext();
        return await query.FirstOrDefaultAsync(e => e.Id!.Equals(id) && e.CreateBy == currentUser);
    }

    /// <summary>
    ///     Gets a queryable table for the entity, including specified navigation properties. (EN)<br />
    ///     Lấy một bảng có thể truy vấn cho thực thể, bao gồm các thuộc tính điều hướng được chỉ định. (VI)
    /// </summary>
    /// <param name="includes">Navigation properties to include.</param>
    /// <returns>An IQueryable of entities.</returns>
    public IQueryable<TEntity> GetQueryableTable(params Expression<Func<TEntity, object>>[] includes)
    {
        var query = Entities.AsQueryable();
        query = includes.Aggregate(query, (current, include) => current.Include(include));
        
        var currentUser = GetUserNameInHttpContext();
        return query.Where(e => e.CreateBy == currentUser);
    }

    /// <summary>
    ///     Gets entities without tracking and with identity resolution, including specified navigation properties. (EN)<br />
    ///     Lấy các thực thể không theo dõi và với độ phân giải định danh, bao gồm các thuộc tính điều hướng được chỉ định. (VI)
    /// </summary>
    /// <param name="includes">Navigation properties to include.</param>
    /// <returns>An IQueryable of entities.</returns>
    public IQueryable<TEntity> GetNoTrackingEntitiesIdentityResolution(params Expression<Func<TEntity, object>>[] includes)
    {
        var query = Entities.AsNoTrackingWithIdentityResolution();
        query = includes.Aggregate(query, (current, include) => current.Include(include));
        
        var currentUser = GetUserNameInHttpContext();
        return query.Where(e => e.CreateBy == currentUser);
    }

    /// <summary>
    ///     Checks if any entity exists based on a predicate asynchronously. (EN)<br />
    ///     Kiểm tra xem có bất kỳ thực thể nào tồn tại dựa trên một điều kiện một cách bất đồng bộ. (VI)
    /// </summary>
    /// <param name="predicate">The predicate to filter entities.</param>
    /// <returns>A task representing the asynchronous operation, containing a value indicating whether any entity exists.</returns>
    public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate)
    {
        var currentUser = GetUserNameInHttpContext();
        return await Entities.Where(predicate).Where(e => e.CreateBy == currentUser).AnyAsync();
    }

    #endregion public

    #region private

    /// <summary>
    ///     Gets the DbSet for the entity type. (EN)<br />
    ///     Lấy DbSet cho kiểu thực thể. (VI)
    /// </summary>
    private DbSet<TEntity> Entities => EntitiesDbSet ??= context.Set<TEntity>();

    /// <summary>
    ///     Extracts the username from the HTTP context. (EN)<br />
    ///     Trích xuất tên người dùng từ ngữ cảnh HTTP. (VI)
    /// </summary>
    /// <returns>The username from the authentication context.</returns>
    private string GetUserNameInHttpContext()
    {
        var userContext = httpContextAccessor?.HttpContext?.User;
        if (userContext?.Identity?.IsAuthenticated == true)
        {
            return userContext.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? userContext.FindFirst("sub")?.Value
                   ?? userContext.FindFirst("user_id")?.Value
                   ?? "unknown";
        }
        return "system"; // Fallback for cases where HTTP context is not available
    }

    #endregion private
}

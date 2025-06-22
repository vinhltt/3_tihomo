using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PlanningInvestment.Domain.Entities;

#pragma warning disable CS8618, CS9264

namespace PlanningInvestment.Infrastructure;

/// <summary>
///     Database context for the Planning Investment application (EN)<br />
///     Ngữ cảnh cơ sở dữ liệu cho ứng dụng Lập kế hoạch Đầu tư (VI)
/// </summary>
public class PlanningInvestmentDbContext : DbContext
{
    /// <summary>
    ///     The default connection string name for the database (EN)<br />
    ///     Tên chuỗi kết nối mặc định cho cơ sở dữ liệu (VI)
    /// </summary>
    public const string DEFAULT_CONNECTION_STRING = "PlanningInvestmentDb";

    // ReSharper disable once NotAccessedField.Local
    private readonly IConfiguration _configuration;

    /// <summary>
    ///     Initializes a new instance of the PlanningInvestmentDbContext class (EN)<br />
    ///     Khởi tạo một phiên bản mới của lớp PlanningInvestmentDbContext (VI)
    /// </summary>
    public PlanningInvestmentDbContext()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the PlanningInvestmentDbContext class with specified options and configuration (EN)
    ///     <br />
    ///     Khởi tạo một phiên bản mới của lớp PlanningInvestmentDbContext với các tùy chọn và cấu hình được chỉ định (VI)
    /// </summary>
    /// <param name="options">The DbContext options (EN)<br />Các tùy chọn DbContext (VI)</param>
    /// <param name="configuration">The application configuration (EN)<br />Cấu hình ứng dụng (VI)</param>
    public PlanningInvestmentDbContext(DbContextOptions<PlanningInvestmentDbContext> options,
        IConfiguration configuration) : base(options)
    {
        _configuration = configuration;
    }

    /// <summary>
    ///     Gets or sets the DbSet for Debts (EN)<br />
    ///     Lấy hoặc thiết lập DbSet cho các Khoản nợ (VI)
    /// </summary>
    public DbSet<Debt> Debts { get; set; }

    /// <summary>
    ///     Configures the database model for the Planning Investment context (EN)<br />
    ///     Cấu hình mô hình cơ sở dữ liệu cho ngữ cảnh Lập kế hoạch Đầu tư (VI)
    /// </summary>
    /// <param name="modelBuilder">The model builder instance (EN)<br />Phiên bản xây dựng mô hình (VI)</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure table names using snake_case convention
        // Cấu hình tên bảng sử dụng quy ước snake_case
        modelBuilder.Entity<Debt>().ToTable("debts");        // Configure entity properties
        // Cấu hình thuộc tính thực thể
        modelBuilder.Entity<Debt>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.CreateBy).HasColumnName("create_by");
            entity.Property(e => e.UpdateBy).HasColumnName("update_by");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        });
    }

    /// <summary>
    ///     Configures the database connection if not already configured (EN)<br />
    ///     Cấu hình kết nối cơ sở dữ liệu nếu chưa được cấu hình (VI)
    /// </summary>
    /// <param name="optionsBuilder">The options builder (EN)<br />Trình xây dựng tùy chọn (VI)</param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Default configuration for design-time
            // Cấu hình mặc định cho thời gian thiết kế
            optionsBuilder.UseNpgsql("Host=localhost;Database=db_planning;Username=planning_user;Password=planning_pass;Port=5436");
        }
    }
}

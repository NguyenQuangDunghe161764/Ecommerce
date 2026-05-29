using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace demoWebAPI.Models;

public partial class EcomDbContext
    : IdentityDbContext<ApplicationUser>
{
    public EcomDbContext()
    {
    }

    public EcomDbContext(
        DbContextOptions<EcomDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<Orderdetail> Orderdetails { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Productimage> Productimages { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<RolePermission> RolePermissions { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }
    public DbSet<Address> UserAddresses { get; set; }


    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PRIMARY");

            entity.ToTable("categories");

            entity.Property(e => e.Description)
                .HasColumnType("text");

            entity.Property(e => e.Name)
                .HasMaxLength(255);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PRIMARY");

            entity.ToTable("orders");

            entity.HasIndex(e => e.UserId,
                "IX_Orders_UserId");

            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql(
                    "CURRENT_TIMESTAMP")
                .HasColumnType("datetime");

            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Pending");

            entity.Property(e => e.TotalAmount)
                .HasPrecision(18, 2);

            entity.Property(e => e.ZaloPayAppTransId)
                .HasMaxLength(50);

            entity.HasOne(d => d.User)
                .WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Orderdetail>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PRIMARY");

            entity.ToTable("orderdetails");

            entity.HasIndex(e => e.OrderId,
                "IX_OrderDetails_OrderId");

            entity.HasIndex(e => e.ProductId,
                "ProductId");

            entity.Property(e => e.UnitPrice)
                .HasPrecision(18, 2);

            entity.HasOne(d => d.Order)
                .WithMany(p => p.Orderdetails)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Product)
                .WithMany(p => p.Orderdetails)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PRIMARY");

            entity.ToTable("permissions");

            entity.HasIndex(e => e.Name)
                .IsUnique();

            entity.Property(e => e.Name)
                .HasMaxLength(100);

            entity.Property(e => e.Description)
                .HasColumnType("text");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PRIMARY");

            entity.ToTable("products");

            entity.HasIndex(e => e.CategoryId,
                "IX_Products_CategoryId");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql(
                    "CURRENT_TIMESTAMP")
                .HasColumnType("datetime");

            entity.Property(e => e.Description)
                .HasColumnType("text");

            entity.Property(e => e.Name)
                .HasMaxLength(255);

            entity.Property(e => e.Price)
                .HasPrecision(18, 2);

            entity.HasOne(d => d.Category)
                .WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Owner)
                .WithMany()
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Productimage>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PRIMARY");

            entity.ToTable("productimages");

            entity.HasIndex(e => e.ProductId,
                "ProductId");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql(
                    "CURRENT_TIMESTAMP")
                .HasColumnType("datetime");

            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500);

            entity.Property(e => e.IsMain)
                .HasDefaultValue(false);

            entity.HasOne(d => d.Product)
                .WithMany(p => p.Productimages)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PRIMARY");

            entity.ToTable("reviews");

            entity.HasIndex(e => e.ProductId,
                "ProductId");

            entity.HasIndex(e => e.UserId,
                "UserId");

            entity.Property(e => e.Comment)
                .HasColumnType("text");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql(
                    "CURRENT_TIMESTAMP")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.User)
                .WithMany(p => p.Reviews)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        // REFRESH TOKEN
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.ToTable("refreshtokens");

            entity.Property(e => e.Token)
                .HasMaxLength(500);

            entity.HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ROLE PERMISSION
        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(rp => new
            {
                rp.RoleId,
                rp.PermissionId
            });

            entity.ToTable("rolepermissions");

            entity.HasOne(rp => rp.Role)
                .WithMany()
                .HasForeignKey(rp => rp.RoleId);

            entity.HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(
        ModelBuilder modelBuilder);
}
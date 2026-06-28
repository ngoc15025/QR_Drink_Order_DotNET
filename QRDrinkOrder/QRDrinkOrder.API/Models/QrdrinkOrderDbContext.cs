using Microsoft.EntityFrameworkCore;

namespace QRDrinkOrder.API.Models;

public partial class QrdrinkOrderDbContext : DbContext
{
    public QrdrinkOrderDbContext()
    {
    }

    public QrdrinkOrderDbContext(DbContextOptions<QrdrinkOrderDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<BankAccount> BankAccounts { get; set; }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<CategoryTranslation> CategoryTranslations { get; set; }

    public virtual DbSet<Coupon> Coupons { get; set; }

    public virtual DbSet<CouponUsage> CouponUsages { get; set; }

    public virtual DbSet<CustomerSession> CustomerSessions { get; set; }

    public virtual DbSet<Drink> Drinks { get; set; }

    public virtual DbSet<DrinkTranslation> DrinkTranslations { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Manager> Managers { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Promotion> Promotions { get; set; }

    public virtual DbSet<PromotionTranslation> PromotionTranslations { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<ReviewImage> ReviewImages { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<StaffBenefit> StaffBenefits { get; set; }

    public virtual DbSet<Membership> Memberships { get; set; }

    public virtual DbSet<PointHistory> PointHistories { get; set; }

    public virtual DbSet<SystemConfig> SystemConfigs { get; set; }

    public virtual DbSet<PushSubscription> PushSubscriptions { get; set; }

    public virtual DbSet<Size> Sizes { get; set; }

    public virtual DbSet<Topping> Toppings { get; set; }

    public virtual DbSet<OrderItemTopping> OrderItemToppings { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Connection string should be provided via dependency injection in Program.cs
        // optionsBuilder.UseSqlServer("...");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.AccountId).HasName("PK__Accounts__349DA5866FDE3939");

            entity.HasIndex(e => e.Email, "UQ__Accounts__A9D1053403132823").IsUnique();

            entity.Property(e => e.AccountId).HasColumnName("AccountID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.RoleId).HasColumnName("RoleID");

            entity.HasOne(d => d.Role).WithMany(p => p.Accounts)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Accounts__RoleID__5070F446");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__AuditLog__5E5499A8951FD95E");

            entity.Property(e => e.LogId).HasColumnName("LogID");
            entity.Property(e => e.AccountId).HasColumnName("AccountID");
            entity.Property(e => e.Action).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.TableName).HasMaxLength(100);
            entity.Property(e => e.Description).HasColumnType("nvarchar(max)");

            entity.HasOne(d => d.Account).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.AccountId)
                .HasConstraintName("FK__AuditLogs__Accou__14270015");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Categori__19093A2B1E0922B6");

            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.DisplayOrder).HasDefaultValue(0);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<CategoryTranslation>(entity =>
        {
            entity.HasKey(e => e.CategoryTranslationId).HasName("PK__Category__403F34FB79847652");

            entity.Property(e => e.CategoryTranslationId).HasColumnName("CategoryTranslationID");
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CategoryName).HasMaxLength(100);
            entity.Property(e => e.LanguageCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();

            entity.HasOne(d => d.Category).WithMany(p => p.CategoryTranslations)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__CategoryT__Categ__5FB337D6");
        });

        modelBuilder.Entity<Coupon>(entity =>
        {
            entity.HasKey(e => e.CouponId).HasName("PK__Coupons__384AF1DAA6F16E8D");

            entity.HasIndex(e => e.CouponCode, "UQ__Coupons__D34908007458F87A").IsUnique();

            entity.Property(e => e.CouponId).HasColumnName("CouponID");
            entity.Property(e => e.CouponCode).HasMaxLength(50);
            entity.Property(e => e.DiscountValue).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MaxDiscountAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.MinOrderValue)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.StartDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.UsedCount).HasDefaultValue(0);
        });

        modelBuilder.Entity<CouponUsage>(entity =>
        {
            entity.HasKey(e => e.UsageId).HasName("PK__CouponUs__29B197C028A86D3B");

            entity.HasIndex(e => new { e.CouponId, e.Phone }, "UX_Coupon_Phone").IsUnique();

            entity.Property(e => e.UsageId).HasColumnName("UsageID");
            entity.Property(e => e.CouponId).HasColumnName("CouponID");
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.Phone).HasMaxLength(15);
            entity.Property(e => e.UsedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Coupon).WithMany(p => p.CouponUsages)
                .HasForeignKey(d => d.CouponId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CouponUsa__Coupo__22751F6C");

            entity.HasOne(d => d.Order).WithMany(p => p.CouponUsages)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CouponUsa__Order__236943A5");
        });

        modelBuilder.Entity<CustomerSession>(entity =>
        {
            entity.HasKey(e => e.SessionId).HasName("PK__Customer__C9F492701963CAE8");

            entity.HasIndex(e => e.Phone, "IX_CustomerSessions_Phone");

            entity.Property(e => e.SessionId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("SessionID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.DeviceInfo).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(15);
            entity.Property(e => e.PreferredLanguage)
                .HasMaxLength(2)
                .IsUnicode(false)
                .HasDefaultValue("vi")
                .IsFixedLength();
        });

        modelBuilder.Entity<Drink>(entity =>
        {
            entity.HasKey(e => e.DrinkId).HasName("PK__Drinks__C094D3C8A1032C6A");

            entity.Property(e => e.DrinkId).HasColumnName("DrinkID");
            entity.Property(e => e.BasePrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.Category).WithMany(p => p.Drinks)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Drinks__Category__6477ECF3");
        });

        modelBuilder.Entity<DrinkTranslation>(entity =>
        {
            entity.HasKey(e => e.TranslationId).HasName("PK__DrinkTra__663DA0ACB15C22A2");

            entity.Property(e => e.TranslationId).HasColumnName("TranslationID");
            entity.Property(e => e.DrinkId).HasColumnName("DrinkID");
            entity.Property(e => e.DrinkName).HasMaxLength(150);
            entity.Property(e => e.LanguageCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();

            entity.HasOne(d => d.Drink).WithMany(p => p.DrinkTranslations)
                .HasForeignKey(d => d.DrinkId)
                .HasConstraintName("FK__DrinkTran__Drink__6754599E");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmployeeId).HasName("PK__Employee__7AD04FF185C9DB36");

            entity.HasIndex(e => e.AccountId, "UQ__Employee__349DA587D8C40403").IsUnique();

            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.AccountId).HasColumnName("AccountID");
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(15);

            entity.HasOne(d => d.Account).WithOne(p => p.Employee)
                .HasForeignKey<Employee>(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Employees__Accou__5812160E");
        });

        modelBuilder.Entity<Manager>(entity =>
        {
            entity.HasKey(e => e.ManagerId).HasName("PK__Managers__3BA2AA81672BAF8C");

            entity.HasIndex(e => e.AccountId, "UQ__Managers__349DA587206538A8").IsUnique();

            entity.Property(e => e.ManagerId).HasColumnName("ManagerID");
            entity.Property(e => e.AccountId).HasColumnName("AccountID");
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(15);

            entity.HasOne(d => d.Account).WithOne(p => p.Manager)
                .HasForeignKey<Manager>(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Managers__Accoun__5441852A");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Orders__C3905BAFF76502E2");

            entity.HasIndex(e => e.OrderDate, "IX_Orders_Date");

            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.CouponId).HasColumnName("CouponID");
            entity.Property(e => e.DiscountAmount)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.PointsUsed).HasDefaultValue(0);
            entity.Property(e => e.FinalAmount)
                .HasComputedColumnSql("([TotalAmount]-[DiscountAmount])", false)
                .HasColumnType("decimal(19, 2)");
            entity.Property(e => e.OrderDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.OrderStatus).HasDefaultValue((byte)0);
            entity.Property(e => e.SessionId).HasColumnName("SessionID");
            entity.Property(e => e.TableNumber).HasMaxLength(10);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Coupon).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CouponId)
                .HasConstraintName("FK__Orders__CouponID__02084FDA");

            entity.HasOne(d => d.Employee).WithMany(p => p.Orders)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK__Orders__Employee__01142BA1");

            entity.HasOne(d => d.Session).WithMany(p => p.Orders)
                .HasForeignKey(d => d.SessionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Orders__SessionI__00200768");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.OrderItemId).HasName("PK__OrderIte__57ED06A153CE0FD5");

            entity.Property(e => e.OrderItemId).HasColumnName("OrderItemID");
            entity.Property(e => e.DrinkId).HasColumnName("DrinkID");
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.Quantity).HasDefaultValue(1);
            entity.Property(e => e.SubTotal)
                .HasComputedColumnSql("([Quantity]*[UnitPrice])", false)
                .HasColumnType("decimal(29, 2)");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Drink).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.DrinkId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderItem__Drink__06CD04F7");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderItem__Order__05D8E0BE");

            entity.HasOne(d => d.Size).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.SizeId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__OrderItem__Size");
        });

        modelBuilder.Entity<Size>(entity =>
        {
            entity.HasKey(e => e.SizeId).HasName("PK__Size");

            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.PriceOffset).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<Topping>(entity =>
        {
            entity.HasKey(e => e.ToppingId).HasName("PK__Topping");

            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<OrderItemTopping>(entity =>
        {
            entity.HasKey(e => new { e.OrderItemId, e.ToppingId }).HasName("PK__OrderItemTopping");

            entity.HasOne(d => d.OrderItem).WithMany(p => p.OrderItemToppings)
                .HasForeignKey(d => d.OrderItemId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__OrderItemTopping__OrderItem");

            entity.HasOne(d => d.Topping).WithMany(p => p.OrderItemToppings)
                .HasForeignKey(d => d.ToppingId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__OrderItemTopping__Topping");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payments__9B556A5816726265");

            entity.HasIndex(e => e.OrderId, "UQ__Payments__C3905BAE210CF8AF").IsUnique();

            entity.Property(e => e.PaymentId).HasColumnName("PaymentID");
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.PaymentStatus).HasDefaultValue((byte)0);
            entity.Property(e => e.TransactionId)
                .HasMaxLength(100)
                .HasColumnName("TransactionID");

            entity.HasOne(d => d.Order).WithOne(p => p.Payment)
                .HasForeignKey<Payment>(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Payments__OrderI__0B91BA14");
        });

        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.HasKey(e => e.PromotionId).HasName("PK__Promotio__52C42F2FC033942A");

            entity.Property(e => e.PromotionId).HasColumnName("PromotionID");
            entity.Property(e => e.CouponId).HasColumnName("CouponID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.Coupon).WithMany(p => p.Promotions)
                .HasForeignKey(d => d.CouponId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__Promotion__Coupo__72C60C4A");
        });

        modelBuilder.Entity<PromotionTranslation>(entity =>
        {
            entity.HasKey(e => e.TranslationId).HasName("PK__Promotio__663DA0AC27E05244");

            entity.Property(e => e.TranslationId).HasColumnName("TranslationID");
            entity.Property(e => e.LanguageCode)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.PromotionId).HasColumnName("PromotionID");
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.Promotion).WithMany(p => p.PromotionTranslations)
                .HasForeignKey(d => d.PromotionId)
                .HasConstraintName("FK__Promotion__Promo__75A278F5");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PK__Reviews__74BC79AE30D12490");

            entity.HasIndex(e => e.OrderId, "UQ__Reviews__C3905BAEF421A0F6").IsUnique();

            entity.Property(e => e.ReviewId).HasColumnName("ReviewID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.SessionId).HasColumnName("SessionID");

            entity.HasOne(d => d.Order).WithOne(p => p.Review)
                .HasForeignKey<Review>(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reviews__OrderID__19DFD96B");

            entity.HasOne(d => d.Session).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.SessionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reviews__Session__1AD3FDA4");
        });

        modelBuilder.Entity<ReviewImage>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("PK__ReviewIm__7516F4ECDFF38E6C");

            entity.Property(e => e.ImageId).HasColumnName("ImageID");
            entity.Property(e => e.ReviewId).HasColumnName("ReviewID");
            entity.Property(e => e.UploadedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Review).WithMany(p => p.ReviewImages)
                .HasForeignKey(d => d.ReviewId)
                .HasConstraintName("FK__ReviewIma__Revie__1EA48E88");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE3A1F1B8206");

            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<StaffBenefit>(entity =>
        {
            entity.HasKey(e => e.BenefitId).HasName("PK__StaffBen__5754C53A9A8852D5");

            entity.Property(e => e.BenefitId).HasColumnName("BenefitID");
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.UsedDate).HasDefaultValueSql("(CONVERT([date],getdate()))");

            entity.HasOne(d => d.Employee).WithMany(p => p.StaffBenefits)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StaffBene__Emplo__0F624AF8");

            entity.HasOne(d => d.Order).WithMany(p => p.StaffBenefits)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StaffBene__Order__10566F31");
        });

        modelBuilder.Entity<Membership>(entity =>
        {
            entity.HasKey(e => e.MembershipId).HasName("PK__Membership__12345678");

            entity.HasIndex(e => e.Phone, "UQ__Membership__Phone").IsUnique();

            entity.Property(e => e.Phone).HasMaxLength(15);
            entity.Property(e => e.Points).HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<PointHistory>(entity =>
        {
            entity.HasKey(e => e.HistoryId).HasName("PK__PointHistory__12345678");

            entity.Property(e => e.Phone).HasMaxLength(15);
            entity.Property(e => e.Reason).HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Membership).WithMany(p => p.PointHistories)
                .HasPrincipalKey(p => p.Phone)
                .HasForeignKey(d => d.Phone)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PointHistory__Membership");
        });

        modelBuilder.Entity<SystemConfig>(entity =>
        {
            entity.HasKey(e => e.ConfigKey).HasName("PK__SystemConfig__12345678");

            entity.Property(e => e.ConfigKey).HasMaxLength(50);
            entity.Property(e => e.ConfigValue).HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(255);
        });

        modelBuilder.Entity<PushSubscription>(entity =>
        {
            entity.HasKey(e => e.SubscriptionId).HasName("PK__PushSubscription__12345678");

            entity.Property(e => e.Phone).HasMaxLength(15);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

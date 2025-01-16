using DevExpress.ExpressApp.EFCore.Updating;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using DevExpress.Persistent.BaseImpl.EF.PermissionPolicy;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.ExpressApp.Design;
using DevExpress.ExpressApp.EFCore.DesignTime;

namespace GestorTareas.Module.BusinessObjects;

// This code allows our Model Editor to get relevant EF Core metadata at design time.
// For details, please refer to https://supportcenter.devexpress.com/ticket/details/t933891/core-prerequisites-for-design-time-model-editor-with-entity-framework-core-data-model.
public class GestorTareasContextInitializer : DbContextTypesInfoInitializerBase {
    protected override DbContext CreateDbContext() {
        var optionsBuilder = new DbContextOptionsBuilder<GestorTareasEFCoreDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=GestorTareas;Trusted_Connection=True;")//.UseSqlite(";") wrong for a solution with SqLite, see https://isc.devexpress.com/internal/ticket/details/t1240173
            .UseChangeTrackingProxies()
            .UseObjectSpaceLinkProxies();
        return new GestorTareasEFCoreDbContext(optionsBuilder.Options);
    }
}
//This factory creates DbContext for design-time services. For example, it is required for database migration.
public class GestorTareasDesignTimeDbContextFactory : IDesignTimeDbContextFactory<GestorTareasEFCoreDbContext> {
    public GestorTareasEFCoreDbContext CreateDbContext(string[] args) {
        //throw new InvalidOperationException("Make sure that the database connection string and connection provider are correct. After that, uncomment the code below and remove this exception.");
        var optionsBuilder = new DbContextOptionsBuilder<GestorTareasEFCoreDbContext>();
        optionsBuilder.UseSqlServer("Integrated Security=SSPI;Data Source=(localdb)\\mssqllocaldb;Initial Catalog=GestorTareas");
        optionsBuilder.UseChangeTrackingProxies();
        optionsBuilder.UseObjectSpaceLinkProxies();
        return new GestorTareasEFCoreDbContext(optionsBuilder.Options);
    }
}
[TypesInfoInitializer(typeof(GestorTareasContextInitializer))]
public class GestorTareasEFCoreDbContext : DbContext {
    public GestorTareasEFCoreDbContext(DbContextOptions<GestorTareasEFCoreDbContext> options) : base(options) {
    }
    //public DbSet<ModuleInfo> ModulesInfo { get; set; }
    public DbSet<ModelDifference> ModelDifferences { get; set; }
    public DbSet<ModelDifferenceAspect> ModelDifferenceAspects { get; set; }
    public DbSet<PermissionPolicyRole> Roles { get; set; }
    public DbSet<GestorTareas.Module.BusinessObjects.ApplicationUser> Users { get; set; }
    public DbSet<GestorTareas.Module.BusinessObjects.ApplicationUserLoginInfo> UserLoginInfos { get; set; }

    public DbSet<Tarea> Tareas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);
        modelBuilder.UseDeferredDeletion(this);
        modelBuilder.SetOneToManyAssociationDeleteBehavior(DeleteBehavior.SetNull, DeleteBehavior.Cascade);
        modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues);
        modelBuilder.UsePropertyAccessMode(PropertyAccessMode.PreferFieldDuringConstruction);
        modelBuilder.Entity<GestorTareas.Module.BusinessObjects.ApplicationUserLoginInfo>(b => {
            b.HasIndex(nameof(DevExpress.ExpressApp.Security.ISecurityUserLoginInfo.LoginProviderName), nameof(DevExpress.ExpressApp.Security.ISecurityUserLoginInfo.ProviderUserKey)).IsUnique();
        });
        modelBuilder.Entity<ModelDifference>()
            .HasMany(t => t.Aspects)
            .WithOne(t => t.Owner)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EfCoreTest
{
    class Program
    {
        static void Main()
        {
            Parent parent;
            using (var db = new Context())
            {
                parent = new Parent();
                parent.Simple = SimpleValueObject.Create();
                db.Add(parent);
                db.SaveChanges();
            }
            using (var db = new Context())
            {
                parent = db.Parents.Single();
                parent.Simple = SimpleValueObject.Create();
                // Set entity modified for no particular reason other than searching this state in audit logging
                db.Entry(parent).State = EntityState.Modified;
                db.SaveChanges();
            }
        }
    }

    public static class DbContextLogging
    {
        // Here we do the actual audit logging
        public static void SaveAuditLog(ChangeTracker changeTracker)
        {
            foreach (var entry in changeTracker.Entries<IAuditable>())
            {
                switch (entry.State)
                {
                    case EntityState.Modified:
                        var simpleAdded = entry.References.Where(r =>
                            r.TargetEntry != null
                            && r.TargetEntry.State == EntityState.Added
                            && r.TargetEntry.Metadata.IsOwned());

                        // We have Modified parent entities with Added value objects (owned type)
                        foreach (var added in simpleAdded)
                        {
                            // Find corresponding Deleted value objects of the same type
                            var simplesDeleted = changeTracker.Entries().Where(a => a.State == EntityState.Deleted
                                && a.Metadata.ClrType.Equals(added.Metadata.ClrType));

                            // Go through deleted objects to find a match if any
                            foreach (var deleted in simplesDeleted)
                            {
                                foreach (var prop in deleted.CurrentValues.Properties)
                                {
                                    // Foreign keys must match
                                    if (prop.GetContainingForeignKeys().Contains(added.Metadata.ForeignKey))
                                    {
                                        // We have match of old (Deleted) and new (Added) entities
                                        Console.WriteLine($"Parent object: {entry.Metadata.Name}");
                                        Console.WriteLine($"Old value: '{deleted.Entity.ToString()}' Name: '{deleted.Metadata.Name}'");
                                        Console.WriteLine($"New value: '{added.CurrentValue.ToString()}' Name: '{added.Metadata.Name}'");
                                    }
                                }
                            }
                        }
                        break;
                    case EntityState.Added:
                    case EntityState.Deleted:
                    case EntityState.Detached:
                    case EntityState.Unchanged:
                    default:
                        break;
                }
            }
        }
    }

    public class Context : DbContext
    {
        public DbSet<Parent> Parents { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseInMemoryDatabase("EfCoreTest");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.ApplyConfiguration(new ParentConfiguration());

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            DbContextLogging.SaveAuditLog(ChangeTracker);
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        internal class ParentConfiguration : IEntityTypeConfiguration<Parent>
        {
            public void Configure(EntityTypeBuilder<Parent> entity)
            {
                // We are looking for owned types when audit logging
                entity.OwnsOne(e => e.Simple)
                    .Property(e => e.Value)
                    .HasColumnName("Simple")
                    .HasMaxLength(50);
            }
        }
    }

    // We check auditable objects only
    public interface IAuditable { }

    // Parent object with value object property
    public class Parent : IAuditable
    {
        public int Id { get; set; }
        // This is the value object that we want to see changed in audit logs
        public SimpleValueObject Simple { get; set; } = null!;
    }
}
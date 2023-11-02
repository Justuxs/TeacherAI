using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TeacherAI.Data;

namespace TeacherAI.EF
{

    public class TeacherContext : DbContext
    {
        public TeacherContext(DbContextOptions<TeacherContext> options) : base(options)
        {
        }

        public DbSet<Stage> Stages { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Topic> Topics { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Subject>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Name).IsRequired();
                entity.HasMany(s => s.Stages)
                      .WithOne(stage => stage.Subject)
                      .HasForeignKey(stage => stage.SubjectID);
            });

            modelBuilder.Entity<Topic>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Content).IsRequired();
                entity.HasOne(t => t.Stage)
                .WithMany(s => s.Topics)
                .HasForeignKey(t => t.StageID);
            });

            modelBuilder.Entity<Stage>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Name).IsRequired();
                entity.HasOne(s => s.Subject)
                .WithMany(sub => sub.Stages)
                .HasForeignKey(s => s.SubjectID);
            });
        }
    }
}

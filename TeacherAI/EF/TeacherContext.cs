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
            // Define foreign key relationships
            modelBuilder.Entity<Topic>()
                .HasOne(t => t.Stage)
                .WithMany(s => s.Topics)
                .HasForeignKey(t => t.StageID);

            modelBuilder.Entity<Stage>()
                .HasOne(s => s.Subject)
                .WithMany(sub => sub.Stages)
                .HasForeignKey(s => s.SubjectID);
        }
    }
}

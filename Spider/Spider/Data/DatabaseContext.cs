using Microsoft.EntityFrameworkCore;
using Spider.Models;
using System.IO;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Spider.Data
{
    /// <summary>
    /// Контекст базы данных приложения
    /// </summary>
    public class DatabaseContext : DbContext
    {
        /// <summary>
        /// Таблица категорий
        /// </summary>
        public DbSet<Category> Categories { get; set; }

        /// <summary>
        /// Таблица данных категорий
        /// </summary>
        public DbSet<CategoryData> CategoryData { get; set; }

        /// <summary>
        /// Таблица команд
        /// </summary>
        public DbSet<Models.Command> Commands { get; set; }

        /// <summary>
        /// Таблица Docker проектов
        /// </summary>
        public DbSet<DockerProject> DockerProjects { get; set; }

        /// <summary>
        /// Таблица Docker образов
        /// </summary>
        public DbSet<DockerImage> DockerImages { get; set; }

        ///<summary>
        /// Настройка подключения к базе дааных
        /// </summary>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "spider.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        /// <summary>
        /// Настройка моделей и связей между таблицами
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CategoryData>()
                .HasOne(categoryData => categoryData.Category)
                .WithMany(category => category.DataItems)
                .HasForeignKey(categoryData => categoryData.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DockerImage>()
                .HasOne(dockerImage => dockerImage.Project)
                .WithMany(dockerProject => dockerProject.Images)
                .HasForeignKey(dockerImage => dockerImage.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Category>().HasIndex(category => category.Name);

            modelBuilder.Entity<Models.Command>().HasIndex(command => command.Name);

            modelBuilder.Entity<DockerProject>().HasIndex(dockerProject => dockerProject.Name);
        }
    }
}

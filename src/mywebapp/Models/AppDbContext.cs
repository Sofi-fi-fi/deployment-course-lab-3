using Microsoft.EntityFrameworkCore;
using mywebapp.Models.Configuration;
using TaskEntity = mywebapp.Models.Entities.Task;

namespace mywebapp.Models;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
	public DbSet<TaskEntity> Tasks { get; set; }
	
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfiguration(new TaskConfiguration());
		base.OnModelCreating(modelBuilder);
	}
}

using ChatService.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatService.Data
{
	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

		public DbSet<Message> Messages { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Message>()
				.HasIndex(m => m.SenderId);

			modelBuilder.Entity<Message>()
				.HasIndex(m => m.ReceiverId);
		}
	}
}
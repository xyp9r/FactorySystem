using Microsoft.EntityFrameworkCore;
using FactorySystem.Models;

namespace FactorySystem.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    public DbSet<User> Users { get; set; }
    public DbSet<Detail> Details { get; set; }
}
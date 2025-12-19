using Bixd.Models;
using EmployeeService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Migrations;

public partial class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{

    public DbSet<AuthData> AuthData => Set<AuthData>();
    public DbSet<Employee>  Employees => Set<Employee>();
    public DbSet<Role>  Roles => Set<Role>();
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql("Host=postgre;Port=5432;Database=postgres_db;Username=postgres_user;Password=postgres_password");
        }
    }
}

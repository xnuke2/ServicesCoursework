using Bixd.Models;
using EmployeeService.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeService.Migrations;

public partial class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Division> Divisions => Set<Division>();
    public DbSet<Role> Role => Set<Role>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<AuthData>  AuthData => Set<AuthData>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql("Host=postgres;Port=5432;Database=postgres_db;Username=postgres_user;Password=postgres_password");
        }
    }
}

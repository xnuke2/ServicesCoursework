using BenefitService.Models;
using Bixd.Models;
using Microsoft.EntityFrameworkCore;

namespace BenefitService.Migrations;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Benefit> Benefits => Set<Benefit>(); 
    public DbSet<Employee>  Employees => Set<Employee>();
    public DbSet<EmployeeBenefit> EmployeeBenefits => Set<EmployeeBenefit>();


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql("Host=postgres;Port=5432;Database=postgres_db;Username=postgres_user;Password=postgres_password");
        }
    }
}

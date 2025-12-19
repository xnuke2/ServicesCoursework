using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using EmployeeService.Models;
using Microsoft.AspNetCore.Authorization;

namespace EmployeeService.Controllers;

[Authorize(Roles = "Сотрудник кадровой службы")]
public class HomeController (ILogger<HomeController> logger,IWebHostEnvironment _environment): Controller
{

    [Authorize(Roles = "Сотрудник кадровой службы")]
    public IActionResult Index()
    {
        var filePath = Path.Combine(_environment.WebRootPath, "index.html");
        return PhysicalFile(filePath, "text/html");
    }

    [HttpGet("/divisions")]
    public IActionResult Divisions()
    {
        var filePath = Path.Combine(_environment.WebRootPath, "divisions.html");
        return PhysicalFile(filePath, "text/html");
    }
    [HttpGet("/role")]
    public IActionResult Roles()
    {
        var filePath = Path.Combine(_environment.WebRootPath, "role.html");
        return PhysicalFile(filePath, "text/html");
    }
    
}
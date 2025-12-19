using System.Diagnostics;
using BenefitService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BenefitService.Controllers;

//[Authorize(Roles = "Сотрудник кадровой службы")]
[AllowAnonymous]
public class HomeController(ILogger<HomeController> logger,IWebHostEnvironment _environment) : Controller
{


    public IActionResult Index()
    {
        var filePath = Path.Combine(_environment.WebRootPath, "index.html");
        return PhysicalFile(filePath, "text/html");
    }
    [HttpGet("/EmployeeBenefit")]
    public IActionResult EmployeeBenefits()
    {
        var filePath = Path.Combine(_environment.WebRootPath, "empbenefit.html");
        return PhysicalFile(filePath, "text/html");
    }



    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
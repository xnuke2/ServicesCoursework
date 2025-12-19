using BenefitService.Migrations;
using BenefitService.Models.DTO.EmployeeBenefits;
using Bixd.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BenefitService.Controllers;


[ApiController]
[Route("[controller]")]
public class EmployeeBenefitController:Controller
{
    private readonly DbSet<EmployeeBenefit> _employeeBenefits;
    private readonly ApplicationDbContext _context;

    public EmployeeBenefitController( ApplicationDbContext context)
    {
        _context = context;
        _employeeBenefits = context.EmployeeBenefits;
    }
    /// <summary>
    /// return all employee Benefits
    /// </summary>
    /// <returns>list with all employee Benefits(always Ok)</returns>
    [HttpGet("all")]
    public IActionResult Get()
    {
        return Ok(_employeeBenefits.ToList());
    }
    
    /// <summary>
    /// return employeeBenefits by id
    /// </summary>
    /// <returns> employee Benefits</returns>
    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        var role = _employeeBenefits.Find(id);
        if (role == null) return NotFound();
        return Ok(role);
    }
    
    /// <summary>
    /// add new employee Benefits
    /// </summary>
    /// <param name="employeeBenefits"> employee Benefits to add</param>
    /// <returns> employee Benefits to add or BadRequest</returns>
    [HttpPost]
    public IActionResult Post([FromBody] EmployeeBenefitDto? employeeBenefits)
    {
        
        if (employeeBenefits == null
            ||_context.Benefits.Find(employeeBenefits.BenefitsId)==null
            ||_context.Employees.Find(employeeBenefits.EmployeeId)==null) return BadRequest();
        _employeeBenefits.Add(new EmployeeBenefit()
        {
            EmployeeId = employeeBenefits.EmployeeId,
            StartDate = employeeBenefits.StartDate,
            EndDate = employeeBenefits.EndDate,
            BenefitsId = employeeBenefits.BenefitsId
        });
        _context.SaveChanges();
        return Ok(employeeBenefits);
    }
    
    /// <summary>
    /// update employeeBenefits
    /// </summary>
    /// <param name="employeeBenefits"></param>
    /// <returns></returns>
    [HttpPatch]
    public IActionResult Put([FromBody] EmployeeBenefitDtoUpdate? employeeBenefits)
    {
        if (employeeBenefits == null) return BadRequest();
        var toUpdate = _employeeBenefits.Find(employeeBenefits.Id);
        if (toUpdate == null) return NotFound();
        toUpdate.StartDate = employeeBenefits.StartDate??toUpdate.StartDate;
        toUpdate.EndDate = employeeBenefits.EndDate??toUpdate.EndDate;
        toUpdate.BenefitsId = employeeBenefits.BenefitsId??toUpdate.BenefitsId;
        toUpdate.EmployeeId = employeeBenefits.EmployeeId??toUpdate.EmployeeId;
        _employeeBenefits.Update(toUpdate);
        _context.SaveChanges();
        return Ok(toUpdate);

    }
    
    /// <summary>
    /// deletes employee Benefits
    /// </summary>
    /// <param name="id"> employee Benefits id to delete</param>
    /// <returns> employees</returns>
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var employeeBenefits = _employeeBenefits.Find(id);
        if (employeeBenefits == null) return NotFound();
        _employeeBenefits.Remove(employeeBenefits);
        _context.SaveChanges();
        return Ok(employeeBenefits);
    }
}
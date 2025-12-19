using Bixd.Models;
using Bixd.Models.DTO.Divisions;
using EmployeeService.Migrations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeService.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(Roles = "Сотрудник кадровой службы")]
public class DivisionController(ILogger<RoleController> logger, ApplicationDbContext context)
    : Controller
{
    private readonly ILogger<RoleController> _logger = logger;
    private readonly DbSet<Division> _divisions = context.Divisions;

    /// <summary>
    /// return all divisions
    /// </summary>
    /// <returns>list with all divisions(always Ok)</returns>
    [HttpGet("all")]
    public IActionResult Get()
    {
        return Ok(_divisions.ToList().Select(d => new DivisionsDtoUpdate() { Id = d.Id,Name = d.Name, ParentId = d.ParentId }).ToList());
    }

    /// <summary>
    /// return division by id
    /// </summary>
    /// <returns> division</returns>
    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        var role = _divisions.Find(id);
        if (role == null) return NotFound();
        return Ok(new DivisionsDtoUpdate() {Id=role.Id, Name = role.Name, ParentId = role.ParentId });
    }
    
    /// <summary>
    /// add new role
    /// </summary>
    /// <param name="division"> division to add</param>
    /// <returns> division to add or BadRequest</returns>
    [HttpPost]
    public IActionResult Post([FromBody] DivisionDto division)
    {
        if (division == null) return BadRequest();  
        _divisions.Add(new Division()
        {
            Name = division.Name,
            ParentId = division.ParentId
        });
        context.SaveChanges();
        return Ok(division);
    }
    
    /// <summary>
    /// update division
    /// </summary>
    /// <param name="division"></param>
    /// <returns></returns>
    [HttpPatch]
    public IActionResult Put([FromBody] DivisionsDtoUpdate division)
    {
        if (division == null) return BadRequest();
        var toUpdate = _divisions.Find(division.Id);
        if (toUpdate == null) return NotFound();
        toUpdate.Name = division.Name;
        toUpdate.ParentId = division.ParentId;
        _divisions.Update(toUpdate);
        context.SaveChanges();
        return Ok(new DivisionDto() { Name = division.Name, ParentId = division.ParentId });

    }
    
    /// <summary>
    /// deletes division
    /// </summary>
    /// <param name="id"> division id to delete</param>
    /// <returns> division</returns>
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var role = _divisions.Find(id);
        if (role == null) return NotFound();
        _divisions.Remove(role);
        context.SaveChanges();
        return Ok(new DivisionDto() { Name = role.Name, ParentId = role.ParentId });
    }
}
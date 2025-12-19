using Bixd.Models;
using Bixd.Models.DTO;
using EmployeeService.Migrations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeService.Controllers;
[ApiController]
[Route("[controller]")]
[Authorize(Roles = "Сотрудник кадровой службы")]
public class RoleController:Controller
{
    private readonly ILogger<RoleController> _logger;
    private readonly ApplicationDbContext _context;

    public RoleController(ILogger<RoleController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }
    
    /// <summary>
    /// return all roles
    /// </summary>
    /// <returns>list with all roles(always Ok)</returns>
    [HttpGet("all")]
    public IActionResult Get()
    {
        return Ok(_context.Role.ToList());
    }
    
    /// <summary>
    /// return roles by id
    /// </summary>
    /// <returns> role</returns>
    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        var role = _context.Role.Find(id);
        if (role == null) return NotFound();
        return Ok(role);
    }
    
    /// <summary>
    /// add new role
    /// </summary>
    /// <param name="role"> role to add</param>
    /// <returns> role to add or BadRequest</returns>
    [HttpPost]
    public IActionResult Post([FromBody] RoleDto role)
    {
        if (role == null||_context.Role.ToList().Find(r=>r.Name==role.Name)!=null) return BadRequest();  
        _context.Role.Add(new Role()
        {
            Name = role.Name,
        });
        _context.SaveChanges();
        return Ok(role);
    }
    
    /// <summary>
    /// update role
    /// </summary>
    /// <param name="role"></param>
    /// <returns></returns>
    [HttpPatch]
    public IActionResult Put([FromBody] Role role)
    {
        if (role == null) return BadRequest();
        var roleToUpdate = _context.Role.Find(role.Id);
        if (roleToUpdate == null) return NotFound();
        roleToUpdate.Name = role.Name;
        _context.Role.Update(roleToUpdate);
        _context.SaveChanges();
        return Ok(roleToUpdate);

    }
    
    /// <summary>
    /// deletes role
    /// </summary>
    /// <param name="id"> role id to delete</param>
    /// <returns> rol</returns>
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var role = _context.Role.Find(id);
        if (role == null) return NotFound();
        _context.Role.Remove(role);
        _context.SaveChanges();
        return Ok(role);
    }
}
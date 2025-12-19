using BenefitService.Migrations;
using Bixd.Models;
using Bixd.Models.DTO.Benefits;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BenefitService.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
[Authorize(Roles = "Сотрудник кадровой службы")]
// [AllowAnonymous]
public class BenefitController(ApplicationDbContext _context) : Controller
{
    private readonly DbSet<Benefit> _benefits = _context.Benefits;

    /// <summary>
    /// return all benefits
    /// </summary>
    /// <returns>list with all benefits(always Ok)</returns>
    [HttpGet("all")]
    public IActionResult Get()
    {
        return Ok(_benefits.ToList());
    }


    /// <summary>
    /// return benefits by id
    /// </summary>
    /// <returns> benefits</returns>
    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        var role = _benefits.Find(id);
        if (role == null) return NotFound();
        return Ok(role);
    }
    
    /// <summary>
    /// add new benefits
    /// </summary>
    /// <param name="benefit"> benefits to add</param>
    /// <returns> benefits to add or BadRequest</returns>
    [HttpPost]
    public IActionResult Post([FromBody] BenefitDto? benefit)
    {
        
        if (benefit == null) return BadRequest();
        _benefits.Add(new Benefit()
        {
            Name = benefit.Name,
            AddDay = benefit.AddDay,
            
        });
        _context.SaveChanges();
        return Ok(benefit);
    }
    
    /// <summary>
    /// update benefits
    /// </summary>
    /// <param name="benefits"></param>
    /// <returns></returns>
    [HttpPatch]
    public IActionResult Put([FromBody] BenefitDtoUpdate? benefits)
    {
        if (benefits == null) return BadRequest();
        var toUpdate = _benefits.Find(benefits.Id);
        if (toUpdate == null) return NotFound();
        toUpdate.Name = benefits.Name ?? toUpdate.Name;
        toUpdate.AddDay = benefits.AddDay?? toUpdate.AddDay;
        
        _benefits.Update(toUpdate);
        _context.SaveChanges();
        return Ok(toUpdate);

    }
    
    /// <summary>
    /// deletes benefits
    /// </summary>
    /// <param name="id"> benefits id to delete</param>
    /// <returns> benefits</returns>
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var role = _benefits.Find(id);
        if (role == null) return NotFound();
        _benefits.Remove(role);
        _context.SaveChanges();
        return Ok(role);
    }
}
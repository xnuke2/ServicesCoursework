using Bixd.Models;
using Bixd.Models.DTO.AuthDataDto;
using Bixd.Models.DTO.Employees;
using EmployeeService.Migrations;
using EmployeeService.Models;
using EmployeeService.Models.DTO.CompineDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeService.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(Roles = "Сотрудник кадровой службы")]
[EnableCors("AllowLocalhost8081")]
public class EmployeeController:Controller
{
    private readonly ILogger<RoleController> _logger;
    private readonly DbSet<Employee> _employees;
    private readonly ApplicationDbContext _context;
    private readonly IKafkaProducerService _producerService;

    public EmployeeController(ILogger<RoleController> logger, ApplicationDbContext context,IKafkaProducerService kafkaProducer)
    {
        _logger = logger;
        _context = context;
        _employees = context.Employees;
        _producerService = kafkaProducer;
    }
    /// <summary>
    /// return all employees
    /// </summary>
    /// <returns>list with all employees(always Ok)</returns>
    [HttpGet("all")]
    public IActionResult Get()
    {
        return Ok(_employees.ToList());
    }
    
    /// <summary>
    /// return employees by id
    /// </summary>
    /// <returns> employees</returns>
    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        var role = _employees.Find(id);
        if (role == null) return NotFound();
        return Ok(role);
    }
    
    /// <summary>
    /// add new employees
    /// </summary>
    /// <param name="employees"> employees to add</param>
    /// <returns> employees to add or BadRequest</returns>
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CombineDto? employees)
    {
        
        if (employees == null
            ||_context.Role.Find(employees.RoleId)==null
            ||_context.Divisions.Find(employees.DivisionId)==null
            ) return BadRequest();
        _employees.Add(new Employee()
        {
            Name = employees.Name,
            Surname = employees.Surname,
            Patronymic = employees.Patronymic,
            BirthDate = employees.BirthDate,
            RoleId = employees.RoleId,
            DivisionId = employees.DivisionId,
            
        });
        _context.SaveChanges();
        int id = _employees.ToList().First(e=>e.Name==employees.Name
                                              &&e.BirthDate==employees.BirthDate
                                              &&e.DivisionId==employees.DivisionId
                                              &&e.Patronymic==employees.Patronymic
                                              &&e.RoleId==employees.RoleId
                                              &&e.Surname==employees.Surname).Id;
        
        _context.AuthData.Add(new AuthData()
        {
            Id = id,
            Login = employees.Login,
            Password = employees.Password,
        });
        _context.SaveChanges();
        var rol=_context.Role.Find(employees.RoleId);
        await _producerService.SendEmployeeCreatedAsync(
            id, 
            employees.Name, 
            rol.Name);
        return Ok(employees);
    }
    
    /// <summary>
    /// update employees
    /// </summary>
    /// <param name="employees"></param>
    /// <returns></returns>
    [HttpPatch]
    public IActionResult Put([FromBody] CombineDtoUpdate? employees)
    {
        if (employees == null) return BadRequest();
        var toUpdate = _employees.Find(employees.Id);
        var toUpd = _context.AuthData.Find(employees.Id);
        if (toUpdate == null||toUpd==null) return NotFound();
        toUpdate.Name = employees.Name ?? toUpdate.Name;
        toUpdate.Surname = employees.Surname ?? toUpdate.Surname;
        toUpdate.Patronymic = employees.Patronymic ?? toUpdate.Patronymic;
        toUpdate.BirthDate = (DateTime)(employees.BirthDate ?? toUpdate.BirthDate);
        toUpdate.RoleId = (int)(employees.RoleId ?? toUpdate.RoleId);
        toUpdate.DivisionId = (int)(employees.DivisionId ?? toUpdate.DivisionId);
        toUpd.Login = employees.Login??toUpd.Login;
        toUpd.Password = employees.Password??toUpd.Password;
        _employees.Update(toUpdate);
        _context.SaveChanges();
        return Ok(toUpdate);

    }
    
    /// <summary>
    /// deletes employees
    /// </summary>
    /// <param name="id"> employees id to delete</param>
    /// <returns> employees</returns>
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var role = _employees.Find(id);
        var toDelete = _context.AuthData.Find(id);
        if (role == null&&toDelete==null) return NotFound();
        _employees.Remove(role);
        _context.AuthData.Remove(toDelete);
        _context.SaveChanges();
        return Ok(role);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EatCleanAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EatCleanAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : Controller
    {
        private readonly VegafoodContext _context;
        public EmployeesController(VegafoodContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<List<Employee>> GetEmployees()
        {
            return await _context.Employees.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Employee>> GetEmployeesById(int id)
        {
            var employee = await _context.Employees.Where(em => em.EmployeeId == id).FirstAsync();
            if (employee == null)
                return NotFound();
            return employee;
        }

        [HttpPost]
        public async Task<ActionResult<Employee>> PostEmployee(Employee employee)
        {
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
            return employee;
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Employee>> DeleteEmployees(int id)
        {
            var employee = _context.Employees.Where(em => em.EmployeeId == id).First();
            if (employee == null)
                return NotFound();
            else
            {
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
                return NoContent();
            }    
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Employee>> PutEmployee(int id, Employee employee)
        {
            if (id != employee.EmployeeId)
                return BadRequest();
            _context.Entry(employee).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Employees.Any(employee => employee.EmployeeId == id))
                    return NotFound();
                throw;
            }
            return NoContent();
        }
    }
}

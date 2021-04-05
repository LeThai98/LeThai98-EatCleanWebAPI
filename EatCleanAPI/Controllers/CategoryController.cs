using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EatCleanAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EatCleanAPI.Controllers
{
    // create route to controller
    [Route("api/[controller]")]
    // atttribute to receive respone from API HTTP
    [ApiController]
    public class CategoryController : Controller
    {
        private readonly VegafoodContext _context;
        public CategoryController( VegafoodContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IEnumerable<Category>> GetCategories()
        {
            return await _context.Categories.ToListAsync();
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategoriesById(int id)
        {
            var category = await _context.Categories.Where(category => category.CategoryId == id).FirstAsync();
            if (category == null)
                return NotFound();
            return category;
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Category>> DeleteCategories(int id)
        {
            var category = _context.Categories.Where(cate => cate.CategoryId == id).FirstOrDefault();
            if (category == null)
                return BadRequest();
            else
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                return category;
            }   
        }

        [HttpPost]
        public async Task<ActionResult<Category>> PostCategories(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
             return category;
            //return CreatedAtAction("GetCategories", new { id = category.CategoryId }, category);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Category>> PutCategories(int id, Category category)
        {
            if (id != category.CategoryId)
                return BadRequest();

            _context.Entry(category).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Categories.Any(category => category.CategoryId == id))
                    return NotFound();
                else
                    throw;
            }
            return NoContent();
                
        }

    }
}

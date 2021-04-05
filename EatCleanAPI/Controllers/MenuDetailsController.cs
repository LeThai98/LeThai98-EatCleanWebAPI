using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EatCleanAPI.Models;

namespace EatCleanAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuDetailsController : ControllerBase
    {
        private readonly VegafoodContext _context;

        public MenuDetailsController(VegafoodContext context)
        {
            _context = context;
        }

        // GET: api/MenuDetails
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MenuDetail>>> GetMenuDetails()
        {
            return await _context.MenuDetails.ToListAsync();
        }

        // GET: api/MenuDetails/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MenuDetail>> GetMenuDetail(int id)
        {
            var menuDetail = await _context.MenuDetails.FindAsync(id);

            if (menuDetail == null)
            {
                return NotFound();
            }

            return menuDetail;
        }

        // PUT: api/MenuDetails/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMenuDetail(int id, MenuDetail menuDetail)
        {
            if (id != menuDetail.ProductId)
            {
                return BadRequest();
            }

            _context.Entry(menuDetail).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MenuDetailExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/MenuDetails
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<MenuDetail>> PostMenuDetail(MenuDetail menuDetail)
        {
            _context.MenuDetails.Add(menuDetail);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (MenuDetailExists(menuDetail.ProductId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetMenuDetail", new { id = menuDetail.ProductId }, menuDetail);
        }

        // DELETE: api/MenuDetails/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<MenuDetail>> DeleteMenuDetail(int id)
        {
            var menuDetail = await _context.MenuDetails.FindAsync(id);
            if (menuDetail == null)
            {
                return NotFound();
            }

            _context.MenuDetails.Remove(menuDetail);
            await _context.SaveChangesAsync();

            return menuDetail;
        }

        private bool MenuDetailExists(int id)
        {
            return _context.MenuDetails.Any(e => e.ProductId == id);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EatCleanAPI.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;

namespace EatCleanAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {   
      
        private readonly VegafoodContext _context;
        private readonly JWTSettings _jwtsettings;

        public CustomersController(VegafoodContext context,  IOptions<JWTSettings> jwtsettings)
        {
            _context = context;
            _jwtsettings = jwtsettings.Value;
        }

        // GET: api/Customers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            return await _context.Customers.ToListAsync();

        }

        // GET: api/Customers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetCustomer(int id)
        {
            //EntityEntry
            //SqlServerDbContextOptionsExtensions
            // Khi ta có 1 đối tượng là Customer, ta sẽ truyền đối tượng đó vào tham số cus trong biểu thức lambda, 
            // biểu thức này dạng Func và trả ra đó là 1 đối tượng Order là 1 thuộc tính trong Customer
            var customer = await _context.Customers
                                         .Include(cus => cus.Orders)
                                         .Where(cus => cus.CustomerId == id).FirstOrDefaultAsync();

            //var customer = await _context.Customers.SingleAsync(cus => cus.CustomerId == id);


            if (customer == null)
            {
                return NotFound();
            }

            return customer;
        }

        [HttpGet("GetCustomerByAccount")]
        public async Task<ActionResult<Customer>> GetCustomerByAccount()
        {
            string email = HttpContext.User.Identity.Name;
            var customer = await _context.Customers.Where(cus => cus.Email == email ).FirstOrDefaultAsync();
            customer.Password = null;
            if (customer == null)
            {
                return NotFound();
            }

            return customer;
        }

        [HttpGet("GetCustomerDetail/{id}")]
        public async Task<ActionResult<Customer>> GetCustomerDetail( int id)
        {
            var customer = await  _context.Customers.SingleAsync(cus => cus.CustomerId == id);

           // var cus1 = await _context.Customers.AllAsync(cus => cus.City == "Hồ Chí Minh");
            
            _context.Entry(customer)
                .Collection(cus => cus.Orders)
                .Query()
                .Include(order => order.OrderDetails)
                .Load();

           
            return customer;

          
        }

        [HttpPost("Login")]
        public async Task<ActionResult<Customer>> Login([FromBody] Customer customer)
        {
            customer = await _context.Customers.
                Where(cus => cus.Email == customer.Email && cus.Password == customer.Password)
                .FirstOrDefaultAsync();


            CustomerWithToken customerWithToken = new CustomerWithToken(customer);

            if (customer != null)
            {
                RefreshToken refreshToken = GenerateRefreshToken();
                customer.RefreshTokens.Add(refreshToken);
                await _context.SaveChangesAsync();

                customerWithToken = new CustomerWithToken(customer);
                customerWithToken.RefreshToken = refreshToken.Token;
            }

            if (customerWithToken == null)
            {
                return NotFound();
            }


            //sign your token here here..

            customerWithToken.AccessToken = GenerateAccessToken(customer.CustomerId);

            //var tokerHandler = new JwtSecurityTokenHandler();
            //var key = Encoding.ASCII.GetBytes(_jwtsettings.SecretKey);
            //var tokenDescriptor = new SecurityTokenDescriptor
            //{
            //    Subject = new ClaimsIdentity(new Claim[]
            //    {
            //        new Claim(ClaimTypes.Name, Convert.ToString( customer.CustomerId))
            //    }),
            //    Expires = DateTime.UtcNow.AddMonths(6),
            //    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
            //    SecurityAlgorithms.HmacSha256Signature)
            //};
            //var token = tokerHandler.CreateToken(tokenDescriptor);
            //customerWithToken.AccessToken = tokerHandler.WriteToken(token);

            return customerWithToken;

        }

        [HttpPost("RefreshToken")]
        public async Task<ActionResult<Customer>> RefreshToken([FromBody] RefreshRequest refreshRequest)
        {
            Customer customer = await GetUserFromAccessToken(refreshRequest.AccessToken);

            if (customer != null && ValidateRefreshToken(customer, refreshRequest.RefreshToken))
            {
                CustomerWithToken customerWithToken = new CustomerWithToken(customer);
                customerWithToken.AccessToken = GenerateAccessToken(customer.CustomerId);

                return customerWithToken;
            }

            return null;
        }

        private bool ValidateRefreshToken(Customer customer, string refreshToken)
        {

            RefreshToken refreshTokenUser = _context.RefreshTokens.Where(rt => rt.Token == refreshToken)
                                                .OrderByDescending(rt => rt.ExpiryDate)
                                                .FirstOrDefault();

            if (refreshTokenUser != null && refreshTokenUser.CustomerId == customer.CustomerId
                && refreshTokenUser.ExpiryDate > DateTime.UtcNow)
            {
                return true;
            }

            return false;
        }


        private async Task<Customer> GetUserFromAccessToken(string accessToken)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_jwtsettings.SecretKey);

                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };

                SecurityToken securityToken;
                var principle = tokenHandler.ValidateToken(accessToken, tokenValidationParameters, out securityToken);

                JwtSecurityToken jwtSecurityToken = securityToken as JwtSecurityToken;

                if (jwtSecurityToken != null && jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    var customerId = principle.FindFirst(ClaimTypes.Name)?.Value;

                    return await _context.Customers
                                        .Where(u => u.CustomerId == Convert.ToInt32(customerId)).FirstOrDefaultAsync();
                }
            }
            catch (Exception)
            {
                return new Customer();
            }

            return new Customer();
        }
        private RefreshToken GenerateRefreshToken()
        {
            RefreshToken refreshToken = new RefreshToken();

            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                refreshToken.Token = Convert.ToBase64String(randomNumber);
            }
            refreshToken.ExpiryDate = DateTime.UtcNow.AddMonths(6);

            return refreshToken;
        }

        private string GenerateAccessToken(int userId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtsettings.SecretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, Convert.ToString(userId))
                }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }


        // PUT: api/Customers/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCustomer(int id, Customer customer)
        {
            if (id != customer.CustomerId)
            {
                return BadRequest();
            }
            _context.Entry(customer).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerExists(id))
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

        // POST: api/Customers
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Customer>> PostCustomer(Customer customer)
        {
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetCustomer", new { id = customer.CustomerId }, customer);
        }

        // DELETE: api/Customers/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Customer>> DeleteCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return customer;
        }

        private bool CustomerExists(int id)
        {
        

            return _context.Customers.Any(e => e.CustomerId == id);
        }
    }
}

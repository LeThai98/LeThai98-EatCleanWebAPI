using EatCleanAPI.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace EatCleanAPI.Handler
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly VegafoodContext _context;
        public BasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, VegafoodContext context) : base(options, logger, encoder, clock)
        {
            _context = context;
        }
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {


            //return AuthenticateResult.Fail("Need to implement");
            // Kiểm tra trong Request , tại Header, có chứa Key là : Authorization hay khong
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return AuthenticateResult.Fail("Authentication header was not found");
            }
            try
            {
                // Parse cái Hearder thành giá trị trong AuthenticationHeaderValue và lấy ra AuthenticationHeaderValue
                // Lấy ra ca Value trong header ra ở phần Key "Authorization"
                var authenticationHeaderValue = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);

                // giải mã
                // Lấy thông tin Authentication ra và giải mã  nó thành byte 8bit( chữ thường) từ dạng 64bit
                //The credentials containing the authentication information.
                var bytes = Convert.FromBase64String(authenticationHeaderValue.Parameter);

                // đầu tiên mã hóa dạng byte 8bit thành dạng UTF8
                // tách mảng ra ra thành mảng chứa các chuỗi bằng phân biệt nhau bởi dấu :
                string[] credential = Encoding.UTF8.GetString(bytes).Split(":");
                // email là phần tử đầu tiên trong mảng
                string email = credential[0];
                // password là  phần tử thứ 2 trong mảng
                string password = credential[1];

                // tìm ra customer có email và password tương ứng 
                Customer customer = _context.Customers.Where(cus => cus.Email == email && cus.Password == password).FirstOrDefault();
                //Customer customer = _context.Customers.Where(cus => cus.CustomerId == 9).FirstOrDefault();

                // nếu customer không có giá trị tức là không tìm thấy user nào cả
                if (customer == null)
                    return AuthenticateResult.Fail("Invalid Email and Password");
                else
                {
                    // khi mà xác thực được customer thì ta tiến hành tạo 1 đối tượng Claim với Name là email của customer. 
                    var claims = new[] { new Claim(ClaimTypes.Name, customer.Email) };
                    var identity = new ClaimsIdentity(claims, Scheme.Name);
                    var principal = new ClaimsPrincipal(identity);
                    var ticket = new AuthenticationTicket(principal, Scheme.Name);
                    return AuthenticateResult.Success(ticket);
                }
            }
            catch (Exception)
            {

                return AuthenticateResult.Fail("Error has accured");
            }

            return AuthenticateResult.Fail("");
        }
    }
    
}

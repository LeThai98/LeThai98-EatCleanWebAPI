using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EatCleanAPI.Models
{
    public class CustomerWithToken: Customer
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public CustomerWithToken(Customer customer)
        {
            this.CustomerId = customer.CustomerId;
            this.CustomerName = customer.CustomerName;
            this.Address = customer.Address;
            this.City = customer.City;
            this.District = customer.District;
            this.Phone = customer.Phone;
            this.Password = customer.Password;
            this.Email = customer.Email;

        }
    }
}

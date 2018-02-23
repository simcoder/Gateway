using System;
namespace GOC.ApiGateway.Dtos
{
    public class CompanyDto
    {
        public Guid Id { get; set; }

        public AddressDto Address { get; set; }

        public string Name { get; set; }

        public int UserId { get; set; }

        public string PhoneNumber { get; set; }
    }
}

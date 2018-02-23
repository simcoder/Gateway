namespace GOC.ApiGateway.Dtos
{
    public class CompanyPostDto
    {
        public AddressDto Address { get; set; }

        public string Name { get; set; }

        public int UserId { get; set; }

        public string PhoneNumber { get; set; }
    }
}

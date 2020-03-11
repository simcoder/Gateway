using System;
namespace GOC.ApiGateway.Dtos
{
    public class InventoryPostDto
    {
        public string Name { get; set; }

        public Guid CompanyId { get; set; }

        public int UserId { get; set; }
    }
}

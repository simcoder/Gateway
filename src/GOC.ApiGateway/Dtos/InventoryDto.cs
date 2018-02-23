using System;
namespace GOC.ApiGateway.Dtos
{
    public class InventoryDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Guid CompanyId { get; set; }

        public int UserId { get; set; }
    }
}

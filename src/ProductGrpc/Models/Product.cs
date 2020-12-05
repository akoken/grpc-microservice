using System;

namespace ProductGrpc.Models
{
    public class Product
    {

        public int ProductId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public decimal Price { get; set; }

        public ProductStatus Status { get; set; }

        public DateTime CreatedDate { get; set; }
    }

    public enum ProductStatus
    {
        None = 0,

        InStock = 1,

        Low = 2
    }
}

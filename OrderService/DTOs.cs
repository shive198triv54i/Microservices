
    namespace OrderService.DTOs
    {
        public class PlaceOrderDto
        {
            public List<OrderItemDto> Items { get; set; } = new();
        }

        public class OrderItemDto
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
        }
    }




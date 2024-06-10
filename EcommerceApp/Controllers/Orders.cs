using Microsoft.AspNetCore.Mvc;

namespace dapp1.Controllers
{
    using dapp1.DataAccess;
    using ECommerceApp.Models;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        public async Task<IActionResult> GetLatestOrder([FromBody] RequestModel request)
        {
            var response = await _orderService.GetLatestOrderAsync(request.User, request.CustomerId);
            if (response == null)
            {
                return BadRequest(new { Message = "Invalid customer ID or email." });
            }

            return Ok(response);
        }
    }

}

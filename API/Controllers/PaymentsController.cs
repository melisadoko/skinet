
using Core.Entities;
using Core.Entities.OrderAggregate;
using Core.Interfaces;
using Core.Specification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace API.Controllers
{
    public class PaymentsController(IPaymentService paymentService, IUnitOfWork unit, ILogger<PaymentsController> logger) : BaseAPIController
    {
        private readonly string _whSecret = "";
        [Authorize]
        [HttpPost("{cartId}")]
        public async Task<ActionResult<ShoppingCart>> CreateOrUpdatePaymentIntent(string cartId)
        {
            var cart = await paymentService.CreateOrUpdatePaymentIntent(cartId);

            if (cart == null) return BadRequest("Problem with your cart");

            return Ok(cart);
        }

        [HttpGet("delivery-methods")]
        public async Task<ActionResult<IReadOnlyList<DeliveryMethod>>> GetDeliveryMethods()
        {
            return Ok(await unit.Repository<DeliveryMethod>().ListAllAsync());
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> StripeWebHook()
        {
            var json = await new StreamReader(Request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = ConstructStripeEvent(json);

                if(stripeEvent.Data.Object is not PaymentIntent intent)
                {
                    return BadRequest("Invalid event data");
                }

                await HandlePaymentIntentSucceeded(intent);

                return Ok();
            }
            catch(StripeException ex)
            {
                logger.LogError(ex, "Stripe webhook error");
                return StatusCode(StatusCodes.Status500InternalServerError, "Webhook error");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An expected error occured");
                return StatusCode(StatusCodes.Status500InternalServerError, "An expected error occured");
            }
        }

        private async Task HandlePaymentIntentSucceeded(PaymentIntent intent)
        {
            if(intent.Status == "succeeded")
            {
                var spec = new OrderSpecification(intent.Id, true);

                var order = await unit.Repository<Order>().GetEntityWithSpec(spec)
                    ?? throw new Exception("Order not found");

                if((long)order.GetTotal() * 100 != intent.Amount)
                {
                    order.Status = OrderStatus.PaymentMisMatch;
                }
                else
                {
                    order.Status = OrderStatus.PaymentReceived;
                }

                await unit.Complete();

            }
        }

        private Event ConstructStripeEvent(string json)
        {
            try
            {
                return EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], _whSecret);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to construct stripe event");
                throw new StripeException("Invalid signature");
            }
        }
    }
}

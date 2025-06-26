using Stripe;
using eShoes.DTO;
using System.Security.Claims;
using eShoes.Context;
using eShoes.Models;

namespace eShoes.Services
{
    public class CheckoutService
    {
        private readonly CartService _cart;
        private readonly UserService _userService;
        private readonly eShoesDbContext _context;

        public CheckoutService(
            CartService cart,
            UserService userService,
            eShoesDbContext context)
        {
            _cart   = cart;
            _userService = userService;
            _context = context;
        }

        public async Task<CheckoutResponse> ProcessCheckout(CheckoutRequest rq)
        {
            //Get Cart
            var cart = _cart.GetUserCart();
            var subtotal = cart.Items.Sum(i => i.TotalPrice);
            if (!cart.Items.Any())
                throw new ArgumentException("Empty Cart");

            //Create PaymentIntent with Stripe
            var user = _userService.GetCurrentUser();
            if (user == null || string.IsNullOrEmpty(user.StripeCustomerId))
                throw new ArgumentException("Invalid user or without Stripe Id");

            if (string.IsNullOrEmpty(user.StripeCustomerId))
            {
                var customerService = new CustomerService();
                var customerOptions = new CustomerCreateOptions
                {
                    Email = user.Email,
                    Name = user.Username
                };

                var stripeCustomer = await customerService.CreateAsync(customerOptions);
                user.StripeCustomerId = stripeCustomer.Id;
                await _context.SaveChangesAsync();
            }

            if (rq == null || rq.Address == null)
                throw new ArgumentException("Address is required");
            if (rq.Payment == null || string.IsNullOrEmpty(rq.Payment.Method))
                throw new ArgumentException("Payment method is required");
            
            var shipping = CalculateShippingCost(rq.Address.State);

            long amountInCents = (long)((subtotal + shipping) * 100);
            var options = new PaymentIntentCreateOptions
            {
                Amount             = amountInCents,
                Currency           = "brl",
                Customer           = user.StripeCustomerId,
                PaymentMethodTypes = new List<string> { rq.Payment.Method.ToLower() },
                Expand             = new List<string>
                {
                    "payment_method",
                    "charges.data.payment_method_details.card",
                    "next_action.pix_display_qr_code",
                }
            };

            if (rq.Payment.Method == "card")
            {
                try
                {
                    var paymentMethodService = new PaymentMethodService();
                    var paymentMethod = await paymentMethodService.AttachAsync(
                        rq.Payment.PaymentMethodId,
                        new PaymentMethodAttachOptions { Customer = user.StripeCustomerId }
                    );

                    options.PaymentMethod = paymentMethod.Id;
                    options.Confirm = true;
                }
                catch (StripeException e)
                {
                    throw new Exception($"Fail to vinculate the card: {e.Message}");
                }
            }
            else if (rq.Payment.Method == "pix")
            {
                options.PaymentMethodOptions = new PaymentIntentPaymentMethodOptionsOptions
                {
                    Pix = new PaymentIntentPaymentMethodOptionsPixOptions
                    {
                        ExpiresAfterSeconds = 3600
                    }
                };
            }

            var pi = await new PaymentIntentService().CreateAsync(options);
            var paymentIntentService = new PaymentIntentService();
            var updatedPi = await new PaymentIntentService().GetAsync(
                pi.Id,
                new PaymentIntentGetOptions { Expand = new List<string> { "payment_method" } }
            );
            string last4 = updatedPi.PaymentMethod?.Card?.Last4;

            //Save Order in database
            var order = new Order
            {
                UserId               = user.Id,
                OrderDate            = DateTime.UtcNow,
                ShippingCost         = shipping,
                TotalProducts        = cart.Items.Sum(i => i.TotalPrice),
                StripePaymentIntentId= pi.Id,
                PaymentMethodType    = rq.Payment.Method,
                Status               = pi.Status == "succeeded" ? "Paid" : "Pending",
                Last4 = last4,
                Items = cart.Items.Select(i => new OrderItem
                {
                    ProductId   = i.ProductId,
                    ProductName = i.ProductName,
                    Price       = i.ProductPrice,
                    Quantity    = i.Quantity,
                    Size        = i.Size
                }).ToList()
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            _cart.ClearCart();

            // 6) Construir resposta
            string pixQr = null;
            string pixCode = null;
            if (rq.Payment.Method == "pix")
            {
                if (pi.NextAction?.Type == "pix_display_qr_code" 
                    && pi.NextAction.PixDisplayQrCode != null)
                {
                    pixQr   = pi.NextAction.PixDisplayQrCode.ImageUrlSvg;
                    pixCode = pi.NextAction.PixDisplayQrCode.Data;
                }
            }

            return new CheckoutResponse
            {
                ShippingCost = shipping,
                TotalAmount  = subtotal + shipping,
                Last4        = rq.Payment.Method == "card"   ? last4 : null,
                PixQrCode    = pixQr,
                PixCode      = pixCode,
            };
        }

        //Calculate the Shipping Cost
        public decimal CalculateShippingCost(string state)
        {
            state = state.ToUpper().Trim();

            //States per region
            var sudeste = new[] {"SP", "RJ", "MG", "ES"};
            var sul = new[] {"PR", "SC", "RS"};
            var centroOeste = new[] {"DF", "GO", "MT", "MS"};
            var nordeste = new[] {"BA", "SE", "AL", "PE", "PB", "RN", "CE", "PI", "MA"};
            var norte = new[] {"AC", "AP", "AM", "PA", "RO", "RR", "TO"};

            return state switch
            {
                "SP" => 15m,
                var s when sudeste.Contains(s) => 25m,
                var s when sul.Contains(s) => 35m,
                var s when centroOeste.Contains(s) => 50m,
                var s when nordeste.Contains(s) => 58m,
                var s when norte.Contains(s) => 60m,
                _ => 100m
            };
        }
    }
}
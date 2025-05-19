using Stripe;
using System.Drawing;
using System.Security.AccessControl;
using VitoriaAirlinesLibrary.Enums;

namespace VitoriaAirlinesLibrary.Services
{
    public class PaymentService
    {
        private readonly string _stripeApiKey;
        public string LastPaymentMessage { get; private set; }

        private static readonly Random _random = new Random();

        public PaymentService(string stripeApiKeyFromApp)
        {
            LastPaymentMessage = string.Empty;
            _stripeApiKey = stripeApiKeyFromApp;

        }

        public async Task<bool> ProcessPaymentAsync(decimal total, VitoriaAirlinesLibrary.Enums.PaymentMethod paymentMethod)
        {
            bool result;

            switch (paymentMethod)
            {
                case Enums.PaymentMethod.Card:
                    result = await ProcessCardPaymentAsync(total);
                    break;
                case Enums.PaymentMethod.PayPal:
                    result = await ProcessPayPalPaymentAsync();
                    break;
                case Enums.PaymentMethod.MBWay:
                    result = await ProcessMBWayPaymentAsync();
                    break;
                default:
                    return false;
            }
            return result;
        }
        private async Task<bool> ProcessPayPalPaymentAsync()
        {
            await Task.Delay(2000);

            bool success = _random.Next(2) == 1;

            LastPaymentMessage = success ? "PayPal payment approved." : "PayPal payment denied.";

            return success;
        }

        private async Task<bool> ProcessMBWayPaymentAsync()
        {
            await Task.Delay(1500);

            bool success = _random.Next(2) == 1;

            LastPaymentMessage = success ? "MBWay payment approved." : "MBWay payment denied.";

            return success;
        }

        private async Task<bool> ProcessCardPaymentAsync(decimal valor)
        {
            try
            {
                StripeConfiguration.ApiKey = _stripeApiKey;

                string token = "tok_mastercard";

                var options = new ChargeCreateOptions
                {
                    Amount = Convert.ToInt32(valor * 100),
                    Currency = "eur",
                    Description = "Ticket purchase via Stripe.",
                    Source = token
                };

                var chargeService = new ChargeService();

                Charge charge = await chargeService.CreateAsync(options);

                if (charge.Paid)
                {
                    LastPaymentMessage = "Payment completed successfully!";
                    return true;
                }
                else
                {
                    LastPaymentMessage = "The payment was declined by the provider.";
                    return false;
                }
            }
            catch (StripeException ex)
            {
                LastPaymentMessage = "Payment Denied: " + ex.Message;
                return false;
            }
            catch (Exception ex)
            {
                LastPaymentMessage = "Unexpected error: " + ex.Message;
                return false;
            }
        }
    }

}

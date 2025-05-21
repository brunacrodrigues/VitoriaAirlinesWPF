using Stripe;
using System.Net;
using System.Net.Mail;
using System.Text;
using VitoriaAirlinesLibrary.Models;

namespace VitoriaAirlinesLibrary.Services
{
    public class EmailService
    {
        private async Task SendEmailAsync(string from, List<string> to, string subject, string body)
        {
            MailAddress fromMailAddress = new MailAddress(from);

            MailMessage mail = new MailMessage();

            foreach (string email in to)
            {
                mail.To.Add(email);
            }

            mail.From = fromMailAddress;
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true;

            SmtpClient client = GetSmtpClient();

            await client.SendMailAsync(mail);
        }

        private SmtpClient GetSmtpClient()
        {
            string smtpHost = "127.0.0.1";
            int smtpPort = 25;
            string smtpUserName = "Vitoria Airlines";
            string smtpPassword = "vitoria";
            bool enableSsl = false;

            SmtpClient client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUserName, smtpPassword),
                EnableSsl = enableSsl
            };

            return client;
        }

        public async Task<bool> NotifyPassengersAboutFlightChangesAsync(List<Client> flightPassengers, Flight flight)
        {
            string from = "vitoriaairlines@vitoria.com";
            List<string> to = new List<string>();
            string subject = $"Changes to flight {flight.FlightNumber}, {flight.Origin.IATA} - {flight.Destination.IATA}";
            StringBuilder body = new StringBuilder();

            try
            {
                foreach (Client passenger in flightPassengers)
                {
                    body.Clear();

                    body.AppendLine($"Dear {passenger.FullName}");
                    body.AppendLine("<p>");
                    body.AppendLine($"Your flight {flight.FlightNumber} has been updated.");
                    body.AppendLine($"New departure time: {flight.DepartureTime}, Duration: {flight.Duration}");
                    body.AppendLine("<p>");
                    body.AppendLine("We apologize for any inconvenience this may have caused.");
                    body.AppendLine("<p>");
                    body.AppendLine("Sincerely,");
                    body.AppendLine("Vitoria Airlines");

                    to.Clear();
                    to.Add(passenger.Email);

                    await SendEmailAsync(from, to, subject, body.ToString());
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> NotifyPassengersAboutFlightCancellationAsync(List<Client> flightPassengers, Flight flight)
        {
            string from = "vitoriaairlines@vitoria.com";
            List<string> to = new List<string>();
            string subject = $"Flight Cancellation {flight.FlightNumber}, {flight.Origin.IATA} - {flight.Destination.IATA}";
            StringBuilder body = new StringBuilder();

            try
            {
                foreach (Client passenger in flightPassengers)
                {
                    body.Clear();

                    body.AppendLine($"Dear {passenger.FullName}");
                    body.AppendLine("<p>");
                    body.AppendLine($"We regret to inform you that your flight {flight.FlightNumber} has been cancelled.");
                    body.AppendLine($"A refund has been issued to your original payment method.");
                    body.AppendLine("<p>");
                    body.AppendLine("We apologize for any inconvenience this may have caused.");
                    body.AppendLine("<p>");
                    body.AppendLine("Sincerely,");
                    body.AppendLine("Vitoria Airlines");

                    to.Clear();
                    to.Add(passenger.Email);

                    await SendEmailAsync(from, to, subject, body.ToString());
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> NotifyPassengersAboutRefundAsync(Client client, Flight flight)
        {
            string from = "vitoriaairlines@vitoria.com";
            List<string> to = new List<string>();
            string subject = $"Refund due to class downgrade on flight {flight.FlightNumber}, {flight.Origin.IATA} - {flight.Destination.IATA}";
            StringBuilder body = new StringBuilder();

            try
            {
                body.AppendLine($"Dear {client.FullName}");
                body.AppendLine("<p>");
                body.AppendLine($"We would like to inform you that your booking for flight {flight.FlightNumber}" +
                    $" has been changed from <strong>executive class</strong> to <strong>economy class</strong>.");
                body.AppendLine($"A refund has been issued to your original payment method.");
                body.AppendLine("<p>");
                body.AppendLine("We apologize for any inconvenience this may have caused.");
                body.AppendLine("<p>");
                body.AppendLine("Sincerely,");
                body.AppendLine("Vitoria Airlines");

                to.Clear();
                to.Add(client.Email);

                await SendEmailAsync(from, to, subject, body.ToString());
                
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}

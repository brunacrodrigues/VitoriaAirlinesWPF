using System.Net.Mail;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using VitoriaAirlinesLibrary.Enums;
using VitoriaAirlinesLibrary.Models;
using VitoriaAirlinesLibrary.Services;
using VitoriaAirlinesWPF.Pages;

namespace VitoriaAirlinesWPF.Windows
{
    /// <summary>
    /// Interaction logic for CheckoutWindow.xaml
    /// </summary>
    public partial class CheckoutWindow : Window
    {
        List<Ticket> TicketsToPurchase;
        PaymentService _paymentService;
        TicketService _ticketService;
        SellTicketsWindow _sellTicketsWindow;
        public CheckoutWindow(List<Ticket> ticketsToPurchase, SellTicketsWindow sellTicketsWindow)
        {
            InitializeComponent();
            TicketsToPurchase = ticketsToPurchase;
            string stripeSecretKey = App.AppConfig["StripeSettings:SecretKey"];

            if (string.IsNullOrEmpty(stripeSecretKey))
            {
                MessageBox.Show("Stripe Secret Key not found. " +
                    "Please check appsettings.json (StripeSettings:SecretKey). " +
                    "Card payment processing will fail.",
                    "Configuration Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            _paymentService = new PaymentService(stripeSecretKey);
            _ticketService = new TicketService();
            _sellTicketsWindow = sellTicketsWindow;
        }

        #region Events

        private void btnRestore_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;

                btnIcon.Source = new BitmapImage(new Uri("/Resources/Icons/restore.png", UriKind.Relative));
            }
            else
            {
                WindowState = WindowState.Maximized;

                btnIcon.Source = new BitmapImage(new Uri("/Resources/Icons/restore2.png", UriKind.Relative));
            }
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void btnCompletePurchase_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateData())
                return;

            DisableUI();

            try
            {
                
                PaymentMethod paymentMethod = (PaymentMethod)comboPaymentMethod.SelectedItem;

                decimal totalAmount = TicketsToPurchase.Sum(t => t.Price);

                bool success = await _paymentService.ProcessPaymentAsync(totalAmount, paymentMethod);

                if (!success)
                {
                    MessageBox.Show($"Payment declined.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    EnableUI();
                    return;
                }
                
                await UpdateTicketAsync(paymentMethod);
                FinalizePurchase();
                

            } 
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during the purchase process: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void comboPaymentMethod_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PaymentMethod paymentMethod = (PaymentMethod)comboPaymentMethod.SelectedItem;

            genericTextBox.Text = string.Empty;
            genericTextBox.Focus();

            panelDetails.Visibility = Visibility.Visible;
            BorderToHide.Visibility = Visibility.Visible;

            switch (paymentMethod)
            {
                case PaymentMethod.Card:
                    genericLabel.Text = "Card Number";
                    ShowCardInput();
                    break;

                case PaymentMethod.PayPal:
                    genericLabel.Text = "Email";
                    ShowPayPalInput();
                    break;


                case PaymentMethod.MBWay:
                    genericLabel.Text = "Phone Number";
                    ShowMBWAYInput();
                    break;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            comboPaymentMethod.ItemsSource = Enum.GetValues(typeof(PaymentMethod));

            dataGridCart.ItemsSource = TicketsToPurchase;
            BorderToHide.Visibility = Visibility.Collapsed;
            CalculateDisplayTotal();
        }

        #endregion

        #region Methods
        private void DisableUI()
        {
            dataGridCart.IsEnabled = false;
            btnCompletePurchase.IsEnabled = false;
            comboPaymentMethod.IsEnabled = false;
            btnClose.IsEnabled = false;
           
        }

        public void EnableUI()
        {
            dataGridCart.IsEnabled = true;
            btnCompletePurchase.IsEnabled = true;
            comboPaymentMethod.IsEnabled = true;
            btnClose.IsEnabled = true;
        }

        private void FinalizePurchase()
        {
            TicketsToPurchase.Clear();
            _sellTicketsWindow.RefreshDataGridCart();
            MessageBox.Show("Purchase completed successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            /*_sellTicketsWindow.LoadAvailableSeatsAsync();*/ // sem await aqui pois o método não precisa travar a UI
            Close();
        }

        private async Task UpdateTicketAsync(PaymentMethod paymentMethod)
        {
            foreach (var ticket in TicketsToPurchase)
            {
                var response = await _ticketService.GetAvailableTicketByFlightAndSeatAsync(ticket.Flight.Id, ticket.Seat.Id);

                if (response.IsSuccess && response.Result is Ticket existingTicket)
                {
                    existingTicket.ClientId = ticket.Client.Id;
                    existingTicket.PaymentMethod = paymentMethod;

                    await _ticketService.UpdateAsync(existingTicket);
                }
            }


        }        

        private void ShowMBWAYInput()
        {
            cardDatePanel.Visibility = Visibility.Hidden;
            txtCCV.Visibility = Visibility.Hidden;
            CCVLabel.Visibility = Visibility.Hidden;
            panelCard.Visibility = Visibility.Hidden;
        }

        private void ShowPayPalInput()
        {
            cardDatePanel.Visibility = Visibility.Hidden;
            txtCCV.Visibility = Visibility.Hidden;
            cardDatePanel.Visibility = Visibility.Hidden;
            txtCCV.Visibility = Visibility.Hidden;
            CCVLabel.Visibility = Visibility.Hidden;
            panelCard.Visibility = Visibility.Hidden;
        }

        private void ShowCardInput()
        {

            cardDatePanel.Visibility = Visibility.Visible;
            txtCCV.Visibility = Visibility.Visible;
            cardDatePanel.Visibility = Visibility.Visible;
            txtCCV.Visibility = Visibility.Visible;
            CCVLabel.Visibility = Visibility.Visible;
            panelCard.Visibility = Visibility.Visible;
        }

        private void CalculateDisplayTotal()
        {
            if (TicketsToPurchase == null)
            {
                if (txtTotalAmount != null)
                {
                    txtTotalAmount.Text = 0m.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("pt-PT"));
                }
                return;
            }

            decimal totalAmount = TicketsToPurchase.Sum(ticket => ticket.Price);

            if (txtTotalAmount != null)
            {
                txtTotalAmount.Text = $"Total: {totalAmount.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("pt-PT"))}";
            }
        }

        private bool ValidateData()
        {

            if (comboPaymentMethod.SelectedItem == null)
            {
                MessageBox.Show("Please select a payment method.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            PaymentMethod paymentMethod = (PaymentMethod)comboPaymentMethod.SelectedItem;


            int selectedMonth = (int)MonthUpDown.Value;
            int selectedYear = (int)YearUpDown.Value;

            int currentMonth = DateTime.Now.Month;
            int currentYear = DateTime.Now.Year;


            if (paymentMethod == PaymentMethod.Card)
            {
                if (string.IsNullOrWhiteSpace(genericTextBox.Text) || genericTextBox.Text.Length != 16 || !genericTextBox.Text.All(char.IsDigit))
                {
                    MessageBox.Show("The card number must be 16 numeric digits.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                if (selectedYear < currentYear || (selectedYear == currentYear && selectedMonth < currentMonth))
                {
                    MessageBox.Show("The card has expired. Please enter a valid date.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                if (string.IsNullOrWhiteSpace(txtCCV.Text) || txtCCV.Text.Length != 3 || !txtCCV.Text.All(char.IsDigit))
                {
                    MessageBox.Show("The CCV code must be 3 numeric digits.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
            else if (paymentMethod == PaymentMethod.PayPal)
            {
                string email = genericTextBox.Text.Replace(" ", "").Trim();

                if (string.IsNullOrWhiteSpace(email) || !MailAddress.TryCreate(email, out MailAddress _))
                {
                    MessageBox.Show("Please enter a valid email address in the format name@domain.xxx.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(genericTextBox.Text) || !genericTextBox.Text.All(char.IsDigit) || genericTextBox.Text.Length != 9)
                {

                    MessageBox.Show("Please enter a valid phone number (9 numeric digits).", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}

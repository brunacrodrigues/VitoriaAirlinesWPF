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
    /// Interaction logic for ChangeSeatWindow.xaml
    /// </summary>
    public partial class ChangeSeatWindow : Window
    {
        Ticket _ticketToChangeSeat;
        FlightService _flightService;
        TicketService _ticketService;
        EmailService _emailService;
        TicketsPage _ticketsPage;

        List<Seat> AvailableSeats = new List<Seat>();

        public ChangeSeatWindow(Ticket ticketToChangeSeat, TicketsPage ticketsPage)
        {
            InitializeComponent();
            _ticketToChangeSeat = ticketToChangeSeat;
            _flightService = new FlightService();
            _ticketService = new TicketService();
            _emailService = new EmailService();
            _ticketsPage = ticketsPage;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            comboBoxSeatType.ItemsSource = Enum.GetValues(typeof(SeatType));
            LoadPassengersDetails();
            await LoadAvailableSeatsAsync();

        }

        private void LoadListBoxAvailableSeats()
        {
            if (comboBoxSeatType.SelectedItem != null)
            {
                SeatType seatType = (SeatType)comboBoxSeatType.SelectedItem;
                List<Seat> SeatsToDisplay = AvailableSeats.Where(s => s.Type == seatType && s.IsAvailable).ToList();

                listBoxAvailableSeats.ItemsSource = null;
                listBoxAvailableSeats.ItemsSource = SeatsToDisplay;
                listBoxAvailableSeats.DisplayMemberPath = "Name";
            }

        }

        private void LoadPassengersDetails()
        {
            txtFullName.Text = _ticketToChangeSeat.Client.FullName;
            txtPassport.Text = _ticketToChangeSeat.Client.Passaport;
            txtCurrentSeat.Text = $"{_ticketToChangeSeat.Seat.Name} {_ticketToChangeSeat.Seat.Type}";
        }

        public async Task LoadAvailableSeatsAsync()
        {
            comboBoxSeatType.IsEnabled = false;
            panelSeatLoadingOverlay.Visibility = Visibility.Visible;


            try
            {
                var ticketsResponse = await _flightService.GetTicketsForFlightAsync(_ticketToChangeSeat.Flight.Id);
                if (ticketsResponse.IsSuccess && ticketsResponse.Result is List<Ticket> tickets)
                {
                    AvailableSeats = tickets.Where(t => t.ClientId == null).Select(t => t.Seat).Where(s => s != null && s.IsAvailable).ToList();
                }
                else
                {
                    MessageBox.Show($"Error loading flight tickets: {ticketsResponse.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred while loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                panelSeatLoadingOverlay.Visibility = Visibility.Collapsed;

                if (comboBoxSeatType.Items.Count > 0)
                {
                    comboBoxSeatType.SelectedIndex = 0;
                }

                comboBoxSeatType.IsEnabled = true;
            }

        }

        private async void btnConfirmChange_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxAvailableSeats.SelectedItem == null)
            {
                MessageBox.Show("Please select a new seat", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Seat newSeat = listBoxAvailableSeats.SelectedItem as Seat;    
            Ticket availableTicketForNewSeat = await GetAvailableTicketForSeatAsync(newSeat);
            if (availableTicketForNewSeat == null)
                return;



            Seat oldSeat = _ticketToChangeSeat.Seat;
            if (oldSeat.Type == newSeat.Type)
            {
                await ChangeSeatWithinSameClassAsync(oldSeat, newSeat, availableTicketForNewSeat);
            }
            else if (oldSeat.Type == SeatType.Executive && newSeat.Type == SeatType.Economic)
            {
                await DowngradeFromExecutiveToEconomicAsync(oldSeat, newSeat, availableTicketForNewSeat);
            }
            else if (oldSeat.Type == SeatType.Economic && newSeat.Type == SeatType.Executive)
            {
                await UpgradeFromEconomicToExecutiveAsync(oldSeat, newSeat, availableTicketForNewSeat);
            }

            await _ticketsPage.LoadFlightTicketsAsync(_ticketToChangeSeat.Flight);
            Close();


        }

        private async Task<Ticket> GetAvailableTicketForSeatAsync(Seat newSeat)
        {
            var response = await _flightService.GetTicketsForFlightAsync(_ticketToChangeSeat.FlightId);
            if (!response.IsSuccess || response.Result is not List<Ticket> allTicketsInFlight)
            {
                MessageBox.Show($"Could not get tickets: {response.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            if (allTicketsInFlight.Any(t => t.SeatId == newSeat.Id && t.ClientId != null))
            {
                MessageBox.Show($"Seat {newSeat.Name} is already occupied.", "Seat Unavailable", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            var availableTicket = allTicketsInFlight.FirstOrDefault(t => t.SeatId == newSeat.Id && t.ClientId == null);
            if (availableTicket == null)
            {
                MessageBox.Show($"No available ticket for seat {newSeat.Name}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            return availableTicket;
        }

        private async Task UpgradeFromEconomicToExecutiveAsync(Seat oldSeat, Seat newSeat, Ticket newSeatTicket)
        {
            var originalTicketPrice = newSeatTicket.Price;

            PrepTicketForUpgradePayment(newSeatTicket);

            List<Ticket> ticketToPurchase = new List<Ticket> { newSeatTicket };

            var checkoutWindow = new CheckoutWindow(ticketToPurchase, null);
            bool? result = checkoutWindow.ShowDialog();

            newSeatTicket.Price = originalTicketPrice;

            if (result == true)
            {
                newSeatTicket.PaymentMethod = ticketToPurchase.First().PaymentMethod;
                await ChangeSeatAsync(oldSeat, newSeat, newSeatTicket, true);
            }
            else
            {
                MessageBox.Show("Seat change cancelled. No changes were made.", "Cancelled", MessageBoxButton.OK, MessageBoxImage.Information);
            }


        }

        private void PrepTicketForUpgradePayment(Ticket newSeatTicket)
        {
            decimal totalAmount = newSeatTicket.Price - _ticketToChangeSeat.Price;
            newSeatTicket.Price = totalAmount;
            newSeatTicket.Client = _ticketToChangeSeat.Client;
        }

        private async Task<bool> ChangeSeatAsync(Seat oldSeat, Seat newSeat, Ticket newSeatTicket, bool hasNewPaymentMethod = false)
        {
            newSeatTicket.ClientId = _ticketToChangeSeat.ClientId;

            if (!hasNewPaymentMethod)
            {
                newSeatTicket.PaymentMethod = _ticketToChangeSeat.PaymentMethod;
            }

            var updateNewTicketResponse = await _ticketService.UpdateAsync(newSeatTicket);



            if (!updateNewTicketResponse.IsSuccess)
            {
                MessageBox.Show($"Error assigning new seat {newSeat.Name}: {updateNewTicketResponse.Message}." +
                    $" Your original seat {oldSeat.Name} remains assigned.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }


            var cancelOldTicketResponse = await _ticketService.CancelTicketAsync(_ticketToChangeSeat.Id);

            if (cancelOldTicketResponse.IsSuccess)
            {
                MessageBox.Show($"Seat successfully changed from {oldSeat.Name}" +
                    $" to {newSeat.Name}.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                return true;
            }
            else
            {
                MessageBox.Show($"New seat {newSeat.Name} was assigned, but an error occurred releasing your old seat {oldSeat.Name}:" +
                    $" {cancelOldTicketResponse.Message}. Please contact support to resolve this. Your booking is now for the new seat.",
                    "Partial Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return true;

            }
        }

        private async Task DowngradeFromExecutiveToEconomicAsync(Seat oldSeat, Seat newSeat, Ticket newSeatTicket)
        {
            bool success = await ChangeSeatAsync(oldSeat, newSeat, newSeatTicket);

            newSeatTicket.Client = _ticketToChangeSeat.Client;
            newSeatTicket.Flight = _ticketToChangeSeat.Flight;

            if (success)
            {
                await _emailService.NotifyPassengersAboutRefundAsync(newSeatTicket.Client, newSeatTicket.Flight);
            }
        }

        private async Task ChangeSeatWithinSameClassAsync(Seat oldSeat, Seat newSeat, Ticket newSeatTicket)
        {
            await ChangeSeatAsync(oldSeat, newSeat, newSeatTicket);
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

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

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void comboBoxSeatType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadListBoxAvailableSeats();
        }

        
    }
}

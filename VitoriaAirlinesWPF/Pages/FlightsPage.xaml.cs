using System.Windows;
using System.Windows.Controls;
using VitoriaAirlinesLibrary.Models;
using VitoriaAirlinesLibrary.Services;
using VitoriaAirlinesWPF.Windows;

namespace VitoriaAirlinesWPF.Pages
{
    /// <summary>
    /// Interaction logic for Flights.xaml
    /// </summary>
    public partial class FlightsPage : Page
    {
        FlightService _flightService;
        EmailService _emailService;
        List<Flight> _allFlights;

        public FlightsPage()
        {
            InitializeComponent();
            _flightService = new FlightService();
            _emailService = new EmailService();
            Loaded += Page_Loaded;

        }

        private void btnSellTickets_Click(object sender, RoutedEventArgs e)
        {
            var selectedFlight = flightsDataGrid.SelectedItem as Flight;

            if (selectedFlight.DepartureDateTime <= DateTime.Now)
            {
                MessageBox.Show("Can't sell tickets after a flight has departed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SellTicketsWindow sellTicketsWindow = new SellTicketsWindow(selectedFlight);
            sellTicketsWindow.ShowDialog();
        }

        private async void btnDeleteFlight_Click(object sender, RoutedEventArgs e)
        {
            var selectedFlight = flightsDataGrid.SelectedItem as Flight;


            if (!ConfirmDeletion(selectedFlight.FlightNumber))
            {
                return;
            }

            if (!await FlightExistsAsync(selectedFlight.Id))
            {
                await LoadFlightsAsync();
                return;
            }

            await DeleteFlightAsync(selectedFlight);
        }



        private void btnEditFlight_Click(object sender, RoutedEventArgs e)
        {
            var selectedFlight = flightsDataGrid.SelectedItem as Flight;

            if (selectedFlight.DepartureDateTime <= DateTime.Now)
            {
                MessageBox.Show("The flight can't be updated after it has departed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            EditFlightWindow editFlightWindow = new EditFlightWindow(this, selectedFlight);
            editFlightWindow.ShowDialog();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadFlightsAsync();
        }

        private void btnScheduleFlight_Click(object sender, RoutedEventArgs e)
        {
            var addFlightWindow = new AddFlightWindow(this);
            addFlightWindow.ShowDialog();
        }

        public async Task LoadFlightsAsync()
        {
            panelFlightsLoading.Visibility = Visibility.Visible;
            cmbOriginFilter.IsEnabled = false;

            try
            {
                var flightsResponse = await _flightService.GetAllAsync();
                if (flightsResponse.IsSuccess && flightsResponse.Result is List<Flight> flights)
                {
                    var tasks = flights.Select(async flight =>
                    {
                        var ticketsResponse = await _flightService.GetTicketsForFlightAsync(flight.Id);

                        if (ticketsResponse.IsSuccess && ticketsResponse.Result is List<Ticket> tickets)
                        {
                            flight.Tickets = tickets;
                        }
                        else
                        {
                            MessageBox.Show($"Error loading fligh tickets: {(ticketsResponse.Message ?? "Unknown error")}", "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                    });

                    await Task.WhenAll(tasks);

                    _allFlights = flights;

                    cmbOriginFilter.IsEnabled = true;

                    cmbOriginFilter.SelectedIndex = 0;

                }
                else
                {
                    MessageBox.Show($"Error loading flights: {(flightsResponse.Message ?? "Unknown error")}", "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    flightsDataGrid.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
                flightsDataGrid.ItemsSource = null;
            }
            finally
            {
                panelFlightsLoading.Visibility = Visibility.Collapsed;
            }
        }

        private async Task DeleteFlightAsync(Flight selectedFlight)
        {
            panelFlightsLoading.Visibility = Visibility.Visible;
            txtFlightsInfo.Text = "Deleting flight...";

            List<Client> flightPassengers = null;
            var clientsResponse = await _flightService.GetClientsForFlightAsync(selectedFlight.Id);

            if (!clientsResponse.IsSuccess || clientsResponse.Result is not List<Client> passengersWithTickets)
            {
                MessageBox.Show($"Could not get passengers for flight {selectedFlight.Id}.\n" +    
                                $"Error: {clientsResponse.Message}\n" +
                               "If you continue, passengers won't be notified.","Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

                flightPassengers = new List<Client>();
            }
            else
            {
                flightPassengers = passengersWithTickets;
            }

            var flightResponse = await _flightService.DeleteAsync(selectedFlight.Id);

            if (flightResponse.IsSuccess)
            {
                if (flightPassengers != null && flightPassengers.Any())
                {
                    await NotifyPassengersAsync(flightPassengers, selectedFlight);
                    MessageBox.Show("Flight deleted successfully and passengers have been notified.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Flight deleted successfully. No passengers were found.", "Success (No Passengers Notified)", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                await LoadFlightsAsync();
            }
            else
            {
                MessageBox.Show($"Error deleting flight: {flightResponse.Message}", "Deletion Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            panelFlightsLoading.Visibility = Visibility.Collapsed;

        }


        private async Task NotifyPassengersAsync(List<Client> flightPassengers, Flight selectedFlight)
        {
            if (flightPassengers.Count == 0)
                return;

            bool success = await _emailService.NotifyPassengersAboutFlightCancellationAsync(flightPassengers, selectedFlight);

            if (!success)
            {
                MessageBox.Show("Error notifying flight passengers.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBox.Show("Flight passengers were notified about flight cancellation.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        private async Task<bool> FlightExistsAsync(int flightId)
        {
            var flightExists = await _flightService.ExistsAsync(flightId);

            if (!flightExists)
            {
                MessageBox.Show("The selected flight no longer exists in the database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private bool ConfirmDeletion(string flightNumber)
        {
            var confirmResult = MessageBox.Show($"Are you sure you want to delete the flight '{flightNumber}'?",
                                                "Confirm Delete",
                                                MessageBoxButton.YesNo,
                                                MessageBoxImage.Question);

            return confirmResult == MessageBoxResult.Yes;
        }

        private void cmbOriginFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_allFlights == null) return;

            var selectedItem = cmbOriginFilter.SelectedIndex;

            if (selectedItem == 1)
            {
                lblFlights.Content = "Scheduled Flights";
                flightsDataGrid.ItemsSource = _allFlights.Where(f => f.DepartureDateTime > DateTime.Now)
                    .OrderBy(f => f.DepartureDateTime);

            }
            else if (selectedItem == 2)
            {
                lblFlights.Content = "Past Flights";
                flightsDataGrid.ItemsSource = _allFlights.Where(f => f.DepartureDateTime <= DateTime.Now)
                    .OrderByDescending(f => f.DepartureDateTime);

            }
        }
    }
}

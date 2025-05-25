using System.Windows;
using System.Windows.Controls;
using VitoriaAirlinesLibrary.Enums;
using VitoriaAirlinesLibrary.Models;
using VitoriaAirlinesLibrary.Services;

namespace VitoriaAirlinesWPF.Pages
{
    /// <summary>
    /// Interaction logic for SearchFlightsPage.xaml
    /// </summary>
    public partial class SearchFlightsPage : Page
    {
        FlightService _flightService;
        AirportService _airportService;
        FlightSearchService _flightSearchService;
        // Lista com todos os voos carregados
        List<Flight> _flightsResult = new List<Flight>();


        List<Flight> _filteredFlights = new List<Flight>();

        public SearchFlightsPage()
        {
            InitializeComponent();
            _flightService = new FlightService();
            _airportService = new AirportService();
            _flightSearchService = new FlightSearchService();

        }


        private void btnSearchFlights_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateSearch())
                return;

            var origin = comboOrigin.SelectedItem as Airport;
            var destination = comboDestination.SelectedItem as Airport;
            var departureDate = dpDepartureDate.SelectedDate.Value;
            var returnDate = chkOneWay.IsChecked == true ? null : dpReturnDate.SelectedDate;
            var isRoundTrip = checkboxRoundTrip.IsChecked == true;
            var seatType = (SeatType)comboClass.SelectedItem;
            var passengersCount = (int)udAdults.Value + (int)udChildren.Value + (int)udInfants.Value;

            _filteredFlights = _flightSearchService.FilterFlights(
                _flightsResult,
                origin.Id,
                destination.Id,
                departureDate,
                returnDate,
                isRoundTrip,
                seatType,
                passengersCount);

            if (_filteredFlights.Any())
            {
                SortAndUpdateFlightsGrid(comboSortBy.SelectedIndex); 
            }
            else
            {
                MessageBox.Show("No flights were found matching your search criteria.", "Search Results", MessageBoxButton.OK, MessageBoxImage.Information);
                dataGridFlightsResults.ItemsSource = null;
            }
        }

        private void SortAndUpdateFlightsGrid(int selectedIndex)
        {

            List<Flight> sortedFlights = _filteredFlights;

            switch (selectedIndex)
            {
                case 0:
                    sortedFlights = _filteredFlights.OrderBy(f => f.DisplayPrice).ToList();
                    break;
                case 1:
                    sortedFlights = _filteredFlights.OrderBy(f => f.Duration).ToList();
                    break;
                case 2:
                    sortedFlights = _filteredFlights.OrderBy(f => f.DepartureDateTime).ToList();
                    break;

                default:
                    break;
            }

            dataGridFlightsResults.ItemsSource = null;
            dataGridFlightsResults.ItemsSource = sortedFlights;
        }

        private void btnSwapDestinations_Click(object sender, RoutedEventArgs e)
        {
            var aux = comboOrigin.SelectedItem;
            comboOrigin.SelectedItem = comboDestination.SelectedItem;
            comboDestination.SelectedItem = aux;
        }

        private void checkboxRoundTrip_Checked(object sender, RoutedEventArgs e)
        {
            chkOneWay.IsChecked = false;
        }

        private void checkboxRoundTrip_Unchecked(object sender, RoutedEventArgs e)
        {
            checkboxRoundTrip.IsChecked = false;
        }

        private void chkOneWay_Checked(object sender, RoutedEventArgs e)
        {
            brdReturnDateLabel.Visibility = Visibility.Collapsed;
            dpReturnDate.Visibility = Visibility.Collapsed;
            checkboxRoundTrip.IsChecked = false;
        }

        private void chkOneWay_Unchecked(object sender, RoutedEventArgs e)
        {
            brdReturnDateLabel.Visibility = Visibility.Visible;
            dpReturnDate.Visibility = Visibility.Visible;
            checkboxRoundTrip.IsChecked = true;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            comboClass.ItemsSource = Enum.GetValues(typeof(SeatType));
            comboClass.SelectedIndex = 0;
            await LoadAirportsAsync();
            await LoadFligthsAndTicketsAsync();
        }

        private async Task LoadFligthsAndTicketsAsync()
        {
            btnSearchFlights.IsEnabled = false;
            panelFlightsLoading.Visibility = Visibility.Visible;

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

                    _flightsResult = flights;


                }
                else
                {
                    MessageBox.Show($"Error loading flights: {(flightsResponse.Message ?? "Unknown error")}", "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                btnSearchFlights.IsEnabled = true;
                panelFlightsLoading.Visibility = Visibility.Collapsed;
            }
        }

        public async Task LoadAirportsAsync()
        {
            var response = await _airportService.GetAllAsync();

            if (response.IsSuccess && response.Result is List<Airport> airports)
            {
                comboOrigin.ItemsSource = airports;
                comboOrigin.SelectedIndex = 0;
                comboDestination.ItemsSource = airports;
                comboDestination.SelectedIndex = 0;
            }
            else
            {
                MessageBox.Show($"Error loading aiports: {(response.Message ?? "Unknown error")}", "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateSearch()
        {
            if (checkboxRoundTrip.IsChecked == false && chkOneWay.IsChecked == false)
            {
                MessageBox.Show($"Please select: 'Round Trip' or 'One Way.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                checkboxRoundTrip.Focus();
                return false;
            }

            if (comboOrigin.SelectedItem == null)
            {
                MessageBox.Show($"Please select the origin airport.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                comboOrigin.Focus();
                return false;
            }

            if (comboOrigin.SelectedItem == null)
            {
                MessageBox.Show($"Please select the destination airport.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                comboOrigin.Focus();
                return false;
            }

            if (comboOrigin.SelectedItem == comboDestination.SelectedItem)
            {
                MessageBox.Show($"Origin and Destination Airports cannot be the same.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                comboDestination.Focus();
                return false;
            }

            if (dpDepartureDate.SelectedDate == null)
            {
                MessageBox.Show($"Please select a departure date.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                dpDepartureDate.Focus();
                return false;
            }


            if (checkboxRoundTrip.IsChecked == true)
            {

                if (dpReturnDate.SelectedDate == null)
                {
                    MessageBox.Show($"Please select a returning date.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    dpReturnDate.Focus();
                    return false;
                }

                if (dpDepartureDate.SelectedDate.Value > dpReturnDate.SelectedDate.Value)
                {
                    MessageBox.Show($"The Departure date cannot be higher than the Return date. ", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    dpDepartureDate.Focus();
                    return false;
                }
            }

            if (((int)udAdults.Value + (int)udChildren.Value + (int)udInfants.Value) == 0)
            {
                MessageBox.Show($"Please insert at least one passenger. ", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                udAdults.Focus();
                return false;
            }

            return true;
        }

        private void comboSortBy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SortAndUpdateFlightsGrid(comboSortBy.SelectedIndex);
        }
    }
}


        

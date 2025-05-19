using System.Windows;
using System.Windows.Controls;
using VitoriaAirlinesLibrary.Models;
using VitoriaAirlinesLibrary.Services;
using VitoriaAirlinesWPF.Windows;

namespace VitoriaAirlinesWPF.Pages
{
    /// <summary>
    /// Interaction logic for TicketsPage.xaml
    /// </summary>
    public partial class TicketsPage : Page
    {
        FlightService _flightService;
        ClientService _clientService;
        List<Ticket> TicketsToDisplay = new List<Ticket>();
        public TicketsPage()
        {
            InitializeComponent();
            _flightService = new FlightService();
            _clientService = new ClientService();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadFlightsAsync();
        }

        private async void comboFlights_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var flight = comboFlights.SelectedItem as Flight;

            if (flight == null)
                return;

            await LoadFlightTicketsAsync(flight);

            LoadDataGrid();
        }



        private void btnSellTickets_Click(object sender, RoutedEventArgs e)
        {
            if (comboFlights.SelectedItem == null)
            {
                MessageBox.Show("Please select a flight.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
                var selectedFlight = comboFlights.SelectedItem as Flight;
                SellTicketsWindow sellTicketsWindow = new SellTicketsWindow(selectedFlight, this);
                sellTicketsWindow.ShowDialog(); 
            
        }

        private void btnChangeSeat_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnCancelTicket_Click(object sender, RoutedEventArgs e)
        {

        }

        public async Task LoadFlightTicketsAsync(Flight flight)
        {
            try
            {
                var ticketsResponse = await _flightService.GetTicketsForFlightAsync(flight.Id);
                if (ticketsResponse.IsSuccess && ticketsResponse.Result is List<Ticket> tickets)
                {
                    flight.Tickets = tickets;


                    var soldTickets = tickets.Where(t => t.ClientId.HasValue).ToList();

                    List<Task> getClientsTasks = new List<Task>();

                    

                    foreach (var ticket in soldTickets)
                    {
                        if (ticket.ClientId.HasValue)
                        {
                            getClientsTasks.Add(Task.Run(async () =>
                            {
                                var clientResponse = await _clientService.GetByIdAsync(ticket.ClientId.Value);
                                if (clientResponse.IsSuccess && clientResponse.Result is Client client)
                                {
                                    ticket.Client = client;
                                }
                                else
                                {
                                    ticket.Client = new Client { FullName = "Client N/A (Error)" };
                                }
                            }));
                        }
                    }

                    if (getClientsTasks.Any())
                    {
                        await Task.WhenAll(getClientsTasks);
                    }

                    TicketsToDisplay = soldTickets
                        .OrderBy(t => t.Seat?.Row)
                        .ThenBy(t => t.Seat?.Letter)
                        .ToList();

                    LoadDataGrid();
                    
                }
                else
                {
                    MessageBox.Show($"Error loading fligh tockets: {(ticketsResponse.Message ?? "Unknown error")}", "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
                dataGridTickets.ItemsSource = null;
            }
        }

        public void LoadDataGrid()
        {
            dataGridTickets.ItemsSource = null;
            dataGridTickets.ItemsSource = TicketsToDisplay;
        }

        private async Task LoadFlightsAsync()
        {
            try
            {
                var flightsResponse = await _flightService.GetAllAsync();
                if (flightsResponse.IsSuccess && flightsResponse.Result is List<Flight> flights)
                {
                    comboFlights.ItemsSource = flights;
                }
                else
                {
                    MessageBox.Show($"Error loading flights: {(flightsResponse.Message ?? "Unknown error")}", "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    comboFlights.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
                comboFlights.ItemsSource = null;
            }
        }
    }
}

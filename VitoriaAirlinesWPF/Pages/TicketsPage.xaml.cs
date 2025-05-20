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
        TicketService _ticketService;

        List<Ticket> TicketsToDisplay = new List<Ticket>();
        public TicketsPage()
        {
            InitializeComponent();
            _flightService = new FlightService();
            _clientService = new ClientService();
            _ticketService = new TicketService();
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

        private async void btnCancelTicket_Click(object sender, RoutedEventArgs e)
        {
            var selectedTicket = dataGridTickets.SelectedItem as Ticket;
            selectedTicket.Flight = comboFlights.SelectedItem as Flight;

            if (selectedTicket.Flight.DepartureDateTime <= DateTime.Now)
            {
                MessageBox.Show("The ticket can't be cancelled after a flight has departed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

                var confirmResult = MessageBox.Show($"Are you sure you want to cancel the ticket '{selectedTicket.Seat.Name}'?",
                                                    "Confirm Cancelation",
                                                    MessageBoxButton.YesNo,
                                                    MessageBoxImage.Question);

                if(confirmResult == MessageBoxResult.Yes)
                {

                    var response = await _ticketService.CancelTicket(selectedTicket.Id);

                    if(response.IsSuccess)
                    {
                        await LoadFlightTicketsAsync(selectedTicket.Flight);

                        MessageBox.Show("Ticket cancelled successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                    else
                    {
                        MessageBox.Show($"Failed to cancel ticket.\nReason: {response.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
            }
           
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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
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
                            MessageBox.Show($"Error loading fligh tockets: {(ticketsResponse.Message ?? "Unknown error")}", "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
							return;
                        }
                        
                    });

                    await Task.WhenAll(tasks);

                    flightsDataGrid.ItemsSource = flights;
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
		}

        private async Task DeleteFlightAsync(Flight selectedFlight)
        {
            var clientsResponse = await _flightService.GetClientsForFlightAsync(selectedFlight.Id);

            if (!clientsResponse.IsSuccess || clientsResponse.Result is not List<Client> flightPassengers)
            {
                MessageBox.Show($"Error getting flight passengers: {clientsResponse.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            await NotifyPassengersAsync(flightPassengers, selectedFlight);

            var flightResponse = await _flightService.DeleteAsync(selectedFlight.Id);

            if (flightResponse.IsSuccess)
            {
                MessageBox.Show("Flight deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadFlightsAsync();
            }
            else
            {
                MessageBox.Show($"Error deleting flight: {flightResponse.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
	}
}

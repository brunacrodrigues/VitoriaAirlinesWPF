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

		public FlightsPage()
		{
			InitializeComponent();
			_flightService = new FlightService();
			Loaded += Page_Loaded;
		}

		private void btnSellTickets_Click(object sender, RoutedEventArgs e)
		{

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
				await LoadFlights();
				return;
			}

			await DeleteFlightAsync(selectedFlight.Id);
		}

		

		private void btnEditFlight_Click(object sender, RoutedEventArgs e)
		{
			var selectedFlight = flightsDataGrid.SelectedItem as Flight;
			EditFlightWindow editFlightWindow = new EditFlightWindow(this, selectedFlight);
			editFlightWindow.ShowDialog();
		}

		private async void Page_Loaded(object sender, RoutedEventArgs e)
		{
			await LoadFlights();
		}

		private void btnScheduleFlight_Click(object sender, RoutedEventArgs e)
		{
			var addFlightWindow = new AddFlightWindow(this);
			addFlightWindow.ShowDialog();
		}

		public async Task LoadFlights()
		{
			try
			{
				var response = await _flightService.GetAllAsync();
				if (response.IsSuccess && response.Result is List<Flight> flights)
				{
				
					flightsDataGrid.ItemsSource = flights;
				}
				else
				{
					MessageBox.Show($"Error loading flights: {(response.Message ?? "Unknown error")}", "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
					flightsDataGrid.ItemsSource = null; 
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
				flightsDataGrid.ItemsSource = null;
			}
		}

		private async Task DeleteFlightAsync(int flightId)
		{
			var response = await _flightService.DeleteAsync(flightId);

			if (response.IsSuccess)
			{
				MessageBox.Show("Flight deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
				await LoadFlights();
			}
			else
			{
				MessageBox.Show($"Error deleting flight: {response.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
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

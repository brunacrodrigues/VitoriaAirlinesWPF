using System.Windows;
using System.Windows.Media.Imaging;
using VitoriaAirlinesLibrary.Models;
using VitoriaAirlinesLibrary.Services;
using VitoriaAirlinesWPF.Pages;

namespace VitoriaAirlinesWPF.Windows
{
	/// <summary>
	/// Interaction logic for ScheduleFlightWindow.xaml
	/// </summary>
	public partial class AddFlightWindow : Window
	{
		FlightService _flightService;
		FlightsPage _flightsPage;
		AirportService _airportService;
		AirplaneService _airplaneService;

		public AddFlightWindow(FlightsPage flightsPage)
		{
			InitializeComponent();
			_flightService = new FlightService();
			_flightsPage = flightsPage;
			_airportService = new AirportService();
			_airplaneService = new AirplaneService();
			Loaded += Window_Loaded;
		}

		private void btnClose_Click(object sender, RoutedEventArgs e)
		{
			Close();
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

		private void btnMinimize_Click(object sender, RoutedEventArgs e)
		{
			WindowState = WindowState.Minimized;
		}

		private async void btnScheduleFlight_Click(object sender, RoutedEventArgs e)
		{
			if (ValidateData())
			{
				DateTime selectedDate = dpDepartureDate.SelectedDate.Value;
				TimeSpan selectedTime = tpDepartureTime.Value.Value.TimeOfDay; 


				int durationHours = Convert.ToInt32(DurationHoursUpDown.Value);
				int durationMinutes = Convert.ToInt32(DurationMinutesUpDown.Value);
				decimal executivePrice = Convert.ToDecimal(ExecutivePriceUpDown.Value);
				decimal economicPrice = Convert.ToDecimal(EconomicPriceUpDown.Value);

				Airplane selectedAirplane = comboBoxModels.SelectedItem as Airplane;
				Airport selectedOrigin = comboBoxOrigin.SelectedItem as Airport;
				Airport selectedDestination = comboBoxDestination.SelectedItem as Airport;



				Flight newFlight = new Flight
				{
					AirplaneId = selectedAirplane.Id,
					OriginAirportId =selectedOrigin.Id,
					DestinationAirportId = selectedDestination.Id,
					DepartureDate = selectedDate,
					DepartureTime = selectedTime,
					Duration = new TimeSpan(durationHours, durationMinutes, 0),

					ExecutivePrice = executivePrice,
					EconomicPrice = economicPrice,

				};
				var response = await _flightService.CreateAsync(newFlight);

				if (response.IsSuccess)
				{
					MessageBox.Show("Flight created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

					await _flightsPage.LoadFlights();

					this.Close();
				}
				else
				{
					MessageBox.Show($"Error creating flight: {response.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}


		private bool ValidateData()
		{
			// --- DATE AND TIME
			if (dpDepartureDate.SelectedDate == null)
			{
				MessageBox.Show("Please select a departure date.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
				dpDepartureDate.Focus();
				return false;
			}

			if (tpDepartureTime.Value == null)
			{
				MessageBox.Show("Please select a departure time.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
				tpDepartureTime.Focus();
				return false;
			}

			DateTime departureDateTime;

			try
			{
				departureDateTime = dpDepartureDate.SelectedDate.Value.Date.Add(tpDepartureTime.Value.Value.TimeOfDay);
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Invalid departure date or time format: {ex.Message}", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
				return false;
			}

			if (departureDateTime <= DateTime.Now)
			{
				MessageBox.Show("The departure date and time must be in the future.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
				if (dpDepartureDate.SelectedDate.Value.Date == DateTime.Today) tpDepartureTime.Focus();
				else dpDepartureDate.Focus();
				return false;
			}

			// --- COMBOBOX SELECTION ---
			if (comboBoxModels.SelectedValue == null)
			{
				MessageBox.Show("Please select an airplane model.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
				comboBoxModels.Focus();
				return false;
			}
			if (comboBoxOrigin.SelectedValue == null)
			{
				MessageBox.Show("Please select an origin airport.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
				comboBoxOrigin.Focus();
				return false;
			}
			if (comboBoxDestination.SelectedValue == null)
			{
				MessageBox.Show("Please select a destination airport.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
				comboBoxDestination.Focus();
				return false;
			}
			if ((int)comboBoxOrigin.SelectedValue == (int)comboBoxDestination.SelectedValue)
			{
				MessageBox.Show("The origin airport cannot be the same as the destination airport.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
				comboBoxDestination.Focus();
				return false;
			}

			// --- DURATION ---

			int hours = Convert.ToInt32(DurationHoursUpDown.Value ?? 0);
			int minutes = Convert.ToInt32(DurationMinutesUpDown.Value ?? 0);


			TimeSpan flightDuration = new TimeSpan(hours, minutes, 0);
			if (flightDuration < TimeSpan.FromMinutes(30))
			{
				MessageBox.Show("The flight duration must be at least 30 minutes.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
				DurationHoursUpDown.Focus();
				return false;
			}

			//PRICES

			decimal executivePriceVal = Convert.ToDecimal(ExecutivePriceUpDown.Value);
			decimal economicPriceVal = Convert.ToDecimal(EconomicPriceUpDown.Value);


			if (executivePriceVal <= economicPriceVal)
			{
				MessageBox.Show("The executive ticket price must be higher than the economic ticket price.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
				ExecutivePriceUpDown.Focus();
				return false;
			}

			return true;
		}

		private async void Window_Loaded(object sender, RoutedEventArgs e)
		{
			lblAvailableSeatsInfo.Visibility = Visibility.Hidden;
			await LoadComboDataAsync();

		}

		private async Task LoadComboDataAsync()
		{
			try
			{
				var airplanesResponse = await _airplaneService.GetAllAsync();
				if (airplanesResponse.IsSuccess && airplanesResponse.Result is List<Airplane> airplanes)
				{
					comboBoxModels.ItemsSource = airplanes.Where(a => a.IsActive).ToList();
					comboBoxModels.DisplayMemberPath = "Model";
					comboBoxModels.SelectedValuePath = "Id";
				}
				else
				{
					MessageBox.Show($"Error loading airplanes: {airplanesResponse.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				}


				var airportsResponse = await _airportService.GetAllAsync();
				if (airportsResponse.IsSuccess && airportsResponse.Result is List<Airport> airports)
				{
					comboBoxOrigin.ItemsSource = airports;
					comboBoxOrigin.DisplayMemberPath = "IATA";
					comboBoxOrigin.SelectedValuePath = "Id";

					comboBoxDestination.ItemsSource = airports;
					comboBoxDestination.DisplayMemberPath = "IATA";
					comboBoxDestination.SelectedValuePath = "Id";
				}
				else
				{
					MessageBox.Show($"Error loading airports: {airportsResponse.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"An unexpected error occurred while loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void comboBoxModels_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			if (comboBoxModels.SelectedItem is Airplane selectedAirplane)
			{
				lblAvailableSeatsInfo.Visibility = Visibility.Visible;
				lblAvailableSeatsInfo.Text = $"Total Seats: {selectedAirplane.TotalCapacity}";
			}
		}
	}
}
	


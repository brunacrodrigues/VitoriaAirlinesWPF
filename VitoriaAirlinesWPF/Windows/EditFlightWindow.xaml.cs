﻿using Syncfusion.UI.Xaml.Grid;
using System.Windows;
using System.Windows.Media.Imaging;
using VitoriaAirlinesLibrary.Helpers;
using VitoriaAirlinesLibrary.Models;
using VitoriaAirlinesLibrary.Services;
using VitoriaAirlinesWPF.Pages;

namespace VitoriaAirlinesWPF.Windows
{
    /// <summary>
    /// Interaction logic for EditFlightWindow.xaml
    /// </summary>
    public partial class EditFlightWindow : Window
	{
		FlightsPage _flightsPage;
		Flight _flightToEdit;
		FlightService _flightService;
		AirportService _airportService;
		AirplaneService _airplaneService;
		EmailService _emailService;
		bool hasSoldTickets;

        public EditFlightWindow(FlightsPage flightsPage, Flight flightToEdit)
		{
			InitializeComponent();
			_flightsPage = flightsPage;
			_flightToEdit = flightToEdit;
			_flightService = new FlightService();
			_airportService = new AirportService();
			_airplaneService = new AirplaneService();
			_emailService = new EmailService();
            hasSoldTickets = _flightToEdit.Tickets.Any(t => t.ClientId != null);
            Loaded += Window_Loaded;
		}

		private async void btnSaveFlight_Click(object sender, RoutedEventArgs e)
		{
			if (!ValidateData())
				return;

			if (!await FlightExistsAsync(_flightToEdit.Id))
			{
				await HandleFlightNotFoundErrorAsync();
				return;
			}

			TimeSpan originalDepartureTime = _flightToEdit.DepartureTime;
			TimeSpan originalDuration = _flightToEdit.Duration;

			UpdateFlightModel();

			bool departureOrDurationChanged = _flightToEdit.DepartureTime != originalDepartureTime ||
													 _flightToEdit.Duration != originalDuration;

			savingFlightOverlay.Visibility = Visibility.Visible;

            var updateResponse = await _flightService.UpdateAsync(_flightToEdit);

			if (updateResponse.IsSuccess)
			{
                savingFlightOverlay.Visibility = Visibility.Collapsed;

                MessageBox.Show("Flight updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

				if (departureOrDurationChanged)
				{
					await NotifyPassengersIfChangedAsync(_flightToEdit);
				}


				await _flightsPage.LoadFlightsAsync();
				Close();
			}
			else
			{
				await HandleUpdateResponseAsync(updateResponse);
			}
		}

		private async Task NotifyPassengersIfChangedAsync(Flight updatedFlight)
		{
			var clientsResponse = await _flightService.GetClientsForFlightAsync(updatedFlight.Id);

            savingFlightOverlay.Visibility = Visibility.Visible;
			txtMessgage.Text = "Notifying passengers...";

            if (clientsResponse.IsSuccess)
			{
				if (clientsResponse.Result is List<Client> flightPassengers && flightPassengers.Any())
				{
					bool success = await _emailService.NotifyPassengersAboutFlightChangesAsync(flightPassengers, updatedFlight);

					if (!success)
					{
						MessageBox.Show("Error notifying flight passengers.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
						return;
					}

                    savingFlightOverlay.Visibility = Visibility.Collapsed;

                    MessageBox.Show("Flight passengers were notified about flight changes.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
				}

				else
				{
					MessageBox.Show($"Error getting flight passengers: {clientsResponse.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}
			}
		}

		private void DisableUI()
		{
            comboBoxOrigin.IsEnabled = false;
            comboBoxDestination.IsEnabled = false;
            dpDepartureDate.IsEnabled = false;
            ExecutivePriceUpDown.IsEnabled = false;
            EconomicPriceUpDown.IsEnabled = false;
           
        }

		private void LoadFlightData()
		{
			lblFlightNumber.Text = _flightToEdit.FlightNumber;

			comboBoxModels.SelectedValue = _flightToEdit.AirplaneId;

			comboBoxOrigin.SelectedValue = _flightToEdit.OriginAirportId;
			comboBoxDestination.SelectedValue = _flightToEdit.DestinationAirportId;

			if (_flightToEdit.DepartureDate != default)
			{
				dpDepartureDate.SelectedDate = _flightToEdit.DepartureDate;
			}


			if (_flightToEdit.DepartureTime != default)
			{
				tpDepartureTime.Value = DateTime.Today.Add(_flightToEdit.DepartureTime);
			}


			DurationHoursUpDown.Value = _flightToEdit.Duration.Hours;
			DurationMinutesUpDown.Value = _flightToEdit.Duration.Minutes;


			ExecutivePriceUpDown.Value = (double)_flightToEdit.ExecutiveTicketPrice;
			EconomicPriceUpDown.Value = (double)_flightToEdit.EconomicTicketPrice;
		}

		private void UpdateFlightModel()
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

			_flightToEdit.AirplaneId = selectedAirplane.Id;
			_flightToEdit.OriginAirportId = selectedOrigin.Id;
			_flightToEdit.DestinationAirportId = selectedDestination.Id;
			_flightToEdit.DepartureDate = selectedDate;
			_flightToEdit.DepartureTime = selectedTime;
			_flightToEdit.Duration = new TimeSpan(durationHours, durationMinutes, 0);
			_flightToEdit.ExecutivePrice = executivePrice;
			_flightToEdit.EconomicPrice = economicPrice;
		}
		private async Task HandleFlightNotFoundErrorAsync()
		{
			MessageBox.Show("This flight no longer exists in the database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			await _flightsPage.LoadFlightsAsync();
			this.Close();
		}


		private async Task HandleUpdateResponseAsync(Response response)
		{
			if (response.IsSuccess)
			{
				MessageBox.Show("Flight updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
				await _flightsPage.LoadFlightsAsync();
				this.Close();
			}
			else
			{
				MessageBox.Show($"Error updating flight: {response.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
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

		private void comboBoxModels_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			if (comboBoxModels.SelectedItem is Airplane selectedAirplane)
			{
				lblAvailableSeatsInfo.Visibility = Visibility.Visible;
				lblAvailableSeatsInfo.Text = $"Total Seats: {selectedAirplane.TotalCapacity}";
			}
		}

		private async void Window_Loaded(object sender, RoutedEventArgs e)
		{
			lblAvailableSeatsInfo.Visibility = Visibility.Hidden;
			await LoadComboDataAsync();
			LoadFlightData();
			if (hasSoldTickets)
			{
				DisableUI(); 
			}

        }

		private async Task<bool> FlightExistsAsync(int flightId)
		{
			try
			{
				return await _flightService.ExistsAsync(flightId);
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error checking if flight exists: {ex.Message}", "Service Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return false;
			}
		}

		private async Task LoadComboDataAsync()
		{
			try
			{
				var airplanesResponse = await _airplaneService.GetAllAsync();
				if (airplanesResponse.IsSuccess && airplanesResponse.Result is List<Airplane> airplanes)
				{
					if (hasSoldTickets)
					{
                        comboBoxModels.ItemsSource = airplanes.Where(
							a => a.IsActive && a.TotalCapacity >= _flightToEdit.Tickets.Count).ToList();
                    }
					else
					{
                        comboBoxModels.ItemsSource = airplanes.Where(a => a.IsActive).ToList();

                    }
					
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
	}
}

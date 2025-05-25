using System.Windows;
using System.Windows.Controls;
using VitoriaAirlinesLibrary.Models;
using VitoriaAirlinesLibrary.Services;
using VitoriaAirlinesWPF.Windows;


namespace VitoriaAirlinesWPF.Pages
{
    /// <summary>
    /// Interaction logic for Airports.xaml
    /// </summary>
    public partial class AirportsPage : Page
    {
        AirportService _airportService;
        FlightService _flightService;
        List<Flight> Flights = new List<Flight>();
        public AirportsPage()
        {
            InitializeComponent();
            _airportService = new AirportService();
            _flightService = new FlightService();
            Loaded += Page_Loaded;
        }

        #region Events
        private void btnAddAirport_Click(object sender, RoutedEventArgs e)
        {
            var addAirportWindow = new AddAirportWindow(this);
            addAirportWindow.ShowDialog();
        }

        private async void btnDeleteAirport_Click(object sender, RoutedEventArgs e)
        {
            var selectedAirport = airportsDataGrid.SelectedItem as Airport;			

			if (!ConfirmDeletion(selectedAirport.Name))
				return;

            if (!await AirportExistsAsync(selectedAirport.Id))
                return;

            await DeleteAirportAsync(selectedAirport.Id);
		}

        private void btnEditAirport_Click(object sender, RoutedEventArgs e)
        {
            var selectedAirport = airportsDataGrid.SelectedItem as Airport;


            bool isUsed = Flights.Any(f => f.OriginAirportId == selectedAirport.Id ||
            f.DestinationAirportId == selectedAirport.Id);

            if (isUsed)
            {
                MessageBox.Show($"The Airport '{selectedAirport.Name}' cannot be updated because it has been used in flights.");
                return;
            }

            var editAirportWindow = new EditAirportWindow(this, selectedAirport);
            editAirportWindow.ShowDialog();

        }
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            panelAirportsLoading.Visibility = Visibility.Visible;

            await LoadAirportsAsync();
            await LoadFlightsAsync();

            panelAirportsLoading.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region Methods

        public async Task LoadFlightsAsync()
        {
            var response = await _flightService.GetAllAsync();

            if (response.IsSuccess)
            {
                Flights = response.Result as List<Flight>;
            }
        }
        public async Task LoadAirportsAsync()
        {
            List<Airport> Airports = new List<Airport>();

            var response = await _airportService.GetAllAsync();

            if (response.IsSuccess)
            {
                Airports = response.Result as List<Airport>;
                airportsDataGrid.ItemsSource = null;
                airportsDataGrid.ItemsSource = Airports;
            }
        }

		private async Task<bool> AirportExistsAsync(int airportId)
		{
			var clientExists = await _airportService.ExistsAsync(airportId);

			if (!clientExists)
			{
				MessageBox.Show("The selected airport no longer exists in the database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return false;
			}

			return true;
		}

		private bool ConfirmDeletion(string airportName)
		{
			var confirmResult = MessageBox.Show($"Are you sure you want to delete the airport '{airportName}'?",
												"Confirm Delete",
												MessageBoxButton.YesNo,
												MessageBoxImage.Question);

			return confirmResult == MessageBoxResult.Yes;
		}

		private async Task DeleteAirportAsync(int airportId)
		{
            panelAirportsLoading.Visibility = Visibility.Visible;
            txtAirports.Text = "Deleting airport.";

			var response = await _airportService.DeleteAsync(airportId);

			if (response.IsSuccess)
			{
				MessageBox.Show("Airport deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
				
			}
			else
			{
				MessageBox.Show($"Error deleting airport: {response.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}

            panelAirportsLoading.Visibility = Visibility.Collapsed;

            await LoadAirportsAsync();
            
        }

		#endregion

	}
}

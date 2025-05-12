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
        public AirportsPage()
        {
            InitializeComponent();
            _airportService = new AirportService();
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
		}

        private void btnEditAirport_Click(object sender, RoutedEventArgs e)
        {
            var selectedAirport = airportsDataGrid.SelectedItem as Airport;

            var editAirportWindow = new EditAirportWindow(this, selectedAirport);
            editAirportWindow.ShowDialog();

        }
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadAirports();
        }

        #endregion

        #region Methods
        public async Task LoadAirports()
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
			var response = await _airportService.DeleteAsync(airportId);

			if (response.IsSuccess)
			{
				MessageBox.Show("Airport deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
				await LoadAirports();
			}
			else
			{
				MessageBox.Show($"Error deleting airport: {response.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		#endregion

	}
}

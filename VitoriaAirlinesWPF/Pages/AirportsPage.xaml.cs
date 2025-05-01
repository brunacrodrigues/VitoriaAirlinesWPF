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
        List<Airport> Airports = new List<Airport>();
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

            var airportExists = await _airportService.GetByIdAsync(selectedAirport.Id);

            if (airportExists == null)
            {
                MessageBox.Show("The airport no longer exists in the database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var confirmResult = MessageBox.Show($"Are you sure you want to delete the airport '{selectedAirport.Name}'?",
                                                "Confirm Delete",
                                                MessageBoxButton.YesNo,
                                                MessageBoxImage.Question);

            if (confirmResult != MessageBoxResult.Yes)
                return;

            var response = await _airportService.DeleteAsync(selectedAirport.Id);

            if (response.IsSuccess)
            {
                MessageBox.Show("Airport deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                await InitPage();
            }
            else
            {
                MessageBox.Show($"Error deleting airport: {response.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnEditAirport_Click(object sender, RoutedEventArgs e)
        {
            var selectedAirport = airportsDataGrid.SelectedItem as Airport;

            var editAirportWindow = new EditAirportWindow(this, selectedAirport);
            editAirportWindow.ShowDialog();

        }
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await InitPage();
        }

        #endregion

        #region Methods
        public async Task InitPage()
        {
            var response = await _airportService.GetAllAsync();

            if (response.IsSuccess)
            {
                Airports = response.Result as List<Airport>;
                airportsDataGrid.ItemsSource = null;
                airportsDataGrid.ItemsSource = Airports;
            }
        }

        #endregion

    }
}

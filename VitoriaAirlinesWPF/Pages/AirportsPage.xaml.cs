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

        private void btnAddAirport_Click(object sender, RoutedEventArgs e)
        {
            var addAirportWindow = new AddAirportWindow();
            addAirportWindow.ShowDialog();
        }

        private async void btnDeleteAirport_Click(object sender, RoutedEventArgs e)
        {
            var selectedAirport = airportsDataGrid.SelectedItem as Airport;

            Airports.Remove(selectedAirport);

            await InitPage();
            
        }

        private void btnEditAirport_Click(object sender, RoutedEventArgs e)
        {
            var editAirportWindow = new EditAirportWindow();
            editAirportWindow.ShowDialog();

        }
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

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await InitPage();
        }
    }
}

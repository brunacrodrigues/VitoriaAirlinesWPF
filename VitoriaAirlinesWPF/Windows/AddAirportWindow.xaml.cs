using System.Windows;
using System.Windows.Media.Imaging;
using VitoriaAirlinesLibrary.Models;
using VitoriaAirlinesLibrary.Services;
using VitoriaAirlinesWPF.Pages;

namespace VitoriaAirlinesWPF.Windows
{
    /// <summary>
    /// Interaction logic for AddAirportWindow.xaml
    /// </summary>
    public partial class AddAirportWindow : Window
    {
        AirportService _airportService;
        AirportsPage _airportsPage;
        public AddAirportWindow(AirportsPage airportsPage)
        {
            InitializeComponent();
            _airportService = new AirportService();
            _airportsPage = airportsPage;
        }

        #region Events

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

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void btnAddAirport_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateData())
            {
                Airport newAirport = new Airport
                {
                    IATA = txtIATA.Text.ToUpper(),
                    Name = txtName.Text,
                    City = txtCity.Text,
                    Country = txtCountry.Text,
                };

                var response = await _airportService.CreateAsync(newAirport);

                creatingAirportOverlay.Visibility = Visibility.Visible;

                if (response.IsSuccess)
                {
                    MessageBox.Show("Airport added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    creatingAirportOverlay.Visibility = Visibility.Collapsed;

                    await _airportsPage.LoadAirportsAsync();
                    this.Close();
                }
                else
                {
                    MessageBox.Show($"Error adding airport: {response.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

        }

        #endregion

        #region Methods

        private bool ValidateData()
        {             
            if (string.IsNullOrEmpty(txtIATA.Text))
            {
                MessageBox.Show("Please enter the airport code.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (txtIATA.Text.Length != 3 || !txtIATA.Text.All(char.IsLetter))
            {
                MessageBox.Show("The airport code must have exactly 3 letters.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (string.IsNullOrEmpty(txtName.Text))
            {
                MessageBox.Show("Please enter the airport name.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (string.IsNullOrEmpty(txtCity.Text))
            {
                MessageBox.Show("Please enter the city of the airport.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (txtCity.Text.Any(char.IsDigit))
            {
                MessageBox.Show("The city name cannot contain numbers.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (string.IsNullOrEmpty(txtCountry.Text))
            {
                MessageBox.Show("Please enter the country of the airport.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (txtCountry.Text.Any(char.IsDigit))
            {
                MessageBox.Show("The country name cannot contain numbers.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        #endregion 
    }
}

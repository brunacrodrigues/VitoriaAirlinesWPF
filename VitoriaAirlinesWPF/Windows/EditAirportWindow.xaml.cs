using System.Windows;
using System.Windows.Media.Imaging;
using VitoriaAirlinesLibrary.Models;
using VitoriaAirlinesLibrary.Services;
using VitoriaAirlinesWPF.Pages;

namespace VitoriaAirlinesWPF.Windows
{
    /// <summary>
    /// Interaction logic for EditAirportWindow.xaml
    /// </summary>
    public partial class EditAirportWindow : Window
    {
        AirportsPage _airportsPage;
        Airport _updatedAirport;
        AirportService _airportService;

        public EditAirportWindow(AirportsPage airportsPage, Airport updatedAirport)
        {
            InitializeComponent();
            _airportsPage = airportsPage;
            _updatedAirport = updatedAirport;
            _airportService = new AirportService();
            InitWindow();
        }          

        #region Events
        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
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

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();

        }

        private async void btnUpdateAirport_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateData())
            {
                var airportExists = await _airportService.ExistsAsync(_updatedAirport.Id);

                if (!airportExists)
                {
                    MessageBox.Show("The airport no longer exists in the database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _updatedAirport.IATA = txtIATA.Text;
                _updatedAirport.Name = txtName.Text;
                _updatedAirport.City = txtCity.Text;
                _updatedAirport.Country = txtCountry.Text;

                var response = await _airportService.UpdateAsync(_updatedAirport);

                if (response.IsSuccess)
                {
                    MessageBox.Show("Airport updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    await _airportsPage.LoadAirports();
                    this.Close();
                }
                else
                {
                    MessageBox.Show($"Error updating airport: {response.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #endregion

        #region Methods
        private void InitWindow()
        {
            txtIATA.Text = _updatedAirport.IATA;
            txtName.Text = _updatedAirport.Name;
            txtCity.Text = _updatedAirport.City;
            txtCountry.Text = _updatedAirport.Country;
        }

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

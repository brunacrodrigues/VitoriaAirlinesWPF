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
        Airport _airportToEdit;
        AirportService _airportService;

        public EditAirportWindow(AirportsPage airportsPage, Airport airportToEdit)
        {
            InitializeComponent();
            _airportsPage = airportsPage;
            _airportToEdit = airportToEdit;
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
                if (!await AirportExistsAsync(_airportToEdit.Id))
                {
                    await HandleAirportNotFoundErrorAsync();
                    return;
                }

                UpdateAirportData();
               

               

                var response = await _airportService.UpdateAsync(_airportToEdit);

                await HandleUpdateResponseAsync(response);

                
            }
        }

        #endregion

        #region Methods
        private void InitWindow()
        {
            txtIATA.Text = _airportToEdit.IATA;
            txtName.Text = _airportToEdit.Name;
            txtCity.Text = _airportToEdit.City;
            txtCountry.Text = _airportToEdit.Country;
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

        private async Task HandleUpdateResponseAsync(Response response)
        {
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

        private void UpdateAirportData()
        {
            _airportToEdit.IATA = txtIATA.Text;
            _airportToEdit.Name = txtName.Text;
            _airportToEdit.City = txtCity.Text;
            _airportToEdit.Country = txtCountry.Text;
        }

        private async Task HandleAirportNotFoundErrorAsync()
        {
            MessageBox.Show("This airport no longer exists in the database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            await _airportsPage.LoadAirports();
            this.Close();
        }

        private async Task<bool> AirportExistsAsync(int airportId)
        {
            try
            {
                return await _airportService.ExistsAsync(airportId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error checking if airport exists: {ex.Message}", "Service Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        #endregion
    }
}

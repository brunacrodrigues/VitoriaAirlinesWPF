using System.Windows;
using System.Windows.Media.Imaging;
using VitoriaAirlinesLibrary.Models;
using VitoriaAirlinesLibrary.Services;
using VitoriaAirlinesWPF.Pages;

namespace VitoriaAirlinesWPF.Windows
{
    /// <summary>
    /// Interaction logic for EditAirplaneWindow.xaml
    /// </summary>
    public partial class EditAirplaneWindow : Window
    {
        AirplanesPage _airplanesPage;
        Airplane _airplaneToEdit;
        AirplaneService _airplaneService;

		public EditAirplaneWindow(AirplanesPage airplanesPage, Airplane airplaneToEdit)
        {
            InitializeComponent();
            _airplanesPage = airplanesPage;
            _airplaneToEdit = airplaneToEdit;
            _airplaneService = new AirplaneService();
            InitWindow();
        }


        #region Events
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

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

        private async void btnSaveModel_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateData())
            {

                if (!await AirplaneExistsAsync(_airplaneToEdit.Id))
                {
                    await HandleAirplaneNotFoundErrorAsync();
                    return;
                }

                UpdateAirplaneData();

                var response = await _airplaneService.UpdateAsync(_airplaneToEdit);

                await HandleUpdateResponseAsync(response);

			}
        }

        #endregion

        #region Methods

        private void InitWindow()
		{
			txtModel.Text = _airplaneToEdit.Model;
            ExecutiveSeatsUpDown.Value = _airplaneToEdit.ExecutiveSeats;
            EconomicSeatsUpDown.Value = _airplaneToEdit.EconomicSeats;
            CheckStatus();
            statusCheckBox.IsChecked = false;
		}

        private void CheckStatus()
        {

            txtStatus.Text = _airplaneToEdit.IsActive ? "Disable" : "Enable";
		}

		private bool ValidateData()
		{
            if (string.IsNullOrWhiteSpace(txtModel.Text))
            {
                MessageBox.Show("Please insert the airplane model.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                txtModel.Focus();
                return false;
            }

            if (ExecutiveSeatsUpDown.Value < 0 || EconomicSeatsUpDown.Value < 0)
            {
                MessageBox.Show("Seat counts cannot be negative.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (ExecutiveSeatsUpDown.Value == 0 && EconomicSeatsUpDown.Value == 0)
            {
                MessageBox.Show("Airplane must have at least one seat.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }
        private async Task<bool> AirplaneExistsAsync(int airplaneId)
        {
            try
            {
                return await _airplaneService.ExistsAsync(airplaneId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error checking if airplane exists: {ex.Message}", "Service Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void UpdateAirplaneData()
        {
            bool finalStatus;
            if (statusCheckBox.IsChecked == true)
            {
                finalStatus = !_airplaneToEdit.IsActive;
            }
            else
            {
                finalStatus = _airplaneToEdit.IsActive;
            }

            _airplaneToEdit.Model = txtModel.Text.Trim();
            _airplaneToEdit.IsActive = finalStatus;
            _airplaneToEdit.ExecutiveSeats = (int)ExecutiveSeatsUpDown.Value;
            _airplaneToEdit.EconomicSeats = (int)EconomicSeatsUpDown.Value;
        }

        private async Task HandleAirplaneNotFoundErrorAsync()
        {
            MessageBox.Show("This airplane model no longer exists in the database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            await _airplanesPage.LoadAirplanesAsync();
            this.Close();
        }

        private async Task HandleUpdateResponseAsync(Response response)
        {
            if (response.IsSuccess)
            {
                MessageBox.Show("Airplane model updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                await _airplanesPage.LoadAirplanesAsync();
                this.Close();
            }
            else
            {
                MessageBox.Show($"Error updating airplane model: {response.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

    }
}

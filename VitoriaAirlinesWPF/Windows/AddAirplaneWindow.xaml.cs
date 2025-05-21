using System.Windows;
using System.Windows.Media.Imaging;
using VitoriaAirlinesLibrary.Models;
using VitoriaAirlinesLibrary.Services;
using VitoriaAirlinesWPF.Pages;

namespace VitoriaAirlinesWPF.Windows
{
    /// <summary>
    /// Interaction logic for AddAirplaneWindow.xaml
    /// </summary>
    public partial class AddAirplaneWindow : Window
    {
        AirplaneService _airplaneService;
        AirplanesPage _airplanesPage;

        public AddAirplaneWindow(AirplanesPage airplanesPage)
        {
            InitializeComponent();
			_airplaneService = new AirplaneService();
            _airplanesPage = airplanesPage;
        }

        #region Events
        private async void btnAddAirplane_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateData())
            {
                Airplane newAirplaneModel = new Airplane
                {
                    Model = txtModel.Text.Trim(),
                    IsActive = true,
                    ExecutiveSeats = (int)ExecutiveSeatsUpDown.Value,
                    EconomicSeats = (int)EconomicSeatsUpDown.Value,
                };

                var response = await _airplaneService.CreateAsync(newAirplaneModel);

                if (response.IsSuccess)
                {
					MessageBox.Show("Airplane model created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

					await _airplanesPage.LoadAirplanesAsync();

					this.Close();
				}
				else
				{
					MessageBox.Show($"Error creating airplane model: {response.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				}
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

        #endregion

        #region Methods

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

		#endregion
	}
}

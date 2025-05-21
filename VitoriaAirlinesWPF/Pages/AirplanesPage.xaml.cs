using System.Windows;
using System.Windows.Controls;
using VitoriaAirlinesLibrary.Models;
using VitoriaAirlinesLibrary.Services;
using VitoriaAirlinesWPF.Windows;

namespace VitoriaAirlinesWPF.Pages
{
    /// <summary>
    /// Interaction logic for Airplanes.xaml
    /// </summary>
    public partial class AirplanesPage : Page
    {
        AirplaneService _airplaneService;

        public AirplanesPage()
        {
            InitializeComponent();
            _airplaneService = new AirplaneService();
			Loaded += Page_Loaded;
		}

        #region Events

        private async void Page_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            await LoadAirplanesAsync();
        }

        private void btnAddModel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var addAirplaneWindow = new AddAirplaneWindow(this);
            addAirplaneWindow.ShowDialog();
        }

        private async void btnDeleteModel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
			var selectedAirplaneModel = airplanesDataGrid.SelectedItem as Airplane;

            if (!ConfirmDeletion(selectedAirplaneModel.Model))
            {
                return;
            }

            if (!await AirplaneExistsAsync(selectedAirplaneModel.Id))
            {
                await LoadAirplanesAsync();
                return; 
            }

            await DeleteAirplaneAsync(selectedAirplaneModel.Id);


        }


        private void btnEditModel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var selectedAirplaneModel = airplanesDataGrid.SelectedItem as Airplane;
            EditAirplaneWindow airplaneWindow = new EditAirplaneWindow(this, selectedAirplaneModel);
            airplaneWindow.ShowDialog();

		}

        #endregion

        #region Methods
        public async Task LoadAirplanesAsync()
        {
            List<Airplane> Airplanes = new List<Airplane>();

            var response = await _airplaneService.GetAllAsync();

            if (response.IsSuccess)
            {
                Airplanes = response.Result as List<Airplane>;
                airplanesDataGrid.ItemsSource = null;
                airplanesDataGrid.ItemsSource = Airplanes;
            }
        }

		private async Task<bool> AirplaneExistsAsync(int airplaneId)
		{
			var airplaneExists = await _airplaneService.ExistsAsync(airplaneId);

			if (!airplaneExists)
			{
				MessageBox.Show("The selected airplane model no longer exists in the database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return false;
			}

			return true;
		}

		private bool ConfirmDeletion(string airplaneModel)
		{
			var confirmResult = MessageBox.Show($"Are you sure you want to delete the model '{airplaneModel}'?",
												"Confirm Delete",
												MessageBoxButton.YesNo,
												MessageBoxImage.Question);

			return confirmResult == MessageBoxResult.Yes;
		}

		private async Task DeleteAirplaneAsync(int airplaneId)
		{
			var response = await _airplaneService.DeleteAsync(airplaneId);

			if (response.IsSuccess)
			{
				MessageBox.Show("Model deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
				await LoadAirplanesAsync();
			}
			else
			{
				MessageBox.Show($"Error deleting airplane model: {response.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

        #endregion

    }
}

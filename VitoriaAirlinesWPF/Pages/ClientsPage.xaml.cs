using System.Windows;
using System.Windows.Controls;
using VitoriaAirlinesLibrary.Models;
using VitoriaAirlinesLibrary.Services;
using VitoriaAirlinesWPF.Windows;

namespace VitoriaAirlinesWPF.Pages
{
    /// <summary>
    /// Interaction logic for Passengers.xaml
    /// </summary>
    public partial class ClientsPage : Page
    {                          
        ClientService _clientService;
        public ClientsPage()
        {
            InitializeComponent();
            _clientService = new ClientService();
            Loaded += Page_Loaded;
        }

        private void btnAddClient_Click(object sender, RoutedEventArgs e)
        {

            var addClientWindow = new AddClientWindow(this);
            addClientWindow.ShowDialog();
        }

        private async void btnDeleteClient_Click(object sender, RoutedEventArgs e)
        {
            var selectedClient = clientsDataGrid.SelectedItem as Client;

			if (!await ClientExistsAsync(selectedClient.Id))
				return;

			if (!ConfirmDeletion(selectedClient.FullName))
				return;

			await DeleteClientAsync(selectedClient.Id);

		}

        private void btnEditClient_Click(object sender, RoutedEventArgs e)
        {
            var selectedClient = clientsDataGrid.SelectedItem as Client;

            var editClientWindow = new EditClientWindow(this, selectedClient);
            editClientWindow.ShowDialog();

        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadClients();
        }

        public async Task LoadClients()
        {
            panelClientsLoading.Visibility = Visibility.Visible;

            List<Client> Clients = new List<Client>();

            var response = await _clientService.GetAllAsync();
            

            if (response.IsSuccess)
            {
                Clients = response.Result as List<Client>;
                clientsDataGrid.ItemsSource = null;
                clientsDataGrid.ItemsSource = Clients;
            }

            panelClientsLoading.Visibility = Visibility.Collapsed;

        }

		private async Task<bool> ClientExistsAsync(int clientId)
		{
			var clientExists = await _clientService.ExistsAsync(clientId);

			if (!clientExists)
			{
				MessageBox.Show("The selected client no longer exists in the database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return false;
			}

			return true;
		}

		private bool ConfirmDeletion(string clientName)
		{
			var confirmResult = MessageBox.Show($"Are you sure you want to delete the client '{clientName}'?",
												"Confirm Delete",
												MessageBoxButton.YesNo,
												MessageBoxImage.Question);

			return confirmResult == MessageBoxResult.Yes;
		}

		private async Task DeleteClientAsync(int clientId)
		{
            panelClientsLoading.Visibility = Visibility.Visible;
            txtLoading.Text = "Deleting client...";

			var response = await _clientService.DeleteAsync(clientId);

			if (response.IsSuccess)
			{
				MessageBox.Show("Client deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
				
			}
			else
			{
				MessageBox.Show($"Error deleting client: {response.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}

            panelClientsLoading.Visibility = Visibility.Visible;
            await LoadClients();
        }

	}
}

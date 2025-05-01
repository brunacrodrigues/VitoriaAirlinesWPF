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

            var clientExists = await _clientService.GetByIdAsync(selectedClient.Id);

            if (clientExists == null)
            {
                MessageBox.Show("The selected client no longer exists in the database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var confirmResult = MessageBox.Show($"Are you sure you want to delete the client '{selectedClient.FullName}'?",
                                                "Confirm Delete",
                                                MessageBoxButton.YesNo,
                                                MessageBoxImage.Question);

            if (confirmResult != MessageBoxResult.Yes)
                return;

            var response = await _clientService.DeleteAsync(selectedClient.Id);

            if (response.IsSuccess)
            {
                MessageBox.Show("Client deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadClients();
            }
            else
            {
                MessageBox.Show($"Error deleting client: {response.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

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
            List<Client> Clients = new List<Client>();

            var response = await _clientService.GetAllAsync();
            

            if (response.IsSuccess)
            {
                Clients = response.Result as List<Client>;
                clientsDataGrid.ItemsSource = null;
                clientsDataGrid.ItemsSource = Clients;
            }

        }
    }
}

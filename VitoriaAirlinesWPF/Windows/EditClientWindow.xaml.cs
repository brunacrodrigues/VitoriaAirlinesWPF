using System.Net.Mail;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using VitoriaAirlinesLibrary.Models;
using VitoriaAirlinesLibrary.Services;
using VitoriaAirlinesWPF.Pages;

namespace VitoriaAirlinesWPF.Windows
{
    /// <summary>
    /// Interaction logic for EditClientWindow.xaml
    /// </summary>
    public partial class EditClientWindow : Window
    {
        private readonly ClientsPage _clientsPage;
        private readonly Client _clientToEdit;
        ClientService _clientService;

        public EditClientWindow(ClientsPage clientsPage, Client clientToEdit)
        {
            InitializeComponent();
            _clientsPage = clientsPage;
            _clientToEdit = clientToEdit;
            _clientService = new ClientService();
            InitWindow();
        }

        private async void btnUpdateClient_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateData())
            {
                var clientExists = await _clientService.ExistsAsync(_clientToEdit.Id);

                if (!clientExists)
                {
                    MessageBox.Show("The client no longer exists in the database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!await ClientExistsAsync(_clientToEdit.Id))
                {
                    await HandleClientNotFoundErrorAsync();
                    return;
                }

                UpdateClienteData();

                

                

                var response = await _clientService.UpdateAsync(_clientToEdit);
                await HandleUpdateResponseAsync(response);


               
            }

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

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #region Methods

        private void InitWindow()
        {
            txtFullName.Text = _clientToEdit.FullName;
            txtPassport.Text = _clientToEdit.Passaport;
            txtEmail.Text = _clientToEdit.Email;
            txtContact.Text = _clientToEdit.Contact;
        }
        private bool ValidateData()
        {
            string email = txtEmail.Text.Replace(" ", "").Trim();

            if (string.IsNullOrEmpty(txtFullName.Text))
            {
                MessageBox.Show("Please enter the client's name", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            string passport = txtPassport.Text.Replace(" ", "").Trim();

            if (string.IsNullOrEmpty(passport))
            {
                MessageBox.Show("Please enter the client's passport number", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (string.IsNullOrEmpty(email))
            {
                MessageBox.Show("Please enter the client's email", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!MailAddress.TryCreate(email, out MailAddress _))
            {
                MessageBox.Show("Please enter a valid email in the format name@domain.xxx.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (string.IsNullOrEmpty(txtContact.Text))
            {
                MessageBox.Show("Please enter the client's phone number", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!txtContact.Text.All(char.IsDigit))
            {
                MessageBox.Show("The phone number can only contain digits.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (txtContact.Text.Length != 9)
            {
                MessageBox.Show("The phone number must have exactly 9 characters.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;

        }

        private async Task HandleUpdateResponseAsync(Response response)
        {
            if (response.IsSuccess)
            {
                MessageBox.Show("Client updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                await _clientsPage.LoadClients();
                this.Close();
            }
            else
            {
                MessageBox.Show($"Error updating client: {response.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateClienteData()
        {
            _clientToEdit.FullName = txtFullName.Text;
            _clientToEdit.Passaport = txtPassport.Text;
            _clientToEdit.Email = txtEmail.Text.Replace(" ", "").Trim();
            _clientToEdit.Contact = txtContact.Text;
        }

        private async Task HandleClientNotFoundErrorAsync()
        {
            MessageBox.Show("This client no longer exists in the database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            await _clientsPage.LoadClients();
            this.Close();
        }

        private async Task<bool> ClientExistsAsync(int clientId)
        {
            try
            {
                return await _clientService.ExistsAsync(clientId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error checking if client exists: {ex.Message}", "Service Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        #endregion
    }
}

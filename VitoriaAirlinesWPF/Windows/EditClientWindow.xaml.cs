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
        private readonly Client _updatedClient;
        ClientService _clientService;

        public EditClientWindow(ClientsPage clientsPage, Client updatedClient)
        {
            InitializeComponent();
            _clientsPage = clientsPage;
            _updatedClient = updatedClient;
            _clientService = new ClientService();
            InitWindow();
        }

        private async void btnUpdateClient_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateData())
            {
                var clientExists = await _clientService.GetByIdAsync(_updatedClient.Id);

                if (clientExists == null)
                {
                    MessageBox.Show("The client no longer exists in the database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _updatedClient.FullName = txtFullName.Text;
                _updatedClient.Passaport = txtPassport.Text;
                _updatedClient.Email = txtEmail.Text;
                _updatedClient.Contact = txtContact.Text;

                var response = await _clientService.UpdateAsync(_updatedClient);

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
            txtFullName.Text = _updatedClient.FullName;
            txtPassport.Text = _updatedClient.Passaport;
            txtEmail.Text = _updatedClient.Email;
            txtContact.Text = _updatedClient.Contact;
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

        #endregion
    }
}

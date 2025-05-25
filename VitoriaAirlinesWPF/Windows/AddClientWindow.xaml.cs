using System.Net.Mail;
using System.Windows;
using System.Windows.Media.Imaging;
using VitoriaAirlinesLibrary.Models;
using VitoriaAirlinesLibrary.Services;
using VitoriaAirlinesWPF.Pages;

namespace VitoriaAirlinesWPF.Windows
{
    /// <summary>
    /// Interaction logic for AddClientWindow.xaml
    /// </summary>
    public partial class AddClientWindow : Window
    {
        private readonly ClientsPage _clientsPage;
        ClientService _clientService;

        public AddClientWindow(ClientsPage clientsPage)
        {
            InitializeComponent();
            _clientsPage = clientsPage;
            _clientService = new ClientService();
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

        private async void btnAddClient_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateData())
            {
                var newClient = new Client
                {
                    FullName = txtFullName.Text,
                    Passaport = txtPassport.Text,
                    Email = txtEmail.Text.Replace(" ", "").Trim(),
                    Contact = txtContact.Text,
                };

                creatingClientOverlay.Visibility = Visibility.Visible;

                var response = await _clientService.CreateAsync(newClient);

                if (response.IsSuccess)
                {
                    creatingClientOverlay.Visibility = Visibility.Visible;

                    MessageBox.Show("Client added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    await _clientsPage.LoadClients();
                    this.Close();
                }
                else
                {
                    MessageBox.Show($"Error adding client: {response.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

        }

        #endregion

        #region Methods
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

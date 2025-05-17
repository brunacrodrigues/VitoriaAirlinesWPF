using System;
using System.Net.Mail;
using System.Numerics;
using System.Security.Policy;
using System.Windows;
using System.Windows.Media.Imaging;
using VitoriaAirlinesLibrary.Enums;
using VitoriaAirlinesLibrary.Models;
using VitoriaAirlinesLibrary.Services;

namespace VitoriaAirlinesWPF.Windows
{
    /// <summary>
    /// Interaction logic for SellTicketsWindow.xaml
    /// </summary>
    public partial class SellTicketsWindow : Window
    {
        Flight _selectedFlight;
        ClientService _clientService;
        FlightService _flightService;

        List<Seat> AvailableSeats = new List<Seat>();
        List<Ticket> TicketsToPurchase = new List<Ticket>();
        List<Seat> SeatsToAssign = new List<Seat>();


        public SellTicketsWindow(Flight selectedFlight)
        {
            InitializeComponent();
            _selectedFlight = selectedFlight;
            _clientService = new ClientService();
            _flightService = new FlightService();
            Loaded += Window_Loaded;
        }

        private void btnRegisterNewClient_Click(object sender, RoutedEventArgs e)
        {
            comboBoxClients.SelectedItem = null;



            txtFullName.IsEnabled = true;
            txtPassport.IsEnabled = true;
            txtEmail.IsEnabled = true;
            txtContact.IsEnabled = true;

            txtFullName.Focus();

            if (txtFullName.Text != string.Empty && txtPassport.Text != string.Empty && txtEmail.Text != string.Empty && txtContact.Text != string.Empty)
            {
                ClearInput();
            }

        }

        private void ClearInput()
        {
            txtFullName.Text = string.Empty;
            txtPassport.Text = string.Empty;
            txtEmail.Text = string.Empty;
            txtContact.Text = string.Empty;
        }

        private async void btnAddPassenger_Click(object sender, RoutedEventArgs e)
        {

            SeatsToAssign = listBoxAvailableSeats.SelectedItems.Cast<Seat>().ToList();

            if (SeatsToAssign.Count == 0)
            {
                MessageBox.Show("Please select at least one seat.", "No seat selected", MessageBoxButton.OK, MessageBoxImage.Warning);
              return;
            }

            Client client = null;

            if (comboBoxClients.SelectedItem != null)
            {
                client = comboBoxClients.SelectedItem as Client;
            }
            else
            {
                if (ValidateNewClientData())
                {
                    client = new Client
                    {
                        FullName = txtFullName.Text,
                        Passaport = txtPassport.Text,
                        Email = txtEmail.Text.Replace(" ", "").Trim(),
                        Contact = txtContact.Text,
                    };

                    var response = await _clientService.CreateAsync(client);

                    if (response.IsSuccess)
                    {
                        await LoadClientsAsync();
                    }
                    else
                    {
                        MessageBox.Show($"Error adding client: {response.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                else
                {
                    return;
                }
            }

            if (client != null)
            {
                Seat seatToAssign = SeatsToAssign.First();

                CreateAndAddTicketToCart(seatToAssign, client);

                SeatsToAssign.Remove(seatToAssign);
                RefreshDataGridCart();

                ClearInput();                
            }

            comboBoxClients.SelectedItem = null;
            LoadListBoxAvailableSeats();
            
        }

        private void RefreshDataGridCart()
        {
            dataGridCart.ItemsSource = null;
            dataGridCart.ItemsSource = TicketsToPurchase;
        }
        private void CreateAndAddTicketToCart(Seat seat, Client client)
        {
            Seat selectedSeat = AvailableSeats.FirstOrDefault(s => s.Id == seat.Id);

            if (selectedSeat == null)
            {
                MessageBox.Show($"Internal error: Seat {selectedSeat.Name} not found in master list.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }



            if (TicketsToPurchase.Any(t => t.Seat.Id == seat.Id))
            {
                MessageBox.Show($"Seat {seat.Name} is already in your purchase list.", "Seat Already in Cart", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (TicketsToPurchase.Any(t => t.Client.Id == client.Id))
            {
                MessageBox.Show($"Client {client.FullName} already has a ticket in cart.", "Seat Already in Cart", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            decimal ticketPrice;
            if (seat.Type == SeatType.Executive)
            {
                ticketPrice = (decimal)_selectedFlight.ExecutiveTicketPrice;
            }
            else if (seat.Type == SeatType.Economic)
            {
                ticketPrice = (decimal)_selectedFlight.EconomicTicketPrice;
            }
            else
            {
                MessageBox.Show($"Could not determine the price for seat {seat.Name}.", "Pricing Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Ticket newTicketForCart = new Ticket
            {
                Flight = _selectedFlight,
                Seat = seat,
                Price = ticketPrice,
                Client = client
            };

            TicketsToPurchase.Add(newTicketForCart);

            selectedSeat.IsAvailable = false;
        }

        private void btnCancelPurchase_Click(object sender, RoutedEventArgs e)
        {
            if (!TicketsToPurchase.Any()) 
            {
                MessageBox.Show("The cart is already empty.", "Empty Cart", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            
            var result = MessageBox.Show("Are you sure you want to cancel the current purchase? All items in the cart will be removed.",
                                         "Cancel Purchase Confirmation",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
               

                foreach (var ticketInCart in TicketsToPurchase)
                {
                    Seat seatToMakeAvailable = AvailableSeats.FirstOrDefault(s => s.Id == ticketInCart.Seat.Id);
                    if (seatToMakeAvailable != null)
                    {
                        seatToMakeAvailable.IsAvailable = true;

                    }
                    else
                    {
                        MessageBox.Show("The seat could not be found.", "Seat Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                
                TicketsToPurchase.Clear();

                
                RefreshDataGridCart();    
                LoadListBoxAvailableSeats(); 

                MessageBox.Show("Purchase has been cancelled. The cart is now empty.", "Purchase Cancelled", MessageBoxButton.OK, MessageBoxImage.Information);
            }
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

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtFlightNumber.Text = _selectedFlight.FlightNumber;
            txtFlightRouteInfo.Text = _selectedFlight.FlightRoute;

            await LoadCombos();
            await LoadAvailableSeatsAsync();
        }


        private void btnProceedToCheckout_Click(object sender, RoutedEventArgs e)
        {
            CheckoutWindow checkoutWindow = new CheckoutWindow();
            checkoutWindow.ShowDialog();
        }

        private void btnRemoveFromCart_Click(object sender, RoutedEventArgs e)
        {
            RemoveTicketFromCart();

        }

        private void RemoveTicketFromCart()
        {
            if (dataGridCart.SelectedItem == null)
            {
                MessageBox.Show("Please select a ticket from the cart to remove.", "No Ticket Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var ticketToRemove = dataGridCart.SelectedItem as Ticket;

            if (ticketToRemove != null)
            {

                Seat seatToUpdate = AvailableSeats.FirstOrDefault(s => s.Id == ticketToRemove.Seat.Id);

                if (seatToUpdate != null)
                {

                    seatToUpdate.IsAvailable = true;
                }
                else
                {
                    MessageBox.Show("The seat could not be found.", "Seat Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                TicketsToPurchase.Remove(ticketToRemove);


                RefreshDataGridCart();
                LoadListBoxAvailableSeats();

                MessageBox.Show($"Ticket for seat {ticketToRemove.Seat.Name} has been removed from the cart.", "Ticket Removed", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async Task LoadCombos()
        {
            comboBoxSeatType.ItemsSource = Enum.GetValues(typeof(SeatType));
            await LoadClientsAsync();
        }

        private async Task LoadClientsAsync()
        {
            try

            {
                var clientsResponse = await _clientService.GetAllAsync();
                if (clientsResponse.IsSuccess && clientsResponse.Result is List<Client> clients)
                {
                    comboBoxClients.ItemsSource = clients;
                    comboBoxClients.DisplayMemberPath = "FullName";
                    comboBoxClients.SelectedValuePath = "Id";
                }
                else
                {
                    MessageBox.Show($"Error loading clients: {clientsResponse.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred while loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void comboBoxSeatType_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            LoadListBoxAvailableSeats();
        }

        private void comboBoxClients_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            txtFullName.IsEnabled = false;
            txtPassport.IsEnabled = false;
            txtEmail.IsEnabled = false;
            txtContact.IsEnabled = false;

            if (comboBoxClients.SelectedItem != null)
            {
                Client client = comboBoxClients.SelectedItem as Client;
                txtFullName.Text = client.FullName;
                txtPassport.Text = client.Passaport;
                txtEmail.Text = client.Email;
                txtContact.Text = client.Contact;
            }

        }

        private bool ValidateNewClientData()
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

        private async Task LoadAvailableSeatsAsync()
        {
            DisableControls();
            panelSeatLoading.Visibility = Visibility.Visible;


            try

            {
                var ticketsResponse = await _flightService.GetTicketsForFlight(_selectedFlight.Id);
                if (ticketsResponse.IsSuccess && ticketsResponse.Result is List<Ticket> tickets)
                {
                    AvailableSeats = tickets.Where(t => t.Client == null).Select(t => t.Seat).Where(s => s != null && s.IsAvailable).ToList();
                }
                else
                {
                    MessageBox.Show($"Error loading flight tickets: {ticketsResponse.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred while loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                panelSeatLoading.Visibility = Visibility.Collapsed;

                if (comboBoxSeatType.Items.Count > 0)
                {
                    comboBoxSeatType.SelectedIndex = 0;
                }

                EnableControls();
            }

        }

        private void LoadListBoxAvailableSeats()
        {
            SeatType seatType = (SeatType)comboBoxSeatType.SelectedItem;

            List<Seat> SeatsToDisplay = AvailableSeats.Where(s => s.Type == seatType && s.IsAvailable).ToList();

            listBoxAvailableSeats.ItemsSource = null;
            listBoxAvailableSeats.ItemsSource = SeatsToDisplay;
            listBoxAvailableSeats.DisplayMemberPath = "Name";

        }

        private void DisableControls()
        {
            comboBoxSeatType.IsEnabled = false;

        }

        private void EnableControls()
        {
            comboBoxSeatType.IsEnabled = true;
        }


    }
}

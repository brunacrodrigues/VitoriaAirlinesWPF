﻿using System.Windows;
using System.Windows.Media.Imaging;
using VitoriaAirlinesWPF.Pages;

namespace VitoriaAirlinesWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            frameContainer.Navigate(new SearchFlightsPage());
        }

        private void btnFlights_Click(object sender, RoutedEventArgs e)
        {
            frameContainer.Navigate(new FlightsPage());
        }

        private void btnAirplanes_Click(object sender, RoutedEventArgs e)
        {
            frameContainer.Navigate(new AirplanesPage());

        }

        private void btnAirports_Click(object sender, RoutedEventArgs e)
        {
            frameContainer.Navigate(new AirportsPage());
        }

        private void btnTickets_Click(object sender, RoutedEventArgs e)
        {
            frameContainer.Navigate(new TicketsPage());
        }

        private void btnPassengers_Click(object sender, RoutedEventArgs e)
        {
            frameContainer.Navigate(new ClientsPage());
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            frameContainer.Navigate(new SearchFlightsPage());
        }

    }
}
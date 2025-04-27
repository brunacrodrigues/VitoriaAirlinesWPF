using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VitoriaAirlinesLibrary.Models;

namespace VitoriaAirlinesWPF.Pages
{
    /// <summary>
    /// Interaction logic for Airports.xaml
    /// </summary>
    public partial class Airports : Page
    {
        List<Airport> airports = new List<Airport>();
        public Airports()
        {
            InitializeComponent();        
                        

            airports.Add(new Airport
            {
                Id = 1,
                IATA = "LIS",
                Name = "Humberto Delgado",
                City = "Lisbon",
                Country = "Portugal"
            });
            airports.Add(new Airport
            {
                Id = 1,
                IATA = "LIS",
                Name = "Humberto Delgado",
                City = "Lisbon",
                Country = "Portugal"
            });

            airportsDataGrid.ItemsSource = airports;
        }

        private void btnAddAirport_Click(object sender, RoutedEventArgs e)
        {

        }

      

        private void btnDeleteAirport_Click(object sender, RoutedEventArgs e)
        {
            var selectedAirport = airportsDataGrid.SelectedItem as Airport;

            airports.Remove(selectedAirport);
            airportsDataGrid.ItemsSource = null; // Atualize a fonte de dados
            airportsDataGrid.ItemsSource = airports;
        }

        private void btnEditAirport_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

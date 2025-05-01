using System.Windows;
using System.Windows.Media.Imaging;

namespace VitoriaAirlinesWPF.Windows
{
    /// <summary>
    /// Interaction logic for AddAirportWindow.xaml
    /// </summary>
    public partial class AddAirportWindow : Window
    {
        public AddAirportWindow()
        {
            InitializeComponent();
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

        private void ValidateData()
        {

        }
    }
}

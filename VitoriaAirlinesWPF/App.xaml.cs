using Microsoft.Extensions.Configuration;
using Syncfusion.Licensing;
using System.Configuration;
using System.IO;
using System.Windows;

namespace VitoriaAirlinesWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IConfiguration AppConfig { get; private set; }
        public App()
        {
            SyncfusionLicenseProvider.RegisterLicense("Mzg0Njg3N0AzMjM5MmUzMDJlMzAzYjMyMzkzYkczOUpmTVhsUCtCamJrSEs0dTE5VXlsenptWEFuM0VMQUdMbDA0a213TU09");
            InitializeComponent();
        }

             protected override void OnStartup(StartupEventArgs e)
        {
           
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            AppConfig = builder.Build();

            base.OnStartup(e); 


        }
    }

}

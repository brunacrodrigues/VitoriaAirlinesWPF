using Syncfusion.Licensing;
using Syncfusion.SfSkinManager;
using System.Configuration;
using System.Data;
using System.Windows;

namespace VitoriaAirlinesWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            SyncfusionLicenseProvider.RegisterLicense("Mzg0Njg3N0AzMjM5MmUzMDJlMzAzYjMyMzkzYkczOUpmTVhsUCtCamJrSEs0dTE5VXlsenptWEFuM0VMQUdMbDA0a213TU09");

            //SfSkinManager.ApplyStylesOnApplication = true;
        }

    }

}

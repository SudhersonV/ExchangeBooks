using Xamarin.Forms;
using ExchangeBooks.Bootstrap;
using ExchangeBooks.Interfaces.Framework;

[assembly: ExportFont("iconize-material.ttf", Alias= "MaterialIcons")]
namespace ExchangeBooks
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            AppContainer.RegisterDependencies();
            AppContainer.RegisterRoutes();
            AppContainer.InitializeMessenger();
            MainPage = new AppShell();
        }
        
        protected override void OnStart()
        {
            var eventTracker = AppContainer.Resolve<IEventTracker>();
            eventTracker.SetUserId("12345");
            eventTracker.SetUserProperty("UserAge", "35");
            eventTracker.SetUserProperty("UserGender", "Male");
            eventTracker.SetUserProperty("AppVersionwithBuild", "1.0 (1)");
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}

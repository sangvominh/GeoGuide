namespace MauiApp1
{
    public partial class AppShell : Shell
    {
        public const string MainMapNavigationRoute = "main_map_page";

        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(MainMapNavigationRoute, typeof(Pages.MainMapPage));
        }
    }
}

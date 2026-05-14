namespace MauiApp1
{
    public partial class AppShell : Shell
    {
        public const string MainMapNavigationRoute = "main_map_page";
        public const string DemoStatusNavigationRoute = "demo_status_page";

        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(MainMapNavigationRoute, typeof(Pages.MainMapPage));
            Routing.RegisterRoute(DemoStatusNavigationRoute, typeof(Pages.DemoStatusPage));
        }
    }
}

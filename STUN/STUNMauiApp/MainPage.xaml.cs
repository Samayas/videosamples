using STUNLibrary.Client;

namespace STUNMauiApp
{
    public partial class MainPage : ContentPage
    {
        private readonly STUNClient stunClient;

        // Configuration
        private readonly string stunServer = "stun.l.google.com";
        private readonly int port = 19302;

        // Constructor Injection: MAUI automatically passes the registered STUNClient here
        public MainPage(STUNClient stunClient)
        {
            InitializeComponent();
            this.stunClient = stunClient;
        }

        private async void OnDiscoverClicked(object sender, EventArgs e)
        {
            // UI State: Loading
            DiscoverBtn.IsEnabled = false;
            LoadingSpinner.IsRunning = true;
            OutputLabel.Text = "Querying Google STUN...";

            try
            {
                // Call the library (async)
                STUNNetworkInfo result = await stunClient.QueryAsync(stunServer, port);

                // Update UI with result
                OutputLabel.Text = result.ToString();
            }
            catch (Exception ex)
            {
                OutputLabel.Text = $"Error: {ex.Message}";
                await DisplayAlertAsync("Connection Failed", ex.Message, "OK");
            }
            finally
            {
                // UI State: Ready
                LoadingSpinner.IsRunning = false;
                DiscoverBtn.IsEnabled = true;
            }
        }
    }
}

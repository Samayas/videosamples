using System.Net;
using System.Net.Sockets;

namespace STUNLibrary.Client
{
    public class STUNClient
    {
        private const int DefaultSTUNPort = 3478; // Standard STUN port, Google uses 19302 often

        /// <summary>
        /// Queries a STUN server to discover public network information.
        /// </summary>
        /// <param name="serverAddress">The hostname or IP of the STUN server.</param>
        /// <param name="port">The port number (defaults to 3478).</param>
        /// <returns>A STUNNetworkInfo object containing the discovery results.</returns>
        public async Task<STUNNetworkInfo> QueryAsync(string serverAddress, int port = DefaultSTUNPort)
        {
            // Resolve DNS
            IPAddress[] addresses = await Dns.GetHostAddressesAsync(serverAddress);
            if (addresses.Length == 0) throw new Exception("Could not resolve host");

            IPEndPoint remoteEndPoint = new IPEndPoint(addresses[0], port);

            using (UdpClient udpClient = new UdpClient())
            {
                // Ensure we don't block forever
                udpClient.Client.ReceiveTimeout = 3000;
                udpClient.Client.SendTimeout = 3000;

                // Create and send request
                byte[] requestBytes = STUNProtocol.CreateBindingRequest();
                DateTime startTime = DateTime.Now;

                // Send async
                await udpClient.SendAsync(requestBytes, requestBytes.Length, remoteEndPoint);

                // Receive async
                Task<UdpReceiveResult> receiveTask = udpClient.ReceiveAsync();
                Task delayTask = Task.Delay(3000);

                Task completedTask = await Task.WhenAny(receiveTask, delayTask);
                if (completedTask == delayTask)
                {
                    throw new TimeoutException($"Timeout waiting for response from {serverAddress}");
                }

                UdpReceiveResult result = await receiveTask;
                TimeSpan latency = DateTime.Now - startTime;

                // Parse
                STUNNetworkInfo info = STUNProtocol.ParseResponse(result.Buffer);

                // Fill local metadata
                IPEndPoint localPoint = (IPEndPoint)udpClient.Client.LocalEndPoint;
                info.LocalIPAddress = localPoint.Address;
                info.LocalPort = localPoint.Port;
                info.Latency = latency;
                info.STUNServerUsed = serverAddress;

                return info;
            }
        }
    }
}

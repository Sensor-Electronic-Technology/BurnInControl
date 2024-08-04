using System.Net.NetworkInformation;

namespace BurnInControl.Dashboard.Services;

public static class PingService {
    public static bool Ping(string ipAddress) {
        try {
            using var ping = new Ping();
            var reply = ping.Send(ipAddress, 500);
            Console.WriteLine($"Ip Address: {ipAddress} Status: {reply.Status}");
            return reply.Status == IPStatus.Success;
        } catch (Exception) {
            return false;
        }
    }
}
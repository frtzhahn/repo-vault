using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using RepoVault.Core.Services;

namespace RepoVault.Core.GitServer;

public static class NetworkAddressHelper
{
    public static string GetPreferredLanIpv4Address()
    {
        string fallback = "127.0.0.1";
        string? best = null;
        int bestScore = 99;

        foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (nic.OperationalStatus != OperationalStatus.Up)
            {
                continue;
            }

            if (nic.NetworkInterfaceType == NetworkInterfaceType.Loopback ||
                nic.NetworkInterfaceType == NetworkInterfaceType.Tunnel)
            {
                continue;
            }

            bool wantedType = nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet ||
                              nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211;

            foreach (UnicastIPAddressInformation addr in nic.GetIPProperties().UnicastAddresses)
            {
                if (addr.Address.AddressFamily != AddressFamily.InterNetwork)
                {
                    continue;
                }

                if (IPAddress.IsLoopback(addr.Address))
                {
                    continue;
                }

                string ip = addr.Address.ToString();
                if (ip.StartsWith("169.254.", StringComparison.Ordinal))
                {
                    continue;
                }

                int score = IsPrivateLan(ip) ? (wantedType ? 0 : 1) : 2;
                if (score < bestScore)
                {
                    bestScore = score;
                    best = ip;
                }
            }
        }

        return best ?? fallback;
    }

    public static string BuildCloneUrl(string repoSlug, int port = 5050, string? hostOverride = null)
    {
        string slug = RepositoryCatalogService.NormalizeSlug(repoSlug);
        string host = string.IsNullOrWhiteSpace(hostOverride)
            ? GetPreferredLanIpv4Address()
            : hostOverride.Trim();
        return $"http://{host}:{port}/repos/{slug}.git";
    }

    private static bool IsPrivateLan(string ip)
    {
        if (ip.StartsWith("192.168.", StringComparison.Ordinal))
        {
            return true;
        }

        if (ip.StartsWith("10.", StringComparison.Ordinal))
        {
            return true;
        }

        // 172.16.0.0 to 172.31.255.255
        string[] parts = ip.Split('.');
        if (parts.Length == 4 && parts[0] == "172" &&
            int.TryParse(parts[1], out int second) && second >= 16 && second <= 31)
        {
            return true;
        }

        return false;
    }
}

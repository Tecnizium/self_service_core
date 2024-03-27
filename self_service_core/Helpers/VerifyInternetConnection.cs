using System.Net;

namespace self_service_core.Helpers;

public static class VerifyInternetConnection
{
    public static bool IsConnected()
    {
        try
        {
            using var client = new WebClient();
            using var stream = client.OpenRead("https://clients3.google.com/generate_204");
            return true;
        }
        catch
        {
            return false;
        }
    }
}
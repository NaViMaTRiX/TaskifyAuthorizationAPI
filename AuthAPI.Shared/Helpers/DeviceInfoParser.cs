using UAParser;

namespace AuthAPI.Shared.Helpers;

public static class DeviceInfoParser
{
    public static string GetDeviceInfo(string userAgent)
    {
        var parser = Parser.GetDefault();
        var clientInfo = parser.Parse(userAgent);
        
        return $"{clientInfo.Device.Family} | {clientInfo.OS.Family} {clientInfo.OS.Major}";
    }
}

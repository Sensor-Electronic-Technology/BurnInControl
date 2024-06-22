namespace BurnInControl.HubDefinitions.Hubs;

public class HostHubConstants {
    public static string HostHubAddress=>"http://localhost:4000/hubs/host";
    public static class Methods {
        public static string RestartService => "RestartService";
        public static string RestartBrowser => "RestartBrowser";
    }
}
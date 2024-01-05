namespace BurnIn.Shared;

public static class Addresses
{
    public static string HubUrl => "http://localhost:5070/hubs/testhub";

    public static class Events {
        public static string MessageSent => "ShowMessage";
        public static string OnGetIncrement => "OnGetIncrement";
    }
}
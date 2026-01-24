namespace Y.Authentication.Domain.Constants;
public static class KafkaConstants
{
    public static class Producers
    {
        public const string Authentication = "authentication.default.producer";
    }

    public static class Topics
    {
        public const string NotifyChannelTopic = "notification.notify-channel.topic";
    }

    public static class ConsumerGroups
    {
    }
}

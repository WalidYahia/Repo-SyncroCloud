using SyncroInfraLayer.Enums;

namespace SyncroInfraLayer.Helpers
{
    public class MqttHelper
    {
        // Standard pattern: Syncro/{deviceId}/{topic}
        public static string GetMqttTopic(MqttTopics mqttTopics, string deviceId) =>
            $"Syncro/{deviceId}/{mqttTopics}";

        // Same pattern with MQTT wildcard instead of a specific deviceId
        public static string GetWildcardTopic(MqttTopics mqttTopics) =>
            GetMqttTopic(mqttTopics, "+");

        // Non-standard patterns published by the device (different segment structure)
        public const string SensorDataWildcard   = "Syncro/+/sensors/+/data";
        public const string DeviceStatusWildcard = "Syncro/+/status";
    }
}

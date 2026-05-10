using SyncroInfraLayer.Enums;

namespace SyncroInfraLayer.Helpers
{
    public class MqttHelper
    {
        // ── Build: Syncro device topics ───────────────────────────────
        // Pattern: Syncro/{deviceId}/{topic}

        public static string GetMqttTopic(MqttTopics topic, string deviceId) =>
            $"Syncro/{deviceId}/{topic}";

        public static string GetWildcardTopic(MqttTopics topic) =>
            GetMqttTopic(topic, "+");

        // ── Build: SmartGuard hub topics ──────────────────────────────
        // Pattern: {hubId}/{topic}

        public static string GetHubTopic(MqttTopics topic, string hubId) =>
            GetMqttTopic(topic, hubId);

        public static string GetHubWildcardTopic(MqttTopics topic) =>
            GetWildcardTopic(topic);

        // ── Build: command topic ──────────────────────────────────────
        // Pattern: Syncro/{deviceId}/commands/{action}

        public static string GetCommandTopic(string deviceId, string action) =>
            $"Syncro/{deviceId}/commands/{action}";

        // ── Parse: Syncro/{deviceId}/sensors/{sensorId}/data ──────────
        public static bool TryParseSensorData(string topic, out string deviceId, out Guid sensorId)
        {
            deviceId = string.Empty;
            sensorId = Guid.Empty;
            var p = topic.Split('/');
            if (p.Length != 5 || p[0] != "Syncro" || p[2] != "sensors" || p[4] != "data")
                return false;
            deviceId = p[1];
            return Guid.TryParse(p[3], out sensorId);
        }

        // ── Parse: Syncro/{deviceId}/status ──────────────────────────
        public static bool TryParseDeviceStatus(string topic, out string deviceId)
        {
            deviceId = string.Empty;
            var p = topic.Split('/');
            if (p.Length != 3 || p[0] != "Syncro" || p[2] != "status")
                return false;
            deviceId = p[1];
            return true;
        }

        // ── Parse: Syncro/{deviceId}/{mqttTopic} ──────────────────────
        public static bool TryParseDeviceTopic(string topic, MqttTopics mqttTopic, out string deviceId)
        {
            deviceId = string.Empty;
            var p = topic.Split('/');
            if (p.Length != 3 || p[0] != "Syncro" || p[2] != mqttTopic.ToString())
                return false;
            deviceId = p[1];
            return true;
        }

        // ── Parse: {hubId}/{mqttTopic} ────────────────────────────────
        public static bool TryParseHubTopic(string topic, MqttTopics mqttTopic, out string hubId)
        {
            hubId = string.Empty;
            var p = topic.Split('/');
            if (p.Length != 2 || p[1] != mqttTopic.ToString())
                return false;
            hubId = p[0];
            return true;
        }
    }
}

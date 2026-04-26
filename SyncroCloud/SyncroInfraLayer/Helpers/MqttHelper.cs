using SyncroInfraLayer.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncroInfraLayer.Helpers
{
    public class MqttHelper
    {
        public static string GetMqttTopic(MqttTopics mqttTopics, string deviceId)
        {
            return $"Syncro/{deviceId}/{mqttTopics.ToString()}";
        }
    }
}

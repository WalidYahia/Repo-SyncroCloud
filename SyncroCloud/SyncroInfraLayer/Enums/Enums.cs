namespace SyncroInfraLayer.Enums;

// ── Device ────────────────────────────────────────────────
public enum DeviceType
{
    SmartHome,
    Monitoring
}

public enum DeviceStatus
{
    Online,
    Offline,
    Maintenance
}

// ── Sensor ────────────────────────────────────────────────
public enum SensorType
{
    Swich,
    Temperature,
    Humidity,
    Pressure,
    Motion,
    Gas,
    Light,
    Vibration,
    Current,
    Voltage
}
public enum UnitType
{
    Unknown = -1,
    SonoffMiniR3 = 0,
    SonoffMiniR4M = 1,
}
public enum SwitchNo
{
    Non = -1,
    Switch1,
    Switch2,
    Switch3,
    Switch4,
    Switch5,
    Switch6,
    Switch7,
    Switch8
}

// ── Alarm ─────────────────────────────────────────────────
public enum AlarmCondition
{
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual,
    Equal,
    Between
}

public enum AlarmSeverity
{
    Info,
    Warning,
    Critical
}

public enum ConnectionProtocol
{
    MQTT,
    HTTP,
    CoAP,
    Modbus,
    Zigbee,
    ZWave,
    BLE,
    LoRa,
    RS485
}

// ── Scenario ──────────────────────────────────────────────
public enum SwitchOutletStatus
{
    Off = 0,
    On = 1
}

public enum ScenarioCondition
{
    Duration = 0,
    OnTime = 1,
    OnOtherSensorValue = 2
}

public enum ScenarioOperator
{
    Equals,
    NotEquals,
    GreaterThan,
    LessThan,
    GreaterOrEqual,
    LessOrEqual
}

public enum ScenarioLogic
{
    And,
    Or
}

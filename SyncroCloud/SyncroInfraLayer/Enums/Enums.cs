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
    Unknown           = 0,
    SonOffMiniR3Swich = 1,
    Temperature       = 2,
    Humidity          = 3,
    Pressure          = 4,
    Motion            = 5,
    Gas               = 6,
    Light             = 7,
    Vibration         = 8,
    Current           = 9,
    Voltage           = 10,
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

// ── Remote Action ─────────────────────────────────────────
public enum JsonCommandType
{
    TurnOff      = 0,
    TurnOn       = 1,
    EnableInching  = 2,
    DisableInching = 3,
    UpdateUnitName = 6,
    SaveScenario   = 10,
    DeleteScenario = 11
}

public enum RemoteActionState
{
    Ok                           = 0,
    Error                        = 1,
    Timeout                      = 3,
    BadRequest                   = 4,
    DeviceDataIsRequired         = 5,
    DeviceAlreadyRegistered      = 6,
    DeviceNameAlreadyRegistered  = 7,
    Conflict                     = 8,
    InchingIntervalValidationError = 9,
    EmptyPayload                 = 10,
    NoContent                    = 11
}

public enum MqttTopics
{
    /// <summary>
    /// Publish from Cloud
    /// </summary>
    CloudSensorConfig,

    /// <summary>
    /// Publish from Device
    /// </summary>
    DeviceSensorConfig,

    /// <summary>
    /// Publish from Cloud
    /// </summary>
    CloudUserScenario,

    /// <summary>
    /// Publish from Device
    /// </summary>
    UserScenario,

    /// <summary>
    /// Publish from Device
    /// </summary>
    DeviceData,

    /// <summary>
    /// Publish from Cloud
    /// </summary>
    RemoteAction,

    /// <summary>
    /// Publish from Device
    /// </summary>
    RemoteAction_Ack,

    /// <summary>
    /// Publish from Cloud
    /// </summary>
    RemoteUpdate,

    /// <summary>
    /// Publish from Device
    /// </summary>
    RemoteUpdate_Ack,
}

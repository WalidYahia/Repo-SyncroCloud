namespace SyncroInfraLayer.Identity;

public static class PrivilegeCodes
{
    public const string CreateEditUser          = "CreateEditUser";
    public const string AssignDeviceSensorToUser = "AssignDeviceSensorToUser";
    public const string DefineSensor            = "DefineSensor";
    public const string CreateDevice            = "CreateDevice";
    public const string ManageSensorToDevice = "ManageSensorToDevice";
    public const string ManageScenarioToDevice = "ManageScenarioToDevice";
    public const string ManageRoles                 = "ManageRoles";
    public const string ManageTenants               = "ManageTenants";

    public static readonly IReadOnlyList<(string Code, string Name)> All =
    [
        (CreateEditUser,          "Can Create User/Edit"),
        (AssignDeviceSensorToUser, "Can Assign device/sensor to user"),
        (DefineSensor,            "Can Define sensor to the system"),
        (CreateDevice,            "Can Create new device"),
        (ManageSensorToDevice,    "Can Manage Sensors to device"),
        (ManageScenarioToDevice,  "Can Manage scenario to device"),
        (ManageRoles,                 "Can Manage Roles"),
        (ManageTenants,                 "Can Manage Tenants"),
    ];
}

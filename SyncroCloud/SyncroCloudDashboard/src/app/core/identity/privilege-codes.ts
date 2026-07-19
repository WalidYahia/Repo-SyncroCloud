export const PrivilegeCodes = {
  CreateEditUser:           'CreateEditUser',
  AssignDeviceSensorToUser: 'AssignDeviceSensorToUser',
  DefineSensor:             'DefineSensor',
  CreateDevice:             'CreateDevice',
  ManageSensorToDevice:     'ManageSensorToDevice',
  ManageScenarioToDevice:   'ManageScenarioToDevice',
  ManageRoles:              'ManageRoles',
  ManageTenants:            'ManageTenants',
} as const;

export type PrivilegeCode = typeof PrivilegeCodes[keyof typeof PrivilegeCodes];

export interface PrivilegeDto {
  id: string;
  code: string;
  name: string;
}

export interface RoleDetailDto {
  id: string;
  name: string;
  privileges: PrivilegeDto[];
}

export interface CreateRoleDto {
  name: string;
  privilegeIds: string[];
}

export interface UpdateRolePrivilegesDto {
  privilegeIds: string[];
}

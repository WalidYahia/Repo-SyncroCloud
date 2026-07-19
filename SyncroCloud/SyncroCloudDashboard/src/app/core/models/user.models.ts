export interface UserDto {
  id: string;
  phoneNumber: string;
  email: string | null;
  firstName: string;
  lastName: string;
  createdAt: string;
  isActive: boolean;
  roles: string[];
}

export interface CreateUserDto {
  phoneNumber: string;
  password: string;
  firstName: string;
  lastName: string;
  tenantIds: string[];
  roleId: string;
  email?: string | null;
}

export interface UpdateUserDto {
  email: string | null;
  firstName: string;
  lastName: string;
  isActive: boolean;
}

export interface UpdateUserRoleDto {
  roleId: string;
}

export interface RoleDto {
  id: string;
  name: string;
}

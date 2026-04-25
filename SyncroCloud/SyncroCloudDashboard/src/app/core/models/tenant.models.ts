export interface TenantDto {
  id: string;
  name: string;
  createdAt: string;
  isActive: boolean;
}

export interface CreateTenantDto { name: string; }
export interface UpdateTenantDto { name: string; isActive: boolean; }

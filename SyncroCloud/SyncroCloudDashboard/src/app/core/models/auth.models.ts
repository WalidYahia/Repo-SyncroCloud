export interface LoginDto { emailOrPhone: string; password: string; }
export interface RegisterDto { phoneNumber: string; password: string; firstName: string; lastName: string; tenantId: string; email?: string | null; }
export interface TokenResponse { accessToken: string; refreshToken: string; expiresAt: string; }

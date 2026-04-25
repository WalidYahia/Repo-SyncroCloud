export interface LoginDto { emailOrPhone: string; password: string; }
export interface RegisterDto { email: string; password: string; firstName: string; lastName: string; phoneNumber?: string; }
export interface TokenResponse { accessToken: string; refreshToken: string; expiresAt: string; }

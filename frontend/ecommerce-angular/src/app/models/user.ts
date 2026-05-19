export interface LoginDTO {
  email: string;
  password: string;
}

export interface RegisterDTO {
  name: string;
  email: string;
  password: string;
  role: string;        // "User" or "Admin"
}

export interface AuthResponse {
  refreshToken: string;
  user: {
    userId: number;
    name: string;
    email: string;
    role: string;
  };
}
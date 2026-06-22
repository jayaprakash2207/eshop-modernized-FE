import { apiRequest } from "../../app/http";

export type AuthResponse = {
  accessToken: string;
  expiresAtUtc: string;
  refreshToken: string;
  username: string;
  role: string;
};

export type UserProfile = {
  userId: string;
  username: string;
  email: string;
  phoneNumber: string;
  role: string;
  emailConfirmed: boolean;
};

export async function authenticate(username: string, password: string) {
  return apiRequest<AuthResponse>("/api/authenticate", {
    method: "POST",
    body: JSON.stringify({ username, password })
  });
}

export function fetchProfile() {
  return apiRequest<UserProfile>("/Manage/MyAccount");
}

export function updateProfile(email: string, phoneNumber: string) {
  return apiRequest<UserProfile>("/Manage/MyAccount", {
    method: "POST",
    body: JSON.stringify({ email, phoneNumber })
  });
}

export function changePassword(currentPassword: string, newPassword: string) {
  return apiRequest<{ message: string }>("/Manage/ChangePassword", {
    method: "POST",
    body: JSON.stringify({ currentPassword, newPassword })
  });
}

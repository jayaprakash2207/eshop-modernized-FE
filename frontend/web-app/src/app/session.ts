const tokenKey = "platform-app-token";

export function getAccessToken(): string | null {
  return window.localStorage.getItem(tokenKey);
}

export function setAccessToken(token: string | null) {
  if (token) {
    window.localStorage.setItem(tokenKey, token);
  } else {
    window.localStorage.removeItem(tokenKey);
  }
}

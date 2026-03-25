"use client";

import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
} from "react";
import type { AdminUser, AuthTokens, LoginResponse } from "./types";
import { apiPost } from "./api-client";

interface AuthState {
  user: AdminUser | null;
  token: string | null;
  isLoading: boolean;
}

interface AuthContextValue extends AuthState {
  login: (email: string, password: string) => Promise<{ success: boolean; error?: string }>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextValue | null>(null);

const STORAGE_KEY_TOKEN = "gs_admin_token";
const STORAGE_KEY_REFRESH = "gs_admin_refresh";
const STORAGE_KEY_USER = "gs_admin_user";
const STORAGE_KEY_EXPIRES = "gs_admin_expires";

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [state, setState] = useState<AuthState>({
    user: null,
    token: null,
    isLoading: true,
  });

  // Restore session from localStorage on mount
  useEffect(() => {
    try {
      const token = localStorage.getItem(STORAGE_KEY_TOKEN);
      const userJson = localStorage.getItem(STORAGE_KEY_USER);
      const expiresAt = localStorage.getItem(STORAGE_KEY_EXPIRES);

      if (token && userJson && expiresAt) {
        const expires = new Date(expiresAt);
        if (expires > new Date()) {
          const user = JSON.parse(userJson) as AdminUser;
          setState({ user, token, isLoading: false });
          return;
        }
        // Token expired — try refresh
        const refreshToken = localStorage.getItem(STORAGE_KEY_REFRESH);
        if (refreshToken) {
          refreshSession(refreshToken);
          return;
        }
      }
    } catch {
      // Corrupted storage — clear it
      clearStorage();
    }
    setState((prev) => ({ ...prev, isLoading: false }));
  }, []);

  function clearStorage() {
    localStorage.removeItem(STORAGE_KEY_TOKEN);
    localStorage.removeItem(STORAGE_KEY_REFRESH);
    localStorage.removeItem(STORAGE_KEY_USER);
    localStorage.removeItem(STORAGE_KEY_EXPIRES);
  }

  function persistSession(user: AdminUser, tokens: AuthTokens) {
    localStorage.setItem(STORAGE_KEY_TOKEN, tokens.accessToken);
    localStorage.setItem(STORAGE_KEY_REFRESH, tokens.refreshToken);
    localStorage.setItem(STORAGE_KEY_USER, JSON.stringify(user));
    localStorage.setItem(STORAGE_KEY_EXPIRES, tokens.expiresAt);
  }

  async function refreshSession(refreshToken: string) {
    try {
      const res = await apiPost<LoginResponse>("/auth/refresh", { refreshToken });
      if (res.success && res.data) {
        persistSession(res.data.user, res.data.tokens);
        setState({
          user: res.data.user,
          token: res.data.tokens.accessToken,
          isLoading: false,
        });
        return;
      }
    } catch {
      // Refresh failed
    }
    clearStorage();
    setState({ user: null, token: null, isLoading: false });
  }

  const login = useCallback(
    async (
      email: string,
      password: string
    ): Promise<{ success: boolean; error?: string }> => {
      try {
        const res = await apiPost<LoginResponse>("/auth/login", {
          email,
          password,
        });
        if (res.success && res.data) {
          persistSession(res.data.user, res.data.tokens);
          setState({
            user: res.data.user,
            token: res.data.tokens.accessToken,
            isLoading: false,
          });
          return { success: true };
        }
        return {
          success: false,
          error: res.error?.message ?? "Invalid credentials",
        };
      } catch {
        return { success: false, error: "Network error. Please try again." };
      }
    },
    []
  );

  const logout = useCallback(() => {
    clearStorage();
    setState({ user: null, token: null, isLoading: false });
  }, []);

  const value = useMemo(
    () => ({ ...state, login, logout }),
    [state, login, logout]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return ctx;
}

"use client";

import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
} from "react";
import type { AdminUser, AuthTokens } from "./types";
import { apiPost } from "./api-client";

interface AuthState {
  user: AdminUser | null;
  token: string | null;
  isLoading: boolean;
}

interface AuthContextValue extends AuthState {
  login: (email: string, password: string) => Promise<{ success: boolean; error?: string }>;
  logout: () => void;
  isAdmin: () => boolean;
  isSupplier: () => boolean;
  isBuyer: () => boolean;
}

const AuthContext = createContext<AuthContextValue | null>(null);

const STORAGE_KEY_TOKEN = "gs_admin_token";
const STORAGE_KEY_REFRESH = "gs_admin_refresh";
const STORAGE_KEY_USER = "gs_admin_user";
const STORAGE_KEY_EXPIRES = "gs_admin_expires";

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

function decodeUserFromToken(accessToken: string): AdminUser {
  const payload = JSON.parse(atob(accessToken.split(".")[1]));
  return {
    id: payload.sub,
    email: payload.email,
    displayName: payload.displayName ?? payload.email.split("@")[0],
    role: payload.role,
    organizationName: payload.organizationName,
  };
}

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [state, setState] = useState<AuthState>({
    user: null,
    token: null,
    isLoading: true,
  });

  // Restore session from localStorage on mount
  useEffect(() => {
    async function restoreSession() {
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
          // Token expired -- try refresh
          const refreshToken = localStorage.getItem(STORAGE_KEY_REFRESH);
          if (refreshToken) {
            try {
              const res = await apiPost<{ accessToken: string; refreshToken: string; expiresAt: string }>("/auth/refresh", { refreshToken });
              if (res.success && res.data) {
                const user = decodeUserFromToken(res.data.accessToken);
                const tokens: AuthTokens = {
                  accessToken: res.data.accessToken,
                  refreshToken: res.data.refreshToken,
                  expiresAt: res.data.expiresAt,
                };
                persistSession(user, tokens);
                setState({
                  user,
                  token: tokens.accessToken,
                  isLoading: false,
                });
                return;
              }
            } catch {
              // Refresh failed
            }
            clearStorage();
          }
        }
      } catch {
        // Corrupted storage -- clear it
        clearStorage();
      }
      setState((prev) => ({ ...prev, isLoading: false }));
    }

    restoreSession();
  }, []);

  const login = useCallback(
    async (
      email: string,
      password: string
    ): Promise<{ success: boolean; error?: string }> => {
      try {
        const res = await apiPost<{ accessToken: string; refreshToken: string; expiresAt: string }>("/auth/login", {
          email,
          password,
        });
        if (res.success && res.data) {
          const user = decodeUserFromToken(res.data.accessToken);
          const tokens: AuthTokens = {
            accessToken: res.data.accessToken,
            refreshToken: res.data.refreshToken,
            expiresAt: res.data.expiresAt,
          };
          persistSession(user, tokens);
          setState({
            user,
            token: tokens.accessToken,
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

  const isAdmin = useCallback(
    () => state.user?.role === "admin",
    [state.user]
  );

  const isSupplier = useCallback(
    () =>
      state.user?.role === "supplier_admin" ||
      state.user?.role === "supplier_user",
    [state.user]
  );

  const isBuyer = useCallback(
    () => state.user?.role === "buyer",
    [state.user]
  );

  const value = useMemo(
    () => ({ ...state, login, logout, isAdmin, isSupplier, isBuyer }),
    [state, login, logout, isAdmin, isSupplier, isBuyer]
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

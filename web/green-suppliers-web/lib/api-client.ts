import type { ApiResponse } from "./types";

const API_BASE =
  process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000/api/v1";

/**
 * Handle 401/403 responses globally.
 * Clears stored auth tokens and redirects to login when the session has expired
 * or the user is not authorised for the requested resource.
 */
function handleAuthError(status: number): void {
  if (typeof window === "undefined") return;
  if (status === 401 || status === 403) {
    // Clear persisted session
    localStorage.removeItem("gs_admin_token");
    localStorage.removeItem("gs_admin_refresh");
    localStorage.removeItem("gs_admin_user");
    localStorage.removeItem("gs_admin_expires");
    // Redirect to login (preserve current path for return)
    const returnUrl = encodeURIComponent(window.location.pathname);
    window.location.href = `/admin/login?returnUrl=${returnUrl}`;
  }
}

export async function apiGet<T>(
  path: string,
  options?: { revalidate?: number }
): Promise<ApiResponse<T>> {
  try {
    const res = await fetch(`${API_BASE}${path}`, {
      next: { revalidate: options?.revalidate ?? 60 },
    });
    if (!res.ok) {
      const body = await res.json().catch(() => null);
      return (
        body ?? {
          success: false,
          data: null,
          error: { code: "FETCH_ERROR", message: res.statusText },
        }
      );
    }
    return res.json();
  } catch {
    return {
      success: false,
      data: null,
      error: { code: "NETWORK_ERROR", message: "Network error. Please check your connection and try again." },
    };
  }
}

export async function apiPost<T>(
  path: string,
  body: unknown,
  token?: string
): Promise<ApiResponse<T>> {
  try {
    const headers: Record<string, string> = {
      "Content-Type": "application/json",
    };
    if (token) headers["Authorization"] = `Bearer ${token}`;

    const res = await fetch(`${API_BASE}${path}`, {
      method: "POST",
      headers,
      body: JSON.stringify(body),
    });
    if (!res.ok) {
      if (token) handleAuthError(res.status);
      const body = await res.json().catch(() => null);
      return (
        body ?? {
          success: false,
          data: null,
          error: { code: "FETCH_ERROR", message: res.statusText },
        }
      );
    }
    return res.json();
  } catch {
    return {
      success: false,
      data: null,
      error: { code: "NETWORK_ERROR", message: "Network error. Please check your connection and try again." },
    };
  }
}

export async function apiPut<T>(
  path: string,
  body: unknown,
  token: string
): Promise<ApiResponse<T>> {
  try {
    const res = await fetch(`${API_BASE}${path}`, {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      },
      body: JSON.stringify(body),
    });
    if (!res.ok) {
      handleAuthError(res.status);
      const body = await res.json().catch(() => null);
      return (
        body ?? {
          success: false,
          data: null,
          error: { code: "FETCH_ERROR", message: res.statusText },
        }
      );
    }
    return res.json();
  } catch {
    return {
      success: false,
      data: null,
      error: { code: "NETWORK_ERROR", message: "Network error. Please check your connection and try again." },
    };
  }
}

export async function apiPatch<T>(
  path: string,
  body: unknown,
  token: string
): Promise<ApiResponse<T>> {
  try {
    const res = await fetch(`${API_BASE}${path}`, {
      method: "PATCH",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      },
      body: JSON.stringify(body),
    });
    if (!res.ok) {
      handleAuthError(res.status);
      const body = await res.json().catch(() => null);
      return (
        body ?? {
          success: false,
          data: null,
          error: { code: "FETCH_ERROR", message: res.statusText },
        }
      );
    }
    return res.json();
  } catch {
    return {
      success: false,
      data: null,
      error: { code: "NETWORK_ERROR", message: "Network error. Please check your connection and try again." },
    };
  }
}

export async function apiDelete<T>(
  path: string,
  token: string
): Promise<ApiResponse<T>> {
  try {
    const res = await fetch(`${API_BASE}${path}`, {
      method: "DELETE",
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });
    if (res.status === 204) {
      return { success: true, data: null, error: null };
    }
    if (!res.ok) {
      handleAuthError(res.status);
      const body = await res.json().catch(() => null);
      return (
        body ?? {
          success: false,
          data: null,
          error: { code: "FETCH_ERROR", message: res.statusText },
        }
      );
    }
    return res.json();
  } catch {
    return {
      success: false,
      data: null,
      error: { code: "NETWORK_ERROR", message: "Network error. Please check your connection and try again." },
    };
  }
}

export async function apiPostMultipart<T>(
  path: string,
  formData: FormData,
  token: string
): Promise<ApiResponse<T>> {
  try {
    const res = await fetch(`${API_BASE}${path}`, {
      method: "POST",
      headers: {
        Authorization: `Bearer ${token}`,
      },
      body: formData,
    });
    if (!res.ok) {
      handleAuthError(res.status);
      const body = await res.json().catch(() => null);
      return (
        body ?? {
          success: false,
          data: null,
          error: { code: "FETCH_ERROR", message: res.statusText },
        }
      );
    }
    return res.json();
  } catch {
    return {
      success: false,
      data: null,
      error: { code: "NETWORK_ERROR", message: "Network error. Please check your connection and try again." },
    };
  }
}

export async function apiGetAuth<T>(
  path: string,
  token: string
): Promise<ApiResponse<T>> {
  try {
    const res = await fetch(`${API_BASE}${path}`, {
      headers: {
        Authorization: `Bearer ${token}`,
      },
      cache: "no-store",
    });
    if (!res.ok) {
      handleAuthError(res.status);
      const body = await res.json().catch(() => null);
      return (
        body ?? {
          success: false,
          data: null,
          error: { code: "FETCH_ERROR", message: res.statusText },
        }
      );
    }
    return res.json();
  } catch {
    return {
      success: false,
      data: null,
      error: { code: "NETWORK_ERROR", message: "Network error. Please check your connection and try again." },
    };
  }
}

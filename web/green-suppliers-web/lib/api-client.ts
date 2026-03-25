import type { ApiResponse } from "./types";

const API_BASE =
  process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000/api/v1";

export async function apiGet<T>(
  path: string,
  options?: { revalidate?: number }
): Promise<ApiResponse<T>> {
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
}

export async function apiPost<T>(
  path: string,
  body: unknown,
  token?: string
): Promise<ApiResponse<T>> {
  const headers: Record<string, string> = {
    "Content-Type": "application/json",
  };
  if (token) headers["Authorization"] = `Bearer ${token}`;

  const res = await fetch(`${API_BASE}${path}`, {
    method: "POST",
    headers,
    body: JSON.stringify(body),
  });
  return res.json();
}

export async function apiPut<T>(
  path: string,
  body: unknown,
  token: string
): Promise<ApiResponse<T>> {
  const res = await fetch(`${API_BASE}${path}`, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
    },
    body: JSON.stringify(body),
  });
  return res.json();
}

export async function apiPatch<T>(
  path: string,
  body: unknown,
  token: string
): Promise<ApiResponse<T>> {
  const res = await fetch(`${API_BASE}${path}`, {
    method: "PATCH",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
    },
    body: JSON.stringify(body),
  });
  return res.json();
}

export async function apiDelete<T>(
  path: string,
  token: string
): Promise<ApiResponse<T>> {
  const res = await fetch(`${API_BASE}${path}`, {
    method: "DELETE",
    headers: {
      Authorization: `Bearer ${token}`,
    },
  });
  if (res.status === 204) {
    return { success: true, data: null, error: null };
  }
  return res.json();
}

export async function apiGetAuth<T>(
  path: string,
  token: string
): Promise<ApiResponse<T>> {
  const res = await fetch(`${API_BASE}${path}`, {
    headers: {
      Authorization: `Bearer ${token}`,
    },
    cache: "no-store",
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
}

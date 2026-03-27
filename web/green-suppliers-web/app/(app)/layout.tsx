"use client";

import { useCallback, useEffect, useState } from "react";
import { usePathname, useRouter } from "next/navigation";
import Link from "next/link";
import { AuthProvider, useAuth } from "@/lib/auth-context";
import { apiGetAuth } from "@/lib/api-client";
import { Button } from "@/components/ui/button";
import {
  LayoutDashboard,
  User,
  Inbox,
  BarChart3,
  ShieldCheck,
  CreditCard,
  Settings,
  LogOut,
  Leaf,
  Menu,
  X,
} from "lucide-react";
import { cn } from "@/lib/utils";

interface NavItem {
  href: string;
  label: string;
  icon: React.ComponentType<{ className?: string }>;
  badge?: number;
}

const BASE_NAV_ITEMS: NavItem[] = [
  { href: "/dashboard", label: "Overview", icon: LayoutDashboard },
  { href: "/dashboard/leads", label: "Leads", icon: Inbox },
  { href: "/dashboard/analytics", label: "Analytics", icon: BarChart3 },
  { href: "/dashboard/profile", label: "Profile", icon: User },
  { href: "/dashboard/certifications", label: "Certifications", icon: ShieldCheck },
  { href: "/dashboard/billing", label: "Billing", icon: CreditCard },
  { href: "/dashboard/settings", label: "Settings", icon: Settings },
];

function SupplierDashboardShell({ children }: { children: React.ReactNode }) {
  const { user, token, isLoading, logout, isSupplier } = useAuth();
  const pathname = usePathname();
  const router = useRouter();
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const [newLeadCount, setNewLeadCount] = useState(0);
  const [menuPathname, setMenuPathname] = useState(pathname);

  // Close sidebar on route change: when pathname changes from the one
  // that was current when the sidebar was opened, the sidebar becomes closed.
  const isSidebarOpen = sidebarOpen && menuPathname === pathname;

  // Fetch new lead count for badge
  useEffect(() => {
    if (!token) return;
    async function fetchLeadCount() {
      const res = await apiGetAuth<{ newLeads: number }>(
        "/supplier/me/dashboard",
        token!
      );
      if (res.success && res.data) {
        setNewLeadCount(res.data.newLeads);
      }
    }
    fetchLeadCount();
  }, [token]);

  const NAV_ITEMS: NavItem[] = BASE_NAV_ITEMS.map((item) =>
    item.href === "/dashboard/leads" && newLeadCount > 0
      ? { ...item, badge: newLeadCount }
      : item
  );

  // Close sidebar on Escape key
  const handleKeyDown = useCallback(
    (e: KeyboardEvent) => {
      if (e.key === "Escape" && isSidebarOpen) {
        setSidebarOpen(false);
      }
    },
    [isSidebarOpen]
  );

  useEffect(() => {
    document.addEventListener("keydown", handleKeyDown);
    return () => document.removeEventListener("keydown", handleKeyDown);
  }, [handleKeyDown]);

useEffect(() => {
    if (!isLoading && !token) {
      router.replace("/admin/login");
    }
  }, [isLoading, token, router]);

  useEffect(() => {
    if (!isLoading && token && !isSupplier()) {
      router.replace("/admin/login");
    }
  }, [isLoading, token, isSupplier, router]);

  if (isLoading) {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <div
          role="status"
          aria-label="Loading dashboard"
          className="flex flex-col items-center gap-3"
        >
          <div className="h-8 w-8 animate-spin rounded-full border-4 border-brand-green border-t-transparent" />
          <p className="text-sm text-muted-foreground">Loading...</p>
        </div>
      </div>
    );
  }

  if (!token) {
    return null;
  }

  function handleLogout() {
    logout();
    router.replace("/admin/login");
  }

  return (
    <div className="flex min-h-screen">
      {/* Mobile overlay */}
      {isSidebarOpen && (
        <div
          className="fixed inset-0 z-40 bg-black/50 lg:hidden"
          aria-hidden="true"
          onClick={() => setSidebarOpen(false)}
        />
      )}

      {/* Sidebar */}
      <aside
        aria-label="Supplier dashboard navigation"
        className={cn(
          "fixed inset-y-0 left-0 z-50 flex w-60 flex-col bg-brand-dark transition-transform duration-200 lg:static lg:translate-x-0",
          isSidebarOpen ? "translate-x-0" : "-translate-x-full"
        )}
      >
        {/* Logo */}
        <div className="flex h-14 items-center gap-2 border-b border-white/10 px-4">
          <div className="flex h-8 w-8 items-center justify-center rounded-md bg-brand-green" aria-hidden="true">
            <Leaf className="h-4 w-4 text-white" />
          </div>
          <span className="text-sm font-semibold text-white">
            Supplier Portal
          </span>
          <button
            className="ml-auto rounded-md p-1 text-white/60 transition-colors hover:text-white focus-visible:ring-2 focus-visible:ring-white/50 lg:hidden"
            onClick={() => setSidebarOpen(false)}
            aria-label="Close navigation menu"
          >
            <X className="h-5 w-5" />
          </button>
        </div>

        {/* Navigation */}
        <nav aria-label="Dashboard" className="flex-1 space-y-1 px-3 py-4">
          {NAV_ITEMS.map((item) => {
            const isActive =
              item.href === "/dashboard"
                ? pathname === "/dashboard"
                : pathname.startsWith(item.href);
            return (
              <Link
                key={item.href}
                href={item.href}
                aria-current={isActive ? "page" : undefined}
                className={cn(
                  "flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium transition-colors focus-visible:ring-2 focus-visible:ring-white/50",
                  isActive
                    ? "bg-brand-green text-white"
                    : "text-white/60 hover:bg-white/10 hover:text-white"
                )}
              >
                <item.icon className="h-4 w-4 shrink-0" aria-hidden="true" />
                {item.label}
                {item.badge != null && item.badge > 0 && (
                  <span className="ml-auto inline-flex h-5 min-w-5 items-center justify-center rounded-full bg-blue-500 px-1.5 text-[10px] font-bold text-white">
                    {item.badge}
                  </span>
                )}
              </Link>
            );
          })}
        </nav>

        {/* User / Logout */}
        <div className="border-t border-white/10 p-3">
          <div className="mb-2 px-3 text-xs text-white/40">
            Signed in as
          </div>
          <div className="mb-3 px-3 text-sm font-medium text-white/80 truncate">
            {user?.displayName ?? user?.email ?? "Supplier"}
          </div>
          <button
            onClick={handleLogout}
            className="flex w-full items-center gap-3 rounded-md px-3 py-2 text-sm font-medium text-white/60 transition-colors hover:bg-white/10 hover:text-white focus-visible:ring-2 focus-visible:ring-white/50"
          >
            <LogOut className="h-4 w-4 shrink-0" aria-hidden="true" />
            Sign out
          </button>
        </div>
      </aside>

      {/* Main content area */}
      <div className="flex flex-1 flex-col">
        {/* Top bar */}
        <header className="flex h-14 items-center gap-4 border-b bg-white px-4 lg:px-6">
          <button
            className="rounded-md p-1 text-muted-foreground transition-colors hover:text-foreground focus-visible:ring-2 focus-visible:ring-ring lg:hidden"
            onClick={() => {
              setMenuPathname(pathname);
              setSidebarOpen(true);
            }}
            aria-label="Open navigation menu"
            aria-expanded={isSidebarOpen}
            aria-controls="sidebar-nav"
          >
            <Menu className="h-5 w-5" />
          </button>
          <div className="flex-1" />
          <div className="flex items-center gap-3">
            <div className="hidden text-right text-sm sm:block">
              <div className="font-medium">
                {user?.displayName ?? "Supplier"}
              </div>
              <div className="text-xs text-muted-foreground capitalize">
                {user?.role?.replace("_", " ") ?? "supplier"}
              </div>
            </div>
            <Button variant="outline" size="sm" onClick={handleLogout}>
              <LogOut className="mr-1 h-3 w-3" aria-hidden="true" />
              Logout
            </Button>
          </div>
        </header>

        {/* Page content */}
        <main className="flex-1 overflow-y-auto bg-muted/30 p-4 lg:p-6">
          {children}
        </main>
      </div>
    </div>
  );
}

export default function SupplierDashboardLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <AuthProvider>
      <SupplierDashboardShell>{children}</SupplierDashboardShell>
    </AuthProvider>
  );
}

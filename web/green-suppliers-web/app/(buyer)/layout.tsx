"use client";

import { useCallback, useEffect, useState } from "react";
import { usePathname, useRouter } from "next/navigation";
import Link from "next/link";
import { AuthProvider, useAuth } from "@/lib/auth-context";
import {
  LayoutDashboard,
  Bookmark,
  MessageSquare,
  Settings,
  LogOut,
  Leaf,
  Menu,
  X,
  ChevronDown,
} from "lucide-react";
import { cn } from "@/lib/utils";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";

const NAV_ITEMS = [
  { href: "/buyer", label: "Dashboard", icon: LayoutDashboard, exact: true },
  { href: "/buyer/saved", label: "Saved Suppliers", icon: Bookmark, exact: false },
  { href: "/buyer/leads", label: "My Inquiries", icon: MessageSquare, exact: false },
  { href: "/buyer/settings", label: "Settings", icon: Settings, exact: false },
] as const;

function BuyerDashboardShell({ children }: { children: React.ReactNode }) {
  const { user, token, isLoading, logout, isBuyer } = useAuth();
  const pathname = usePathname();
  const router = useRouter();
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);

  const handleKeyDown = useCallback(
    (e: KeyboardEvent) => {
      if (e.key === "Escape" && mobileMenuOpen) {
        setMobileMenuOpen(false);
      }
    },
    [mobileMenuOpen]
  );

  useEffect(() => {
    document.addEventListener("keydown", handleKeyDown);
    return () => document.removeEventListener("keydown", handleKeyDown);
  }, [handleKeyDown]);

  // Close mobile menu on route change
  useEffect(() => {
    setMobileMenuOpen(false);
  }, [pathname]);

  // Auth guard: redirect if not authenticated
  useEffect(() => {
    if (!isLoading && !token) {
      router.replace("/admin/login");
    }
  }, [isLoading, token, router]);

  // Auth guard: redirect if not buyer role
  useEffect(() => {
    if (!isLoading && token && !isBuyer()) {
      router.replace("/admin/login");
    }
  }, [isLoading, token, isBuyer, router]);

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
    <div className="flex min-h-screen flex-col">
      {/* Horizontal top navigation */}
      <header className="sticky top-0 z-50 border-b bg-white shadow-sm">
        <div className="mx-auto flex h-14 max-w-7xl items-center justify-between px-4 lg:px-6">
          {/* Left: Logo + brand */}
          <div className="flex items-center gap-3">
            <Link href="/buyer" className="flex items-center gap-2">
              <div
                className="flex h-8 w-8 items-center justify-center rounded-lg bg-brand-green"
                aria-hidden="true"
              >
                <Leaf className="h-4 w-4 text-white" />
              </div>
              <span className="text-sm font-semibold text-brand-dark">
                Buyer Portal
              </span>
            </Link>
          </div>

          {/* Center: Desktop nav links */}
          <nav
            aria-label="Buyer dashboard navigation"
            className="hidden items-center gap-1 md:flex"
          >
            {NAV_ITEMS.map((item) => {
              const isActive = item.exact
                ? pathname === item.href
                : pathname.startsWith(item.href);
              return (
                <Link
                  key={item.href}
                  href={item.href}
                  aria-current={isActive ? "page" : undefined}
                  className={cn(
                    "flex items-center gap-2 rounded-lg px-3 py-2 text-sm font-medium transition-colors",
                    isActive
                      ? "bg-brand-green-light text-brand-green"
                      : "text-muted-foreground hover:bg-muted hover:text-foreground"
                  )}
                >
                  <item.icon className="h-4 w-4" aria-hidden="true" />
                  {item.label}
                </Link>
              );
            })}
          </nav>

          {/* Right: User dropdown + mobile burger */}
          <div className="flex items-center gap-3">
            {/* User dropdown (desktop) */}
            <div className="hidden md:block">
              <DropdownMenu>
                <DropdownMenuTrigger className="flex items-center gap-2 rounded-lg px-3 py-2 text-sm font-medium text-muted-foreground transition-colors hover:bg-muted hover:text-foreground focus-visible:ring-2 focus-visible:ring-ring">
                  <div className="flex h-7 w-7 items-center justify-center rounded-full bg-brand-green text-xs font-bold text-white">
                    {user?.displayName
                      ? user.displayName
                          .split(" ")
                          .map((n) => n[0])
                          .join("")
                          .slice(0, 2)
                          .toUpperCase()
                      : user?.email?.[0]?.toUpperCase() ?? "B"}
                  </div>
                  <span className="max-w-[120px] truncate">
                    {user?.displayName ?? user?.email ?? "Buyer"}
                  </span>
                  <ChevronDown className="h-3 w-3" aria-hidden="true" />
                </DropdownMenuTrigger>
                <DropdownMenuContent align="end" sideOffset={8} className="w-56">
                  <div className="px-2 py-1.5">
                    <p className="text-sm font-medium">
                      {user?.displayName ?? "Buyer"}
                    </p>
                    <p className="text-xs text-muted-foreground">
                      {user?.email}
                    </p>
                  </div>
                  <DropdownMenuSeparator />
                  <DropdownMenuItem
                    className="cursor-pointer"
                    onClick={() => router.push("/buyer/settings")}
                  >
                    <Settings className="mr-2 h-4 w-4" />
                    Settings
                  </DropdownMenuItem>
                  <DropdownMenuSeparator />
                  <DropdownMenuItem
                    className="cursor-pointer text-red-600 focus:text-red-600"
                    onClick={handleLogout}
                  >
                    <LogOut className="mr-2 h-4 w-4" />
                    Sign Out
                  </DropdownMenuItem>
                </DropdownMenuContent>
              </DropdownMenu>
            </div>

            {/* Mobile hamburger */}
            <button
              className="rounded-lg p-2 text-muted-foreground transition-colors hover:bg-muted hover:text-foreground md:hidden"
              onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
              aria-label={mobileMenuOpen ? "Close menu" : "Open menu"}
              aria-expanded={mobileMenuOpen}
            >
              {mobileMenuOpen ? (
                <X className="h-5 w-5" />
              ) : (
                <Menu className="h-5 w-5" />
              )}
            </button>
          </div>
        </div>

        {/* Mobile nav menu */}
        {mobileMenuOpen && (
          <nav
            aria-label="Buyer dashboard mobile navigation"
            className="border-t bg-white px-4 pb-4 pt-2 md:hidden"
          >
            <div className="space-y-1">
              {NAV_ITEMS.map((item) => {
                const isActive = item.exact
                  ? pathname === item.href
                  : pathname.startsWith(item.href);
                return (
                  <Link
                    key={item.href}
                    href={item.href}
                    aria-current={isActive ? "page" : undefined}
                    className={cn(
                      "flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium transition-colors",
                      isActive
                        ? "bg-brand-green-light text-brand-green"
                        : "text-muted-foreground hover:bg-muted hover:text-foreground"
                    )}
                  >
                    <item.icon className="h-4 w-4" aria-hidden="true" />
                    {item.label}
                  </Link>
                );
              })}
            </div>
            <div className="mt-3 border-t pt-3">
              <div className="mb-2 px-3 text-xs text-muted-foreground">
                Signed in as {user?.displayName ?? user?.email ?? "Buyer"}
              </div>
              <button
                onClick={handleLogout}
                className="flex w-full items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium text-red-600 transition-colors hover:bg-red-50"
              >
                <LogOut className="h-4 w-4" aria-hidden="true" />
                Sign Out
              </button>
            </div>
          </nav>
        )}
      </header>

      {/* Page content */}
      <main className="flex-1 bg-muted/30">
        <div className="mx-auto max-w-7xl p-4 lg:p-6">{children}</div>
      </main>
    </div>
  );
}

export default function BuyerDashboardLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <AuthProvider>
      <BuyerDashboardShell>{children}</BuyerDashboardShell>
    </AuthProvider>
  );
}

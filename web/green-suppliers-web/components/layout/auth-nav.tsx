"use client";

import * as React from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useAuth } from "@/lib/auth-context";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { LogOut, User, LayoutDashboard, ShieldCheck } from "lucide-react";

export function AuthNav() {
  const { user, token, isLoading, logout, isAdmin, isSupplier, isBuyer } =
    useAuth();
  const router = useRouter();

  function handleLogout() {
    logout();
    router.push("/");
  }

  // Still loading auth state -- render nothing to avoid flash
  if (isLoading) {
    return (
      <div className="flex items-center gap-3">
        <div className="h-8 w-20 animate-pulse rounded-lg bg-white/10" />
      </div>
    );
  }

  // Not authenticated
  if (!token || !user) {
    return (
      <div className="flex items-center gap-3">
        <Link
          href="/admin/login"
          className="text-sm font-medium text-white/70 transition-colors hover:text-white"
        >
          Sign In
        </Link>
        <Link href="/register">
          <Button className="bg-gradient-to-r from-brand-green to-brand-emerald text-white font-semibold shadow-md hover:from-brand-green-hover hover:to-brand-emerald-hover">
            Register
          </Button>
        </Link>
      </div>
    );
  }

  // Authenticated -- determine dashboard link
  let dashboardHref = "/admin";
  let dashboardLabel = "Admin";
  let dashboardIcon = <ShieldCheck className="mr-2 h-4 w-4" />;

  if (isSupplier()) {
    dashboardHref = "/app";
    dashboardLabel = "Dashboard";
    dashboardIcon = <LayoutDashboard className="mr-2 h-4 w-4" />;
  } else if (isBuyer()) {
    dashboardHref = "/buyer";
    dashboardLabel = "Dashboard";
    dashboardIcon = <LayoutDashboard className="mr-2 h-4 w-4" />;
  } else if (isAdmin()) {
    dashboardHref = "/admin";
    dashboardLabel = "Admin";
    dashboardIcon = <ShieldCheck className="mr-2 h-4 w-4" />;
  }

  const initials = user.displayName
    ? user.displayName
        .split(" ")
        .map((n) => n[0])
        .join("")
        .slice(0, 2)
        .toUpperCase()
    : user.email[0].toUpperCase();

  return (
    <div className="flex items-center gap-2">
      <Link
        href={dashboardHref}
        className="hidden text-sm font-medium text-white/70 transition-colors hover:text-white sm:block"
      >
        My Account
      </Link>
      <DropdownMenu>
        <DropdownMenuTrigger
          className="flex h-8 w-8 items-center justify-center rounded-full bg-white/20 text-xs font-bold text-white transition-colors hover:bg-white/30 focus-visible:ring-2 focus-visible:ring-white focus-visible:ring-offset-2 focus-visible:ring-offset-brand-green-dark"
          aria-label={`Account menu for ${user.displayName || user.email}`}
        >
          {initials}
        </DropdownMenuTrigger>
        <DropdownMenuContent align="end" sideOffset={8} className="w-56">
          <DropdownMenuLabel>
            <div className="flex flex-col">
              <span className="text-sm font-medium">
                {user.displayName || user.email}
              </span>
              <span className="text-xs text-muted-foreground">
                {user.email}
              </span>
            </div>
          </DropdownMenuLabel>
          <DropdownMenuSeparator />
          <DropdownMenuItem
            className="cursor-pointer"
            onClick={() => router.push(dashboardHref)}
          >
            {dashboardIcon}
            {dashboardLabel}
          </DropdownMenuItem>
          <DropdownMenuItem
            className="cursor-pointer"
            onClick={() => router.push(`${dashboardHref}/profile`)}
          >
            <User className="mr-2 h-4 w-4" />
            Profile
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
  );
}

/**
 * Mobile version of auth nav for the sheet/sidebar menu
 */
export function AuthNavMobile({ onNavigate }: { onNavigate?: () => void }) {
  const { user, token, isLoading, logout, isAdmin, isSupplier, isBuyer } =
    useAuth();
  const router = useRouter();

  function handleLogout() {
    logout();
    onNavigate?.();
    router.push("/");
  }

  if (isLoading) {
    return (
      <div className="h-8 w-full animate-pulse rounded-lg bg-white/10" />
    );
  }

  if (!token || !user) {
    return (
      <>
        <Link
          href="/admin/login"
          onClick={onNavigate}
          className="block rounded-lg px-3 py-2.5 text-sm font-medium text-white/70 transition-colors hover:bg-white/10 hover:text-white"
        >
          Sign In
        </Link>
        <Link href="/register" onClick={onNavigate} className="mt-2 block">
          <Button className="w-full bg-gradient-to-r from-brand-green to-brand-emerald text-white font-semibold">
            Register
          </Button>
        </Link>
      </>
    );
  }

  let dashboardHref = "/admin";
  let dashboardLabel = "Admin";

  if (isSupplier()) {
    dashboardHref = "/app";
    dashboardLabel = "Dashboard";
  } else if (isBuyer()) {
    dashboardHref = "/buyer";
    dashboardLabel = "Dashboard";
  } else if (isAdmin()) {
    dashboardHref = "/admin";
    dashboardLabel = "Admin";
  }

  return (
    <>
      <div className="px-3 text-xs text-white/40">
        Signed in as {user.displayName || user.email}
      </div>
      <Link
        href={dashboardHref}
        onClick={onNavigate}
        className="block rounded-lg px-3 py-2.5 text-sm font-medium text-white/70 transition-colors hover:bg-white/10 hover:text-white"
      >
        {dashboardLabel}
      </Link>
      <button
        onClick={handleLogout}
        className="block w-full rounded-lg px-3 py-2.5 text-left text-sm font-medium text-white/70 transition-colors hover:bg-white/10 hover:text-white"
      >
        Sign Out
      </button>
    </>
  );
}

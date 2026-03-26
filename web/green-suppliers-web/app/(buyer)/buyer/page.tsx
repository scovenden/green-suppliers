"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { useAuth } from "@/lib/auth-context";
import { apiGetAuth } from "@/lib/api-client";
import type { BuyerDashboardStats } from "@/lib/types";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import {
  Bookmark,
  Send,
  MessageSquareCheck,
  Search,
  Heart,
  ArrowRight,
  AlertTriangle,
} from "lucide-react";

function StatCard({
  label,
  value,
  icon: Icon,
  accent = false,
}: {
  label: string;
  value: React.ReactNode;
  icon: React.ComponentType<{ className?: string }>;
  accent?: boolean;
}) {
  return (
    <div className="rounded-2xl border bg-white p-5 shadow-sm">
      <div className="flex items-start justify-between">
        <div>
          <p className="text-xs font-medium uppercase tracking-wide text-muted-foreground">
            {label}
          </p>
          <div className="mt-2 text-2xl font-bold text-foreground">{value}</div>
        </div>
        <div
          aria-hidden="true"
          className={
            accent
              ? "flex h-10 w-10 items-center justify-center rounded-xl bg-brand-green/10"
              : "flex h-10 w-10 items-center justify-center rounded-xl bg-muted/50"
          }
        >
          <Icon
            className={
              accent
                ? "h-5 w-5 text-brand-green"
                : "h-5 w-5 text-muted-foreground"
            }
          />
        </div>
      </div>
    </div>
  );
}

function DashboardSkeleton() {
  return (
    <div role="status" aria-label="Loading dashboard" className="space-y-6">
      <div className="flex items-center gap-4">
        <Skeleton className="h-8 w-56" />
      </div>
      <div className="grid gap-4 sm:grid-cols-3">
        {Array.from({ length: 3 }).map((_, i) => (
          <Skeleton key={i} className="h-28 rounded-2xl" />
        ))}
      </div>
      <Skeleton className="h-40 rounded-2xl" />
      <span className="sr-only">Loading dashboard data</span>
    </div>
  );
}

export default function BuyerDashboardPage() {
  const { user, token } = useAuth();
  const [stats, setStats] = useState<BuyerDashboardStats | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!token) return;

    async function fetchDashboard() {
      setLoading(true);
      setError(null);
      const res = await apiGetAuth<BuyerDashboardStats>(
        "/buyer/me/dashboard",
        token!
      );
      if (res.success && res.data) {
        setStats(res.data);
      } else {
        setError(res.error?.message ?? "Failed to load dashboard");
      }
      setLoading(false);
    }

    fetchDashboard();
  }, [token]);

  if (loading) {
    return <DashboardSkeleton />;
  }

  if (error || !stats) {
    return (
      <div
        role="alert"
        className="flex flex-col items-center justify-center gap-4 py-20"
      >
        <AlertTriangle
          className="h-10 w-10 text-destructive"
          aria-hidden="true"
        />
        <p className="text-sm text-muted-foreground">
          {error ?? "Something went wrong"}
        </p>
        <Button variant="outline" onClick={() => window.location.reload()}>
          Try Again
        </Button>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Welcome */}
      <div>
        <h1 className="text-2xl font-bold tracking-tight">
          Welcome back{user?.displayName ? `, ${user.displayName}` : ""}
        </h1>
        <p className="text-sm text-muted-foreground">
          Your procurement dashboard at a glance
        </p>
      </div>

      {/* Stat cards */}
      <div className="grid gap-4 sm:grid-cols-3">
        <StatCard
          label="Saved Suppliers"
          value={stats.savedSupplierCount}
          icon={Bookmark}
          accent
        />
        <StatCard
          label="Inquiries Sent"
          value={stats.inquirySentCount}
          icon={Send}
          accent={stats.inquirySentCount > 0}
        />
        <StatCard
          label="Inquiries Responded"
          value={stats.inquiryRespondedCount}
          icon={MessageSquareCheck}
          accent={stats.inquiryRespondedCount > 0}
        />
      </div>

      {/* Quick actions */}
      <div className="rounded-2xl border bg-white p-6 shadow-sm">
        <h2 className="mb-4 text-sm font-semibold uppercase tracking-wide text-muted-foreground">
          Quick Actions
        </h2>
        <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
          <Link
            href="/suppliers"
            className="flex items-center gap-3 rounded-xl border p-4 transition-colors hover:border-brand-green hover:bg-brand-green-light"
          >
            <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-brand-green/10">
              <Search className="h-5 w-5 text-brand-green" />
            </div>
            <div>
              <p className="text-sm font-medium">Find Suppliers</p>
              <p className="text-xs text-muted-foreground">
                Browse our verified directory
              </p>
            </div>
          </Link>

          <Link
            href="/buyer/saved"
            className="flex items-center gap-3 rounded-xl border p-4 transition-colors hover:border-brand-green hover:bg-brand-green-light"
          >
            <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-brand-green/10">
              <Heart className="h-5 w-5 text-brand-green" />
            </div>
            <div>
              <p className="text-sm font-medium">View Saved</p>
              <p className="text-xs text-muted-foreground">
                {stats.savedSupplierCount} supplier
                {stats.savedSupplierCount !== 1 ? "s" : ""} saved
              </p>
            </div>
          </Link>

          <Link
            href="/buyer/leads"
            className="flex items-center gap-3 rounded-xl border p-4 transition-colors hover:border-brand-green hover:bg-brand-green-light"
          >
            <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-brand-green/10">
              <Send className="h-5 w-5 text-brand-green" />
            </div>
            <div>
              <p className="text-sm font-medium">My Inquiries</p>
              <p className="text-xs text-muted-foreground">
                Track your conversations
              </p>
            </div>
          </Link>
        </div>
      </div>

      {/* Tips / CTA */}
      {stats.savedSupplierCount === 0 && stats.inquirySentCount === 0 && (
        <div className="rounded-2xl border border-brand-green/20 bg-brand-green-light p-6">
          <div className="flex flex-col items-start gap-4 sm:flex-row sm:items-center sm:justify-between">
            <div>
              <h3 className="text-sm font-semibold text-brand-green-dark">
                Get started with Green Suppliers
              </h3>
              <p className="mt-1 text-sm text-brand-earth">
                Search our directory to find verified sustainable suppliers.
                Save your favourites and send inquiries directly.
              </p>
            </div>
            <Link href="/suppliers">
              <Button className="shrink-0">
                Browse Suppliers
                <ArrowRight className="ml-1 h-4 w-4" />
              </Button>
            </Link>
          </div>
        </div>
      )}
    </div>
  );
}

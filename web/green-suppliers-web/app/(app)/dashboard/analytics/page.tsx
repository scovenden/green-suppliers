"use client";

import { useEffect, useState } from "react";
import { useAuth } from "@/lib/auth-context";
import { apiGetAuth } from "@/lib/api-client";
import type { ProfileAnalytics } from "@/lib/types";
import { Skeleton } from "@/components/ui/skeleton";
import { Button } from "@/components/ui/button";
import {
  Eye,
  TrendingUp,
  TrendingDown,
  Inbox,
  Search,
  AlertTriangle,
  RefreshCw,
} from "lucide-react";
import {
  AreaChart,
  Area,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
} from "recharts";

// ---------------------------------------------------------------------------
// Loading skeleton
// ---------------------------------------------------------------------------

function AnalyticsSkeleton() {
  return (
    <div role="status" aria-label="Loading analytics" className="space-y-6">
      <Skeleton className="h-8 w-64" />
      <div className="grid gap-4 sm:grid-cols-3">
        <Skeleton className="h-28 rounded-2xl" />
        <Skeleton className="h-28 rounded-2xl" />
        <Skeleton className="h-28 rounded-2xl" />
      </div>
      <Skeleton className="h-72 rounded-2xl" />
      <Skeleton className="h-72 rounded-2xl" />
      <span className="sr-only">Loading analytics data</span>
    </div>
  );
}

// ---------------------------------------------------------------------------
// Stat card component
// ---------------------------------------------------------------------------

function StatCard({
  icon,
  label,
  value,
  subtitle,
  trend,
}: {
  icon: React.ReactNode;
  label: string;
  value: string | number;
  subtitle?: string;
  trend?: { value: number; label: string };
}) {
  return (
    <div className="flex flex-col gap-3 rounded-2xl border bg-white p-5 shadow-sm">
      <div className="flex items-center justify-between">
        <span className="text-sm font-medium text-muted-foreground">
          {label}
        </span>
        <div className="flex h-9 w-9 items-center justify-center rounded-xl bg-brand-green-light">
          {icon}
        </div>
      </div>
      <div>
        <p className="text-2xl font-bold tracking-tight">{value}</p>
        {subtitle && (
          <p className="text-xs text-muted-foreground">{subtitle}</p>
        )}
      </div>
      {trend && (
        <div
          className={`flex items-center gap-1 text-xs font-medium ${
            trend.value >= 0 ? "text-green-600" : "text-red-500"
          }`}
        >
          {trend.value >= 0 ? (
            <TrendingUp className="h-3.5 w-3.5" />
          ) : (
            <TrendingDown className="h-3.5 w-3.5" />
          )}
          {trend.value >= 0 ? "+" : ""}
          {trend.value}% {trend.label}
        </div>
      )}
    </div>
  );
}

// ---------------------------------------------------------------------------
// Custom tooltip for charts
// ---------------------------------------------------------------------------

interface TooltipPayload {
  name?: string;
  value?: number;
  color?: string;
}

function ChartTooltip({
  active,
  payload,
  label,
}: {
  active?: boolean;
  payload?: TooltipPayload[];
  label?: string;
}) {
  if (!active || !payload?.length) return null;
  return (
    <div className="rounded-lg border bg-white px-3 py-2 shadow-md">
      <p className="text-xs font-medium text-muted-foreground">{label}</p>
      {payload.map((entry, i) => (
        <p key={i} className="text-sm font-semibold" style={{ color: entry.color }}>
          {entry.value}
        </p>
      ))}
    </div>
  );
}

// ---------------------------------------------------------------------------
// Empty state
// ---------------------------------------------------------------------------

function EmptyState() {
  return (
    <div className="flex flex-col items-center justify-center gap-3 rounded-2xl border bg-white py-16 shadow-sm">
      <div className="flex h-14 w-14 items-center justify-center rounded-2xl bg-muted">
        <Eye className="h-7 w-7 text-muted-foreground" />
      </div>
      <h3 className="text-base font-semibold">No Analytics Data Yet</h3>
      <p className="max-w-sm text-center text-sm text-muted-foreground">
        Analytics data will appear here once your profile starts receiving views
        and leads. Make sure your profile is published to start collecting data.
      </p>
    </div>
  );
}

// ---------------------------------------------------------------------------
// Main analytics page
// ---------------------------------------------------------------------------

export default function AnalyticsPage() {
  const { token } = useAuth();
  const [analytics, setAnalytics] = useState<ProfileAnalytics | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!token) return;
    async function fetchAnalytics() {
      setLoading(true);
      setError(null);
      const res = await apiGetAuth<ProfileAnalytics>(
        "/supplier/me/analytics",
        token!
      );
      if (res.success && res.data) {
        setAnalytics(res.data);
      } else {
        setError(res.error?.message ?? "Failed to load analytics");
      }
      setLoading(false);
    }
    fetchAnalytics();
  }, [token]);

  if (loading) return <AnalyticsSkeleton />;

  if (error) {
    return (
      <div
        role="alert"
        className="flex flex-col items-center justify-center gap-4 py-20"
      >
        <AlertTriangle
          className="h-10 w-10 text-destructive"
          aria-hidden="true"
        />
        <p className="text-sm text-muted-foreground">{error}</p>
        <Button
          variant="outline"
          onClick={() => window.location.reload()}
        >
          <RefreshCw className="mr-2 h-4 w-4" />
          Try Again
        </Button>
      </div>
    );
  }

  if (!analytics || (analytics.totalViews === 0 && analytics.totalLeads === 0)) {
    return (
      <div className="space-y-6">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Analytics</h1>
          <p className="text-sm text-muted-foreground">
            Track your profile performance and lead generation
          </p>
        </div>
        <EmptyState />
      </div>
    );
  }

  // Calculate view trend
  const viewTrend =
    analytics.viewsLastMonth > 0
      ? Math.round(
          ((analytics.viewsThisMonth - analytics.viewsLastMonth) /
            analytics.viewsLastMonth) *
            100
        )
      : analytics.viewsThisMonth > 0
      ? 100
      : 0;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Analytics</h1>
        <p className="text-sm text-muted-foreground">
          Track your profile performance and lead generation
        </p>
      </div>

      {/* Stat cards */}
      <div className="grid gap-4 sm:grid-cols-3">
        <StatCard
          icon={<Eye className="h-4 w-4 text-brand-green" />}
          label="Profile Views"
          value={analytics.totalViews.toLocaleString()}
          subtitle={`${analytics.viewsThisMonth} this month`}
          trend={{
            value: viewTrend,
            label: "vs last month",
          }}
        />
        <StatCard
          icon={<Inbox className="h-4 w-4 text-brand-green" />}
          label="Total Leads"
          value={analytics.totalLeads.toLocaleString()}
          subtitle="All time"
        />
        <StatCard
          icon={<Search className="h-4 w-4 text-brand-green" />}
          label="Search Appearances"
          value={analytics.searchAppearances.toLocaleString()}
          subtitle="Times shown in search results"
        />
      </div>

      {/* Views chart */}
      {analytics.viewsByDay.length > 0 && (
        <div className="rounded-2xl border bg-white p-5 shadow-sm">
          <h2 className="mb-4 text-base font-semibold">
            Profile Views (Last 30 Days)
          </h2>
          <div className="h-72">
            <ResponsiveContainer width="100%" height="100%">
              <AreaChart
                data={analytics.viewsByDay}
                margin={{ top: 5, right: 20, left: 0, bottom: 5 }}
              >
                <defs>
                  <linearGradient id="viewsGradient" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="5%" stopColor="#16A34A" stopOpacity={0.3} />
                    <stop offset="95%" stopColor="#059669" stopOpacity={0.05} />
                  </linearGradient>
                </defs>
                <CartesianGrid
                  strokeDasharray="3 3"
                  stroke="#f0f0f0"
                  vertical={false}
                />
                <XAxis
                  dataKey="date"
                  tick={{ fontSize: 11, fill: "#9CA3AF" }}
                  tickLine={false}
                  axisLine={false}
                  tickFormatter={(val: string) => {
                    const d = new Date(val);
                    return `${d.getDate()}/${d.getMonth() + 1}`;
                  }}
                />
                <YAxis
                  tick={{ fontSize: 11, fill: "#9CA3AF" }}
                  tickLine={false}
                  axisLine={false}
                  allowDecimals={false}
                />
                <Tooltip content={<ChartTooltip />} />
                <Area
                  type="monotone"
                  dataKey="count"
                  stroke="#16A34A"
                  strokeWidth={2}
                  fill="url(#viewsGradient)"
                  dot={false}
                  activeDot={{ r: 4, fill: "#16A34A", stroke: "#fff", strokeWidth: 2 }}
                />
              </AreaChart>
            </ResponsiveContainer>
          </div>
        </div>
      )}

      {/* Leads chart */}
      {analytics.leadsByMonth.length > 0 && (
        <div className="rounded-2xl border bg-white p-5 shadow-sm">
          <h2 className="mb-4 text-base font-semibold">
            Leads by Month (Last 6 Months)
          </h2>
          <div className="h-72">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart
                data={analytics.leadsByMonth}
                margin={{ top: 5, right: 20, left: 0, bottom: 5 }}
              >
                <CartesianGrid
                  strokeDasharray="3 3"
                  stroke="#f0f0f0"
                  vertical={false}
                />
                <XAxis
                  dataKey="month"
                  tick={{ fontSize: 11, fill: "#9CA3AF" }}
                  tickLine={false}
                  axisLine={false}
                />
                <YAxis
                  tick={{ fontSize: 11, fill: "#9CA3AF" }}
                  tickLine={false}
                  axisLine={false}
                  allowDecimals={false}
                />
                <Tooltip content={<ChartTooltip />} />
                <Bar
                  dataKey="count"
                  fill="#059669"
                  radius={[6, 6, 0, 0]}
                  maxBarSize={48}
                />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </div>
      )}
    </div>
  );
}

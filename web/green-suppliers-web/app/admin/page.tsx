"use client";

import { useEffect, useState } from "react";
import { useAuth } from "@/lib/auth-context";
import { apiGetAuth } from "@/lib/api-client";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import type { AdminDashboardStats, AdminActivity } from "@/lib/types";
import {
  Building2,
  ShieldCheck,
  Mail,
  Clock,
  Activity,
} from "lucide-react";

export default function AdminDashboardPage() {
  const { token } = useAuth();
  const [stats, setStats] = useState<AdminDashboardStats | null>(null);
  const [activities, setActivities] = useState<AdminActivity[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!token) return;

    async function fetchDashboard() {
      setLoading(true);
      const [statsRes, activityRes] = await Promise.all([
        apiGetAuth<AdminDashboardStats>("/admin/dashboard/stats", token!),
        apiGetAuth<AdminActivity[]>("/admin/dashboard/activity", token!),
      ]);

      if (statsRes.success && statsRes.data) {
        setStats(statsRes.data);
      }
      if (activityRes.success && activityRes.data) {
        setActivities(activityRes.data);
      }
      setLoading(false);
    }

    fetchDashboard();
  }, [token]);

  const statCards = [
    {
      label: "Total Suppliers",
      value: stats?.totalSuppliers ?? 0,
      icon: Building2,
      color: "text-brand-green",
      bg: "bg-brand-green-light",
    },
    {
      label: "Verified Suppliers",
      value: stats?.verifiedSuppliers ?? 0,
      icon: ShieldCheck,
      color: "text-emerald-600",
      bg: "bg-emerald-50",
    },
    {
      label: "New Leads",
      value: stats?.newLeads ?? 0,
      icon: Mail,
      color: "text-blue-600",
      bg: "bg-blue-50",
    },
    {
      label: "Pending Certs",
      value: stats?.pendingCertifications ?? 0,
      icon: Clock,
      color: "text-amber-600",
      bg: "bg-amber-50",
    },
  ];

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-foreground">Dashboard</h1>
        <p className="text-sm text-muted-foreground">
          Overview of your Green Suppliers directory
        </p>
      </div>

      {/* Stat Cards */}
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        {statCards.map((stat) => (
          <Card key={stat.label}>
            <CardContent className="flex items-center gap-4">
              <div
                className={`flex h-10 w-10 shrink-0 items-center justify-center rounded-lg ${stat.bg}`}
              >
                <stat.icon className={`h-5 w-5 ${stat.color}`} />
              </div>
              <div>
                <p className="text-sm text-muted-foreground">{stat.label}</p>
                <p className="text-2xl font-bold">
                  {loading ? (
                    <span className="inline-block h-7 w-12 animate-pulse rounded bg-muted" />
                  ) : (
                    stat.value
                  )}
                </p>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>

      {/* Recent Activity */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Activity className="h-4 w-4" />
            Recent Activity
          </CardTitle>
        </CardHeader>
        <CardContent>
          {loading ? (
            <div className="space-y-3">
              {Array.from({ length: 5 }).map((_, i) => (
                <div key={i} className="flex items-center gap-3">
                  <div className="h-8 w-8 animate-pulse rounded-full bg-muted" />
                  <div className="flex-1 space-y-1">
                    <div className="h-4 w-3/4 animate-pulse rounded bg-muted" />
                    <div className="h-3 w-1/2 animate-pulse rounded bg-muted" />
                  </div>
                </div>
              ))}
            </div>
          ) : activities.length === 0 ? (
            <p className="py-8 text-center text-sm text-muted-foreground">
              No recent activity. Actions will appear here as you manage the
              directory.
            </p>
          ) : (
            <div className="space-y-3">
              {activities.map((activity) => (
                <div
                  key={activity.id}
                  className="flex items-start gap-3 rounded-md border p-3"
                >
                  <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-muted">
                    <Activity className="h-4 w-4 text-muted-foreground" />
                  </div>
                  <div className="flex-1 text-sm">
                    <p className="font-medium">{activity.description}</p>
                    <p className="text-xs text-muted-foreground">
                      {activity.entityType} &middot;{" "}
                      {new Date(activity.createdAt).toLocaleString()}
                    </p>
                  </div>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}

"use client";

import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { toast } from "sonner";
import { useAuth } from "@/lib/auth-context";
import { apiPost } from "@/lib/api-client";
import {
  changePasswordSchema,
  type ChangePasswordFormData,
} from "@/lib/validators";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  User,
  Mail,
  ShieldCheck,
  Building2,
  KeyRound,
  Loader2,
  AlertTriangle,
} from "lucide-react";

function InfoRow({
  icon: Icon,
  label,
  value,
}: {
  icon: React.ComponentType<{ className?: string }>;
  label: string;
  value: string;
}) {
  return (
    <div className="flex items-center gap-4 rounded-xl border px-4 py-3">
      <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-muted/50">
        <Icon className="h-4 w-4 text-muted-foreground" />
      </div>
      <div>
        <p className="text-xs text-muted-foreground">{label}</p>
        <p className="text-sm font-medium text-foreground">{value}</p>
      </div>
    </div>
  );
}

export default function BuyerSettingsPage() {
  const { user, token } = useAuth();
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    document.title = "Settings - Buyer Portal | Green Suppliers";
  }, []);

  const form = useForm<ChangePasswordFormData>({
    resolver: zodResolver(changePasswordSchema),
    defaultValues: {
      currentPassword: "",
      newPassword: "",
      confirmPassword: "",
    },
  });

  async function onSubmitPassword(data: ChangePasswordFormData) {
    if (!token) return;
    setSaving(true);

    const res = await apiPost<null>(
      "/auth/change-password",
      {
        currentPassword: data.currentPassword,
        newPassword: data.newPassword,
      },
      token
    );

    if (res.success) {
      toast.success("Password changed successfully");
      form.reset();
    } else {
      toast.error(res.error?.message ?? "Failed to change password");
    }
    setSaving(false);
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Settings</h1>
        <p className="text-sm text-muted-foreground">
          Manage your account and preferences
        </p>
      </div>

      {/* Account Info */}
      <div className="rounded-2xl border bg-white p-6 shadow-sm">
        <h2 className="mb-4 text-sm font-semibold uppercase tracking-wide text-muted-foreground">
          Account Information
        </h2>
        <div className="grid gap-3 sm:grid-cols-2">
          <InfoRow
            icon={User}
            label="Display Name"
            value={user?.displayName ?? "---"}
          />
          <InfoRow
            icon={Mail}
            label="Email"
            value={user?.email ?? "---"}
          />
          <InfoRow
            icon={ShieldCheck}
            label="Role"
            value={user?.role?.replace("_", " ") ?? "---"}
          />
          <InfoRow
            icon={Building2}
            label="Organization"
            value={user?.organizationName ?? user?.displayName ?? "---"}
          />
        </div>
      </div>

      {/* Change Password */}
      <div className="rounded-2xl border bg-white p-6 shadow-sm">
        <h2 className="mb-4 text-sm font-semibold uppercase tracking-wide text-muted-foreground">
          Change Password
        </h2>
        <form
          onSubmit={form.handleSubmit(onSubmitPassword)}
          className="max-w-md space-y-4"
        >
          <div>
            <Label htmlFor="currentPassword">Current Password</Label>
            <Input
              id="currentPassword"
              type="password"
              {...form.register("currentPassword")}
              autoComplete="current-password"
              aria-required="true"
              aria-invalid={!!form.formState.errors.currentPassword}
              aria-describedby={
                form.formState.errors.currentPassword
                  ? "currentPassword-error"
                  : undefined
              }
              className="mt-1"
            />
            {form.formState.errors.currentPassword && (
              <p
                id="currentPassword-error"
                role="alert"
                className="mt-1 text-xs text-destructive"
              >
                {form.formState.errors.currentPassword.message}
              </p>
            )}
          </div>

          <div>
            <Label htmlFor="newPassword">New Password</Label>
            <Input
              id="newPassword"
              type="password"
              {...form.register("newPassword")}
              autoComplete="new-password"
              aria-required="true"
              aria-invalid={!!form.formState.errors.newPassword}
              aria-describedby={
                form.formState.errors.newPassword
                  ? "newPassword-error"
                  : "newPassword-hint"
              }
              className="mt-1"
            />
            <p
              id="newPassword-hint"
              className="mt-1 text-xs text-muted-foreground"
            >
              At least 8 characters with uppercase, lowercase, and a number.
            </p>
            {form.formState.errors.newPassword && (
              <p
                id="newPassword-error"
                role="alert"
                className="mt-1 text-xs text-destructive"
              >
                {form.formState.errors.newPassword.message}
              </p>
            )}
          </div>

          <div>
            <Label htmlFor="confirmPassword">Confirm New Password</Label>
            <Input
              id="confirmPassword"
              type="password"
              {...form.register("confirmPassword")}
              autoComplete="new-password"
              aria-required="true"
              aria-invalid={!!form.formState.errors.confirmPassword}
              aria-describedby={
                form.formState.errors.confirmPassword
                  ? "confirmPassword-error"
                  : undefined
              }
              className="mt-1"
            />
            {form.formState.errors.confirmPassword && (
              <p
                id="confirmPassword-error"
                role="alert"
                className="mt-1 text-xs text-destructive"
              >
                {form.formState.errors.confirmPassword.message}
              </p>
            )}
          </div>

          <Button type="submit" disabled={saving}>
            {saving ? (
              <Loader2 className="mr-2 h-4 w-4 animate-spin" />
            ) : (
              <KeyRound className="mr-2 h-4 w-4" />
            )}
            {saving ? "Changing..." : "Change Password"}
          </Button>
        </form>
      </div>

      {/* Danger Zone */}
      <section
        aria-labelledby="danger-zone-heading"
        className="rounded-2xl border border-red-200 bg-white p-6 shadow-sm"
      >
        <h2
          id="danger-zone-heading"
          className="mb-2 text-sm font-semibold uppercase tracking-wide text-red-600"
        >
          Danger Zone
        </h2>
        <p className="mb-4 text-sm text-muted-foreground">
          Permanently delete your account and all associated data. This action
          cannot be undone.
        </p>
        <Button
          variant="outline"
          className="border-red-300 text-red-600 hover:bg-red-50 hover:text-red-700"
          onClick={() =>
            toast.info(
              "Account deletion is not yet available. Please contact support."
            )
          }
        >
          <AlertTriangle className="mr-2 h-4 w-4" aria-hidden="true" />
          Delete Account
        </Button>
      </section>
    </div>
  );
}

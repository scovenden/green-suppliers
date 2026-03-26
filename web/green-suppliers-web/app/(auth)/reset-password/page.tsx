"use client";

import * as React from "react";
import Link from "next/link";
import { useSearchParams } from "next/navigation";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import {
  resetPasswordSchema,
  type ResetPasswordFormData,
} from "@/lib/validators";
import { getPasswordStrength } from "@/lib/password-strength";
import { apiPost } from "@/lib/api-client";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
  CheckCircle,
  AlertCircle,
  Loader2,
  KeyRound,
} from "lucide-react";

function ResetPasswordContent() {
  const searchParams = useSearchParams();
  const token = searchParams.get("token");

  const [status, setStatus] = React.useState<
    "idle" | "submitting" | "success" | "error"
  >("idle");
  const [errorMessage, setErrorMessage] = React.useState("");

  const {
    register,
    handleSubmit,
    watch,
    formState: { errors },
  } = useForm<ResetPasswordFormData>({
    resolver: zodResolver(resetPasswordSchema),
    defaultValues: {
      newPassword: "",
      confirmPassword: "",
    },
  });

  const passwordValue = watch("newPassword", "");
  const strength = getPasswordStrength(passwordValue);

  async function onSubmit(data: ResetPasswordFormData) {
    if (!token) {
      setStatus("error");
      setErrorMessage("No reset token provided. Please use the link from your email.");
      return;
    }

    setStatus("submitting");
    setErrorMessage("");

    try {
      const res = await apiPost("/auth/reset-password", {
        token,
        newPassword: data.newPassword,
      });

      if (res.success) {
        setStatus("success");
      } else {
        setStatus("error");
        setErrorMessage(
          res.error?.message ??
            "Password reset failed. The link may have expired."
        );
      }
    } catch {
      setStatus("error");
      setErrorMessage("Network error. Please try again later.");
    }
  }

  if (!token) {
    return (
      <Card>
        <CardContent className="flex flex-col items-center gap-4 py-10">
          <div className="flex h-16 w-16 items-center justify-center rounded-full bg-red-50">
            <AlertCircle className="h-8 w-8 text-red-500" aria-hidden="true" />
          </div>
          <h2 className="text-xl font-bold text-brand-dark">
            Invalid Reset Link
          </h2>
          <p className="max-w-sm text-center text-sm text-brand-earth">
            This password reset link is invalid or has expired. Please request a
            new one.
          </p>
          <Link href="/forgot-password">
            <Button className="mt-2 bg-brand-green text-white hover:bg-brand-green-hover">
              Request New Link
            </Button>
          </Link>
        </CardContent>
      </Card>
    );
  }

  if (status === "success") {
    return (
      <Card>
        <CardContent className="flex flex-col items-center gap-4 py-10">
          <div className="flex h-16 w-16 items-center justify-center rounded-full bg-brand-green-light">
            <CheckCircle className="h-8 w-8 text-brand-green" aria-hidden="true" />
          </div>
          <h2 className="text-xl font-bold text-brand-dark">
            Password Reset!
          </h2>
          <p className="max-w-sm text-center text-sm text-brand-earth">
            Your password has been reset successfully. You can now sign in with
            your new password.
          </p>
          <Link href="/admin/login">
            <Button className="mt-2 bg-brand-green text-white hover:bg-brand-green-hover">
              Sign In
            </Button>
          </Link>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center gap-3">
          <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-brand-green-light text-brand-green">
            <KeyRound className="h-5 w-5" aria-hidden="true" />
          </div>
          <CardTitle className="text-xl">Reset Password</CardTitle>
        </div>
        <p className="mt-2 text-sm text-brand-earth">
          Enter your new password below.
        </p>
      </CardHeader>
      <CardContent>
        <form
          onSubmit={handleSubmit(onSubmit)}
          className="flex flex-col gap-4"
          noValidate
        >
          {status === "error" && errorMessage && (
            <div
              role="alert"
              className="flex items-start gap-2 rounded-lg border border-red-200 bg-red-50 p-3 text-sm text-red-700"
            >
              <AlertCircle className="mt-0.5 h-4 w-4 shrink-0" aria-hidden="true" />
              <span>{errorMessage}</span>
            </div>
          )}

          {/* New Password */}
          <div className="flex flex-col gap-1.5">
            <Label htmlFor="newPassword">
              New Password <span aria-hidden="true" className="text-red-500">*</span>
            </Label>
            <Input
              id="newPassword"
              type="password"
              placeholder="Enter your new password"
              aria-required="true"
              aria-invalid={!!errors.newPassword}
              aria-describedby={
                [
                  errors.newPassword ? "newPassword-error" : null,
                  passwordValue.length > 0 ? "newPassword-strength" : null,
                ]
                  .filter(Boolean)
                  .join(" ") || undefined
              }
              autoComplete="new-password"
              {...register("newPassword")}
            />
            {passwordValue.length > 0 && (
              <div className="flex items-center gap-2">
                <div
                  className="h-1.5 flex-1 overflow-hidden rounded-full bg-gray-200"
                  role="meter"
                  aria-valuenow={strength.score}
                  aria-valuemin={0}
                  aria-valuemax={5}
                  aria-label="Password strength"
                >
                  <div
                    className={`h-full rounded-full transition-all ${strength.color}`}
                    style={{ width: `${(strength.score / 5) * 100}%` }}
                  />
                </div>
                <span id="newPassword-strength" aria-live="polite" className="text-xs text-brand-earth">
                  {strength.label}
                </span>
              </div>
            )}
            {errors.newPassword && (
              <p id="newPassword-error" role="alert" className="text-xs text-red-500">
                {errors.newPassword.message}
              </p>
            )}
          </div>

          {/* Confirm Password */}
          <div className="flex flex-col gap-1.5">
            <Label htmlFor="confirmPassword">
              Confirm Password <span aria-hidden="true" className="text-red-500">*</span>
            </Label>
            <Input
              id="confirmPassword"
              type="password"
              placeholder="Confirm your new password"
              aria-required="true"
              aria-invalid={!!errors.confirmPassword}
              aria-describedby={errors.confirmPassword ? "confirmPassword-error" : undefined}
              autoComplete="new-password"
              {...register("confirmPassword")}
            />
            {errors.confirmPassword && (
              <p id="confirmPassword-error" role="alert" className="text-xs text-red-500">
                {errors.confirmPassword.message}
              </p>
            )}
          </div>

          {/* Submit */}
          <Button
            type="submit"
            disabled={status === "submitting"}
            className="mt-2 h-10 w-full gap-2 rounded-xl bg-brand-green text-sm font-semibold text-white transition-colors hover:bg-brand-green-hover"
          >
            {status === "submitting" ? (
              <>
                <Loader2 className="h-4 w-4 animate-spin" aria-hidden="true" />
                Resetting...
              </>
            ) : (
              "Reset Password"
            )}
          </Button>

          <p className="text-center text-sm text-brand-earth">
            Remember your password?{" "}
            <Link
              href="/admin/login"
              className="font-medium text-brand-green hover:text-brand-green-hover hover:underline"
            >
              Sign in
            </Link>
          </p>
        </form>
      </CardContent>
    </Card>
  );
}

export default function ResetPasswordPage() {
  return (
    <React.Suspense
      fallback={
        <Card>
          <CardContent className="flex flex-col items-center gap-4 py-10">
            <Loader2 className="h-10 w-10 animate-spin text-brand-green" aria-hidden="true" />
            <h2 className="text-lg font-bold text-brand-dark">Loading...</h2>
          </CardContent>
        </Card>
      }
    >
      <ResetPasswordContent />
    </React.Suspense>
  );
}

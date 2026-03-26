"use client";

import * as React from "react";
import Link from "next/link";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { registerSchema, type RegisterFormData } from "@/lib/validators";
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
  Search,
  ArrowLeft,
} from "lucide-react";

export default function BuyerRegisterPage() {
  const [status, setStatus] = React.useState<
    "idle" | "submitting" | "success" | "error"
  >("idle");
  const [errorMessage, setErrorMessage] = React.useState("");

  const {
    register,
    handleSubmit,
    watch,
    formState: { errors },
  } = useForm<RegisterFormData>({
    resolver: zodResolver(registerSchema),
    defaultValues: {
      firstName: "",
      lastName: "",
      email: "",
      password: "",
      confirmPassword: "",
      companyName: "",
    },
  });

  const passwordValue = watch("password", "");
  const strength = getPasswordStrength(passwordValue);

  async function onSubmit(data: RegisterFormData) {
    setStatus("submitting");
    setErrorMessage("");

    try {
      const res = await apiPost("/auth/register", {
        firstName: data.firstName,
        lastName: data.lastName,
        email: data.email,
        password: data.password,
        companyName: data.companyName || undefined,
        accountType: "buyer",
      });

      if (res.success) {
        setStatus("success");
      } else {
        setStatus("error");
        setErrorMessage(
          res.error?.message ?? "Registration failed. Please try again."
        );
      }
    } catch {
      setStatus("error");
      setErrorMessage("Network error. Please try again later.");
    }
  }

  if (status === "success") {
    return (
      <Card>
        <CardContent className="flex flex-col items-center gap-4 py-10">
          <div className="flex h-16 w-16 items-center justify-center rounded-full bg-brand-green-light">
            <CheckCircle className="h-8 w-8 text-brand-green" aria-hidden="true" />
          </div>
          <h2 className="text-xl font-bold text-brand-dark">
            Check your email
          </h2>
          <p className="max-w-sm text-center text-sm text-brand-earth">
            We&apos;ve sent a verification link to your email address. Please
            click the link to verify your account and get started.
          </p>
          <Link href="/admin/login">
            <Button
              variant="outline"
              className="mt-2"
            >
              Go to Sign In
            </Button>
          </Link>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card>
      <CardHeader>
        <Link
          href="/register"
          className="mb-2 inline-flex items-center gap-1 text-sm text-brand-earth hover:text-brand-green"
        >
          <ArrowLeft className="h-4 w-4" aria-hidden="true" />
          Back
        </Link>
        <div className="flex items-center gap-3">
          <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-emerald-50 text-brand-emerald">
            <Search className="h-5 w-5" aria-hidden="true" />
          </div>
          <CardTitle className="text-xl">Buyer Registration</CardTitle>
        </div>
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

          {/* First Name + Last Name */}
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="firstName">
                First Name <span aria-hidden="true" className="text-red-500">*</span>
              </Label>
              <Input
                id="firstName"
                placeholder="First name"
                aria-required="true"
                aria-invalid={!!errors.firstName}
                aria-describedby={errors.firstName ? "firstName-error" : undefined}
                autoComplete="given-name"
                {...register("firstName")}
              />
              {errors.firstName && (
                <p id="firstName-error" role="alert" className="text-xs text-red-500">
                  {errors.firstName.message}
                </p>
              )}
            </div>
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="lastName">
                Last Name <span aria-hidden="true" className="text-red-500">*</span>
              </Label>
              <Input
                id="lastName"
                placeholder="Last name"
                aria-required="true"
                aria-invalid={!!errors.lastName}
                aria-describedby={errors.lastName ? "lastName-error" : undefined}
                autoComplete="family-name"
                {...register("lastName")}
              />
              {errors.lastName && (
                <p id="lastName-error" role="alert" className="text-xs text-red-500">
                  {errors.lastName.message}
                </p>
              )}
            </div>
          </div>

          {/* Email */}
          <div className="flex flex-col gap-1.5">
            <Label htmlFor="email">
              Email <span aria-hidden="true" className="text-red-500">*</span>
            </Label>
            <Input
              id="email"
              type="email"
              placeholder="you@company.com"
              aria-required="true"
              aria-invalid={!!errors.email}
              aria-describedby={errors.email ? "email-error" : undefined}
              autoComplete="email"
              {...register("email")}
            />
            {errors.email && (
              <p id="email-error" role="alert" className="text-xs text-red-500">
                {errors.email.message}
              </p>
            )}
          </div>

          {/* Company Name (optional) */}
          <div className="flex flex-col gap-1.5">
            <Label htmlFor="companyName">Company Name</Label>
            <Input
              id="companyName"
              placeholder="Your company name (optional)"
              autoComplete="organization"
              {...register("companyName")}
            />
          </div>

          {/* Password */}
          <div className="flex flex-col gap-1.5">
            <Label htmlFor="password">
              Password <span aria-hidden="true" className="text-red-500">*</span>
            </Label>
            <Input
              id="password"
              type="password"
              placeholder="Create a strong password"
              aria-required="true"
              aria-invalid={!!errors.password}
              aria-describedby={
                [
                  errors.password ? "password-error" : null,
                  passwordValue.length > 0 ? "password-strength" : null,
                ]
                  .filter(Boolean)
                  .join(" ") || undefined
              }
              autoComplete="new-password"
              {...register("password")}
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
                <span id="password-strength" aria-live="polite" className="text-xs text-brand-earth">
                  {strength.label}
                </span>
              </div>
            )}
            {errors.password && (
              <p id="password-error" role="alert" className="text-xs text-red-500">
                {errors.password.message}
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
              placeholder="Confirm your password"
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
            className="mt-2 h-10 w-full gap-2 rounded-xl bg-brand-emerald text-sm font-semibold text-white transition-colors hover:bg-brand-emerald-hover"
          >
            {status === "submitting" ? (
              <>
                <Loader2 className="h-4 w-4 animate-spin" aria-hidden="true" />
                Creating account...
              </>
            ) : (
              "Create Buyer Account"
            )}
          </Button>

          <p className="text-center text-sm text-brand-earth">
            Already have an account?{" "}
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

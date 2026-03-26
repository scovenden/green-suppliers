"use client";

import * as React from "react";
import Link from "next/link";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { registerSchema, type RegisterFormData } from "@/lib/validators";
import { apiPost } from "@/lib/api-client";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  CheckCircle,
  AlertCircle,
  Loader2,
  Building2,
  ArrowLeft,
} from "lucide-react";

const COUNTRIES = [
  { code: "ZA", name: "South Africa" },
  { code: "KE", name: "Kenya" },
  { code: "NG", name: "Nigeria" },
  { code: "GH", name: "Ghana" },
  { code: "EG", name: "Egypt" },
  { code: "MA", name: "Morocco" },
  { code: "RW", name: "Rwanda" },
  { code: "TZ", name: "Tanzania" },
  { code: "UG", name: "Uganda" },
  { code: "BW", name: "Botswana" },
] as const;

function getPasswordStrength(
  password: string
): { score: number; label: string; color: string } {
  let score = 0;
  if (password.length >= 8) score++;
  if (/[A-Z]/.test(password)) score++;
  if (/[a-z]/.test(password)) score++;
  if (/[0-9]/.test(password)) score++;
  if (/[^A-Za-z0-9]/.test(password)) score++;

  if (score <= 2) return { score, label: "Weak", color: "bg-red-500" };
  if (score <= 3) return { score, label: "Fair", color: "bg-yellow-500" };
  if (score <= 4) return { score, label: "Good", color: "bg-brand-green" };
  return { score, label: "Strong", color: "bg-brand-green" };
}

export default function SupplierRegisterPage() {
  const [status, setStatus] = React.useState<
    "idle" | "submitting" | "success" | "error"
  >("idle");
  const [errorMessage, setErrorMessage] = React.useState("");
  const [selectedCountry, setSelectedCountry] = React.useState("ZA");

  const {
    register,
    handleSubmit,
    watch,
    setValue,
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
      countryCode: "ZA",
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
        companyName: data.companyName,
        countryCode: data.countryCode,
        accountType: "supplier",
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
            <CheckCircle className="h-8 w-8 text-brand-green" />
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
          <ArrowLeft className="h-4 w-4" />
          Back
        </Link>
        <div className="flex items-center gap-3">
          <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-brand-green-light text-brand-green">
            <Building2 className="h-5 w-5" />
          </div>
          <CardTitle className="text-xl">Supplier Registration</CardTitle>
        </div>
      </CardHeader>
      <CardContent>
        <form
          onSubmit={handleSubmit(onSubmit)}
          className="flex flex-col gap-4"
        >
          {status === "error" && errorMessage && (
            <div className="flex items-start gap-2 rounded-lg border border-red-200 bg-red-50 p-3 text-sm text-red-700">
              <AlertCircle className="mt-0.5 h-4 w-4 shrink-0" />
              {errorMessage}
            </div>
          )}

          {/* Company Name */}
          <div className="flex flex-col gap-1.5">
            <Label htmlFor="companyName">
              Company Name <span className="text-red-500">*</span>
            </Label>
            <Input
              id="companyName"
              placeholder="Your company or trading name"
              aria-invalid={!!errors.companyName}
              {...register("companyName")}
            />
            {errors.companyName && (
              <p className="text-xs text-red-500">
                {errors.companyName.message}
              </p>
            )}
          </div>

          {/* First Name + Last Name */}
          <div className="grid grid-cols-2 gap-3">
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="firstName">
                First Name <span className="text-red-500">*</span>
              </Label>
              <Input
                id="firstName"
                placeholder="First name"
                aria-invalid={!!errors.firstName}
                {...register("firstName")}
              />
              {errors.firstName && (
                <p className="text-xs text-red-500">
                  {errors.firstName.message}
                </p>
              )}
            </div>
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="lastName">
                Last Name <span className="text-red-500">*</span>
              </Label>
              <Input
                id="lastName"
                placeholder="Last name"
                aria-invalid={!!errors.lastName}
                {...register("lastName")}
              />
              {errors.lastName && (
                <p className="text-xs text-red-500">
                  {errors.lastName.message}
                </p>
              )}
            </div>
          </div>

          {/* Email */}
          <div className="flex flex-col gap-1.5">
            <Label htmlFor="email">
              Email <span className="text-red-500">*</span>
            </Label>
            <Input
              id="email"
              type="email"
              placeholder="you@company.com"
              aria-invalid={!!errors.email}
              {...register("email")}
            />
            {errors.email && (
              <p className="text-xs text-red-500">{errors.email.message}</p>
            )}
          </div>

          {/* Country */}
          <div className="flex flex-col gap-1.5">
            <Label>Country</Label>
            <Select
              value={selectedCountry}
              onValueChange={(val) => {
                if (val) {
                  setSelectedCountry(val);
                  setValue("countryCode", val);
                }
              }}
            >
              <SelectTrigger className="w-full">
                <SelectValue placeholder="Select country" />
              </SelectTrigger>
              <SelectContent>
                {COUNTRIES.map((c) => (
                  <SelectItem key={c.code} value={c.code}>
                    {c.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          {/* Password */}
          <div className="flex flex-col gap-1.5">
            <Label htmlFor="password">
              Password <span className="text-red-500">*</span>
            </Label>
            <Input
              id="password"
              type="password"
              placeholder="Create a strong password"
              aria-invalid={!!errors.password}
              {...register("password")}
            />
            {passwordValue.length > 0 && (
              <div className="flex items-center gap-2">
                <div className="h-1.5 flex-1 overflow-hidden rounded-full bg-gray-200">
                  <div
                    className={`h-full rounded-full transition-all ${strength.color}`}
                    style={{ width: `${(strength.score / 5) * 100}%` }}
                  />
                </div>
                <span className="text-xs text-brand-earth">
                  {strength.label}
                </span>
              </div>
            )}
            {errors.password && (
              <p className="text-xs text-red-500">{errors.password.message}</p>
            )}
          </div>

          {/* Confirm Password */}
          <div className="flex flex-col gap-1.5">
            <Label htmlFor="confirmPassword">
              Confirm Password <span className="text-red-500">*</span>
            </Label>
            <Input
              id="confirmPassword"
              type="password"
              placeholder="Confirm your password"
              aria-invalid={!!errors.confirmPassword}
              {...register("confirmPassword")}
            />
            {errors.confirmPassword && (
              <p className="text-xs text-red-500">
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
                <Loader2 className="h-4 w-4 animate-spin" />
                Creating account...
              </>
            ) : (
              "Create Supplier Account"
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

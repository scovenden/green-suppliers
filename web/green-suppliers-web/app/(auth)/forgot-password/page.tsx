"use client";

import * as React from "react";
import Link from "next/link";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import {
  forgotPasswordSchema,
  type ForgotPasswordFormData,
} from "@/lib/validators";
import { apiPost } from "@/lib/api-client";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Mail, ArrowLeft, Loader2, CheckCircle } from "lucide-react";

export default function ForgotPasswordPage() {
  const [status, setStatus] = React.useState<
    "idle" | "submitting" | "success"
  >("idle");

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<ForgotPasswordFormData>({
    resolver: zodResolver(forgotPasswordSchema),
    defaultValues: {
      email: "",
    },
  });

  async function onSubmit(data: ForgotPasswordFormData) {
    setStatus("submitting");

    try {
      await apiPost("/auth/forgot-password", { email: data.email });
    } catch {
      // Intentionally swallow -- always show success to prevent email enumeration
    }

    // Always show success to prevent email enumeration
    setStatus("success");
  }

  if (status === "success") {
    return (
      <Card>
        <CardContent className="flex flex-col items-center gap-4 py-10">
          <div className="flex h-16 w-16 items-center justify-center rounded-full bg-brand-green-light">
            <CheckCircle className="h-8 w-8 text-brand-green" />
          </div>
          <h2 className="text-xl font-bold text-brand-dark">Check your email</h2>
          <p className="max-w-sm text-center text-sm text-brand-earth">
            If an account exists with that email address, we&apos;ve sent a
            password reset link. Please check your inbox and spam folder.
          </p>
          <Link href="/admin/login">
            <Button variant="outline" className="mt-2">
              Back to Sign In
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
          href="/admin/login"
          className="mb-2 inline-flex items-center gap-1 text-sm text-brand-earth hover:text-brand-green"
        >
          <ArrowLeft className="h-4 w-4" />
          Back to Sign In
        </Link>
        <div className="flex items-center gap-3">
          <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-brand-green-light text-brand-green">
            <Mail className="h-5 w-5" />
          </div>
          <CardTitle className="text-xl">Forgot Password</CardTitle>
        </div>
        <p className="mt-2 text-sm text-brand-earth">
          Enter your email address and we&apos;ll send you a link to reset your
          password.
        </p>
      </CardHeader>
      <CardContent>
        <form
          onSubmit={handleSubmit(onSubmit)}
          className="flex flex-col gap-4"
        >
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

          <Button
            type="submit"
            disabled={status === "submitting"}
            className="mt-2 h-10 w-full gap-2 rounded-xl bg-brand-green text-sm font-semibold text-white transition-colors hover:bg-brand-green-hover"
          >
            {status === "submitting" ? (
              <>
                <Loader2 className="h-4 w-4 animate-spin" />
                Sending...
              </>
            ) : (
              "Send Reset Link"
            )}
          </Button>
        </form>
      </CardContent>
    </Card>
  );
}

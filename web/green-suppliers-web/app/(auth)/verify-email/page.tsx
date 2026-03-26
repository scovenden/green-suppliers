"use client";

import * as React from "react";
import Link from "next/link";
import { useSearchParams } from "next/navigation";
import { apiPost } from "@/lib/api-client";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { CheckCircle, XCircle, Loader2 } from "lucide-react";

function VerifyEmailContent() {
  const searchParams = useSearchParams();
  const token = searchParams.get("token");

  const [status, setStatus] = React.useState<
    "loading" | "success" | "error"
  >("loading");
  const [errorMessage, setErrorMessage] = React.useState("");
  const hasVerified = React.useRef(false);

  React.useEffect(() => {
    if (hasVerified.current) return;
    hasVerified.current = true;

    if (!token) {
      setStatus("error");
      setErrorMessage("No verification token provided.");
      return;
    }

    async function verify() {
      try {
        const res = await apiPost("/auth/verify-email", { token });
        if (res.success) {
          setStatus("success");
        } else {
          setStatus("error");
          setErrorMessage(
            res.error?.message ??
              "Verification failed. The link may have expired."
          );
        }
      } catch {
        setStatus("error");
        setErrorMessage("Network error. Please try again later.");
      }
    }

    verify();
  }, [token]);

  return (
    <Card>
      <CardContent className="flex flex-col items-center gap-4 py-10">
        {status === "loading" && (
          <div role="status" aria-label="Verifying your email" className="flex flex-col items-center gap-4">
            <Loader2 className="h-10 w-10 animate-spin text-brand-green" aria-hidden="true" />
            <h2 className="text-lg font-bold text-brand-dark">
              Verifying your email...
            </h2>
            <p className="text-sm text-brand-earth">
              Please wait while we confirm your account.
            </p>
          </div>
        )}

        {status === "success" && (
          <>
            <div className="flex h-16 w-16 items-center justify-center rounded-full bg-brand-green-light">
              <CheckCircle className="h-8 w-8 text-brand-green" aria-hidden="true" />
            </div>
            <h2 className="text-xl font-bold text-brand-dark">
              Email Verified!
            </h2>
            <p className="max-w-sm text-center text-sm text-brand-earth">
              Your email has been verified successfully. You can now sign in to
              your account.
            </p>
            <Link href="/admin/login">
              <Button className="mt-2 bg-brand-green text-white hover:bg-brand-green-hover">
                Sign In
              </Button>
            </Link>
          </>
        )}

        {status === "error" && (
          <>
            <div className="flex h-16 w-16 items-center justify-center rounded-full bg-red-50">
              <XCircle className="h-8 w-8 text-red-500" aria-hidden="true" />
            </div>
            <h2 className="text-xl font-bold text-brand-dark">
              Verification Failed
            </h2>
            <p className="max-w-sm text-center text-sm text-brand-earth">
              {errorMessage}
            </p>
            <div className="mt-2 flex gap-3">
              <Link href="/admin/login">
                <Button variant="outline">Sign In</Button>
              </Link>
              <Link href="/register">
                <Button className="bg-brand-green text-white hover:bg-brand-green-hover">
                  Register Again
                </Button>
              </Link>
            </div>
          </>
        )}
      </CardContent>
    </Card>
  );
}

export default function VerifyEmailPage() {
  return (
    <React.Suspense
      fallback={
        <Card>
          <CardContent className="flex flex-col items-center gap-4 py-10">
            <div role="status" aria-label="Loading">
              <Loader2 className="h-10 w-10 animate-spin text-brand-green" aria-hidden="true" />
            </div>
            <h2 className="text-lg font-bold text-brand-dark">Loading...</h2>
          </CardContent>
        </Card>
      }
    >
      <VerifyEmailContent />
    </React.Suspense>
  );
}

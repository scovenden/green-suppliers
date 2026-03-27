"use client";

import Link from "next/link";
import { Button } from "@/components/ui/button";
import { XCircle, ArrowRight, RotateCcw } from "lucide-react";

export default function PaymentCancelledPage() {
  return (
    <div className="flex min-h-[60vh] flex-col items-center justify-center px-4 text-center">
      {/* Icon */}
      <div className="mb-6 flex h-20 w-20 items-center justify-center rounded-full bg-gray-100">
        <XCircle className="h-12 w-12 text-gray-400" />
      </div>

      <h1 className="text-2xl font-bold tracking-tight text-foreground sm:text-3xl">
        Payment Cancelled
      </h1>
      <p className="mx-auto mt-3 max-w-md text-sm text-muted-foreground">
        Your payment was not completed. No charges have been made to your account.
        You can try again at any time.
      </p>

      {/* Actions */}
      <div className="mt-8 flex flex-col gap-3 sm:flex-row">
        <Link href="/dashboard/billing/upgrade">
          <Button className="gap-1.5">
            <RotateCcw className="h-4 w-4" />
            Try Again
          </Button>
        </Link>
        <Link href="/dashboard">
          <Button variant="outline" className="gap-1.5">
            Go to Dashboard
            <ArrowRight className="h-4 w-4" />
          </Button>
        </Link>
      </div>

      {/* Help text */}
      <p className="mt-8 text-xs text-muted-foreground">
        Having trouble with payment?{" "}
        <a
          href="mailto:hello@greensuppliers.co.za"
          className="font-medium text-brand-green hover:underline"
        >
          Contact our support team
        </a>
      </p>
    </div>
  );
}

"use client";

import Link from "next/link";
import { Button } from "@/components/ui/button";
import { CheckCircle, ArrowRight, Leaf } from "lucide-react";

export default function PaymentSuccessPage() {
  return (
    <div className="flex min-h-[60vh] flex-col items-center justify-center px-4 text-center">
      {/* Success animation container */}
      <div className="relative mb-6">
        {/* Green glow ring */}
        <div className="absolute inset-0 animate-ping rounded-full bg-brand-green/20" />
        <div className="relative flex h-20 w-20 items-center justify-center rounded-full bg-brand-green/10">
          <CheckCircle className="h-12 w-12 text-brand-green" />
        </div>
      </div>

      <h1 className="text-2xl font-bold tracking-tight text-foreground sm:text-3xl">
        Payment Successful
      </h1>
      <p className="mx-auto mt-3 max-w-md text-sm text-muted-foreground">
        Your subscription has been activated. Thank you for choosing Green Suppliers.
        Your upgraded features are now available.
      </p>

      {/* Highlights */}
      <div className="mx-auto mt-8 grid max-w-sm gap-3">
        <div className="flex items-center gap-3 rounded-xl border bg-white p-4 text-left shadow-sm">
          <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-brand-green/10">
            <Leaf className="h-4 w-4 text-brand-green" />
          </div>
          <div>
            <p className="text-sm font-medium text-foreground">Profile Upgraded</p>
            <p className="text-xs text-muted-foreground">
              Your enhanced features are live immediately
            </p>
          </div>
        </div>
        <div className="flex items-center gap-3 rounded-xl border bg-white p-4 text-left shadow-sm">
          <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-brand-green/10">
            <CheckCircle className="h-4 w-4 text-brand-green" />
          </div>
          <div>
            <p className="text-sm font-medium text-foreground">Receipt Sent</p>
            <p className="text-xs text-muted-foreground">
              A confirmation has been sent to your email
            </p>
          </div>
        </div>
      </div>

      {/* Actions */}
      <div className="mt-8 flex flex-col gap-3 sm:flex-row">
        <Link href="/dashboard">
          <Button className="gap-1.5">
            Go to Dashboard
            <ArrowRight className="h-4 w-4" />
          </Button>
        </Link>
        <Link href="/dashboard/billing">
          <Button variant="outline">View Billing</Button>
        </Link>
      </div>
    </div>
  );
}

"use client";

import { useCallback, useEffect, useState } from "react";
import Link from "next/link";
import { useAuth } from "@/lib/auth-context";
import { apiGetAuth, apiPost } from "@/lib/api-client";
import type { SubscriptionDto, PaymentDto } from "@/lib/types";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import {
  Dialog,
  DialogTrigger,
  DialogContent,
  DialogHeader,
  DialogFooter,
  DialogTitle,
  DialogDescription,
  DialogClose,
} from "@/components/ui/dialog";
import {
  CreditCard,
  AlertTriangle,
  ArrowUpRight,
  Calendar,
  CircleCheck,
  Clock,
  XCircle,
  Ban,
  Receipt,
  Loader2,
} from "lucide-react";
import { toast } from "sonner";

/* -------------------------------------------------------------------------- */
/*  Status badge                                                               */
/* -------------------------------------------------------------------------- */

function SubscriptionStatusBadge({ status }: { status: string }) {
  const lower = status.toLowerCase().replace(/\s+/g, "_");
  const config: Record<string, { bg: string; text: string; label: string; icon: React.ReactNode }> = {
    active: {
      bg: "bg-green-100",
      text: "text-green-700",
      label: "Active",
      icon: <CircleCheck className="h-3.5 w-3.5" />,
    },
    trial: {
      bg: "bg-blue-100",
      text: "text-blue-700",
      label: "Trial",
      icon: <Clock className="h-3.5 w-3.5" />,
    },
    past_due: {
      bg: "bg-red-100",
      text: "text-red-700",
      label: "Past Due",
      icon: <AlertTriangle className="h-3.5 w-3.5" />,
    },
    cancelled: {
      bg: "bg-gray-100",
      text: "text-gray-600",
      label: "Cancelled",
      icon: <Ban className="h-3.5 w-3.5" />,
    },
    expired: {
      bg: "bg-gray-100",
      text: "text-gray-500",
      label: "Expired",
      icon: <XCircle className="h-3.5 w-3.5" />,
    },
    pending: {
      bg: "bg-yellow-100",
      text: "text-yellow-700",
      label: "Pending",
      icon: <Clock className="h-3.5 w-3.5" />,
    },
  };
  const c = config[lower] ?? { bg: "bg-gray-100", text: "text-gray-600", label: status, icon: null };

  return (
    <span
      className={`inline-flex items-center gap-1.5 rounded-full px-3 py-1 text-xs font-semibold ${c.bg} ${c.text}`}
    >
      {c.icon}
      {c.label}
    </span>
  );
}

function PaymentStatusBadge({ status }: { status: string }) {
  const lower = status.toLowerCase();
  const config: Record<string, { bg: string; text: string; label: string }> = {
    completed: { bg: "bg-green-100", text: "text-green-700", label: "Completed" },
    pending: { bg: "bg-yellow-100", text: "text-yellow-700", label: "Pending" },
    failed: { bg: "bg-red-100", text: "text-red-700", label: "Failed" },
    refunded: { bg: "bg-blue-100", text: "text-blue-700", label: "Refunded" },
  };
  const c = config[lower] ?? { bg: "bg-gray-100", text: "text-gray-600", label: status };

  return (
    <span className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${c.bg} ${c.text}`}>
      {c.label}
    </span>
  );
}

/* -------------------------------------------------------------------------- */
/*  Helpers                                                                    */
/* -------------------------------------------------------------------------- */

function formatDate(dateStr: string | null | undefined): string {
  if (!dateStr) return "--";
  const date = new Date(dateStr);
  return date.toLocaleDateString("en-ZA", {
    year: "numeric",
    month: "long",
    day: "numeric",
  });
}

function formatZAR(amount: number): string {
  return `R${amount.toLocaleString("en-ZA", { minimumFractionDigits: 2 })}`;
}

function formatShortDate(dateStr: string): string {
  const date = new Date(dateStr);
  return date.toLocaleDateString("en-ZA", {
    year: "numeric",
    month: "short",
    day: "numeric",
  });
}

/* -------------------------------------------------------------------------- */
/*  Loading skeleton                                                           */
/* -------------------------------------------------------------------------- */

function BillingSkeleton() {
  return (
    <div role="status" aria-label="Loading billing" className="space-y-6">
      <Skeleton className="h-8 w-48" />
      <Skeleton className="h-48 rounded-2xl" />
      <Skeleton className="h-64 rounded-2xl" />
      <span className="sr-only">Loading billing data</span>
    </div>
  );
}

/* -------------------------------------------------------------------------- */
/*  Main billing dashboard page                                                */
/* -------------------------------------------------------------------------- */

export default function BillingDashboardPage() {
  const { token } = useAuth();
  const [subscription, setSubscription] = useState<SubscriptionDto | null>(null);
  const [payments, setPayments] = useState<PaymentDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [cancelling, setCancelling] = useState(false);
  const [cancelDialogOpen, setCancelDialogOpen] = useState(false);

  const fetchBillingData = useCallback(async () => {
    if (!token) return;
    setLoading(true);
    setError(null);

    const [subRes, payRes] = await Promise.all([
      apiGetAuth<SubscriptionDto | null>("/supplier/billing/subscription", token),
      apiGetAuth<PaymentDto[]>("/supplier/billing/payments", token),
    ]);

    if (subRes.success) {
      setSubscription(subRes.data ?? null);
    } else {
      setError(subRes.error?.message ?? "Failed to load subscription data");
    }

    if (payRes.success && payRes.data) {
      setPayments(payRes.data);
    }

    setLoading(false);
  }, [token]);

  useEffect(() => {
    fetchBillingData();
  }, [fetchBillingData]);

  async function handleCancel() {
    if (!token) return;
    setCancelling(true);
    const res = await apiPost<{ cancelled: boolean }>(
      "/supplier/billing/cancel",
      {},
      token
    );
    setCancelling(false);
    setCancelDialogOpen(false);

    if (res.success) {
      toast.success("Subscription cancelled successfully. It will remain active until the end of your billing period.");
      fetchBillingData();
    } else {
      toast.error(res.error?.message ?? "Failed to cancel subscription");
    }
  }

  if (loading) {
    return <BillingSkeleton />;
  }

  if (error) {
    return (
      <div role="alert" className="flex flex-col items-center justify-center gap-4 py-20">
        <AlertTriangle className="h-10 w-10 text-destructive" aria-hidden="true" />
        <p className="text-sm text-muted-foreground">{error}</p>
        <Button variant="outline" onClick={() => fetchBillingData()}>
          Try Again
        </Button>
      </div>
    );
  }

  const hasSubscription = subscription !== null;
  const isActivePlan = hasSubscription && (subscription.status === "active" || subscription.status === "trial");

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Billing</h1>
          <p className="text-sm text-muted-foreground">
            Manage your subscription and payment history
          </p>
        </div>
        <Link href="/dashboard/billing/upgrade">
          <Button className="gap-1.5">
            <ArrowUpRight className="h-4 w-4" />
            {hasSubscription ? "Change Plan" : "Upgrade"}
          </Button>
        </Link>
      </div>

      {/* Current plan card */}
      <div className="rounded-2xl border bg-white p-6 shadow-sm">
        <div className="flex items-center gap-2 mb-4">
          <CreditCard className="h-5 w-5 text-brand-green" />
          <h2 className="text-sm font-semibold uppercase tracking-wide text-muted-foreground">
            Current Plan
          </h2>
        </div>

        {hasSubscription ? (
          <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-4">
            {/* Plan name */}
            <div>
              <p className="text-xs text-muted-foreground mb-1">Plan</p>
              <p className="text-lg font-bold text-foreground">
                {subscription.planDisplayName}
              </p>
              <p className="text-xs text-muted-foreground capitalize mt-0.5">
                {subscription.billingCycle} billing
              </p>
            </div>

            {/* Status */}
            <div>
              <p className="text-xs text-muted-foreground mb-1">Status</p>
              <SubscriptionStatusBadge status={subscription.status} />
              {subscription.status === "trial" && subscription.trialEnd && (
                <p className="mt-1 text-xs text-blue-600">
                  Trial ends {formatDate(subscription.trialEnd)}
                </p>
              )}
            </div>

            {/* Next billing date */}
            <div>
              <p className="text-xs text-muted-foreground mb-1">
                {subscription.status === "cancelled" ? "Access Until" : "Next Billing Date"}
              </p>
              <div className="flex items-center gap-1.5">
                <Calendar className="h-4 w-4 text-muted-foreground" />
                <p className="text-sm font-medium text-foreground">
                  {formatDate(subscription.currentPeriodEnd)}
                </p>
              </div>
            </div>

            {/* Member since */}
            <div>
              <p className="text-xs text-muted-foreground mb-1">Member Since</p>
              <p className="text-sm font-medium text-foreground">
                {formatDate(subscription.createdAt)}
              </p>
            </div>
          </div>
        ) : (
          <div className="flex flex-col items-center py-8 text-center">
            <div className="mb-3 flex h-12 w-12 items-center justify-center rounded-xl bg-muted/50">
              <CreditCard className="h-6 w-6 text-muted-foreground" />
            </div>
            <p className="mb-1 font-semibold text-foreground">No Active Subscription</p>
            <p className="mb-4 max-w-sm text-sm text-muted-foreground">
              You are currently on the Free plan. Upgrade to unlock featured listings, unlimited leads, and analytics.
            </p>
            <Link href="/dashboard/billing/upgrade">
              <Button className="gap-1.5">
                <ArrowUpRight className="h-4 w-4" />
                View Plans
              </Button>
            </Link>
          </div>
        )}

        {/* Cancel button */}
        {isActivePlan && (
          <div className="mt-6 border-t pt-4">
            <Dialog open={cancelDialogOpen} onOpenChange={setCancelDialogOpen}>
              <DialogTrigger
                className="text-sm font-medium text-muted-foreground transition-colors hover:text-destructive"
              >
                Cancel Subscription
              </DialogTrigger>
              <DialogContent className="sm:max-w-md">
                <DialogHeader>
                  <DialogTitle>Cancel Subscription</DialogTitle>
                  <DialogDescription>
                    Are you sure you want to cancel your{" "}
                    <strong>{subscription?.planDisplayName}</strong> subscription? Your
                    plan will remain active until{" "}
                    <strong>{formatDate(subscription?.currentPeriodEnd)}</strong>, after
                    which your account will revert to the Free plan.
                  </DialogDescription>
                </DialogHeader>
                <DialogFooter>
                  <Button
                    variant="destructive"
                    onClick={handleCancel}
                    disabled={cancelling}
                  >
                    {cancelling ? (
                      <>
                        <Loader2 className="mr-1 h-3 w-3 animate-spin" />
                        Cancelling...
                      </>
                    ) : (
                      "Yes, Cancel"
                    )}
                  </Button>
                  <DialogClose render={<Button variant="outline" />}>
                    Keep Subscription
                  </DialogClose>
                </DialogFooter>
              </DialogContent>
            </Dialog>
          </div>
        )}
      </div>

      {/* Payment history */}
      <div className="rounded-2xl border bg-white p-6 shadow-sm">
        <div className="flex items-center gap-2 mb-4">
          <Receipt className="h-5 w-5 text-brand-green" />
          <h2 className="text-sm font-semibold uppercase tracking-wide text-muted-foreground">
            Payment History
          </h2>
        </div>

        {payments.length > 0 ? (
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Date</TableHead>
                <TableHead>Amount</TableHead>
                <TableHead>Status</TableHead>
                <TableHead className="hidden sm:table-cell">Provider</TableHead>
                <TableHead className="hidden md:table-cell">Reference</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {payments.map((payment) => (
                <TableRow key={payment.id}>
                  <TableCell className="font-medium">
                    {formatShortDate(payment.paidAt ?? payment.createdAt)}
                  </TableCell>
                  <TableCell>
                    {formatZAR(payment.amount)}
                    <span className="ml-1 text-xs text-muted-foreground">
                      {payment.currency}
                    </span>
                  </TableCell>
                  <TableCell>
                    <PaymentStatusBadge status={payment.status} />
                  </TableCell>
                  <TableCell className="hidden capitalize sm:table-cell">
                    {payment.provider}
                  </TableCell>
                  <TableCell className="hidden md:table-cell">
                    <span className="text-xs text-muted-foreground">
                      {payment.externalId ?? "--"}
                    </span>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        ) : (
          <div className="flex flex-col items-center py-8 text-center">
            <Receipt className="mb-2 h-8 w-8 text-muted-foreground/40" />
            <p className="text-sm text-muted-foreground">
              No payments yet. Your payment history will appear here.
            </p>
          </div>
        )}
      </div>
    </div>
  );
}

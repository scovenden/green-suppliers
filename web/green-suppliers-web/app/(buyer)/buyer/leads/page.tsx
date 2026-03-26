"use client";

import { useCallback, useEffect, useState } from "react";
import Link from "next/link";
import { useAuth } from "@/lib/auth-context";
import { apiGetAuth } from "@/lib/api-client";
import type { BuyerLead, LeadStatus } from "@/lib/types";
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
  Send,
  Search,
  ArrowRight,
  AlertTriangle,
  ChevronDown,
  ChevronUp,
  ExternalLink,
} from "lucide-react";
import { cn } from "@/lib/utils";

function LeadStatusBadge({ status }: { status: LeadStatus }) {
  const config: Record<LeadStatus, { bg: string; text: string; label: string }> = {
    new: { bg: "bg-blue-100", text: "text-blue-700", label: "New" },
    contacted: {
      bg: "bg-green-100",
      text: "text-green-700",
      label: "Contacted",
    },
    closed: { bg: "bg-gray-100", text: "text-gray-600", label: "Closed" },
  };
  const c = config[status] ?? config.new;

  return (
    <span
      className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-semibold ${c.bg} ${c.text}`}
    >
      {c.label}
    </span>
  );
}

function formatDate(dateStr: string): string {
  const date = new Date(dateStr);
  return date.toLocaleDateString("en-ZA", {
    day: "numeric",
    month: "short",
    year: "numeric",
  });
}

function truncateMessage(msg: string, maxLen = 80): string {
  if (msg.length <= maxLen) return msg;
  return msg.slice(0, maxLen).trimEnd() + "...";
}

function TableSkeleton() {
  return (
    <div role="status" aria-label="Loading inquiries" className="space-y-3">
      {Array.from({ length: 5 }).map((_, i) => (
        <Skeleton key={i} className="h-12 rounded-lg" />
      ))}
      <span className="sr-only">Loading inquiries</span>
    </div>
  );
}

export default function BuyerInquiriesPage() {
  const { token } = useAuth();
  const [leads, setLeads] = useState<BuyerLead[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [expandedId, setExpandedId] = useState<string | null>(null);

  const fetchLeads = useCallback(async () => {
    if (!token) return;
    setLoading(true);
    setError(null);
    const res = await apiGetAuth<BuyerLead[]>("/buyer/me/leads", token);
    if (res.success && res.data) {
      setLeads(res.data);
    } else {
      setError(res.error?.message ?? "Failed to load inquiries");
    }
    setLoading(false);
  }, [token]);

  useEffect(() => {
    fetchLeads();
  }, [fetchLeads]);

  function toggleExpand(id: string) {
    setExpandedId((prev) => (prev === id ? null : id));
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">My Inquiries</h1>
          <p className="text-sm text-muted-foreground">
            Track the inquiries you have sent to suppliers
          </p>
        </div>
        <Link href="/suppliers">
          <Button variant="outline" size="sm">
            <Search className="mr-1 h-4 w-4" />
            Find Suppliers
          </Button>
        </Link>
      </div>

      {loading ? (
        <TableSkeleton />
      ) : error ? (
        <div
          role="alert"
          className="flex flex-col items-center justify-center gap-4 py-20"
        >
          <AlertTriangle
            className="h-10 w-10 text-destructive"
            aria-hidden="true"
          />
          <p className="text-sm text-muted-foreground">{error}</p>
          <Button variant="outline" onClick={fetchLeads}>
            Try Again
          </Button>
        </div>
      ) : leads.length === 0 ? (
        <div className="flex flex-col items-center justify-center gap-4 rounded-2xl border bg-white py-16 shadow-sm">
          <div className="flex h-16 w-16 items-center justify-center rounded-full bg-brand-green-light">
            <Send className="h-8 w-8 text-brand-green" aria-hidden="true" />
          </div>
          <h2 className="text-lg font-semibold text-foreground">
            No inquiries yet
          </h2>
          <p className="max-w-sm text-center text-sm text-muted-foreground">
            Find a supplier you are interested in and send an inquiry. Your
            conversations will appear here.
          </p>
          <Link href="/suppliers">
            <Button>
              Find a Supplier
              <ArrowRight className="ml-1 h-4 w-4" />
            </Button>
          </Link>
        </div>
      ) : (
        <div className="rounded-2xl border bg-white shadow-sm">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Supplier</TableHead>
                <TableHead className="hidden sm:table-cell">Message</TableHead>
                <TableHead>Status</TableHead>
                <TableHead className="hidden md:table-cell">Date</TableHead>
                <TableHead className="w-10">
                  <span className="sr-only">Expand</span>
                </TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {leads.map((lead) => {
                const isExpanded = expandedId === lead.id;
                return (
                  <TableRow
                    key={lead.id}
                    className={cn(
                      "cursor-pointer",
                      isExpanded && "bg-muted/30"
                    )}
                    onClick={() => toggleExpand(lead.id)}
                  >
                    <TableCell>
                      <div className="flex items-center gap-2">
                        <span className="font-medium">
                          {lead.supplierTradingName}
                        </span>
                      </div>
                      {/* Show message on mobile */}
                      {isExpanded && (
                        <div className="mt-3 space-y-3 sm:hidden">
                          <div>
                            <p className="text-xs font-medium text-muted-foreground">
                              Message
                            </p>
                            <p className="mt-1 whitespace-pre-wrap text-sm text-foreground">
                              {lead.message}
                            </p>
                          </div>
                          <div className="flex items-center gap-4 text-xs text-muted-foreground">
                            <span>Sent: {formatDate(lead.createdAt)}</span>
                          </div>
                          <Link
                            href={`/suppliers/${lead.supplierProfileId}`}
                            className="inline-flex items-center gap-1 text-xs font-medium text-brand-green hover:underline"
                            onClick={(e) => e.stopPropagation()}
                          >
                            View supplier
                            <ExternalLink className="h-3 w-3" />
                          </Link>
                        </div>
                      )}
                    </TableCell>
                    <TableCell className="hidden max-w-xs sm:table-cell">
                      {isExpanded ? (
                        <p className="whitespace-pre-wrap text-sm">
                          {lead.message}
                        </p>
                      ) : (
                        <span className="text-muted-foreground">
                          {truncateMessage(lead.message)}
                        </span>
                      )}
                    </TableCell>
                    <TableCell>
                      <LeadStatusBadge status={lead.status} />
                    </TableCell>
                    <TableCell className="hidden text-muted-foreground md:table-cell">
                      {formatDate(lead.createdAt)}
                    </TableCell>
                    <TableCell>
                      {isExpanded ? (
                        <ChevronUp className="h-4 w-4 text-muted-foreground" />
                      ) : (
                        <ChevronDown className="h-4 w-4 text-muted-foreground" />
                      )}
                    </TableCell>
                  </TableRow>
                );
              })}
            </TableBody>
          </Table>
        </div>
      )}
    </div>
  );
}

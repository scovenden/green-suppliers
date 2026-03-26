"use client";

import { useCallback, useEffect, useState } from "react";
import { toast } from "sonner";
import { useAuth } from "@/lib/auth-context";
import { apiGetAuth, apiPatch } from "@/lib/api-client";
import type { SupplierLead, LeadStatus } from "@/lib/types";
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
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import {
  Mail,
  AlertTriangle,
  ChevronDown,
  ChevronUp,
  MoreHorizontal,
  Phone,
  Building2,
  MessageSquare,
  CheckCircle,
  XCircle,
  Loader2,
  Inbox,
} from "lucide-react";
import { cn } from "@/lib/utils";

type FilterTab = "all" | "new" | "contacted" | "closed";

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

function formatDateTime(dateStr: string): string {
  const date = new Date(dateStr);
  return date.toLocaleString("en-ZA", {
    day: "numeric",
    month: "short",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
}

function truncateMessage(msg: string, maxLen = 60): string {
  if (msg.length <= maxLen) return msg;
  return msg.slice(0, maxLen).trimEnd() + "...";
}

function TableSkeleton() {
  return (
    <div role="status" aria-label="Loading leads" className="space-y-3">
      {Array.from({ length: 6 }).map((_, i) => (
        <Skeleton key={i} className="h-12 rounded-lg" />
      ))}
      <span className="sr-only">Loading leads</span>
    </div>
  );
}

export default function SupplierLeadsInboxPage() {
  const { token } = useAuth();
  const [leads, setLeads] = useState<SupplierLead[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [expandedId, setExpandedId] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<FilterTab>("all");
  const [updatingId, setUpdatingId] = useState<string | null>(null);

  const fetchLeads = useCallback(async () => {
    if (!token) return;
    setLoading(true);
    setError(null);
    const res = await apiGetAuth<SupplierLead[]>(
      "/supplier/me/leads",
      token
    );
    if (res.success && res.data) {
      setLeads(res.data);
    } else {
      setError(res.error?.message ?? "Failed to load leads");
    }
    setLoading(false);
  }, [token]);

  useEffect(() => {
    fetchLeads();
  }, [fetchLeads]);

  async function handleStatusUpdate(leadId: string, status: LeadStatus) {
    if (!token) return;
    setUpdatingId(leadId);

    const res = await apiPatch(
      `/supplier/me/leads/${leadId}/status`,
      { status },
      token
    );

    if (res.success) {
      setLeads((prev) =>
        prev.map((l) =>
          l.id === leadId ? { ...l, status, updatedAt: new Date().toISOString() } : l
        )
      );
      toast.success(
        `Lead marked as ${status === "contacted" ? "Contacted" : "Closed"}`
      );
    } else {
      toast.error(res.error?.message ?? "Failed to update lead status");
    }
    setUpdatingId(null);
  }

  function toggleExpand(id: string) {
    setExpandedId((prev) => (prev === id ? null : id));
  }

  const newCount = leads.filter((l) => l.status === "new").length;
  const contactedCount = leads.filter((l) => l.status === "contacted").length;
  const closedCount = leads.filter((l) => l.status === "closed").length;

  const filteredLeads =
    activeTab === "all"
      ? leads
      : leads.filter((l) => l.status === activeTab);

  const tabs: { key: FilterTab; label: string; count: number }[] = [
    { key: "all", label: "All", count: leads.length },
    { key: "new", label: "New", count: newCount },
    { key: "contacted", label: "Contacted", count: contactedCount },
    { key: "closed", label: "Closed", count: closedCount },
  ];

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Leads Inbox</h1>
        <p className="text-sm text-muted-foreground">
          Manage inquiries from potential buyers
        </p>
      </div>

      {/* Filter tabs */}
      <div className="flex flex-wrap gap-2">
        {tabs.map((tab) => (
          <button
            key={tab.key}
            onClick={() => setActiveTab(tab.key)}
            className={cn(
              "inline-flex items-center gap-2 rounded-lg px-4 py-2 text-sm font-medium transition-colors",
              activeTab === tab.key
                ? "bg-brand-green text-white"
                : "bg-white text-muted-foreground hover:bg-muted"
            )}
          >
            {tab.label}
            <span
              className={cn(
                "inline-flex h-5 min-w-5 items-center justify-center rounded-full px-1.5 text-xs font-semibold",
                activeTab === tab.key
                  ? "bg-white/20 text-white"
                  : tab.key === "new" && tab.count > 0
                  ? "bg-blue-100 text-blue-700"
                  : "bg-muted text-muted-foreground"
              )}
            >
              {tab.count}
            </span>
          </button>
        ))}
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
      ) : filteredLeads.length === 0 ? (
        <div className="flex flex-col items-center justify-center gap-4 rounded-2xl border bg-white py-16 shadow-sm">
          <div className="flex h-16 w-16 items-center justify-center rounded-full bg-brand-green-light">
            <Inbox className="h-8 w-8 text-brand-green" aria-hidden="true" />
          </div>
          <h2 className="text-lg font-semibold text-foreground">
            {activeTab === "all"
              ? "No leads yet"
              : `No ${activeTab} leads`}
          </h2>
          <p className="max-w-sm text-center text-sm text-muted-foreground">
            {activeTab === "all"
              ? "When buyers send inquiries about your products or services, they will appear here."
              : `You have no leads with "${activeTab}" status.`}
          </p>
        </div>
      ) : (
        <div className="rounded-2xl border bg-white shadow-sm">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Contact</TableHead>
                <TableHead className="hidden sm:table-cell">Company</TableHead>
                <TableHead className="hidden md:table-cell">Message</TableHead>
                <TableHead>Status</TableHead>
                <TableHead className="hidden lg:table-cell">Date</TableHead>
                <TableHead className="w-20">
                  <span className="sr-only">Actions</span>
                </TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {filteredLeads.map((lead) => {
                const isExpanded = expandedId === lead.id;
                const isUpdating = updatingId === lead.id;

                return (
                  <TableRow
                    key={lead.id}
                    className={cn(
                      "cursor-pointer",
                      isExpanded && "bg-muted/30",
                      lead.status === "new" && "font-medium"
                    )}
                    onClick={() => toggleExpand(lead.id)}
                  >
                    <TableCell>
                      <div>
                        <p
                          className={cn(
                            "text-sm",
                            lead.status === "new" && "font-semibold"
                          )}
                        >
                          {lead.contactName}
                        </p>
                        <p className="text-xs text-muted-foreground">
                          {lead.contactEmail}
                        </p>
                      </div>

                      {/* Expanded detail on mobile */}
                      {isExpanded && (
                        <div className="mt-3 space-y-3 md:hidden">
                          {lead.companyName && (
                            <div className="flex items-center gap-2 text-sm text-muted-foreground">
                              <Building2 className="h-3.5 w-3.5" />
                              {lead.companyName}
                            </div>
                          )}
                          {lead.contactPhone && (
                            <div className="flex items-center gap-2 text-sm text-muted-foreground">
                              <Phone className="h-3.5 w-3.5" />
                              {lead.contactPhone}
                            </div>
                          )}
                          <div>
                            <p className="text-xs font-medium text-muted-foreground">
                              Message
                            </p>
                            <p className="mt-1 whitespace-pre-wrap text-sm text-foreground">
                              {lead.message}
                            </p>
                          </div>
                          <p className="text-xs text-muted-foreground">
                            Received: {formatDateTime(lead.createdAt)}
                          </p>
                        </div>
                      )}
                    </TableCell>
                    <TableCell className="hidden sm:table-cell">
                      <span className="text-sm text-muted-foreground">
                        {lead.companyName ?? "-"}
                      </span>
                    </TableCell>
                    <TableCell className="hidden max-w-xs md:table-cell">
                      {isExpanded ? (
                        <div className="space-y-2">
                          <p className="whitespace-pre-wrap text-sm">
                            {lead.message}
                          </p>
                          {lead.contactPhone && (
                            <div className="flex items-center gap-1.5 text-xs text-muted-foreground">
                              <Phone className="h-3 w-3" />
                              {lead.contactPhone}
                            </div>
                          )}
                          <p className="text-xs text-muted-foreground">
                            Received: {formatDateTime(lead.createdAt)}
                          </p>
                        </div>
                      ) : (
                        <span className="text-sm text-muted-foreground">
                          {truncateMessage(lead.message)}
                        </span>
                      )}
                    </TableCell>
                    <TableCell>
                      <LeadStatusBadge status={lead.status} />
                    </TableCell>
                    <TableCell className="hidden text-sm text-muted-foreground lg:table-cell">
                      {formatDate(lead.createdAt)}
                    </TableCell>
                    <TableCell>
                      <div
                        className="flex items-center gap-1"
                        onClick={(e) => e.stopPropagation()}
                      >
                        {isUpdating ? (
                          <Loader2 className="h-4 w-4 animate-spin text-muted-foreground" />
                        ) : (
                          <>
                            {/* Expand toggle */}
                            <button
                              className="rounded-md p-1 text-muted-foreground hover:bg-muted"
                              onClick={(e) => {
                                e.stopPropagation();
                                toggleExpand(lead.id);
                              }}
                              aria-label={
                                isExpanded ? "Collapse lead" : "Expand lead"
                              }
                            >
                              {isExpanded ? (
                                <ChevronUp className="h-4 w-4" />
                              ) : (
                                <ChevronDown className="h-4 w-4" />
                              )}
                            </button>

                            {/* Action dropdown */}
                            {lead.status !== "closed" && (
                              <DropdownMenu>
                                <DropdownMenuTrigger
                                  className="rounded-md p-1 text-muted-foreground hover:bg-muted focus-visible:ring-2 focus-visible:ring-ring"
                                  aria-label={`Actions for lead from ${lead.contactName}`}
                                >
                                  <MoreHorizontal className="h-4 w-4" />
                                </DropdownMenuTrigger>
                                <DropdownMenuContent
                                  align="end"
                                  sideOffset={4}
                                >
                                  {lead.status === "new" && (
                                    <DropdownMenuItem
                                      className="cursor-pointer"
                                      onClick={() =>
                                        handleStatusUpdate(
                                          lead.id,
                                          "contacted"
                                        )
                                      }
                                    >
                                      <CheckCircle className="mr-2 h-4 w-4 text-green-600" />
                                      Mark as Contacted
                                    </DropdownMenuItem>
                                  )}
                                  <DropdownMenuItem
                                    className="cursor-pointer"
                                    onClick={() =>
                                      handleStatusUpdate(lead.id, "closed")
                                    }
                                  >
                                    <XCircle className="mr-2 h-4 w-4 text-gray-500" />
                                    Mark as Closed
                                  </DropdownMenuItem>
                                  {lead.contactEmail && (
                                    <DropdownMenuItem className="cursor-pointer">
                                      <a
                                        href={`mailto:${lead.contactEmail}`}
                                        className="flex items-center"
                                        onClick={(e) => e.stopPropagation()}
                                      >
                                        <Mail className="mr-2 h-4 w-4" />
                                        Send Email
                                      </a>
                                    </DropdownMenuItem>
                                  )}
                                </DropdownMenuContent>
                              </DropdownMenu>
                            )}
                          </>
                        )}
                      </div>
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

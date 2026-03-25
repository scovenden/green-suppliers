"use client";

import { useCallback, useEffect, useState } from "react";
import { useAuth } from "@/lib/auth-context";
import { apiGetAuth, apiPatch } from "@/lib/api-client";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
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
import type { AdminLead, LeadStatus } from "@/lib/types";
import { MoreHorizontal, Loader2 } from "lucide-react";

const STATUS_TABS: { label: string; value: LeadStatus | "all" }[] = [
  { label: "All", value: "all" },
  { label: "New", value: "new" },
  { label: "Contacted", value: "contacted" },
  { label: "Closed", value: "closed" },
];

function getStatusBadge(status: LeadStatus) {
  switch (status) {
    case "new":
      return <Badge className="bg-blue-500 text-white">New</Badge>;
    case "contacted":
      return <Badge className="bg-amber-500 text-white">Contacted</Badge>;
    case "closed":
      return <Badge variant="outline">Closed</Badge>;
    default:
      return <Badge variant="outline">{status}</Badge>;
  }
}

export default function AdminLeadsPage() {
  const { token } = useAuth();
  const [leads, setLeads] = useState<AdminLead[]>([]);
  const [loading, setLoading] = useState(true);
  const [statusFilter, setStatusFilter] = useState<LeadStatus | "all">("all");
  const [actionLoading, setActionLoading] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);

  const fetchLeads = useCallback(async () => {
    if (!token) return;
    setLoading(true);
    const statusParam =
      statusFilter === "all" ? "" : `&status=${statusFilter}`;
    const res = await apiGetAuth<AdminLead[]>(
      `/admin/leads?page=${page}&pageSize=20${statusParam}`,
      token
    );
    if (res.success && res.data) {
      setLeads(res.data);
      if (res.meta) {
        setTotalPages(res.meta.totalPages);
      }
    }
    setLoading(false);
  }, [token, page, statusFilter]);

  useEffect(() => {
    fetchLeads();
  }, [fetchLeads]);

  useEffect(() => {
    setPage(1);
  }, [statusFilter]);

  async function handleStatusUpdate(leadId: string, newStatus: LeadStatus) {
    if (!token) return;
    setActionLoading(leadId);
    const res = await apiPatch(
      `/admin/leads/${leadId}/status`,
      { status: newStatus },
      token
    );
    if (res.success) {
      await fetchLeads();
    }
    setActionLoading(null);
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-foreground">Leads</h1>
        <p className="text-sm text-muted-foreground">
          Manage buyer inquiries to suppliers
        </p>
      </div>

      {/* Status filter tabs */}
      <div className="flex gap-1 rounded-lg bg-muted p-1">
        {STATUS_TABS.map((tab) => (
          <button
            key={tab.value}
            onClick={() => setStatusFilter(tab.value)}
            className={`rounded-md px-3 py-1.5 text-sm font-medium transition-colors ${
              statusFilter === tab.value
                ? "bg-white text-foreground shadow-sm"
                : "text-muted-foreground hover:text-foreground"
            }`}
          >
            {tab.label}
          </button>
        ))}
      </div>

      <div className="rounded-lg border bg-white">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Contact</TableHead>
              <TableHead>Email</TableHead>
              <TableHead>Company</TableHead>
              <TableHead>Supplier</TableHead>
              <TableHead>Status</TableHead>
              <TableHead>Date</TableHead>
              <TableHead className="w-[70px]">Actions</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {loading ? (
              Array.from({ length: 5 }).map((_, i) => (
                <TableRow key={i}>
                  {Array.from({ length: 7 }).map((_, j) => (
                    <TableCell key={j}>
                      <div className="h-4 w-20 animate-pulse rounded bg-muted" />
                    </TableCell>
                  ))}
                </TableRow>
              ))
            ) : leads.length === 0 ? (
              <TableRow>
                <TableCell colSpan={7} className="py-8 text-center">
                  <p className="text-muted-foreground">
                    {statusFilter === "all"
                      ? "No leads yet."
                      : `No ${statusFilter} leads.`}
                  </p>
                </TableCell>
              </TableRow>
            ) : (
              leads.map((lead) => (
                <TableRow key={lead.id}>
                  <TableCell className="font-medium">
                    {lead.contactName}
                  </TableCell>
                  <TableCell>
                    <a
                      href={`mailto:${lead.contactEmail}`}
                      className="text-brand-emerald hover:underline"
                    >
                      {lead.contactEmail}
                    </a>
                  </TableCell>
                  <TableCell>{lead.companyName ?? "-"}</TableCell>
                  <TableCell>
                    {lead.supplierTradingName ?? "-"}
                  </TableCell>
                  <TableCell>{getStatusBadge(lead.status)}</TableCell>
                  <TableCell className="text-xs text-muted-foreground">
                    {new Date(lead.createdAt).toLocaleDateString()}
                  </TableCell>
                  <TableCell>
                    {actionLoading === lead.id ? (
                      <Loader2 className="h-4 w-4 animate-spin text-muted-foreground" />
                    ) : (
                      <DropdownMenu>
                        <DropdownMenuTrigger
                          className="flex h-8 w-8 items-center justify-center rounded-md hover:bg-muted"
                        >
                          <MoreHorizontal className="h-4 w-4" />
                        </DropdownMenuTrigger>
                        <DropdownMenuContent align="end">
                          {lead.status !== "new" && (
                            <DropdownMenuItem
                              onClick={() =>
                                handleStatusUpdate(lead.id, "new")
                              }
                            >
                              Mark as New
                            </DropdownMenuItem>
                          )}
                          {lead.status !== "contacted" && (
                            <DropdownMenuItem
                              onClick={() =>
                                handleStatusUpdate(lead.id, "contacted")
                              }
                            >
                              Mark as Contacted
                            </DropdownMenuItem>
                          )}
                          {lead.status !== "closed" && (
                            <DropdownMenuItem
                              onClick={() =>
                                handleStatusUpdate(lead.id, "closed")
                              }
                            >
                              Mark as Closed
                            </DropdownMenuItem>
                          )}
                        </DropdownMenuContent>
                      </DropdownMenu>
                    )}
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>

        {/* Pagination */}
        {totalPages > 1 && (
          <div className="flex items-center justify-between border-t px-4 py-3">
            <p className="text-sm text-muted-foreground">
              Page {page} of {totalPages}
            </p>
            <div className="flex gap-2">
              <Button
                variant="outline"
                size="sm"
                disabled={page <= 1}
                onClick={() => setPage((p) => Math.max(1, p - 1))}
              >
                Previous
              </Button>
              <Button
                variant="outline"
                size="sm"
                disabled={page >= totalPages}
                onClick={() => setPage((p) => p + 1)}
              >
                Next
              </Button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}

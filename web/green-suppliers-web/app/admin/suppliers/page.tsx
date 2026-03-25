"use client";

import { useCallback, useEffect, useState } from "react";
import Link from "next/link";
import { useAuth } from "@/lib/auth-context";
import { apiGetAuth, apiPost, apiPatch } from "@/lib/api-client";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
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
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import type { AdminSupplier, ApiResponse } from "@/lib/types";
import { getEsgBadgeColor } from "@/lib/types";
import {
  Plus,
  MoreHorizontal,
  Flag,
  Eye,
  EyeOff,
  RefreshCw,
  Loader2,
} from "lucide-react";

export default function AdminSuppliersPage() {
  const { token } = useAuth();
  const [suppliers, setSuppliers] = useState<AdminSupplier[]>([]);
  const [loading, setLoading] = useState(true);
  const [actionLoading, setActionLoading] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);

  const fetchSuppliers = useCallback(async () => {
    if (!token) return;
    setLoading(true);
    const res = await apiGetAuth<AdminSupplier[]>(
      `/admin/suppliers?page=${page}&pageSize=20`,
      token
    );
    if (res.success && res.data) {
      setSuppliers(res.data);
      if (res.meta) {
        setTotalPages(res.meta.totalPages);
      }
    }
    setLoading(false);
  }, [token, page]);

  useEffect(() => {
    fetchSuppliers();
  }, [fetchSuppliers]);

  async function handleAction(
    supplierId: string,
    action: "flag" | "unflag" | "publish" | "unpublish" | "rescore"
  ) {
    if (!token) return;
    setActionLoading(supplierId);

    let res: ApiResponse<unknown>;
    switch (action) {
      case "flag":
        res = await apiPatch(`/admin/suppliers/${supplierId}/flag`, {}, token);
        break;
      case "unflag":
        res = await apiPatch(`/admin/suppliers/${supplierId}/unflag`, {}, token);
        break;
      case "publish":
        res = await apiPatch(
          `/admin/suppliers/${supplierId}/publish`,
          {},
          token
        );
        break;
      case "unpublish":
        res = await apiPatch(
          `/admin/suppliers/${supplierId}/unpublish`,
          {},
          token
        );
        break;
      case "rescore":
        res = await apiPost(
          `/admin/suppliers/${supplierId}/rescore`,
          {},
          token
        );
        break;
    }

    if (res!.success) {
      await fetchSuppliers();
    }
    setActionLoading(null);
  }

  function getVerificationBadge(status: string) {
    switch (status.toLowerCase()) {
      case "verified":
        return <Badge className="bg-brand-green text-white">Verified</Badge>;
      case "pending":
        return <Badge className="bg-amber-500 text-white">Pending</Badge>;
      case "flagged":
        return <Badge variant="destructive">Flagged</Badge>;
      default:
        return <Badge variant="outline">Unverified</Badge>;
    }
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-foreground">Suppliers</h1>
          <p className="text-sm text-muted-foreground">
            Manage supplier profiles in the directory
          </p>
        </div>
        <Link href="/admin/suppliers/new">
          <Button>
            <Plus className="mr-1 h-4 w-4" />
            Create Supplier
          </Button>
        </Link>
      </div>

      <div className="rounded-lg border bg-white">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Name</TableHead>
              <TableHead>Country</TableHead>
              <TableHead>ESG Level</TableHead>
              <TableHead>Verification</TableHead>
              <TableHead>Published</TableHead>
              <TableHead className="w-[70px]">Actions</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {loading ? (
              Array.from({ length: 5 }).map((_, i) => (
                <TableRow key={i}>
                  {Array.from({ length: 6 }).map((_, j) => (
                    <TableCell key={j}>
                      <div className="h-4 w-20 animate-pulse rounded bg-muted" />
                    </TableCell>
                  ))}
                </TableRow>
              ))
            ) : suppliers.length === 0 ? (
              <TableRow>
                <TableCell colSpan={6} className="py-8 text-center">
                  <p className="text-muted-foreground">
                    No suppliers found. Create one to get started.
                  </p>
                </TableCell>
              </TableRow>
            ) : (
              suppliers.map((supplier) => {
                const esgColor = getEsgBadgeColor(supplier.esgLevel);
                return (
                  <TableRow key={supplier.id}>
                    <TableCell>
                      <div>
                        <p className="font-medium">
                          {supplier.tradingName}
                        </p>
                        <p className="text-xs text-muted-foreground">
                          {supplier.organizationName}
                        </p>
                      </div>
                    </TableCell>
                    <TableCell>{supplier.countryCode}</TableCell>
                    <TableCell>
                      <span
                        className={`inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium ${esgColor.bg} ${esgColor.text}`}
                      >
                        {supplier.esgLevel}
                      </span>
                    </TableCell>
                    <TableCell>
                      {getVerificationBadge(supplier.verificationStatus)}
                    </TableCell>
                    <TableCell>
                      {supplier.isPublished ? (
                        <Badge className="bg-brand-green/10 text-brand-green">
                          Published
                        </Badge>
                      ) : (
                        <Badge variant="outline">Draft</Badge>
                      )}
                    </TableCell>
                    <TableCell>
                      {actionLoading === supplier.id ? (
                        <Loader2 className="h-4 w-4 animate-spin text-muted-foreground" />
                      ) : (
                        <DropdownMenu>
                          <DropdownMenuTrigger
                            className="flex h-8 w-8 items-center justify-center rounded-md hover:bg-muted"
                          >
                            <MoreHorizontal className="h-4 w-4" />
                          </DropdownMenuTrigger>
                          <DropdownMenuContent align="end">
                            <DropdownMenuItem
                              onClick={() =>
                                handleAction(
                                  supplier.id,
                                  supplier.isPublished
                                    ? "unpublish"
                                    : "publish"
                                )
                              }
                            >
                              {supplier.isPublished ? (
                                <>
                                  <EyeOff className="mr-2 h-4 w-4" />
                                  Unpublish
                                </>
                              ) : (
                                <>
                                  <Eye className="mr-2 h-4 w-4" />
                                  Publish
                                </>
                              )}
                            </DropdownMenuItem>
                            <DropdownMenuItem
                              onClick={() =>
                                handleAction(
                                  supplier.id,
                                  supplier.isFlagged ? "unflag" : "flag"
                                )
                              }
                            >
                              <Flag className="mr-2 h-4 w-4" />
                              {supplier.isFlagged ? "Unflag" : "Flag"}
                            </DropdownMenuItem>
                            <DropdownMenuSeparator />
                            <DropdownMenuItem
                              onClick={() =>
                                handleAction(supplier.id, "rescore")
                              }
                            >
                              <RefreshCw className="mr-2 h-4 w-4" />
                              Rescore ESG
                            </DropdownMenuItem>
                          </DropdownMenuContent>
                        </DropdownMenu>
                      )}
                    </TableCell>
                  </TableRow>
                );
              })
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

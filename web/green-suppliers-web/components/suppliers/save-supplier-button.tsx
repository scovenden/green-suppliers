"use client";

import { useState } from "react";
import { toast } from "sonner";
import { apiPost, apiDelete } from "@/lib/api-client";
import { Bookmark, Loader2 } from "lucide-react";
import { cn } from "@/lib/utils";

interface SaveSupplierButtonProps {
  supplierProfileId: string;
  savedId?: string | null;
  token: string;
  className?: string;
}

/**
 * A bookmark button that saves/unsaves a supplier for the authenticated buyer.
 * Renders as a small floating icon button.
 */
export function SaveSupplierButton({
  supplierProfileId,
  savedId: initialSavedId,
  token,
  className,
}: SaveSupplierButtonProps) {
  const [savedId, setSavedId] = useState<string | null>(
    initialSavedId ?? null
  );
  const [loading, setLoading] = useState(false);

  const isSaved = savedId !== null;

  async function handleToggle(e: React.MouseEvent) {
    e.preventDefault();
    e.stopPropagation();

    if (loading) return;
    setLoading(true);

    if (isSaved) {
      // Unsave
      const res = await apiDelete(`/buyer/me/saved-suppliers/${savedId}`, token);
      if (res.success) {
        setSavedId(null);
        toast.success("Supplier removed from saved list");
      } else {
        toast.error(res.error?.message ?? "Failed to remove supplier");
      }
    } else {
      // Save
      const res = await apiPost<{ id: string }>(
        "/buyer/me/saved-suppliers",
        { supplierProfileId },
        token
      );
      if (res.success && res.data) {
        setSavedId(res.data.id);
        toast.success("Supplier saved to your list");
      } else {
        toast.error(res.error?.message ?? "Failed to save supplier");
      }
    }

    setLoading(false);
  }

  return (
    <button
      onClick={handleToggle}
      disabled={loading}
      className={cn(
        "flex h-8 w-8 items-center justify-center rounded-full shadow-sm backdrop-blur-sm transition-all",
        isSaved
          ? "bg-brand-green text-white hover:bg-brand-green-hover"
          : "bg-white/80 text-muted-foreground hover:bg-white hover:text-brand-green",
        "disabled:opacity-50",
        className
      )}
      aria-label={isSaved ? "Remove from saved suppliers" : "Save supplier"}
      title={isSaved ? "Remove from saved" : "Save supplier"}
    >
      {loading ? (
        <Loader2 className="h-4 w-4 animate-spin" />
      ) : (
        <Bookmark
          className={cn("h-4 w-4", isSaved && "fill-current")}
        />
      )}
    </button>
  );
}

"use client";

import { useCallback, useEffect, useState } from "react";
import { useAuth } from "@/lib/auth-context";
import { apiGetAuth, apiPost, apiPut, apiPatch } from "@/lib/api-client";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Badge } from "@/components/ui/badge";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import type { ContentPage } from "@/lib/types";
import {
  Plus,
  X,
  Eye,
  EyeOff,
  Pencil,
  Loader2,
} from "lucide-react";

interface ContentFormData {
  title: string;
  slug: string;
  body: string;
  pageType: string;
  metaTitle: string;
  metaDesc: string;
}

const EMPTY_FORM: ContentFormData = {
  title: "",
  slug: "",
  body: "",
  pageType: "guide",
  metaTitle: "",
  metaDesc: "",
};

const PAGE_TYPES = ["guide", "industry", "country", "pillar"];

export default function AdminContentPage() {
  const { token } = useAuth();
  const [pages, setPages] = useState<ContentPage[]>([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [form, setForm] = useState<ContentFormData>(EMPTY_FORM);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [actionLoading, setActionLoading] = useState<string | null>(null);

  const fetchPages = useCallback(async () => {
    if (!token) return;
    setLoading(true);
    const res = await apiGetAuth<ContentPage[]>(
      "/admin/content?pageSize=100",
      token
    );
    if (res.success && res.data) {
      setPages(res.data);
    }
    setLoading(false);
  }, [token]);

  useEffect(() => {
    fetchPages();
  }, [fetchPages]);

  function openCreate() {
    setForm(EMPTY_FORM);
    setEditingId(null);
    setError(null);
    setShowForm(true);
  }

  function openEdit(page: ContentPage) {
    setForm({
      title: page.title,
      slug: page.slug,
      body: page.body,
      pageType: page.pageType,
      metaTitle: page.metaTitle ?? "",
      metaDesc: page.metaDesc ?? "",
    });
    setEditingId(page.id);
    setError(null);
    setShowForm(true);
  }

  function closeForm() {
    setShowForm(false);
    setEditingId(null);
    setForm(EMPTY_FORM);
    setError(null);
  }

  function updateField<K extends keyof ContentFormData>(
    key: K,
    value: ContentFormData[K]
  ) {
    setForm((prev) => ({ ...prev, [key]: value }));
  }

  function autoSlug(title: string) {
    return title
      .toLowerCase()
      .replace(/[^a-z0-9]+/g, "-")
      .replace(/(^-|-$)/g, "");
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!token) return;
    setError(null);
    setIsSubmitting(true);

    const payload = {
      ...form,
      metaTitle: form.metaTitle || undefined,
      metaDesc: form.metaDesc || undefined,
    };

    const res = editingId
      ? await apiPut(`/admin/content/${editingId}`, payload, token)
      : await apiPost("/admin/content", payload, token);

    if (res.success) {
      closeForm();
      await fetchPages();
    } else {
      setError(res.error?.message ?? "Failed to save content page");
    }
    setIsSubmitting(false);
  }

  async function handleTogglePublish(page: ContentPage) {
    if (!token) return;
    setActionLoading(page.id);
    const action = page.isPublished ? "unpublish" : "publish";
    const res = await apiPatch(
      `/admin/content/${page.id}/${action}`,
      {},
      token
    );
    if (res.success) {
      await fetchPages();
    }
    setActionLoading(null);
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-foreground">Content</h1>
          <p className="text-sm text-muted-foreground">
            Manage SEO and informational pages
          </p>
        </div>
        {!showForm && (
          <Button onClick={openCreate}>
            <Plus className="mr-1 h-4 w-4" />
            New Page
          </Button>
        )}
      </div>

      {/* Create / Edit form */}
      {showForm && (
        <Card>
          <CardHeader className="flex flex-row items-center justify-between">
            <CardTitle>
              {editingId ? "Edit Page" : "Create Page"}
            </CardTitle>
            <button
              onClick={closeForm}
              className="text-muted-foreground hover:text-foreground"
            >
              <X className="h-5 w-5" />
            </button>
          </CardHeader>
          <CardContent>
            <form onSubmit={handleSubmit} className="space-y-4">
              {error && (
                <div className="rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">
                  {error}
                </div>
              )}
              <div className="grid gap-4 sm:grid-cols-2">
                <div className="space-y-2">
                  <Label htmlFor="content-title">Title *</Label>
                  <Input
                    id="content-title"
                    value={form.title}
                    onChange={(e) => {
                      updateField("title", e.target.value);
                      if (!editingId) {
                        updateField("slug", autoSlug(e.target.value));
                      }
                    }}
                    required
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="content-slug">Slug *</Label>
                  <Input
                    id="content-slug"
                    value={form.slug}
                    onChange={(e) => updateField("slug", e.target.value)}
                    required
                    pattern="[a-z0-9-]+"
                    title="Lowercase letters, numbers, and hyphens only"
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="content-type">Page Type *</Label>
                  <select
                    id="content-type"
                    value={form.pageType}
                    onChange={(e) => updateField("pageType", e.target.value)}
                    className="flex h-8 w-full rounded-lg border border-input bg-transparent px-2.5 py-1 text-sm outline-none focus-visible:border-ring focus-visible:ring-3 focus-visible:ring-ring/50"
                  >
                    {PAGE_TYPES.map((type) => (
                      <option key={type} value={type}>
                        {type.charAt(0).toUpperCase() + type.slice(1)}
                      </option>
                    ))}
                  </select>
                </div>
              </div>
              <div className="space-y-2">
                <Label htmlFor="content-body">Body *</Label>
                <Textarea
                  id="content-body"
                  value={form.body}
                  onChange={(e) => updateField("body", e.target.value)}
                  rows={10}
                  required
                  placeholder="HTML or Markdown content..."
                />
              </div>
              <div className="grid gap-4 sm:grid-cols-2">
                <div className="space-y-2">
                  <Label htmlFor="content-metaTitle">Meta Title</Label>
                  <Input
                    id="content-metaTitle"
                    value={form.metaTitle}
                    onChange={(e) =>
                      updateField("metaTitle", e.target.value)
                    }
                    maxLength={70}
                    placeholder="SEO title (max 70 chars)"
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="content-metaDesc">Meta Description</Label>
                  <Input
                    id="content-metaDesc"
                    value={form.metaDesc}
                    onChange={(e) =>
                      updateField("metaDesc", e.target.value)
                    }
                    maxLength={160}
                    placeholder="SEO description (max 160 chars)"
                  />
                </div>
              </div>
              <div className="flex justify-end gap-3">
                <Button
                  type="button"
                  variant="outline"
                  onClick={closeForm}
                >
                  Cancel
                </Button>
                <Button type="submit" disabled={isSubmitting}>
                  {isSubmitting ? (
                    <>
                      <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                      Saving...
                    </>
                  ) : editingId ? (
                    "Update Page"
                  ) : (
                    "Create Page"
                  )}
                </Button>
              </div>
            </form>
          </CardContent>
        </Card>
      )}

      {/* Pages table */}
      <div className="rounded-lg border bg-white">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Title</TableHead>
              <TableHead>Slug</TableHead>
              <TableHead>Type</TableHead>
              <TableHead>Status</TableHead>
              <TableHead>Updated</TableHead>
              <TableHead className="w-[100px]">Actions</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {loading ? (
              Array.from({ length: 3 }).map((_, i) => (
                <TableRow key={i}>
                  {Array.from({ length: 6 }).map((_, j) => (
                    <TableCell key={j}>
                      <div className="h-4 w-20 animate-pulse rounded bg-muted" />
                    </TableCell>
                  ))}
                </TableRow>
              ))
            ) : pages.length === 0 ? (
              <TableRow>
                <TableCell colSpan={6} className="py-8 text-center">
                  <p className="text-muted-foreground">
                    No content pages yet. Create one to get started.
                  </p>
                </TableCell>
              </TableRow>
            ) : (
              pages.map((page) => (
                <TableRow key={page.id}>
                  <TableCell className="font-medium">{page.title}</TableCell>
                  <TableCell className="text-xs text-muted-foreground">
                    /{page.slug}
                  </TableCell>
                  <TableCell>
                    <Badge variant="outline">{page.pageType}</Badge>
                  </TableCell>
                  <TableCell>
                    {page.isPublished ? (
                      <Badge className="bg-brand-green/10 text-brand-green">
                        Published
                      </Badge>
                    ) : (
                      <Badge variant="outline">Draft</Badge>
                    )}
                  </TableCell>
                  <TableCell className="text-xs text-muted-foreground">
                    {new Date(page.updatedAt).toLocaleDateString()}
                  </TableCell>
                  <TableCell>
                    <div className="flex items-center gap-1">
                      <button
                        onClick={() => openEdit(page)}
                        className="flex h-8 w-8 items-center justify-center rounded-md text-muted-foreground hover:bg-muted hover:text-foreground"
                        title="Edit"
                      >
                        <Pencil className="h-4 w-4" />
                      </button>
                      {actionLoading === page.id ? (
                        <Loader2 className="h-4 w-4 animate-spin text-muted-foreground" />
                      ) : (
                        <button
                          onClick={() => handleTogglePublish(page)}
                          className="flex h-8 w-8 items-center justify-center rounded-md text-muted-foreground hover:bg-muted hover:text-foreground"
                          title={
                            page.isPublished ? "Unpublish" : "Publish"
                          }
                        >
                          {page.isPublished ? (
                            <EyeOff className="h-4 w-4" />
                          ) : (
                            <Eye className="h-4 w-4" />
                          )}
                        </button>
                      )}
                    </div>
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </div>
    </div>
  );
}

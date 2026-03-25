"use client";

import { useCallback, useEffect, useState } from "react";
import { useAuth } from "@/lib/auth-context";
import { apiGetAuth, apiPost, apiPut, apiDelete } from "@/lib/api-client";
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
import type { Industry, ServiceTag, CertificationType } from "@/lib/types";
import { Plus, Pencil, Trash2, Loader2, X } from "lucide-react";

type TabValue = "industries" | "certifications" | "serviceTags";

const TABS: { label: string; value: TabValue }[] = [
  { label: "Industries", value: "industries" },
  { label: "Certification Types", value: "certifications" },
  { label: "Service Tags", value: "serviceTags" },
];

// ---- Industry CRUD ----
function IndustriesTab({ token }: { token: string }) {
  const [items, setItems] = useState<Industry[]>([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [sortOrder, setSortOrder] = useState(0);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchItems = useCallback(async () => {
    setLoading(true);
    const res = await apiGetAuth<Industry[]>("/admin/industries", token);
    if (res.success && res.data) setItems(res.data);
    setLoading(false);
  }, [token]);

  useEffect(() => {
    fetchItems();
  }, [fetchItems]);

  function openCreate() {
    setName("");
    setDescription("");
    setSortOrder(0);
    setEditingId(null);
    setError(null);
    setShowForm(true);
  }

  function openEdit(item: Industry) {
    setName(item.name);
    setDescription(item.description ?? "");
    setSortOrder(item.sortOrder);
    setEditingId(item.id);
    setError(null);
    setShowForm(true);
  }

  function closeForm() {
    setShowForm(false);
    setEditingId(null);
    setError(null);
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    setIsSubmitting(true);

    const payload = { name, description: description || undefined, sortOrder };
    const res = editingId
      ? await apiPut(`/admin/industries/${editingId}`, payload, token)
      : await apiPost("/admin/industries", payload, token);

    if (res.success) {
      closeForm();
      await fetchItems();
    } else {
      setError(res.error?.message ?? "Failed to save industry");
    }
    setIsSubmitting(false);
  }

  async function handleDelete(id: string) {
    if (!confirm("Delete this industry? This cannot be undone.")) return;
    const res = await apiDelete(`/admin/industries/${id}`, token);
    if (res.success) await fetchItems();
  }

  return (
    <div className="space-y-4">
      {showForm && (
        <Card>
          <CardHeader className="flex flex-row items-center justify-between">
            <CardTitle>{editingId ? "Edit Industry" : "Add Industry"}</CardTitle>
            <button onClick={closeForm} className="text-muted-foreground hover:text-foreground">
              <X className="h-5 w-5" />
            </button>
          </CardHeader>
          <CardContent>
            <form onSubmit={handleSubmit} className="space-y-4">
              {error && (
                <div className="rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">{error}</div>
              )}
              <div className="grid gap-4 sm:grid-cols-2">
                <div className="space-y-2">
                  <Label htmlFor="ind-name">Name *</Label>
                  <Input id="ind-name" value={name} onChange={(e) => setName(e.target.value)} required />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="ind-sort">Sort Order</Label>
                  <Input
                    id="ind-sort"
                    type="number"
                    value={sortOrder}
                    onChange={(e) => setSortOrder(parseInt(e.target.value) || 0)}
                  />
                </div>
              </div>
              <div className="space-y-2">
                <Label htmlFor="ind-desc">Description</Label>
                <Textarea
                  id="ind-desc"
                  value={description}
                  onChange={(e) => setDescription(e.target.value)}
                  rows={2}
                />
              </div>
              <div className="flex justify-end gap-3">
                <Button type="button" variant="outline" onClick={closeForm}>Cancel</Button>
                <Button type="submit" disabled={isSubmitting}>
                  {isSubmitting ? <Loader2 className="mr-2 h-4 w-4 animate-spin" /> : null}
                  {editingId ? "Update" : "Create"}
                </Button>
              </div>
            </form>
          </CardContent>
        </Card>
      )}

      <div className="flex justify-end">
        {!showForm && (
          <Button onClick={openCreate} size="sm">
            <Plus className="mr-1 h-4 w-4" /> Add Industry
          </Button>
        )}
      </div>

      <div className="rounded-lg border bg-white">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Name</TableHead>
              <TableHead>Slug</TableHead>
              <TableHead>Suppliers</TableHead>
              <TableHead>Status</TableHead>
              <TableHead>Sort</TableHead>
              <TableHead className="w-[100px]">Actions</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {loading ? (
              Array.from({ length: 3 }).map((_, i) => (
                <TableRow key={i}>
                  {Array.from({ length: 6 }).map((_, j) => (
                    <TableCell key={j}><div className="h-4 w-16 animate-pulse rounded bg-muted" /></TableCell>
                  ))}
                </TableRow>
              ))
            ) : items.length === 0 ? (
              <TableRow>
                <TableCell colSpan={6} className="py-8 text-center text-muted-foreground">
                  No industries yet.
                </TableCell>
              </TableRow>
            ) : (
              items.map((item) => (
                <TableRow key={item.id}>
                  <TableCell className="font-medium">{item.name}</TableCell>
                  <TableCell className="text-xs text-muted-foreground">{item.slug}</TableCell>
                  <TableCell>{item.supplierCount}</TableCell>
                  <TableCell>
                    {item.isActive ? (
                      <Badge className="bg-brand-green/10 text-brand-green">Active</Badge>
                    ) : (
                      <Badge variant="outline">Inactive</Badge>
                    )}
                  </TableCell>
                  <TableCell>{item.sortOrder}</TableCell>
                  <TableCell>
                    <div className="flex items-center gap-1">
                      <button
                        onClick={() => openEdit(item)}
                        className="flex h-8 w-8 items-center justify-center rounded-md text-muted-foreground hover:bg-muted hover:text-foreground"
                        title="Edit"
                      >
                        <Pencil className="h-4 w-4" />
                      </button>
                      <button
                        onClick={() => handleDelete(item.id)}
                        className="flex h-8 w-8 items-center justify-center rounded-md text-muted-foreground hover:bg-destructive/10 hover:text-destructive"
                        title="Delete"
                      >
                        <Trash2 className="h-4 w-4" />
                      </button>
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

// ---- Certification Types CRUD ----
function CertificationTypesTab({ token }: { token: string }) {
  const [items, setItems] = useState<CertificationType[]>([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchItems = useCallback(async () => {
    setLoading(true);
    const res = await apiGetAuth<CertificationType[]>("/admin/certification-types", token);
    if (res.success && res.data) setItems(res.data);
    setLoading(false);
  }, [token]);

  useEffect(() => {
    fetchItems();
  }, [fetchItems]);

  function openCreate() {
    setName("");
    setDescription("");
    setEditingId(null);
    setError(null);
    setShowForm(true);
  }

  function openEdit(item: CertificationType) {
    setName(item.name);
    setDescription(item.description ?? "");
    setEditingId(item.id);
    setError(null);
    setShowForm(true);
  }

  function closeForm() {
    setShowForm(false);
    setEditingId(null);
    setError(null);
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    setIsSubmitting(true);

    const payload = { name, description: description || undefined };
    const res = editingId
      ? await apiPut(`/admin/certification-types/${editingId}`, payload, token)
      : await apiPost("/admin/certification-types", payload, token);

    if (res.success) {
      closeForm();
      await fetchItems();
    } else {
      setError(res.error?.message ?? "Failed to save certification type");
    }
    setIsSubmitting(false);
  }

  async function handleDelete(id: string) {
    if (!confirm("Delete this certification type? This cannot be undone.")) return;
    const res = await apiDelete(`/admin/certification-types/${id}`, token);
    if (res.success) await fetchItems();
  }

  return (
    <div className="space-y-4">
      {showForm && (
        <Card>
          <CardHeader className="flex flex-row items-center justify-between">
            <CardTitle>{editingId ? "Edit Certification Type" : "Add Certification Type"}</CardTitle>
            <button onClick={closeForm} className="text-muted-foreground hover:text-foreground">
              <X className="h-5 w-5" />
            </button>
          </CardHeader>
          <CardContent>
            <form onSubmit={handleSubmit} className="space-y-4">
              {error && (
                <div className="rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">{error}</div>
              )}
              <div className="space-y-2">
                <Label htmlFor="cert-name">Name *</Label>
                <Input id="cert-name" value={name} onChange={(e) => setName(e.target.value)} required placeholder="e.g. ISO 14001" />
              </div>
              <div className="space-y-2">
                <Label htmlFor="cert-desc">Description</Label>
                <Textarea
                  id="cert-desc"
                  value={description}
                  onChange={(e) => setDescription(e.target.value)}
                  rows={2}
                />
              </div>
              <div className="flex justify-end gap-3">
                <Button type="button" variant="outline" onClick={closeForm}>Cancel</Button>
                <Button type="submit" disabled={isSubmitting}>
                  {isSubmitting ? <Loader2 className="mr-2 h-4 w-4 animate-spin" /> : null}
                  {editingId ? "Update" : "Create"}
                </Button>
              </div>
            </form>
          </CardContent>
        </Card>
      )}

      <div className="flex justify-end">
        {!showForm && (
          <Button onClick={openCreate} size="sm">
            <Plus className="mr-1 h-4 w-4" /> Add Certification Type
          </Button>
        )}
      </div>

      <div className="rounded-lg border bg-white">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Name</TableHead>
              <TableHead>Slug</TableHead>
              <TableHead>Status</TableHead>
              <TableHead className="w-[100px]">Actions</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {loading ? (
              Array.from({ length: 3 }).map((_, i) => (
                <TableRow key={i}>
                  {Array.from({ length: 4 }).map((_, j) => (
                    <TableCell key={j}><div className="h-4 w-16 animate-pulse rounded bg-muted" /></TableCell>
                  ))}
                </TableRow>
              ))
            ) : items.length === 0 ? (
              <TableRow>
                <TableCell colSpan={4} className="py-8 text-center text-muted-foreground">
                  No certification types yet.
                </TableCell>
              </TableRow>
            ) : (
              items.map((item) => (
                <TableRow key={item.id}>
                  <TableCell className="font-medium">{item.name}</TableCell>
                  <TableCell className="text-xs text-muted-foreground">{item.slug}</TableCell>
                  <TableCell>
                    {item.isActive ? (
                      <Badge className="bg-brand-green/10 text-brand-green">Active</Badge>
                    ) : (
                      <Badge variant="outline">Inactive</Badge>
                    )}
                  </TableCell>
                  <TableCell>
                    <div className="flex items-center gap-1">
                      <button
                        onClick={() => openEdit(item)}
                        className="flex h-8 w-8 items-center justify-center rounded-md text-muted-foreground hover:bg-muted hover:text-foreground"
                        title="Edit"
                      >
                        <Pencil className="h-4 w-4" />
                      </button>
                      <button
                        onClick={() => handleDelete(item.id)}
                        className="flex h-8 w-8 items-center justify-center rounded-md text-muted-foreground hover:bg-destructive/10 hover:text-destructive"
                        title="Delete"
                      >
                        <Trash2 className="h-4 w-4" />
                      </button>
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

// ---- Service Tags CRUD ----
function ServiceTagsTab({ token }: { token: string }) {
  const [items, setItems] = useState<ServiceTag[]>([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [name, setName] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchItems = useCallback(async () => {
    setLoading(true);
    const res = await apiGetAuth<ServiceTag[]>("/admin/service-tags", token);
    if (res.success && res.data) setItems(res.data);
    setLoading(false);
  }, [token]);

  useEffect(() => {
    fetchItems();
  }, [fetchItems]);

  function openCreate() {
    setName("");
    setEditingId(null);
    setError(null);
    setShowForm(true);
  }

  function openEdit(item: ServiceTag) {
    setName(item.name);
    setEditingId(item.id);
    setError(null);
    setShowForm(true);
  }

  function closeForm() {
    setShowForm(false);
    setEditingId(null);
    setError(null);
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    setIsSubmitting(true);

    const payload = { name };
    const res = editingId
      ? await apiPut(`/admin/service-tags/${editingId}`, payload, token)
      : await apiPost("/admin/service-tags", payload, token);

    if (res.success) {
      closeForm();
      await fetchItems();
    } else {
      setError(res.error?.message ?? "Failed to save service tag");
    }
    setIsSubmitting(false);
  }

  async function handleDelete(id: string) {
    if (!confirm("Delete this service tag? This cannot be undone.")) return;
    const res = await apiDelete(`/admin/service-tags/${id}`, token);
    if (res.success) await fetchItems();
  }

  return (
    <div className="space-y-4">
      {showForm && (
        <Card>
          <CardHeader className="flex flex-row items-center justify-between">
            <CardTitle>{editingId ? "Edit Service Tag" : "Add Service Tag"}</CardTitle>
            <button onClick={closeForm} className="text-muted-foreground hover:text-foreground">
              <X className="h-5 w-5" />
            </button>
          </CardHeader>
          <CardContent>
            <form onSubmit={handleSubmit} className="space-y-4">
              {error && (
                <div className="rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">{error}</div>
              )}
              <div className="space-y-2">
                <Label htmlFor="tag-name">Name *</Label>
                <Input id="tag-name" value={name} onChange={(e) => setName(e.target.value)} required placeholder="e.g. Solar Panels" />
              </div>
              <div className="flex justify-end gap-3">
                <Button type="button" variant="outline" onClick={closeForm}>Cancel</Button>
                <Button type="submit" disabled={isSubmitting}>
                  {isSubmitting ? <Loader2 className="mr-2 h-4 w-4 animate-spin" /> : null}
                  {editingId ? "Update" : "Create"}
                </Button>
              </div>
            </form>
          </CardContent>
        </Card>
      )}

      <div className="flex justify-end">
        {!showForm && (
          <Button onClick={openCreate} size="sm">
            <Plus className="mr-1 h-4 w-4" /> Add Service Tag
          </Button>
        )}
      </div>

      <div className="rounded-lg border bg-white">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Name</TableHead>
              <TableHead>Slug</TableHead>
              <TableHead>Status</TableHead>
              <TableHead className="w-[100px]">Actions</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {loading ? (
              Array.from({ length: 3 }).map((_, i) => (
                <TableRow key={i}>
                  {Array.from({ length: 4 }).map((_, j) => (
                    <TableCell key={j}><div className="h-4 w-16 animate-pulse rounded bg-muted" /></TableCell>
                  ))}
                </TableRow>
              ))
            ) : items.length === 0 ? (
              <TableRow>
                <TableCell colSpan={4} className="py-8 text-center text-muted-foreground">
                  No service tags yet.
                </TableCell>
              </TableRow>
            ) : (
              items.map((item) => (
                <TableRow key={item.id}>
                  <TableCell className="font-medium">{item.name}</TableCell>
                  <TableCell className="text-xs text-muted-foreground">{item.slug}</TableCell>
                  <TableCell>
                    {item.isActive ? (
                      <Badge className="bg-brand-green/10 text-brand-green">Active</Badge>
                    ) : (
                      <Badge variant="outline">Inactive</Badge>
                    )}
                  </TableCell>
                  <TableCell>
                    <div className="flex items-center gap-1">
                      <button
                        onClick={() => openEdit(item)}
                        className="flex h-8 w-8 items-center justify-center rounded-md text-muted-foreground hover:bg-muted hover:text-foreground"
                        title="Edit"
                      >
                        <Pencil className="h-4 w-4" />
                      </button>
                      <button
                        onClick={() => handleDelete(item.id)}
                        className="flex h-8 w-8 items-center justify-center rounded-md text-muted-foreground hover:bg-destructive/10 hover:text-destructive"
                        title="Delete"
                      >
                        <Trash2 className="h-4 w-4" />
                      </button>
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

// ---- Main Taxonomy Page ----
export default function AdminTaxonomyPage() {
  const { token } = useAuth();
  const [activeTab, setActiveTab] = useState<TabValue>("industries");

  if (!token) return null;

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-foreground">Taxonomy</h1>
        <p className="text-sm text-muted-foreground">
          Manage industries, certification types, and service tags
        </p>
      </div>

      {/* Tab navigation */}
      <div className="flex gap-1 rounded-lg bg-muted p-1">
        {TABS.map((tab) => (
          <button
            key={tab.value}
            onClick={() => setActiveTab(tab.value)}
            className={`rounded-md px-3 py-1.5 text-sm font-medium transition-colors ${
              activeTab === tab.value
                ? "bg-white text-foreground shadow-sm"
                : "text-muted-foreground hover:text-foreground"
            }`}
          >
            {tab.label}
          </button>
        ))}
      </div>

      {/* Tab content */}
      {activeTab === "industries" && <IndustriesTab token={token} />}
      {activeTab === "certifications" && <CertificationTypesTab token={token} />}
      {activeTab === "serviceTags" && <ServiceTagsTab token={token} />}
    </div>
  );
}

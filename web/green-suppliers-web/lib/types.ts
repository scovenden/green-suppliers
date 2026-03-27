export interface ApiResponse<T> {
  success: boolean;
  data: T | null;
  error: {
    code: string;
    message: string;
    details?: Record<string, string[]>;
  } | null;
  meta?: {
    page: number;
    pageSize: number;
    total: number;
    totalPages: number;
  };
}

export interface SupplierSearchResult {
  id: string;
  slug: string;
  tradingName: string;
  shortDescription: string | null;
  city: string | null;
  countryCode: string;
  verificationStatus: string;
  esgLevel: string;
  esgScore: number;
  logoUrl: string | null;
  industries: string[];
  isVerified: boolean;
  sdgs?: SdgDto[];
}

export interface SupplierProfile {
  id: string;
  slug: string;
  organizationName: string;
  tradingName: string;
  description: string | null;
  shortDescription: string | null;
  logoUrl: string | null;
  bannerUrl: string | null;
  yearFounded: number | null;
  employeeCount: string | null;
  bbbeeLevel: string | null;
  countryCode: string;
  city: string | null;
  province: string | null;
  website: string | null;
  phone: string | null;
  email: string | null;
  renewableEnergyPercent: number | null;
  wasteRecyclingPercent: number | null;
  carbonReporting: boolean;
  waterManagement: boolean;
  sustainablePackaging: boolean;
  verificationStatus: string;
  esgLevel: string;
  esgScore: number;
  isPublished: boolean;
  industries: { id: string; name: string; slug: string }[];
  serviceTags: { id: string; name: string; slug: string }[];
  certifications: CertificationDto[];
  sdgs?: SdgDto[];
}

export interface CertificationDto {
  id: string;
  certTypeName: string;
  certTypeSlug: string;
  certificateNumber: string | null;
  issuedAt: string | null;
  expiresAt: string | null;
  status: string;
  notes: string | null;
}

export interface Industry {
  id: string;
  name: string;
  slug: string;
  description: string | null;
  parentId: string | null;
  sortOrder: number;
  isActive: boolean;
  supplierCount: number;
}

export interface Country {
  code: string;
  name: string;
  slug: string;
  region: string | null;
  isActive: boolean;
  supplierCount: number;
}

export interface ServiceTag {
  id: string;
  name: string;
  slug: string;
  isActive: boolean;
}

export interface ContentPage {
  id: string;
  slug: string;
  title: string;
  metaTitle: string | null;
  metaDesc: string | null;
  body: string;
  pageType: string;
  isPublished: boolean;
  publishedAt: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface LeadRequest {
  supplierProfileId: string;
  contactName: string;
  contactEmail: string;
  contactPhone?: string;
  companyName?: string;
  message: string;
}

export interface GetListedRequest {
  companyName: string;
  contactName: string;
  contactEmail: string;
  contactPhone?: string;
  website?: string;
  industryIds?: string[];
  country: string;
  city?: string;
  description: string;
  certifications?: string;
}

export type EsgLevel = "none" | "bronze" | "silver" | "gold" | "platinum";

export type VerificationStatus = "unverified" | "pending" | "verified" | "flagged";

export type LeadStatus = "new" | "contacted" | "closed";

export interface AdminUser {
  id: string;
  email: string;
  displayName: string;
  role: string;
  organizationName?: string;
}

export interface AuthTokens {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  user: AdminUser;
  tokens: AuthTokens;
}

export interface AdminSupplier {
  id: string;
  slug: string;
  organizationName: string;
  tradingName: string;
  countryCode: string;
  city: string | null;
  verificationStatus: string;
  esgLevel: string;
  esgScore: number;
  isPublished: boolean;
  isFlagged: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateSupplierRequest {
  organizationName: string;
  tradingName: string;
  description?: string;
  shortDescription?: string;
  yearFounded?: number;
  employeeCount?: string;
  bbbeeLevel?: string;
  countryCode: string;
  city?: string;
  province?: string;
  website?: string;
  phone?: string;
  email?: string;
  renewableEnergyPercent?: number;
  wasteRecyclingPercent?: number;
  carbonReporting: boolean;
  waterManagement: boolean;
  sustainablePackaging: boolean;
  industryIds: string[];
  serviceTagIds: string[];
}

export interface AdminLead {
  id: string;
  contactName: string;
  contactEmail: string;
  contactPhone: string | null;
  companyName: string | null;
  message: string;
  status: LeadStatus;
  supplierProfileId: string | null;
  supplierTradingName: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface CertificationType {
  id: string;
  name: string;
  slug: string;
  description: string | null;
  isActive: boolean;
}

export interface SupplierDashboardStats {
  totalLeads: number;
  newLeads: number;
  esgLevel: string;
  esgScore: number;
  verificationStatus: string;
  profileCompleteness: number;
  activeCertifications: number;
  expiringCertifications: number;
  isPublished: boolean;
}

export interface UpdateProfileRequest {
  tradingName?: string;
  description?: string;
  shortDescription?: string;
  yearFounded?: number | null;
  employeeCount?: string;
  bbbeeLevel?: string;
  city?: string;
  province?: string;
  website?: string;
  phone?: string;
  email?: string;
  renewableEnergyPercent?: number | null;
  wasteRecyclingPercent?: number | null;
  carbonReporting?: boolean;
  waterManagement?: boolean;
  sustainablePackaging?: boolean;
  industryIds?: string[];
}

export interface SubmitCertificationRequest {
  certificationTypeId: string;
  certificateNumber?: string;
  issuedAt?: string;
  expiresAt?: string;
}

export interface AdminDashboardStats {
  totalSuppliers: number;
  verifiedSuppliers: number;
  newLeads: number;
  pendingCertifications: number;
}

export interface AdminActivity {
  id: string;
  action: string;
  entityType: string;
  entityId: string;
  description: string;
  createdAt: string;
}

// --- Sprint 3: Buyer Dashboard & Supplier Leads ---

export interface BuyerDashboardStats {
  savedSupplierCount: number;
  inquirySentCount: number;
  inquiryRespondedCount: number;
}

export interface SavedSupplier {
  id: string;
  supplierProfileId: string;
  slug: string;
  tradingName: string;
  shortDescription: string | null;
  city: string | null;
  countryCode: string;
  verificationStatus: string;
  esgLevel: string;
  esgScore: number;
  logoUrl: string | null;
  industries: string[];
  isVerified: boolean;
  savedAt: string;
}

export interface BuyerLead {
  id: string;
  supplierProfileId: string;
  supplierTradingName: string;
  contactName: string;
  contactEmail: string;
  contactPhone: string | null;
  companyName: string | null;
  message: string;
  status: LeadStatus;
  createdAt: string;
  updatedAt: string;
}

export interface SupplierLead {
  id: string;
  contactName: string;
  contactEmail: string;
  contactPhone: string | null;
  companyName: string | null;
  message: string;
  status: LeadStatus;
  createdAt: string;
  updatedAt: string;
}

// --- Sprint 4: Billing, Plans & Checkout ---

export interface PlanDto {
  id: string;
  name: string;
  displayName: string;
  priceMonthly: number;
  priceYearly: number;
  currency: string;
  maxLeadsPerMonth: number | null;
  maxDocuments: number | null;
  featuredListing: boolean;
  analyticsAccess: boolean;
  prioritySupport: boolean;
  trialDays: number;
  sortOrder: number;
}

export interface SubscriptionDto {
  id: string;
  organizationId: string;
  planId: string;
  planName: string;
  planDisplayName: string;
  status: string;
  billingCycle: string;
  currentPeriodStart: string;
  currentPeriodEnd: string;
  trialEnd: string | null;
  cancelledAt: string | null;
  createdAt: string;
}

export interface PaymentDto {
  id: string;
  subscriptionId: string;
  amount: number;
  currency: string;
  status: string;
  provider: string;
  externalId: string | null;
  paidAt: string | null;
  createdAt: string;
}

export interface CheckoutRequest {
  planId: string;
  billingCycle: string;
}

export interface CheckoutResult {
  subscriptionId: string;
  paymentId: string;
  checkoutUrl: string;
}

export type SubscriptionStatus = "pending" | "trial" | "active" | "past_due" | "cancelled" | "expired";

// --- Sprint 5: SDG, Analytics, Sponsored Placements ---

export interface SdgDto {
  id: number;
  name: string;
  description: string;
  color: string;
}

export interface ProfileAnalytics {
  totalViews: number;
  viewsThisMonth: number;
  viewsLastMonth: number;
  viewsByDay: { date: string; count: number }[];
  totalLeads: number;
  leadsByMonth: { month: string; count: number }[];
  searchAppearances: number;
}

export interface FeaturedSupplier extends SupplierSearchResult {
  sdgs?: SdgDto[];
  isSponsored?: boolean;
}

export function getEsgBadgeColor(level: string) {
  switch (level.toLowerCase()) {
    case "platinum":
      return {
        bg: "bg-gradient-to-r from-lime-600 to-green-700",
        text: "text-white",
      };
    case "gold":
      return {
        bg: "bg-gradient-to-r from-amber-500 to-amber-600",
        text: "text-white",
      };
    case "silver":
      return {
        bg: "bg-gradient-to-r from-gray-400 to-gray-500",
        text: "text-white",
      };
    case "bronze":
      return {
        bg: "bg-gradient-to-r from-amber-700 to-amber-800",
        text: "text-white",
      };
    default:
      return {
        bg: "bg-gradient-to-r from-gray-200 to-gray-300",
        text: "text-gray-600",
      };
  }
}

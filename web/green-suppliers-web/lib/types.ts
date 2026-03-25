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

import { z } from "zod";

export const leadSchema = z.object({
  contactName: z.string().min(1, "Name is required").max(150),
  contactEmail: z.string().email("Valid email required"),
  contactPhone: z.string().optional(),
  companyName: z.string().optional(),
  message: z.string().min(1, "Message is required").max(2000),
});

export type LeadFormData = z.infer<typeof leadSchema>;

export const getListedSchema = z.object({
  companyName: z.string().min(1, "Company name is required"),
  contactName: z.string().min(1, "Name is required"),
  contactEmail: z.string().email("Valid email required"),
  contactPhone: z.string().optional(),
  website: z.string().url("Must be a valid URL").optional().or(z.literal("")),
  country: z.string().length(2, "Country code required"),
  city: z.string().optional(),
  description: z.string().min(1, "Description required").max(500),
  certifications: z.string().optional(),
});

export type GetListedFormData = z.infer<typeof getListedSchema>;

export const registerSchema = z
  .object({
    firstName: z.string().min(1, "First name is required"),
    lastName: z.string().min(1, "Last name is required"),
    email: z.string().email("Valid email required"),
    password: z
      .string()
      .min(8, "At least 8 characters")
      .regex(/[A-Z]/, "Must include uppercase letter")
      .regex(/[a-z]/, "Must include lowercase letter")
      .regex(/[0-9]/, "Must include a number"),
    confirmPassword: z.string(),
    companyName: z.string().optional(),
    countryCode: z.string().length(2).optional(),
  })
  .refine((data) => data.password === data.confirmPassword, {
    message: "Passwords do not match",
    path: ["confirmPassword"],
  });

export type RegisterFormData = z.infer<typeof registerSchema>;

export const forgotPasswordSchema = z.object({
  email: z.string().email("Valid email required"),
});

export type ForgotPasswordFormData = z.infer<typeof forgotPasswordSchema>;

export const resetPasswordSchema = z
  .object({
    newPassword: z
      .string()
      .min(8, "At least 8 characters")
      .regex(/[A-Z]/, "Must include uppercase letter")
      .regex(/[a-z]/, "Must include lowercase letter")
      .regex(/[0-9]/, "Must include a number"),
    confirmPassword: z.string(),
  })
  .refine((data) => data.newPassword === data.confirmPassword, {
    message: "Passwords do not match",
    path: ["confirmPassword"],
  });

export type ResetPasswordFormData = z.infer<typeof resetPasswordSchema>;

export const supplierProfileSchema = z.object({
  tradingName: z.string().min(1, "Trading name is required").max(200),
  description: z.string().optional(),
  shortDescription: z.string().optional(),
  yearFounded: z.string().optional(),
  employeeCount: z.string().optional(),
  bbbeeLevel: z.string().optional(),
  city: z.string().optional(),
  province: z.string().optional(),
  website: z.string().optional(),
  phone: z.string().optional(),
  email: z.string().optional(),
  renewableEnergyPercent: z.number().min(0).max(100).optional(),
  wasteRecyclingPercent: z.number().min(0).max(100).optional(),
  carbonReporting: z.boolean(),
  waterManagement: z.boolean(),
  sustainablePackaging: z.boolean(),
  industryIds: z.array(z.string()).optional(),
});

export type SupplierProfileFormData = z.infer<typeof supplierProfileSchema>;

export const submitCertificationSchema = z.object({
  certificationTypeId: z.string().min(1, "Certification type is required"),
  certificateNumber: z.string().optional(),
  issuedAt: z.string().optional(),
  expiresAt: z.string().optional(),
});

export type SubmitCertificationFormData = z.infer<typeof submitCertificationSchema>;

export const changePasswordSchema = z
  .object({
    currentPassword: z.string().min(1, "Current password is required"),
    newPassword: z
      .string()
      .min(8, "At least 8 characters")
      .regex(/[A-Z]/, "Must include uppercase letter")
      .regex(/[a-z]/, "Must include lowercase letter")
      .regex(/[0-9]/, "Must include a number"),
    confirmPassword: z.string(),
  })
  .refine((data) => data.newPassword === data.confirmPassword, {
    message: "Passwords do not match",
    path: ["confirmPassword"],
  });

export type ChangePasswordFormData = z.infer<typeof changePasswordSchema>;

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

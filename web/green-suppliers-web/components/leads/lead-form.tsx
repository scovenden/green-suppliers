"use client";

import * as React from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { leadSchema, type LeadFormData } from "@/lib/validators";
import { apiPost } from "@/lib/api-client";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { CheckCircle, AlertCircle, Send, Loader2 } from "lucide-react";

interface LeadFormProps {
  supplierProfileId: string;
  supplierName: string;
}

export function LeadForm({ supplierProfileId, supplierName }: LeadFormProps) {
  const [status, setStatus] = React.useState<
    "idle" | "submitting" | "success" | "error"
  >("idle");
  const [errorMessage, setErrorMessage] = React.useState("");

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<LeadFormData>({
    resolver: zodResolver(leadSchema),
    defaultValues: {
      contactName: "",
      contactEmail: "",
      contactPhone: "",
      companyName: "",
      message: "",
    },
  });

  async function onSubmit(data: LeadFormData) {
    setStatus("submitting");
    setErrorMessage("");

    try {
      const res = await apiPost("/leads", {
        supplierProfileId,
        ...data,
      });

      if (res.success) {
        setStatus("success");
        reset();
      } else {
        setStatus("error");
        setErrorMessage(
          res.error?.message ?? "Something went wrong. Please try again."
        );
      }
    } catch {
      setStatus("error");
      setErrorMessage(
        "Unable to send your inquiry right now. Please try again later."
      );
    }
  }

  if (status === "success") {
    return (
      <div className="flex flex-col items-center gap-3 rounded-2xl border border-green-200 bg-green-50 p-8 text-center">
        <div className="flex h-12 w-12 items-center justify-center rounded-full bg-brand-green/10">
          <CheckCircle className="h-6 w-6 text-brand-green" />
        </div>
        <h3 className="text-lg font-semibold text-gray-900">
          Inquiry Sent Successfully
        </h3>
        <p className="max-w-sm text-sm text-gray-600">
          Your message has been sent to {supplierName}. They will get back to
          you as soon as possible.
        </p>
        <Button
          variant="outline"
          onClick={() => setStatus("idle")}
          className="mt-2"
        >
          Send another inquiry
        </Button>
      </div>
    );
  }

  return (
    <div className="overflow-hidden rounded-2xl border border-gray-100 bg-white shadow-sm">
      {/* Green header */}
      <div className="bg-gradient-to-r from-brand-green to-brand-emerald px-6 py-4">
        <h3 className="text-lg font-semibold text-white">
          Contact {supplierName}
        </h3>
        <p className="mt-0.5 text-sm text-green-100/80">
          Send an inquiry to learn more about their products and services
        </p>
      </div>

      {/* Form body */}
      <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-4 p-6">
        {status === "error" && errorMessage && (
          <div className="flex items-start gap-2 rounded-lg border border-red-200 bg-red-50 p-3 text-sm text-red-700">
            <AlertCircle className="mt-0.5 h-4 w-4 shrink-0" />
            {errorMessage}
          </div>
        )}

        {/* Name */}
        <div className="flex flex-col gap-1.5">
          <Label htmlFor="contactName">
            Name <span className="text-red-500">*</span>
          </Label>
          <Input
            id="contactName"
            placeholder="Your full name"
            aria-invalid={!!errors.contactName}
            {...register("contactName")}
          />
          {errors.contactName && (
            <p className="text-xs text-red-500">{errors.contactName.message}</p>
          )}
        </div>

        {/* Email */}
        <div className="flex flex-col gap-1.5">
          <Label htmlFor="contactEmail">
            Email <span className="text-red-500">*</span>
          </Label>
          <Input
            id="contactEmail"
            type="email"
            placeholder="you@company.com"
            aria-invalid={!!errors.contactEmail}
            {...register("contactEmail")}
          />
          {errors.contactEmail && (
            <p className="text-xs text-red-500">
              {errors.contactEmail.message}
            </p>
          )}
        </div>

        {/* Phone (optional) */}
        <div className="flex flex-col gap-1.5">
          <Label htmlFor="contactPhone">Phone</Label>
          <Input
            id="contactPhone"
            type="tel"
            placeholder="+27 12 345 6789"
            {...register("contactPhone")}
          />
        </div>

        {/* Company (optional) */}
        <div className="flex flex-col gap-1.5">
          <Label htmlFor="companyName">Company</Label>
          <Input
            id="companyName"
            placeholder="Your company name"
            {...register("companyName")}
          />
        </div>

        {/* Message */}
        <div className="flex flex-col gap-1.5">
          <Label htmlFor="message">
            Message <span className="text-red-500">*</span>
          </Label>
          <Textarea
            id="message"
            placeholder="Tell them what you're looking for..."
            rows={4}
            aria-invalid={!!errors.message}
            {...register("message")}
          />
          {errors.message && (
            <p className="text-xs text-red-500">{errors.message.message}</p>
          )}
        </div>

        {/* Submit */}
        <Button
          type="submit"
          disabled={status === "submitting"}
          className="mt-2 h-10 w-full gap-2 rounded-xl bg-brand-green text-sm font-semibold text-white transition-colors hover:bg-brand-green-hover"
        >
          {status === "submitting" ? (
            <>
              <Loader2 className="h-4 w-4 animate-spin" />
              Sending...
            </>
          ) : (
            <>
              <Send className="h-4 w-4" />
              Send Inquiry
            </>
          )}
        </Button>
      </form>
    </div>
  );
}

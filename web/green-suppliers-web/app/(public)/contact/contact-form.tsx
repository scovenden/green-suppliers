"use client";

import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Loader2, CheckCircle, AlertTriangle } from "lucide-react";
import { toast } from "sonner";

const contactSchema = z.object({
  name: z.string().min(1, "Name is required").max(100),
  email: z.string().email("Valid email required"),
  phone: z.string().optional(),
  subject: z.string().min(1, "Subject is required").max(200),
  message: z.string().min(10, "Message must be at least 10 characters").max(2000),
});

type ContactFormData = z.infer<typeof contactSchema>;

export function ContactForm() {
  const [status, setStatus] = useState<"idle" | "submitting" | "success" | "error">("idle");

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<ContactFormData>({
    resolver: zodResolver(contactSchema),
  });

  async function onSubmit(data: ContactFormData) {
    setStatus("submitting");
    try {
      // Send via the API or directly via mailto
      // For now, we use the API to queue an email to info@agilus.co.za
      const API_BASE = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000/api/v1";
      const res = await fetch(`${API_BASE}/get-listed`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          companyName: data.subject,
          contactName: data.name,
          contactEmail: data.email,
          contactPhone: data.phone || null,
          country: "ZA",
          description: `[Contact Form]\n\n${data.message}`,
        }),
      });

      if (res.ok) {
        setStatus("success");
        reset();
        toast.success("Message sent! We'll get back to you within 1 business day.");
      } else {
        setStatus("error");
        toast.error("Failed to send message. Please try again or email us directly.");
      }
    } catch {
      setStatus("error");
      toast.error("Network error. Please email us at hello@greensuppliers.co.za");
    }
  }

  if (status === "success") {
    return (
      <div className="mt-8 flex flex-col items-center gap-4 py-8 text-center">
        <div className="flex h-16 w-16 items-center justify-center rounded-full bg-brand-green-light">
          <CheckCircle className="h-8 w-8 text-brand-green" />
        </div>
        <h3 className="text-lg font-bold text-gray-900">Message Sent!</h3>
        <p className="max-w-sm text-sm text-gray-500">
          Thank you for reaching out. We&apos;ll respond to your message within
          1 business day at the email address you provided.
        </p>
        <Button
          variant="outline"
          onClick={() => setStatus("idle")}
          className="mt-2"
        >
          Send Another Message
        </Button>
      </div>
    );
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} noValidate className="mt-6 space-y-5">
      {status === "error" && (
        <div className="flex items-center gap-2 rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700" role="alert">
          <AlertTriangle className="h-4 w-4 shrink-0" aria-hidden="true" />
          <span>Failed to send. Please try again or email <a href="mailto:hello@greensuppliers.co.za" className="font-medium underline">hello@greensuppliers.co.za</a> directly.</span>
        </div>
      )}

      <div className="grid gap-5 sm:grid-cols-2">
        <div className="space-y-2">
          <Label htmlFor="name">
            Full Name <span className="text-red-500" aria-hidden="true">*</span>
          </Label>
          <Input
            id="name"
            placeholder="Your full name"
            autoComplete="name"
            aria-required="true"
            aria-invalid={!!errors.name}
            aria-describedby={errors.name ? "name-error" : undefined}
            {...register("name")}
          />
          {errors.name && (
            <p id="name-error" className="text-xs text-red-500" role="alert">
              {errors.name.message}
            </p>
          )}
        </div>

        <div className="space-y-2">
          <Label htmlFor="email">
            Email <span className="text-red-500" aria-hidden="true">*</span>
          </Label>
          <Input
            id="email"
            type="email"
            placeholder="you@company.co.za"
            autoComplete="email"
            aria-required="true"
            aria-invalid={!!errors.email}
            aria-describedby={errors.email ? "email-error" : undefined}
            {...register("email")}
          />
          {errors.email && (
            <p id="email-error" className="text-xs text-red-500" role="alert">
              {errors.email.message}
            </p>
          )}
        </div>
      </div>

      <div className="grid gap-5 sm:grid-cols-2">
        <div className="space-y-2">
          <Label htmlFor="phone">Phone (optional)</Label>
          <Input
            id="phone"
            type="tel"
            placeholder="+27 XX XXX XXXX"
            autoComplete="tel"
            {...register("phone")}
          />
        </div>

        <div className="space-y-2">
          <Label htmlFor="subject">
            Subject <span className="text-red-500" aria-hidden="true">*</span>
          </Label>
          <Input
            id="subject"
            placeholder="How can we help?"
            aria-required="true"
            aria-invalid={!!errors.subject}
            aria-describedby={errors.subject ? "subject-error" : undefined}
            {...register("subject")}
          />
          {errors.subject && (
            <p id="subject-error" className="text-xs text-red-500" role="alert">
              {errors.subject.message}
            </p>
          )}
        </div>
      </div>

      <div className="space-y-2">
        <Label htmlFor="message">
          Message <span className="text-red-500" aria-hidden="true">*</span>
        </Label>
        <Textarea
          id="message"
          placeholder="Tell us more about your enquiry..."
          rows={5}
          aria-required="true"
          aria-invalid={!!errors.message}
          aria-describedby={errors.message ? "message-error" : undefined}
          {...register("message")}
        />
        {errors.message && (
          <p id="message-error" className="text-xs text-red-500" role="alert">
            {errors.message.message}
          </p>
        )}
      </div>

      <Button
        type="submit"
        size="lg"
        className="w-full sm:w-auto"
        disabled={status === "submitting"}
      >
        {status === "submitting" ? (
          <>
            <Loader2 className="mr-2 h-4 w-4 animate-spin" aria-hidden="true" />
            Sending...
          </>
        ) : (
          "Send Message"
        )}
      </Button>
    </form>
  );
}

import type { Metadata } from "next";
import Link from "next/link";
import { Mail, MapPin, Globe, ArrowRight, Leaf, Phone } from "lucide-react";
import { ContactForm } from "./contact-form";

export const metadata: Metadata = {
  title: "Contact Us | Green Suppliers",
  description:
    "Get in touch with the Green Suppliers team. Questions about listings, verification, partnerships, or general enquiries.",
};

const contactMethods = [
  {
    icon: Mail,
    label: "Email",
    value: "hello@greensuppliers.co.za",
    href: "mailto:hello@greensuppliers.co.za",
    description: "For general enquiries, partnerships, and support.",
  },
  {
    icon: Globe,
    label: "Website",
    value: "www.greensuppliers.co.za",
    href: "https://greensuppliers.co.za",
    description: "Visit our directory to browse and discover suppliers.",
  },
  {
    icon: MapPin,
    label: "Location",
    value: "South Africa",
    href: null,
    description: "Based in South Africa, serving the African continent.",
  },
];

export default function ContactPage() {
  return (
    <div className="min-h-screen bg-gray-50/50">
      {/* Breadcrumbs */}
      <div className="border-b border-gray-100 bg-white">
        <div className="mx-auto max-w-7xl px-4 py-3 sm:px-6 lg:px-8">
          <nav className="flex items-center gap-2 text-sm text-gray-500">
            <Link href="/" className="transition-colors hover:text-brand-green">
              Home
            </Link>
            <span>/</span>
            <span className="font-medium text-gray-900">Contact</span>
          </nav>
        </div>
      </div>

      {/* Hero */}
      <section className="border-b border-gray-100 bg-white">
        <div className="mx-auto max-w-7xl px-4 py-12 sm:px-6 sm:py-16 lg:px-8">
          <div className="mx-auto max-w-3xl text-center">
            <div className="mx-auto mb-5 flex h-14 w-14 items-center justify-center rounded-2xl bg-brand-green-light">
              <Mail className="h-7 w-7 text-brand-green" />
            </div>
            <h1
              className="text-3xl font-extrabold tracking-tight text-gray-900 sm:text-4xl"
              style={{ letterSpacing: "-0.5px" }}
            >
              Contact Us
            </h1>
            <p className="mx-auto mt-4 max-w-2xl text-lg leading-relaxed text-gray-500">
              Have a question about Green Suppliers? Want to partner with us?
              We&apos;d love to hear from you.
            </p>
          </div>
        </div>
      </section>

      {/* Contact Form + Info */}
      <section className="py-16">
        <div className="mx-auto max-w-6xl px-4 sm:px-6 lg:px-8">
          <div className="grid gap-12 lg:grid-cols-5">
            {/* Contact Form — takes 3 columns */}
            <div className="lg:col-span-3">
              <div className="rounded-2xl border border-gray-100 bg-white p-6 shadow-sm sm:p-8">
                <h2 className="text-xl font-bold text-gray-900">
                  Send Us a Message
                </h2>
                <p className="mt-1 text-sm text-gray-500">
                  Fill in the form below and we&apos;ll get back to you within 1
                  business day.
                </p>
                <ContactForm />
              </div>
            </div>

            {/* Contact Info — takes 2 columns */}
            <div className="lg:col-span-2">
              <div className="space-y-6">
                {contactMethods.map((method) => {
                  const Icon = method.icon;
                  return (
                    <div
                      key={method.label}
                      className="flex items-start gap-4 rounded-2xl border border-gray-100 bg-white p-5 shadow-sm"
                    >
                      <div className="flex h-11 w-11 shrink-0 items-center justify-center rounded-xl bg-brand-green-light">
                        <Icon className="h-5 w-5 text-brand-green" />
                      </div>
                      <div>
                        <h3 className="text-sm font-semibold text-gray-900">
                          {method.label}
                        </h3>
                        {method.href ? (
                          <a
                            href={method.href}
                            target={
                              method.href.startsWith("http")
                                ? "_blank"
                                : undefined
                            }
                            rel={
                              method.href.startsWith("http")
                                ? "noopener noreferrer"
                                : undefined
                            }
                            className="text-sm font-medium text-brand-green hover:underline"
                          >
                            {method.value}
                          </a>
                        ) : (
                          <p className="text-sm font-medium text-brand-green">
                            {method.value}
                          </p>
                        )}
                        <p className="mt-1 text-xs text-gray-500">
                          {method.description}
                        </p>
                      </div>
                    </div>
                  );
                })}

                {/* Response time */}
                <div className="rounded-2xl border border-brand-green/20 bg-brand-green-light p-5">
                  <h3 className="text-sm font-semibold text-brand-green-dark">
                    Response Time
                  </h3>
                  <p className="mt-1 text-sm text-gray-600">
                    We typically respond within 1 business day. For urgent
                    matters, please email us directly at{" "}
                    <a
                      href="mailto:hello@greensuppliers.co.za"
                      className="font-medium text-brand-green hover:underline"
                    >
                      hello@greensuppliers.co.za
                    </a>
                  </p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* FAQ / Common Enquiries */}
      <section className="bg-white py-16">
        <div className="mx-auto max-w-3xl px-4 sm:px-6 lg:px-8">
          <h2 className="text-center text-2xl font-extrabold tracking-tight text-gray-900">
            Common Enquiries
          </h2>
          <div className="mt-8 space-y-6">
            <div className="rounded-2xl border border-gray-100 bg-gray-50/50 p-6">
              <h3 className="text-sm font-semibold text-gray-900">
                I want to get my company listed
              </h3>
              <p className="mt-2 text-sm leading-relaxed text-gray-500">
                Fill out our{" "}
                <Link
                  href="/get-listed"
                  className="font-medium text-brand-green hover:underline"
                >
                  Get Listed form
                </Link>{" "}
                and our team will review your application within 2 business
                days. Basic listings are free.
              </p>
            </div>
            <div className="rounded-2xl border border-gray-100 bg-gray-50/50 p-6">
              <h3 className="text-sm font-semibold text-gray-900">
                How does ESG verification work?
              </h3>
              <p className="mt-2 text-sm leading-relaxed text-gray-500">
                We use a transparent, rules-based scoring system to assess
                suppliers. Learn more on our{" "}
                <Link
                  href="/verification"
                  className="font-medium text-brand-green hover:underline"
                >
                  How Verification Works
                </Link>{" "}
                page.
              </p>
            </div>
            <div className="rounded-2xl border border-gray-100 bg-gray-50/50 p-6">
              <h3 className="text-sm font-semibold text-gray-900">
                I want to report incorrect information
              </h3>
              <p className="mt-2 text-sm leading-relaxed text-gray-500">
                If you see incorrect information about a supplier, please email
                us at{" "}
                <a
                  href="mailto:hello@greensuppliers.co.za"
                  className="font-medium text-brand-green hover:underline"
                >
                  hello@greensuppliers.co.za
                </a>{" "}
                with the supplier name and details of the issue.
              </p>
            </div>
          </div>
        </div>
      </section>

      {/* CTA */}
      <section className="bg-gradient-to-br from-[#0F172A] via-[#0F172A] to-brand-green-dark py-16">
        <div className="mx-auto max-w-7xl px-4 text-center sm:px-6 lg:px-8">
          <div className="mx-auto mb-5 flex h-12 w-12 items-center justify-center rounded-2xl bg-white/10">
            <Leaf className="h-6 w-6 text-green-300" />
          </div>
          <h2 className="text-2xl font-extrabold tracking-tight text-white sm:text-3xl">
            Ready to find your next green supplier?
          </h2>
          <p className="mx-auto mt-3 max-w-2xl text-base leading-relaxed text-slate-400">
            Search our directory for free. No account required.
          </p>
          <Link
            href="/suppliers"
            className="mt-6 inline-flex items-center gap-2 rounded-xl bg-brand-green px-6 py-3 text-sm font-semibold text-white shadow-lg transition-all hover:bg-brand-green-hover hover:shadow-xl"
          >
            Browse Suppliers
            <ArrowRight className="h-4 w-4" />
          </Link>
        </div>
      </section>
    </div>
  );
}

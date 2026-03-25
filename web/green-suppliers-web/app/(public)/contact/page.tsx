import type { Metadata } from "next";
import Link from "next/link";
import { Mail, MapPin, Globe, ArrowRight, Leaf } from "lucide-react";

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
            <Link
              href="/"
              className="transition-colors hover:text-brand-green"
            >
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

      {/* Contact Methods */}
      <section className="py-16">
        <div className="mx-auto max-w-5xl px-4 sm:px-6 lg:px-8">
          <div className="grid gap-6 sm:grid-cols-3">
            {contactMethods.map((method) => {
              const Icon = method.icon;
              const content = (
                <div className="flex flex-col items-center gap-3 rounded-2xl border border-gray-100 bg-white p-6 text-center shadow-sm transition-all hover:shadow-md">
                  <div className="flex h-12 w-12 items-center justify-center rounded-2xl bg-brand-green-light">
                    <Icon className="h-6 w-6 text-brand-green" />
                  </div>
                  <h3 className="text-sm font-semibold text-gray-900">
                    {method.label}
                  </h3>
                  <p className="text-sm font-medium text-brand-green">
                    {method.value}
                  </p>
                  <p className="text-xs leading-relaxed text-gray-500">
                    {method.description}
                  </p>
                </div>
              );
              if (method.href) {
                return (
                  <a
                    key={method.label}
                    href={method.href}
                    target={method.href.startsWith("http") ? "_blank" : undefined}
                    rel={
                      method.href.startsWith("http")
                        ? "noopener noreferrer"
                        : undefined
                    }
                  >
                    {content}
                  </a>
                );
              }
              return <div key={method.label}>{content}</div>;
            })}
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
      <section className="bg-gradient-to-br from-[#0f4c2e] via-brand-green-dark to-brand-emerald py-16">
        <div className="mx-auto max-w-7xl px-4 text-center sm:px-6 lg:px-8">
          <div className="mx-auto mb-5 flex h-12 w-12 items-center justify-center rounded-2xl bg-white/10">
            <Leaf className="h-6 w-6 text-green-300" />
          </div>
          <h2 className="text-2xl font-extrabold tracking-tight text-white sm:text-3xl">
            Ready to find your next green supplier?
          </h2>
          <p className="mx-auto mt-3 max-w-2xl text-base leading-relaxed text-green-100/80">
            Search our directory for free. No account required.
          </p>
          <Link
            href="/suppliers"
            className="mt-6 inline-flex items-center gap-2 rounded-xl bg-white px-6 py-3 text-sm font-semibold text-brand-green-dark shadow-lg transition-all hover:bg-green-50 hover:shadow-xl"
          >
            Browse Suppliers
            <ArrowRight className="h-4 w-4" />
          </Link>
        </div>
      </section>
    </div>
  );
}

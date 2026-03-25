import type { Metadata } from "next";
import Link from "next/link";

export const metadata: Metadata = {
  title: "Terms of Service | Green Suppliers",
  description:
    "Terms of Service for Green Suppliers. Read the terms and conditions that govern your use of our platform.",
};

export default function TermsPage() {
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
            <span className="font-medium text-gray-900">Terms of Service</span>
          </nav>
        </div>
      </div>

      {/* Header */}
      <section className="border-b border-gray-100 bg-white">
        <div className="mx-auto max-w-3xl px-4 py-10 sm:px-6 sm:py-14 lg:px-8">
          <h1
            className="text-3xl font-extrabold tracking-tight text-gray-900 sm:text-4xl"
            style={{ letterSpacing: "-0.5px" }}
          >
            Terms of Service
          </h1>
          <p className="mt-3 text-sm text-gray-500">
            Last updated: 25 March 2026
          </p>
        </div>
      </section>

      {/* Content */}
      <article className="mx-auto max-w-3xl px-4 py-10 sm:px-6 lg:px-8">
        <div className="prose prose-gray prose-lg max-w-none prose-headings:font-extrabold prose-headings:tracking-tight prose-h2:text-2xl prose-h3:text-xl prose-a:text-brand-green prose-a:no-underline hover:prose-a:underline prose-strong:text-gray-900">
          <h2>1. Acceptance of Terms</h2>
          <p>
            By accessing and using the Green Suppliers website
            (greensuppliers.co.za) and related services, you agree to be bound
            by these Terms of Service. If you do not agree, please do not use
            our platform.
          </p>

          <h2>2. Description of Service</h2>
          <p>
            Green Suppliers is an online directory that connects buyers with
            verified green and sustainable suppliers. We provide:
          </p>
          <ul>
            <li>A searchable directory of supplier profiles.</li>
            <li>ESG scoring and verification badges.</li>
            <li>Lead capture forms for buyer-to-supplier inquiries.</li>
            <li>
              Content pages including sustainability guides and industry
              information.
            </li>
          </ul>

          <h2>3. User Accounts</h2>
          <p>
            Some features may require account registration. You are responsible
            for maintaining the confidentiality of your account credentials and
            for all activities under your account.
          </p>

          <h2>4. Supplier Listings</h2>
          <ul>
            <li>
              Suppliers are responsible for the accuracy of information
              provided in their listings.
            </li>
            <li>
              Green Suppliers reserves the right to verify, edit, flag, or
              remove any listing at our discretion.
            </li>
            <li>
              ESG scores are calculated using our published methodology and are
              updated automatically. They are not endorsements.
            </li>
          </ul>

          <h2>5. Lead / Inquiry Submissions</h2>
          <p>
            When you submit an inquiry through our platform, your contact
            details and message will be shared with the relevant supplier. We
            are not responsible for supplier response times or the quality of
            their products and services.
          </p>

          <h2>6. Intellectual Property</h2>
          <p>
            All content on this platform, including text, graphics, logos, and
            software, is the property of Agilus (Pty) Ltd or its content
            suppliers and is protected by intellectual property laws.
          </p>

          <h2>7. Prohibited Conduct</h2>
          <p>You agree not to:</p>
          <ul>
            <li>Submit false or misleading information.</li>
            <li>Spam suppliers through our lead forms.</li>
            <li>Scrape or harvest data from the platform.</li>
            <li>Attempt to circumvent our security measures.</li>
            <li>
              Use the platform for any unlawful purpose.
            </li>
          </ul>

          <h2>8. Limitation of Liability</h2>
          <p>
            Green Suppliers is provided &quot;as is&quot; without warranties
            of any kind. We are not liable for any damages arising from your
            use of the platform, including reliance on ESG scores or supplier
            information.
          </p>

          <h2>9. Governing Law</h2>
          <p>
            These Terms are governed by the laws of the Republic of South
            Africa. Any disputes will be subject to the jurisdiction of the
            South African courts.
          </p>

          <h2>10. Changes to Terms</h2>
          <p>
            We may update these Terms from time to time. Continued use of the
            platform after changes constitutes acceptance of the revised Terms.
          </p>

          <h2>11. Contact</h2>
          <p>
            For questions about these Terms, contact us at{" "}
            <a href="mailto:hello@greensuppliers.co.za">
              hello@greensuppliers.co.za
            </a>
            .
          </p>
        </div>
      </article>
    </div>
  );
}

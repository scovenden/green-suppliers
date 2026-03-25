import type { Metadata } from "next";
import Link from "next/link";

export const metadata: Metadata = {
  title: "Privacy Policy | Green Suppliers",
  description:
    "Privacy Policy for Green Suppliers. Learn how we collect, use, and protect your personal information.",
};

export default function PrivacyPage() {
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
            <span className="font-medium text-gray-900">Privacy Policy</span>
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
            Privacy Policy
          </h1>
          <p className="mt-3 text-sm text-gray-500">
            Last updated: 25 March 2026
          </p>
        </div>
      </section>

      {/* Content */}
      <article className="mx-auto max-w-3xl px-4 py-10 sm:px-6 lg:px-8">
        <div className="prose prose-gray prose-lg max-w-none prose-headings:font-extrabold prose-headings:tracking-tight prose-h2:text-2xl prose-h3:text-xl prose-a:text-brand-green prose-a:no-underline hover:prose-a:underline prose-strong:text-gray-900">
          <h2>1. Introduction</h2>
          <p>
            Green Suppliers (&quot;we&quot;, &quot;us&quot;, &quot;our&quot;) is
            operated by Agilus (Pty) Ltd. This Privacy Policy explains how we
            collect, use, disclose, and safeguard your information when you
            visit our website at greensuppliers.co.za and use our services.
          </p>

          <h2>2. Information We Collect</h2>
          <h3>Information you provide directly</h3>
          <ul>
            <li>
              <strong>Supplier listings:</strong> Company name, contact details,
              location, industry, sustainability data, and certifications.
            </li>
            <li>
              <strong>Lead / inquiry forms:</strong> Your name, email address,
              phone number, company name, and message content.
            </li>
            <li>
              <strong>Get Listed applications:</strong> Company details,
              contact information, and sustainability credentials.
            </li>
            <li>
              <strong>Newsletter subscriptions:</strong> Email address.
            </li>
          </ul>

          <h3>Information collected automatically</h3>
          <ul>
            <li>
              Browser type, operating system, IP address, and device
              information.
            </li>
            <li>Pages visited, time spent on pages, and referring URLs.</li>
            <li>Cookies and similar tracking technologies.</li>
          </ul>

          <h2>3. How We Use Your Information</h2>
          <ul>
            <li>To operate and maintain the Green Suppliers directory.</li>
            <li>
              To process supplier listing applications and buyer inquiries.
            </li>
            <li>To send transactional emails (lead notifications, certification expiry reminders).</li>
            <li>To improve our platform and user experience.</li>
            <li>To comply with legal obligations.</li>
          </ul>

          <h2>4. Information Sharing</h2>
          <p>
            We do not sell your personal information. We may share information
            with:
          </p>
          <ul>
            <li>
              <strong>Suppliers:</strong> When a buyer submits an inquiry, the
              supplier receives the buyer&apos;s contact details and message.
            </li>
            <li>
              <strong>Service providers:</strong> Hosting, email delivery, and
              analytics providers who process data on our behalf.
            </li>
            <li>
              <strong>Legal requirements:</strong> When required by law or to
              protect our rights.
            </li>
          </ul>

          <h2>5. Data Security</h2>
          <p>
            We implement appropriate technical and organisational measures to
            protect your personal information, including encryption,
            access controls, and regular security assessments.
          </p>

          <h2>6. Your Rights</h2>
          <p>
            Under the Protection of Personal Information Act (POPIA) and
            applicable data protection laws, you have the right to:
          </p>
          <ul>
            <li>Access your personal information.</li>
            <li>Request correction of inaccurate data.</li>
            <li>Request deletion of your data.</li>
            <li>Object to processing of your data.</li>
            <li>Withdraw consent at any time.</li>
          </ul>

          <h2>7. Cookies</h2>
          <p>
            We use cookies and similar technologies to improve your experience.
            You can control cookie preferences through your browser settings.
          </p>

          <h2>8. Changes to This Policy</h2>
          <p>
            We may update this Privacy Policy from time to time. The updated
            version will be indicated by the &quot;Last updated&quot; date at
            the top of this page.
          </p>

          <h2>9. Contact Us</h2>
          <p>
            If you have questions about this Privacy Policy, please contact us
            at{" "}
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

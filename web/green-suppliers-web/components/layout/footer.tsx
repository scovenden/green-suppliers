import Link from "next/link";
import { Leaf } from "lucide-react";

const quickLinks = [
  { label: "Suppliers", href: "/suppliers" },
  { label: "Industries", href: "/industries" },
  { label: "Guides", href: "/guides" },
  { label: "Get Listed", href: "/get-listed" },
  { label: "Contact", href: "/contact" },
];

const resourceLinks = [
  { label: "About Us", href: "/about" },
  { label: "How Verification Works", href: "/verification" },
  { label: "ESG Scoring", href: "/esg-scoring" },
  { label: "Privacy Policy", href: "/privacy" },
  { label: "Terms of Service", href: "/terms" },
];

export function Footer() {
  return (
    <footer className="bg-[#0f4c2e] text-white">
      <div className="mx-auto max-w-7xl px-4 py-12 sm:px-6 lg:px-8">
        <div className="grid gap-8 md:grid-cols-2 lg:grid-cols-4">
          {/* Brand column */}
          <div className="lg:col-span-2">
            <Link href="/" className="inline-flex items-center gap-2">
              <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-brand-green">
                <Leaf className="h-5 w-5 text-white" />
              </div>
              <span className="text-lg font-extrabold tracking-tight text-white">
                Green<span className="text-green-300">Suppliers</span>
              </span>
            </Link>
            <p className="mt-4 max-w-sm text-sm leading-relaxed text-green-200/70">
              South Africa&apos;s trusted directory for verified green and
              sustainable suppliers. Connecting eco-conscious businesses with
              procurement teams across Africa.
            </p>
          </div>

          {/* Quick links */}
          <div>
            <h3 className="text-sm font-semibold uppercase tracking-wider text-green-300">
              Quick Links
            </h3>
            <ul className="mt-4 space-y-2.5">
              {quickLinks.map((link) => (
                <li key={link.href}>
                  <Link
                    href={link.href}
                    className="text-sm text-green-200/70 transition-colors hover:text-white"
                  >
                    {link.label}
                  </Link>
                </li>
              ))}
            </ul>
          </div>

          {/* Resources */}
          <div>
            <h3 className="text-sm font-semibold uppercase tracking-wider text-green-300">
              Resources
            </h3>
            <ul className="mt-4 space-y-2.5">
              {resourceLinks.map((link) => (
                <li key={link.href}>
                  <Link
                    href={link.href}
                    className="text-sm text-green-200/70 transition-colors hover:text-white"
                  >
                    {link.label}
                  </Link>
                </li>
              ))}
            </ul>
          </div>
        </div>

        {/* Bottom bar */}
        <div className="mt-12 flex flex-col items-center gap-4 border-t border-white/10 pt-8 sm:flex-row sm:justify-between">
          <p className="text-sm text-green-200/50">
            &copy; {new Date().getFullYear()} GreenSuppliers. All rights reserved.
          </p>
          <p className="text-sm text-green-200/50">
            Powered by{" "}
            <a
              href="https://agilus.ai"
              target="_blank"
              rel="noopener noreferrer"
              className="font-medium text-green-300/70 transition-colors hover:text-white"
            >
              Agilus.AI
            </a>
          </p>
        </div>
      </div>
    </footer>
  );
}

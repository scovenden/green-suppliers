import Link from "next/link";
import { Leaf } from "lucide-react";

function LinkedinIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="currentColor" xmlns="http://www.w3.org/2000/svg">
      <path d="M20.447 20.452h-3.554v-5.569c0-1.328-.027-3.037-1.852-3.037-1.853 0-2.136 1.445-2.136 2.939v5.667H9.351V9h3.414v1.561h.046c.477-.9 1.637-1.85 3.37-1.85 3.601 0 4.267 2.37 4.267 5.455v6.286zM5.337 7.433a2.062 2.062 0 0 1-2.063-2.065 2.064 2.064 0 1 1 2.063 2.065zm1.782 13.019H3.555V9h3.564v11.452zM22.225 0H1.771C.792 0 0 .774 0 1.729v20.542C0 23.227.792 24 1.771 24h20.451C23.2 24 24 23.227 24 22.271V1.729C24 .774 23.2 0 22.222 0h.003z"/>
    </svg>
  );
}

function FacebookIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="currentColor" xmlns="http://www.w3.org/2000/svg">
      <path d="M24 12.073c0-6.627-5.373-12-12-12s-12 5.373-12 12c0 5.99 4.388 10.954 10.125 11.854v-8.385H7.078v-3.47h3.047V9.43c0-3.007 1.792-4.669 4.533-4.669 1.312 0 2.686.235 2.686.235v2.953H15.83c-1.491 0-1.956.925-1.956 1.874v2.25h3.328l-.532 3.47h-2.796v8.385C19.612 23.027 24 18.062 24 12.073z"/>
    </svg>
  );
}

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
    <footer className="relative">
      {/* SVG wave divider between content and footer */}
      <div className="wave-divider relative -mb-px">
        <svg
          viewBox="0 0 1440 100"
          fill="none"
          xmlns="http://www.w3.org/2000/svg"
          preserveAspectRatio="none"
          className="block w-full h-[40px] sm:h-[60px] md:h-[80px]"
        >
          <path
            d="M0,60 C360,100 720,20 1080,60 C1260,80 1360,70 1440,60 L1440,100 L0,100 Z"
            fill="#0f4c2e"
          />
        </svg>
      </div>

      {/* Footer body with leaf pattern background */}
      <div className="leaf-pattern bg-[#0f4c2e] text-white">
        <div className="mx-auto max-w-7xl px-4 py-12 sm:px-6 lg:px-8">
          {/* Newsletter signup section */}
          <div className="mb-12 rounded-2xl border border-white/10 bg-white/5 p-6 sm:p-8 backdrop-blur-sm">
            <div className="flex flex-col items-center gap-6 lg:flex-row lg:justify-between">
              <div className="text-center lg:text-left">
                <h3 className="text-lg font-bold text-white">Stay Updated</h3>
                <p className="mt-2 max-w-md text-sm leading-relaxed text-green-200/70">
                  Get monthly insights on green procurement, new verified suppliers, and sustainability trends in Africa.
                </p>
              </div>
              <div className="flex w-full max-w-md flex-col gap-3 sm:flex-row">
                <input
                  type="email"
                  placeholder="Enter your email"
                  className="h-11 flex-1 rounded-xl bg-white/10 px-4 text-sm text-white placeholder:text-white/50 outline-none transition-colors focus:bg-white/15 border border-white/10"
                />
                <button
                  type="button"
                  className="h-11 rounded-xl bg-brand-green px-6 text-sm font-semibold text-white transition-all duration-300 hover:bg-brand-green-hover hover:shadow-lg"
                >
                  Subscribe
                </button>
              </div>
            </div>
            <p className="mt-4 text-center text-xs text-green-200/50 lg:text-left">
              Join 200+ procurement professionals
            </p>
          </div>

          <div className="grid gap-8 md:grid-cols-2 lg:grid-cols-4">
            {/* Brand column */}
            <div className="lg:col-span-2">
              <Link href="/" className="inline-flex items-center gap-2">
                <div className="flex h-8 w-8 items-center justify-center rounded-xl bg-brand-green">
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

          {/* Social links */}
          <div className="mt-10 flex items-center justify-center gap-4">
            <a
              href="#"
              aria-label="LinkedIn"
              className="flex h-10 w-10 items-center justify-center rounded-xl bg-white/10 text-green-200/70 transition-all duration-300 hover:bg-white/20 hover:text-white"
            >
              <LinkedinIcon className="h-5 w-5" />
            </a>
            <a
              href="https://www.facebook.com/greensuppliers"
              target="_blank"
              rel="noopener noreferrer"
              aria-label="Facebook"
              className="flex h-10 w-10 items-center justify-center rounded-xl bg-white/10 text-green-200/70 transition-all duration-300 hover:bg-white/20 hover:text-white"
            >
              <FacebookIcon className="h-5 w-5" />
            </a>
          </div>

          {/* Bottom bar */}
          <div className="mt-8 flex flex-col items-center gap-4 border-t border-white/10 pt-8 sm:flex-row sm:justify-between">
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
      </div>
    </footer>
  );
}

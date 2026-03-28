import type { Metadata } from "next";
import { Plus_Jakarta_Sans, Inter } from "next/font/google";
import { Toaster } from "@/components/ui/sonner";
import { AnalyticsScripts } from "@/components/tracking/analytics-scripts";
import "./globals.css";

const heading = Plus_Jakarta_Sans({
  subsets: ["latin"],
  weight: ["600", "700", "800"],
  variable: "--font-heading",
});

const body = Inter({
  subsets: ["latin"],
  variable: "--font-body",
});

export const metadata: Metadata = {
  title: "Green Suppliers - South Africa's Verified Green Supplier Directory",
  description:
    "Find and contact verified green suppliers across South Africa. Search by industry, ESG level, and certification — free, no sign-up required.",
  metadataBase: new URL("https://greensuppliers.co.za"),
  icons: {
    icon: [
      { url: "/favicon.ico", sizes: "32x32" },
      { url: "/favicon.svg", type: "image/svg+xml" },
    ],
  },
  openGraph: {
    title: "Green Suppliers - South Africa's Verified Green Supplier Directory",
    description:
      "Find and contact verified green suppliers across South Africa. Search by industry, ESG level, and certification — free, no sign-up required.",
    siteName: "Green Suppliers",
    type: "website",
  },
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en" className={`${heading.variable} ${body.variable} h-full antialiased`}>
      <body className="min-h-full flex flex-col font-sans">
        {children}
        <Toaster position="top-right" richColors closeButton />
        <AnalyticsScripts />
      </body>
    </html>
  );
}

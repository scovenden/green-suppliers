import type { Metadata } from "next";
import { Plus_Jakarta_Sans, Inter } from "next/font/google";
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
    "Find verified, ESG-compliant green suppliers across South Africa. Search by industry, certification, and sustainability level.",
  metadataBase: new URL("https://greensuppliers.co.za"),
  openGraph: {
    title: "Green Suppliers - South Africa's Verified Green Supplier Directory",
    description:
      "Find verified, ESG-compliant green suppliers across South Africa. Search by industry, certification, and sustainability level.",
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
      <body className="min-h-full flex flex-col font-sans">{children}</body>
    </html>
  );
}

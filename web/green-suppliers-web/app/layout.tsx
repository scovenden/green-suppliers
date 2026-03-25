import type { Metadata } from "next";
import { Inter } from "next/font/google";
import "./globals.css";

const inter = Inter({
  variable: "--font-sans",
  subsets: ["latin"],
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
    <html lang="en" className={`${inter.variable} h-full antialiased`}>
      <body className="min-h-full flex flex-col font-sans">{children}</body>
    </html>
  );
}

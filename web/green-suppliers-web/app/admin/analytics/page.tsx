"use client";

import { useState } from "react";
import { BarChart3, ExternalLink, Globe, Users, TrendingUp } from "lucide-react";

const TABS = [
  {
    id: "google",
    label: "Google Analytics",
    icon: Globe,
    description: "Website traffic, user behaviour, and conversion data",
    color: "text-blue-600",
    bgColor: "bg-blue-50",
    borderColor: "border-blue-200",
    // Looker Studio embed URL — replace with your actual report URL
    // To create: Go to lookerstudio.google.com → Create Report → Add GA4 data source → Share → Get embed link
    embedUrl: process.env.NEXT_PUBLIC_LOOKER_STUDIO_URL || "",
    setupUrl: "https://lookerstudio.google.com",
    setupInstructions: [
      "Go to lookerstudio.google.com and sign in with your Google account",
      "Click 'Create' → 'Report'",
      "Add data source: select 'Google Analytics' → choose 'Green Suppliers' property (G-J5983P39J9)",
      "Build your report with charts: Users, Page Views, Top Pages, Traffic Sources, Conversions",
      "Click 'Share' → 'Get report link' → 'Embed' → copy the embed URL",
      "Set NEXT_PUBLIC_LOOKER_STUDIO_URL in Vercel environment variables with the embed URL",
    ],
  },
  {
    id: "linkedin",
    label: "LinkedIn",
    icon: Users,
    description: "LinkedIn page analytics and campaign performance",
    color: "text-sky-700",
    bgColor: "bg-sky-50",
    borderColor: "border-sky-200",
    embedUrl: "",
    setupUrl: "https://www.linkedin.com/company/greensuppliers/admin/analytics/",
    setupInstructions: [
      "Create the Green Suppliers LinkedIn Company Page (if not done yet)",
      "Go to linkedin.com/company/greensuppliers/admin/analytics/",
      "LinkedIn does not support embedding — view analytics directly on LinkedIn",
      "The LinkedIn Insight Tag (Partner ID: 911110173) is already tracking visitors",
    ],
  },
  {
    id: "facebook",
    label: "Facebook",
    icon: TrendingUp,
    description: "Facebook page insights and ad performance",
    color: "text-indigo-700",
    bgColor: "bg-indigo-50",
    borderColor: "border-indigo-200",
    embedUrl: "",
    setupUrl: "https://business.facebook.com/insights",
    setupInstructions: [
      "Create the Green Suppliers Facebook Business Page",
      "Set up Meta Pixel in Meta Business Suite → Events Manager",
      "Add the Pixel ID as NEXT_PUBLIC_FB_PIXEL_ID in Vercel environment variables",
      "View insights at business.facebook.com/insights",
    ],
  },
];

export default function AdminAnalyticsPage() {
  const [activeTab, setActiveTab] = useState("google");
  const activeConfig = TABS.find((t) => t.id === activeTab)!;

  return (
    <div>
      {/* Header */}
      <div className="mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Analytics</h1>
        <p className="mt-1 text-sm text-gray-500">
          Track website performance, visitor behaviour, and marketing campaigns
        </p>
      </div>

      {/* Quick Stats */}
      <div className="mb-6 grid gap-4 sm:grid-cols-3">
        <div className="rounded-2xl border border-gray-100 bg-white p-5 shadow-sm">
          <div className="flex items-center gap-3">
            <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-blue-50">
              <Globe className="h-5 w-5 text-blue-600" aria-hidden="true" />
            </div>
            <div>
              <p className="text-xs font-medium text-gray-500">Google Analytics</p>
              <p className="text-sm font-semibold text-gray-900">G-J5983P39J9</p>
            </div>
          </div>
          <p className="mt-2 text-xs text-green-600 font-medium">Active — tracking visitors</p>
        </div>
        <div className="rounded-2xl border border-gray-100 bg-white p-5 shadow-sm">
          <div className="flex items-center gap-3">
            <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-sky-50">
              <Users className="h-5 w-5 text-sky-700" aria-hidden="true" />
            </div>
            <div>
              <p className="text-xs font-medium text-gray-500">LinkedIn Insight</p>
              <p className="text-sm font-semibold text-gray-900">911110173</p>
            </div>
          </div>
          <p className="mt-2 text-xs text-green-600 font-medium">Active — tracking visitors</p>
        </div>
        <div className="rounded-2xl border border-gray-100 bg-white p-5 shadow-sm">
          <div className="flex items-center gap-3">
            <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-indigo-50">
              <TrendingUp className="h-5 w-5 text-indigo-700" aria-hidden="true" />
            </div>
            <div>
              <p className="text-xs font-medium text-gray-500">Facebook Pixel</p>
              <p className="text-sm font-semibold text-gray-900">Not configured</p>
            </div>
          </div>
          <p className="mt-2 text-xs text-amber-600 font-medium">Setup required</p>
        </div>
      </div>

      {/* Tabs */}
      <div className="mb-4 flex gap-2 border-b border-gray-200">
        {TABS.map((tab) => {
          const Icon = tab.icon;
          return (
            <button
              key={tab.id}
              onClick={() => setActiveTab(tab.id)}
              className={`flex items-center gap-2 border-b-2 px-4 py-3 text-sm font-medium transition-colors ${
                activeTab === tab.id
                  ? "border-brand-green text-brand-green"
                  : "border-transparent text-gray-500 hover:text-gray-700"
              }`}
            >
              <Icon className="h-4 w-4" aria-hidden="true" />
              {tab.label}
            </button>
          );
        })}
      </div>

      {/* Tab Content */}
      <div className="rounded-2xl border border-gray-100 bg-white shadow-sm">
        {activeConfig.embedUrl ? (
          /* Embedded Looker Studio Report */
          <div className="p-1">
            <iframe
              src={activeConfig.embedUrl}
              className="h-[700px] w-full rounded-xl border-0"
              allowFullScreen
              title={`${activeConfig.label} Report`}
            />
          </div>
        ) : (
          /* Setup Instructions */
          <div className="p-8">
            <div className="mx-auto max-w-lg text-center">
              <div className={`mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-2xl ${activeConfig.bgColor}`}>
                <BarChart3 className={`h-8 w-8 ${activeConfig.color}`} aria-hidden="true" />
              </div>
              <h2 className="text-lg font-bold text-gray-900">
                {activeConfig.label} Dashboard
              </h2>
              <p className="mt-2 text-sm text-gray-500">
                {activeConfig.description}
              </p>

              {/* Setup Steps */}
              <div className="mt-8 text-left">
                <h3 className="text-sm font-semibold text-gray-700 mb-3">
                  Setup Instructions
                </h3>
                <ol className="space-y-3">
                  {activeConfig.setupInstructions.map((step, i) => (
                    <li key={i} className="flex gap-3 text-sm text-gray-600">
                      <span className={`flex h-6 w-6 shrink-0 items-center justify-center rounded-full text-xs font-bold text-white ${
                        activeConfig.id === "google" ? "bg-blue-500" :
                        activeConfig.id === "linkedin" ? "bg-sky-600" : "bg-indigo-600"
                      }`}>
                        {i + 1}
                      </span>
                      <span className="pt-0.5">{step}</span>
                    </li>
                  ))}
                </ol>
              </div>

              <a
                href={activeConfig.setupUrl}
                target="_blank"
                rel="noopener noreferrer"
                className="mt-8 inline-flex items-center gap-2 rounded-xl bg-brand-green px-6 py-3 text-sm font-semibold text-white shadow-sm transition-colors hover:bg-brand-green-hover"
              >
                Open {activeConfig.label}
                <ExternalLink className="h-4 w-4" aria-hidden="true" />
              </a>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}

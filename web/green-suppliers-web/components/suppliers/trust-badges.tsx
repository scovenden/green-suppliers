import { Shield, BarChart3, FileCheck, Globe } from "lucide-react";
import { cn } from "@/lib/utils";

const badges = [
  {
    icon: Shield,
    label: "Verified Suppliers",
    description: "Every listing is independently validated",
    color: "text-brand-green",
    bgColor: "bg-brand-green-light",
    borderColor: "border-brand-green/20",
  },
  {
    icon: BarChart3,
    label: "ESG Scored",
    description: "Transparent, rules-based sustainability scoring",
    color: "text-emerald-600",
    bgColor: "bg-emerald-50",
    borderColor: "border-emerald-200/50",
  },
  {
    icon: FileCheck,
    label: "Cert Tracked",
    description: "Certifications monitored and auto-validated",
    color: "text-harvest-gold",
    bgColor: "bg-amber-50",
    borderColor: "border-amber-200/50",
  },
  {
    icon: Globe,
    label: "SDG Aligned",
    description: "Mapped to UN Sustainable Development Goals",
    color: "text-forest-green",
    bgColor: "bg-green-50",
    borderColor: "border-green-200/50",
  },
];

interface TrustBadgesProps {
  className?: string;
}

export function TrustBadges({ className }: TrustBadgesProps) {
  return (
    <section className={cn("py-10", className)}>
      <div className="mx-auto max-w-5xl px-4 sm:px-6 lg:px-8">
        <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
          {badges.map((badge) => {
            const Icon = badge.icon;
            return (
              <div
                key={badge.label}
                className={cn(
                  "flex flex-col items-center gap-2.5 rounded-3xl border p-5 text-center transition-all duration-300 ease-out hover:shadow-organic hover:-translate-y-1",
                  badge.borderColor,
                  badge.bgColor
                )}
              >
                <div
                  className={cn(
                    "flex h-11 w-11 items-center justify-center rounded-2xl bg-white shadow-sm",
                    badge.color
                  )}
                >
                  <Icon className="h-5 w-5" />
                </div>
                <h3 className="text-sm font-bold text-gray-900">
                  {badge.label}
                </h3>
                <p className="text-xs leading-relaxed text-gray-500">
                  {badge.description}
                </p>
              </div>
            );
          })}
        </div>
      </div>
    </section>
  );
}

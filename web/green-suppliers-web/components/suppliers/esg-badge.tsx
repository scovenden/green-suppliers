import { cn } from "@/lib/utils";
import { getEsgBadgeColor } from "@/lib/types";

interface EsgBadgeProps {
  level: string;
  className?: string;
}

export function EsgBadge({ level, className }: EsgBadgeProps) {
  const colors = getEsgBadgeColor(level);

  if (level.toLowerCase() === "none") {
    return null;
  }

  return (
    <span
      className={cn(
        "inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-semibold uppercase tracking-wide",
        colors.bg,
        colors.text,
        className
      )}
    >
      {level}
    </span>
  );
}

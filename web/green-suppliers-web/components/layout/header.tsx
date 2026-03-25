"use client";

import * as React from "react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import {
  Sheet,
  SheetTrigger,
  SheetContent,
  SheetTitle,
  SheetDescription,
  SheetClose,
} from "@/components/ui/sheet";
import { Leaf, Menu } from "lucide-react";

const navLinks = [
  { label: "Suppliers", href: "/suppliers" },
  { label: "Industries", href: "/industries" },
  { label: "Guides", href: "/guides" },
];

export function Header() {
  const pathname = usePathname();
  const [open, setOpen] = React.useState(false);

  return (
    <header className="sticky top-0 z-50 w-full bg-brand-green-dark/95 backdrop-blur-sm">
      <div className="mx-auto flex h-16 max-w-7xl items-center justify-between px-4 sm:px-6 lg:px-8">
        {/* Logo */}
        <Link href="/" className="flex items-center gap-2 group">
          <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-brand-green">
            <Leaf className="h-5 w-5 text-white" />
          </div>
          <span className="text-lg font-extrabold tracking-tight text-white">
            Green<span className="text-green-300">Suppliers</span>
          </span>
        </Link>

        {/* Desktop nav */}
        <nav className="hidden items-center gap-1 md:flex">
          {navLinks.map((link) => (
            <Link
              key={link.href}
              href={link.href}
              className={cn(
                "rounded-lg px-3 py-2 text-sm font-medium text-white/80 transition-colors hover:bg-white/10 hover:text-white",
                pathname?.startsWith(link.href) &&
                  "bg-white/10 text-white"
              )}
            >
              {link.label}
            </Link>
          ))}
        </nav>

        {/* Desktop actions */}
        <div className="hidden items-center gap-3 md:flex">
          <Link
            href="/admin"
            className="text-sm font-medium text-white/70 transition-colors hover:text-white"
          >
            Sign In
          </Link>
          <Link href="/get-listed">
            <Button
              className="bg-gradient-to-r from-brand-green to-brand-emerald text-white font-semibold shadow-md hover:from-brand-green-hover hover:to-brand-emerald-hover"
            >
              Get Listed
            </Button>
          </Link>
        </div>

        {/* Mobile hamburger */}
        <div className="md:hidden">
          <Sheet open={open} onOpenChange={setOpen}>
            <SheetTrigger
              render={
                <Button
                  variant="ghost"
                  size="icon"
                  className="text-white hover:bg-white/10"
                />
              }
            >
              <Menu className="h-5 w-5" />
              <span className="sr-only">Open menu</span>
            </SheetTrigger>
            <SheetContent side="right" className="w-72 bg-brand-green-dark border-brand-green-dark">
              <SheetTitle className="text-white">Navigation</SheetTitle>
              <SheetDescription className="sr-only">
                Site navigation menu
              </SheetDescription>
              <nav className="flex flex-col gap-1 px-2 pt-4">
                {navLinks.map((link) => (
                  <SheetClose key={link.href} render={<span />}>
                    <Link
                      href={link.href}
                      onClick={() => setOpen(false)}
                      className={cn(
                        "block rounded-lg px-3 py-2.5 text-sm font-medium text-white/80 transition-colors hover:bg-white/10 hover:text-white",
                        pathname?.startsWith(link.href) &&
                          "bg-white/10 text-white"
                      )}
                    >
                      {link.label}
                    </Link>
                  </SheetClose>
                ))}
                <div className="my-3 h-px bg-white/10" />
                <Link
                  href="/admin"
                  onClick={() => setOpen(false)}
                  className="block rounded-lg px-3 py-2.5 text-sm font-medium text-white/70 transition-colors hover:bg-white/10 hover:text-white"
                >
                  Sign In
                </Link>
                <Link
                  href="/get-listed"
                  onClick={() => setOpen(false)}
                  className="mt-2 block"
                >
                  <Button className="w-full bg-gradient-to-r from-brand-green to-brand-emerald text-white font-semibold">
                    Get Listed
                  </Button>
                </Link>
              </nav>
            </SheetContent>
          </Sheet>
        </div>
      </div>
    </header>
  );
}

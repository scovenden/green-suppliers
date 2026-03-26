import Link from "next/link";
import { Building2, Search } from "lucide-react";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";

export const metadata = {
  title: "Register - Green Suppliers",
  description: "Create your Green Suppliers account as a supplier or buyer.",
};

export default function RegisterPage() {
  return (
    <div className="flex flex-col gap-6">
      <div className="text-center">
        <h1 className="text-2xl font-bold tracking-tight text-brand-dark">
          Create your account
        </h1>
        <p className="mt-2 text-sm text-brand-earth">
          Choose how you want to use Green Suppliers
        </p>
      </div>

      <div className="grid gap-4">
        <Link href="/register/supplier" className="group">
          <Card className="cursor-pointer border-2 border-transparent transition-all hover:border-brand-green hover:shadow-md">
            <CardHeader>
              <div className="flex items-center gap-3">
                <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-brand-green-light text-brand-green transition-colors group-hover:bg-brand-green group-hover:text-white">
                  <Building2 className="h-6 w-6" />
                </div>
                <div>
                  <CardTitle className="text-lg">
                    I&apos;m a Supplier
                  </CardTitle>
                  <CardDescription>
                    List your green business and connect with buyers
                  </CardDescription>
                </div>
              </div>
            </CardHeader>
            <CardContent>
              <ul className="flex flex-col gap-1.5 text-sm text-brand-earth">
                <li className="flex items-center gap-2">
                  <span className="h-1.5 w-1.5 rounded-full bg-brand-green" />
                  Showcase your ESG credentials
                </li>
                <li className="flex items-center gap-2">
                  <span className="h-1.5 w-1.5 rounded-full bg-brand-green" />
                  Get verified and earn trust badges
                </li>
                <li className="flex items-center gap-2">
                  <span className="h-1.5 w-1.5 rounded-full bg-brand-green" />
                  Receive leads from enterprise buyers
                </li>
              </ul>
            </CardContent>
          </Card>
        </Link>

        <Link href="/register/buyer" className="group">
          <Card className="cursor-pointer border-2 border-transparent transition-all hover:border-brand-emerald hover:shadow-md">
            <CardHeader>
              <div className="flex items-center gap-3">
                <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-emerald-50 text-brand-emerald transition-colors group-hover:bg-brand-emerald group-hover:text-white">
                  <Search className="h-6 w-6" />
                </div>
                <div>
                  <CardTitle className="text-lg">I&apos;m a Buyer</CardTitle>
                  <CardDescription>
                    Find verified green suppliers for your procurement needs
                  </CardDescription>
                </div>
              </div>
            </CardHeader>
            <CardContent>
              <ul className="flex flex-col gap-1.5 text-sm text-brand-earth">
                <li className="flex items-center gap-2">
                  <span className="h-1.5 w-1.5 rounded-full bg-brand-emerald" />
                  Search verified sustainable suppliers
                </li>
                <li className="flex items-center gap-2">
                  <span className="h-1.5 w-1.5 rounded-full bg-brand-emerald" />
                  Compare ESG scores and certifications
                </li>
                <li className="flex items-center gap-2">
                  <span className="h-1.5 w-1.5 rounded-full bg-brand-emerald" />
                  Contact suppliers directly
                </li>
              </ul>
            </CardContent>
          </Card>
        </Link>
      </div>

      <p className="text-center text-sm text-brand-earth">
        Already have an account?{" "}
        <Link
          href="/admin/login"
          className="font-medium text-brand-green hover:text-brand-green-hover hover:underline"
        >
          Sign in
        </Link>
      </p>
    </div>
  );
}

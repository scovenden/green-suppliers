import Link from "next/link";
import { Leaf } from "lucide-react";
import { AuthProvider } from "@/lib/auth-context";

export default function AuthLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <AuthProvider>
      <div className="flex min-h-screen flex-col items-center justify-center bg-brand-green-light px-4 py-12">
        <div className="mb-8">
          <Link href="/" className="flex items-center gap-2 group">
            <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-brand-green transition-transform group-hover:scale-105">
              <Leaf className="h-6 w-6 text-white" />
            </div>
            <span className="text-2xl font-extrabold tracking-tight text-brand-dark">
              Green<span className="text-brand-green">Suppliers</span>
            </span>
          </Link>
        </div>
        <div className="w-full max-w-md">{children}</div>
      </div>
    </AuthProvider>
  );
}

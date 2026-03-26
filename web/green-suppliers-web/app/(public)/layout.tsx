import { Header } from "@/components/layout/header";
import { Footer } from "@/components/layout/footer";
import { AuthProviderWrapper } from "@/components/layout/auth-provider-wrapper";

export default function PublicLayout({ children }: { children: React.ReactNode }) {
  return (
    <AuthProviderWrapper>
      <Header />
      <main>{children}</main>
      <Footer />
    </AuthProviderWrapper>
  );
}

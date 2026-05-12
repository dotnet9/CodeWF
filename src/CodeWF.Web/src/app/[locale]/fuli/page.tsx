import { redirect } from "next/navigation";
import { withLocale } from "@/i18n";
import type { LocalePageProps } from "../layout";

export default async function FuliAliasPage({ params }: LocalePageProps) {
  const { locale } = await params;
  redirect(withLocale(locale, "/tool/compound-interest-calculator"));
}

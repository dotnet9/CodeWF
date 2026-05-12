import { redirect } from "next/navigation";
import { withLocale } from "@/i18n";
import type { LocalePageProps } from "../layout";

export default async function IconAliasPage({ params }: LocalePageProps) {
  const { locale } = await params;
  redirect(withLocale(locale, "/tool/icon-converter"));
}

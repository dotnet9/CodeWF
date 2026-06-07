import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  async rewrites() {
    const apiBase = (process.env.API_BASE_URL ?? process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5002/api")
      .replace(/\/$/, "")
      .replace(/\/api$/, "");

    return [
      {
        source: "/api/:path*",
        destination: `${apiBase}/api/:path*`
      }
    ];
  },
  images: {
    formats: ["image/avif", "image/webp"],
    minimumCacheTTL: 86400,
    remotePatterns: [
      { protocol: "https", hostname: "img1.dotnet9.com" },
      { protocol: "http", hostname: "localhost" }
    ]
  },
  poweredByHeader: false,
  agentRules: false
};

export default nextConfig;

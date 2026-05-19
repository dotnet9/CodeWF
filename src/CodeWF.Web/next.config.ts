import type { NextConfig } from "next";

const nextConfig: NextConfig = {
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

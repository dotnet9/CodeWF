import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  images: {
    remotePatterns: [
      { protocol: "https", hostname: "img1.dotnet9.com" },
      { protocol: "http", hostname: "localhost" }
    ]
  },
  poweredByHeader: false,
  agentRules: false
};

export default nextConfig;

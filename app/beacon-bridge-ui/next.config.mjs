/** @type {import('next').NextConfig} */
const nextConfig = {
  output: "standalone",
  async headers() {
    return [
      {
        source: "/(.*)",
        headers: [{ key: "x-clacks-overhead", value: "GNU Terry Pratchett" }]
      }
    ];
  }
};

export default nextConfig;

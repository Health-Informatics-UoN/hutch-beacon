import { NextResponse } from "next/server";

// This function can be marked `async` if using `await` inside
export function middleware(request) {
  const newUrl = request.nextUrl.search
    ? `${process.env.BACKEND_URL}${request.nextUrl.pathname}${request.nextUrl.search}`
    : `${process.env.BACKEND_URL}${request.nextUrl.pathname}`;
  return NextResponse.rewrite(
    new URL(
      newUrl,
      request.url,
    ),
  );
}

// See "Matching Paths" below to learn more
export const config = {
  matcher: "/api/:path*",
};

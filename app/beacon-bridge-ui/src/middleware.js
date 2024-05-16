import { NextResponse } from "next/server";

// This function can be marked `async` if using `await` inside
export function middleware(request) {
  if (request.nextUrl.pathname.startsWith("/api/filtering_terms")) {
    return NextResponse.rewrite(
      new URL(`${process.env.BACKEND_URL}/api/filtering_terms`, request.url),
    );
  }

  if (request.nextUrl.pathname.startsWith("/api/individuals")) {
    return NextResponse.rewrite(
      new URL(`${process.env.BACKEND_URL}/api/individuals`, request.url),
    );
  }

  if (request.nextUrl.pathname.startsWith("/api/info")) {
    return NextResponse.rewrite(
      new URL(`${process.env.BACKEND_URL}/api/info`, request.url),
    );
  }

  if (request.nextUrl.pathname.startsWith("/api/service-info")) {
    return NextResponse.rewrite(
      new URL(`${process.env.BACKEND_URL}/api/service-info`, request.url),
    );
  }

  if (
    request.nextUrl.pathname.startsWith("/api/Submission/UpdateStatusForTre")
  ) {
    return NextResponse.rewrite(
      new URL(
        `${process.env.BACKEND_URL}/api/Submission/UpdateStatusForTre`,
        request.url,
      ),
    );
  }

  // root of `/api` must go last
  if (request.nextUrl.pathname.startsWith("/api")) {
    return NextResponse.rewrite(
      new URL(`${process.env.BACKEND_URL}/api`, request.url),
    );
  }
}

// See "Matching Paths" below to learn more
export const config = {
  matcher: "/api/:path*",
};

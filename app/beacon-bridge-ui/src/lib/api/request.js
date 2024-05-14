import { apiUrl as apiUrl } from "@/constants";
import { ApiError } from "./error";

/**
 * Base request function to query the backend API with a users access token
 * @param url URL to query.
 * @param options RequestOptions object
 * @returns The given type T
 */
export const request = async (
  url,
  options = {}
) => {
  // Get the KeyCloak id_token
  // const session = await getServerSession(authOptions);
  // const token = session?.access_token;

  const headers = {
    // Authorization: `Bearer ${token}`,
    ...(options.headers || {}),
  };

  const response = await fetch(`${apiUrl}/api/${url}`, {
    method: options.method || "GET",
    headers: headers,
    body: options.body,
    cache: options.cache,
    next: options.next,
  });

  if (!response.ok) {
    const errorMessage = await response.text();
    throw new ApiError(errorMessage, response.status);
  }

  if (options.download) {
    return response.blob();
  }

  if (response.status === 204) {
    return {};
  }

  return response.json();
};

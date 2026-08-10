(function () {
  "use strict";

  const config = window.VAULT_CONFIG;

  class ApiError extends Error {
    constructor(message, status, details = null) {
      super(message);
      this.name = "ApiError";
      this.status = status;
      this.details = details;
    }
  }

  function loadSession() {
    const raw = localStorage.getItem(config.PERSISTENT_SESSION_KEY)
      || sessionStorage.getItem(config.SESSION_KEY);
    if (!raw) return null;
    try {
      const session = JSON.parse(raw);
      if (!session?.token || !session?.expiresAtUtc) return null;
      if (new Date(session.expiresAtUtc).getTime() <= Date.now()) {
        clearSession();
        return null;
      }
      return session;
    } catch {
      clearSession();
      return null;
    }
  }

  function saveSession(session, persistent = false) {
    clearSession();
    const key = persistent ? config.PERSISTENT_SESSION_KEY : config.SESSION_KEY;
    const storage = persistent ? localStorage : sessionStorage;
    storage.setItem(key, JSON.stringify(session));
  }

  function clearSession() {
    localStorage.removeItem(config.PERSISTENT_SESSION_KEY);
    sessionStorage.removeItem(config.SESSION_KEY);
  }

  function getToken() {
    return loadSession()?.token || null;
  }

  async function parseError(response) {
    let data = null;
    try {
      data = await response.json();
    } catch {
      // Some error responses are empty.
    }

    if (data?.message) return new ApiError(data.message, response.status, data);
    if (data?.error) return new ApiError(data.error, response.status, data);
    if (data?.title && data?.errors) {
      const first = Object.values(data.errors).flat()[0];
      return new ApiError(first || data.title, response.status, data);
    }
    if (data?.title) return new ApiError(data.title, response.status, data);

    const defaults = {
      400: "The request was not accepted. Please check the entered details.",
      401: "Your session is invalid or has expired.",
      403: "You do not have permission to perform this action.",
      404: "The requested item could not be found.",
      409: "This item already exists or conflicts with existing data.",
      500: "The server could not complete the request."
    };
    return new ApiError(defaults[response.status] || `Request failed (${response.status}).`, response.status, data);
  }

  async function request(path, options = {}) {
    const headers = new Headers(options.headers || {});
    const token = getToken();
    if (token) headers.set("Authorization", `Bearer ${token}`);

    const isFormData = options.body instanceof FormData;
    if (options.body && !isFormData && !headers.has("Content-Type")) {
      headers.set("Content-Type", "application/json");
    }

    let response;
    try {
      response = await fetch(`${config.API_BASE_URL}${path}`, {
        ...options,
        headers
      });
    } catch (error) {
      throw new ApiError(
        "Cannot connect to the backend API. Start the ASP.NET Core project and accept its HTTPS certificate.",
        0,
        error
      );
    }

    if (!response.ok) {
      const apiError = await parseError(response);
      if (response.status === 401) {
        clearSession();
        window.dispatchEvent(new CustomEvent("vault:unauthorized"));
      }
      throw apiError;
    }

    if (response.status === 204) return null;
    const contentType = response.headers.get("content-type") || "";
    if (contentType.includes("application/json")) return response.json();
    return response;
  }

  function getDownloadFileName(response, path) {
    const disposition = response.headers.get("content-disposition") || "";
    const utf8Name = disposition.match(/filename\*=UTF-8''([^;]+)/i)?.[1];
    const plainName = disposition.match(/filename="?([^";]+)"?/i)?.[1];

    if (utf8Name) {
      try { return decodeURIComponent(utf8Name.replace(/^"|"$/g, "")); } catch { /* use fallback */ }
    }
    if (plainName) return plainName.trim();

    const lastPart = path.split("/").filter(Boolean).pop();
    return lastPart && lastPart !== "download" ? lastPart : "download";
  }

  async function getBlob(path) {
    const response = await request(path, { method: "GET", cache: "no-store" });
    if (!(response instanceof Response)) {
      throw new ApiError("The server did not return file content.", 500, response);
    }

    const blob = await response.blob();
    if (!blob.size) {
      throw new ApiError("The returned file is empty.", 500);
    }

    return {
      blob,
      contentType: response.headers.get("content-type") || blob.type || "application/octet-stream"
    };
  }

  async function download(path) {
    const response = await request(path, { method: "GET", cache: "no-store" });
    if (!(response instanceof Response)) {
      throw new ApiError("The server did not return a downloadable file.", 500, response);
    }

    const blob = await response.blob();
    if (!blob.size) {
      throw new ApiError("The downloaded file is empty.", 500);
    }

    const fileName = getDownloadFileName(response, path);
    const objectUrl = URL.createObjectURL(blob);
    const anchor = document.createElement("a");
    anchor.href = objectUrl;
    anchor.download = fileName;
    anchor.style.display = "none";
    document.body.appendChild(anchor);

    try {
      anchor.click();
    } finally {
      anchor.remove();
      // Give the browser enough time to start reading the Blob URL before releasing it.
      window.setTimeout(() => URL.revokeObjectURL(objectUrl), 1500);
    }

    return fileName;
  }

  window.VaultApi = Object.freeze({
    ApiError,
    loadSession,
    saveSession,
    clearSession,
    getToken,
    get: (path) => request(path, { method: "GET" }),
    post: (path, data) => request(path, { method: "POST", body: data instanceof FormData ? data : JSON.stringify(data) }),
    put: (path, data) => request(path, { method: "PUT", body: JSON.stringify(data) }),
    patch: (path, data) => request(path, { method: "PATCH", body: JSON.stringify(data) }),
    delete: (path) => request(path, { method: "DELETE" }),
    blob: getBlob,
    download
  });
})();

(function () {
  "use strict";

  const api = window.VaultApi;
  const config = window.VAULT_CONFIG;
  const $ = (selector, root = document) => root.querySelector(selector);
  const $$ = (selector, root = document) => [...root.querySelectorAll(selector)];

  const elements = {
    authView: $("#authView"),
    appView: $("#appView"),
    loginTab: $("#loginTab"),
    registerTab: $("#registerTab"),
    loginPanel: $("#loginPanel"),
    registerPanel: $("#registerPanel"),
    loginForm: $("#loginForm"),
    registerForm: $("#registerForm"),
    content: $("#content"),
    pageTitle: $("#pageTitle"),
    pageEyebrow: $("#pageEyebrow"),
    headerUserName: $("#headerUserName"),
    headerUserRole: $("#headerUserRole"),
    profileInitials: $("#profileInitials"),
    adminNav: $("#adminNav"),
    quickUploadButton: $("#quickUploadButton"),
    profileMenuButton: $("#profileMenuButton"),
    logoutButton: $("#logoutButton"),
    sidebar: $("#sidebar"),
    sidebarOverlay: $("#sidebarOverlay"),
    menuButton: $("#menuButton"),
    connectionDot: $("#connectionDot"),
    connectionText: $("#connectionText"),
    modalBackdrop: $("#modalBackdrop"),
    modal: $("#modal"),
    modalEyebrow: $("#modalEyebrow"),
    modalTitle: $("#modalTitle"),
    modalBody: $("#modalBody"),
    modalClose: $("#modalClose"),
    confirmBackdrop: $("#confirmBackdrop"),
    confirmTitle: $("#confirmTitle"),
    confirmMessage: $("#confirmMessage"),
    confirmCancel: $("#confirmCancel"),
    confirmAccept: $("#confirmAccept"),
    toastRegion: $("#toastRegion")
  };

  const state = {
    session: api.loadSession(),
    route: "dashboard",
    folders: [],
    categories: [],
    documents: [],
    credentials: [],
    profile: null,
    adminTab: "overview",
    documentSearch: "",
    documentFolderId: "",
    credentialSearch: "",
    activePreviewUrl: null,
    confirmResolver: null
  };

  const routeMeta = {
    dashboard: ["Dashboard", "Overview"],
    documents: ["Documents", "Private files"],
    folders: ["Folders", "Organisation"],
    categories: ["Categories", "Credential organisation"],
    credentials: ["Credentials", "Secure records"],
    profile: ["My profile", "Account"],
    admin: ["Administration", "System control"]
  };

  function escapeHtml(value) {
    return String(value ?? "")
      .replaceAll("&", "&amp;")
      .replaceAll("<", "&lt;")
      .replaceAll(">", "&gt;")
      .replaceAll('"', "&quot;")
      .replaceAll("'", "&#039;");
  }

  function formatDate(value, includeTime = false) {
    if (!value) return "—";
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) return "—";
    return new Intl.DateTimeFormat(undefined, includeTime
      ? { dateStyle: "medium", timeStyle: "short" }
      : { dateStyle: "medium" }).format(date);
  }

  function formatBytes(bytes) {
    const value = Number(bytes) || 0;
    if (value === 0) return "0 B";
    const units = ["B", "KB", "MB", "GB"];
    const index = Math.min(Math.floor(Math.log(value) / Math.log(1024)), units.length - 1);
    return `${(value / Math.pow(1024, index)).toFixed(index === 0 ? 0 : 1)} ${units[index]}`;
  }

  function initials(name) {
    return String(name || "User")
      .trim()
      .split(/\s+/)
      .slice(0, 2)
      .map((part) => part[0]?.toUpperCase() || "")
      .join("") || "U";
  }

  function icon(name) {
    return `<svg aria-hidden="true"><use href="#icon-${name}"></use></svg>`;
  }

  function getFileIcon(document) {
    const type = (document.contentType || "").toLowerCase();
    if (type.startsWith("image/")) return ["file", "image"];
    if (type.includes("text") || document.fileName?.toLowerCase().endsWith(".txt")) return ["file", "text"];
    return ["file", ""];
  }

  function setConnection(connected) {
    elements.connectionDot.classList.toggle("offline", !connected);
    elements.connectionText.textContent = connected ? "API connected" : "API unavailable";
  }

  function setButtonLoading(button, loading) {
    if (!button) return;
    button.classList.toggle("loading", loading);
    button.disabled = loading;
  }

  function showToast(message, type = "success", title = null) {
    const titles = { success: "Success", error: "Something went wrong", warning: "Please check", info: "Information" };
    const icons = { success: "check", error: "alert", warning: "alert", info: "info" };
    const toast = document.createElement("div");
    toast.className = `toast ${type}`;
    toast.innerHTML = `
      <span class="toast-icon">${icon(icons[type] || "info")}</span>
      <div><strong>${escapeHtml(title || titles[type] || "Notice")}</strong><small>${escapeHtml(message)}</small></div>
      <button class="toast-close" type="button" aria-label="Dismiss">${icon("x")}</button>`;
    elements.toastRegion.appendChild(toast);
    const remove = () => toast.remove();
    $(".toast-close", toast).addEventListener("click", remove);
    setTimeout(remove, 4800);
  }

  function handleError(error, fallback = "The operation could not be completed.") {
    console.error(error);
    const message = error?.message || fallback;
    setConnection(error?.status !== 0);
    showToast(message, "error");
  }

  function clearFieldErrors(form) {
    $$(".field-error", form).forEach((element) => { element.textContent = ""; });
    $$(".invalid", form).forEach((element) => element.classList.remove("invalid"));
    $$(".input-wrap.invalid", form).forEach((element) => element.classList.remove("invalid"));
  }

  function setFieldError(inputId, message) {
    const input = document.getElementById(inputId);
    const error = document.querySelector(`[data-error-for="${inputId}"]`);
    if (error) error.textContent = message;
    if (input) {
      input.classList.add("invalid");
      input.closest(".input-wrap")?.classList.add("invalid");
    }
  }

  function loadingPage() {
    return `<div class="loading-page"><div class="loading-metrics">${"<div class=\"skeleton\"></div>".repeat(4)}</div><div class="skeleton"></div></div>`;
  }

  function emptyState(iconName, title, message, actionText = "", action = "") {
    return `<div class="empty-state">
      <span class="empty-icon">${icon(iconName)}</span>
      <h3>${escapeHtml(title)}</h3>
      <p>${escapeHtml(message)}</p>
      ${actionText ? `<button class="button button-primary" type="button" data-action="${escapeHtml(action)}">${icon("plus")}<span>${escapeHtml(actionText)}</span></button>` : ""}
    </div>`;
  }

  function openModal({ title, eyebrow = "", body, onOpen = null }) {
    if (state.activePreviewUrl) {
      URL.revokeObjectURL(state.activePreviewUrl);
      state.activePreviewUrl = null;
    }
    elements.modal.classList.remove("modal-wide");
    elements.modalTitle.textContent = title;
    elements.modalEyebrow.textContent = eyebrow;
    elements.modalBody.innerHTML = body;
    elements.modalBackdrop.classList.remove("hidden");
    document.body.style.overflow = "hidden";
    setTimeout(() => {
      const focusable = elements.modal.querySelector("input, select, textarea, button");
      focusable?.focus();
      onOpen?.(elements.modalBody);
    }, 0);
  }

  function closeModal() {
    if (state.activePreviewUrl) {
      URL.revokeObjectURL(state.activePreviewUrl);
      state.activePreviewUrl = null;
    }
    elements.modal.classList.remove("modal-wide");
    elements.modalBackdrop.classList.add("hidden");
    elements.modalBody.innerHTML = "";
    document.body.style.overflow = "";
  }

  function confirmAction({ title = "Confirm action", message, acceptText = "Continue", danger = true }) {
    elements.confirmTitle.textContent = title;
    elements.confirmMessage.textContent = message;
    elements.confirmAccept.textContent = acceptText;
    elements.confirmAccept.className = `button ${danger ? "button-danger" : "button-primary"}`;
    elements.confirmBackdrop.classList.remove("hidden");
    document.body.style.overflow = "hidden";
    return new Promise((resolve) => { state.confirmResolver = resolve; });
  }

  function resolveConfirm(value) {
    elements.confirmBackdrop.classList.add("hidden");
    document.body.style.overflow = "";
    state.confirmResolver?.(value);
    state.confirmResolver = null;
  }

  function setAuthMode(mode) {
    const login = mode === "login";
    elements.loginTab.classList.toggle("active", login);
    elements.registerTab.classList.toggle("active", !login);
    elements.loginTab.setAttribute("aria-selected", String(login));
    elements.registerTab.setAttribute("aria-selected", String(!login));
    elements.loginPanel.classList.toggle("hidden", !login);
    elements.registerPanel.classList.toggle("hidden", login);
    setTimeout(() => (login ? $("#loginEmail") : $("#registerName"))?.focus(), 0);
  }

  function updateSessionUi() {
    const session = state.session;
    if (!session) return;
    elements.headerUserName.textContent = session.fullName;
    elements.headerUserRole.textContent = session.role;
    elements.profileInitials.textContent = initials(session.fullName);
    elements.adminNav.classList.toggle("hidden", session.role !== "Administrator");
  }

  function showAuth(message = "") {
    state.session = null;
    api.clearSession();
    elements.appView.classList.add("hidden");
    elements.authView.classList.remove("hidden");
    closeSidebar();
    setAuthMode("login");
    if (message) showToast(message, "warning", "Session ended");
  }

  function showApp() {
    elements.authView.classList.add("hidden");
    elements.appView.classList.remove("hidden");
    updateSessionUi();
    navigate("dashboard");
  }

  function openSidebar() {
    elements.sidebar.classList.add("open");
    elements.sidebarOverlay.classList.add("open");
  }

  function closeSidebar() {
    elements.sidebar.classList.remove("open");
    elements.sidebarOverlay.classList.remove("open");
  }

  async function navigate(route, options = {}) {
    if (route === "admin" && state.session?.role !== "Administrator") return;
    state.route = route;
    const [title, eyebrow] = routeMeta[route] || ["Vault", ""];
    elements.pageTitle.textContent = title;
    elements.pageEyebrow.textContent = eyebrow;
    $$(".nav-item").forEach((item) => item.classList.toggle("active", item.dataset.route === route));
    closeSidebar();
    elements.content.innerHTML = loadingPage();
    elements.content.focus({ preventScroll: true });

    try {
      if (route === "dashboard") await renderDashboard();
      if (route === "documents") await renderDocuments(options);
      if (route === "folders") await renderFolders();
      if (route === "categories") await renderCategories();
      if (route === "credentials") await renderCredentials();
      if (route === "profile") await renderProfile();
      if (route === "admin") await renderAdmin();
      setConnection(true);
    } catch (error) {
      handleError(error);
      elements.content.innerHTML = `<div class="card">${emptyState("alert", "Unable to load this page", error?.message || "Please try again.", "Try again", "retry-page")}</div>`;
      $("[data-action='retry-page']", elements.content)?.addEventListener("click", () => navigate(route, options));
    }
  }

  async function refreshCoreData() {
    const [folders, categories, documents, credentials] = await Promise.all([
      api.get("/folders"),
      api.get("/categories"),
      api.get("/documents"),
      api.get("/credentials")
    ]);
    state.folders = folders;
    state.categories = categories;
    state.documents = documents;
    state.credentials = credentials;
  }

  function metricCard(label, value, iconName, color, tint) {
    return `<article class="metric-card" style="--metric-color:${color};--metric-tint:${tint}">
      <span class="metric-icon">${icon(iconName)}</span>
      <div class="metric-copy"><span class="metric-label">${escapeHtml(label)}</span><strong class="metric-value">${escapeHtml(value)}</strong></div>
    </article>`;
  }

  async function renderDashboard() {
    await refreshCoreData();
    const totalSize = state.documents.reduce((sum, item) => sum + Number(item.fileSizeBytes || 0), 0);
    const recent = state.documents.slice(0, 5);
    const firstName = state.session?.fullName?.split(" ")[0] || "there";

    elements.content.innerHTML = `
      <div class="page-header">
        <div class="page-header-copy"><h2>Good to see you, ${escapeHtml(firstName)}</h2><p>Here is a quick view of your private vault.</p></div>
        <div class="page-actions"><button class="button button-primary" type="button" data-action="upload-document">${icon("upload")}<span>Upload document</span></button></div>
      </div>
      <section class="metrics-grid">
        ${metricCard("Documents", state.documents.length, "file", "#246bfd", "#eaf1ff")}
        ${metricCard("Folders", state.folders.length, "folder", "#8a62db", "#f1eafd")}
        ${metricCard("Credentials", state.credentials.length, "key", "#15966e", "#e6f7f1")}
        ${metricCard("Storage used", formatBytes(totalSize), "database", "#d98416", "#fff4df")}
      </section>
      <section class="dashboard-grid">
        <article class="card">
          <header class="card-header"><div><h3>Recent documents</h3><p>Your latest encrypted uploads.</p></div><button class="button button-secondary button-compact" type="button" data-route-link="documents">View all</button></header>
          <div class="table-wrap">
            ${recent.length ? documentTable(recent, true) : emptyState("file", "No documents yet", "Upload your first private document to begin building your vault.", "Upload document", "upload-document")}
          </div>
        </article>
        <aside>
          <article class="card">
            <header class="card-header"><div><h3>Quick actions</h3><p>Common vault tasks.</p></div></header>
            <div class="card-body quick-actions">
              <button class="quick-action" type="button" data-action="upload-document"><span class="quick-action-icon">${icon("upload")}</span><div><strong>Upload a document</strong><small>Encrypt and store a private file</small></div>${icon("chevron-right")}</button>
              <button class="quick-action" type="button" data-action="create-folder"><span class="quick-action-icon">${icon("folder")}</span><div><strong>Create a folder</strong><small>Organise your stored documents</small></div>${icon("chevron-right")}</button>
              <button class="quick-action" type="button" data-action="create-category"><span class="quick-action-icon">${icon("tag")}</span><div><strong>Create a category</strong><small>Group secure credential records</small></div>${icon("chevron-right")}</button>
              <button class="quick-action" type="button" data-action="create-credential"><span class="quick-action-icon">${icon("key")}</span><div><strong>Add a credential</strong><small>Store an account record securely</small></div>${icon("chevron-right")}</button>
            </div>
          </article>
          <div class="security-summary">
            <h4>${icon("shield-check")} Protection active</h4>
            <ul><li>${icon("check")} JWT-authenticated access</li><li>${icon("check")} AES-encrypted private data</li><li>${icon("check")} SHA-256 file integrity</li></ul>
          </div>
        </aside>
      </section>`;

    bindCommonContentActions();
    bindDocumentActions();
  }

  function documentTable(items, compact = false) {
    return `<table class="data-table">
      <thead><tr><th>Document</th><th>Folder</th>${compact ? "" : "<th>Integrity hash</th>"}<th>Uploaded</th><th class="table-action-heading">Actions</th></tr></thead>
      <tbody>${items.map((doc) => {
        const [fileIcon, className] = getFileIcon(doc);
        return `<tr>
          <td><div class="file-cell"><span class="file-icon ${className}">${icon(fileIcon)}</span><div><strong title="${escapeHtml(doc.fileName)}">${escapeHtml(doc.fileName)}</strong><small>${escapeHtml(formatBytes(doc.fileSizeBytes))} · ${escapeHtml(doc.contentType || "File")}</small></div></div></td>
          <td>${doc.folderName ? `<span class="badge primary">${icon("folder")} ${escapeHtml(doc.folderName)}</span>` : `<span class="badge">Unfiled</span>`}</td>
          ${compact ? "" : `<td><code class="hash-code" title="${escapeHtml(doc.integrityHashSha256)}">${escapeHtml(doc.integrityHashSha256)}</code></td>`}
          <td>${escapeHtml(formatDate(doc.createdAtUtc))}</td>
          <td><div class="table-actions">
            <button class="action-button" type="button" data-document-action="preview" data-id="${doc.id}" title="View / preview">${icon("eye")}</button>
            <button class="action-button" type="button" data-document-action="download" data-id="${doc.id}" title="Download">${icon("download")}</button>
            <button class="action-button" type="button" data-document-action="integrity" data-id="${doc.id}" title="Verify integrity">${icon("shield-check")}</button>
            <button class="action-button" type="button" data-document-action="rename" data-id="${doc.id}" title="Rename">${icon("edit")}</button>
            <button class="action-button danger" type="button" data-document-action="delete" data-id="${doc.id}" title="Delete">${icon("trash")}</button>
          </div></td>
        </tr>`;
      }).join("")}</tbody>
    </table>`;
  }

  async function loadDocuments() {
    const query = new URLSearchParams();
    if (state.documentSearch) query.set("search", state.documentSearch);
    if (state.documentFolderId) query.set("folderId", state.documentFolderId);
    state.documents = await api.get(`/documents${query.toString() ? `?${query}` : ""}`);
  }

  async function renderDocuments(options = {}) {
    if (options.folderId !== undefined) state.documentFolderId = options.folderId || "";
    const [folders] = await Promise.all([
      api.get("/folders"),
      loadDocuments()
    ]);
    state.folders = folders;
    elements.content.innerHTML = `
      <div class="page-header">
        <div class="page-header-copy"><h2>Private documents</h2><p>Upload, search, view, verify, download, rename, and delete your encrypted files.</p></div>
        <div class="page-actions"><button class="button button-primary" type="button" data-action="upload-document">${icon("upload")}<span>Upload document</span></button></div>
      </div>
      <div class="toolbar">
        <div class="toolbar-group">
          <div class="search-box">${icon("search")}<input id="documentSearch" type="search" placeholder="Search file names…" value="${escapeHtml(state.documentSearch)}" /></div>
          <select id="documentFolderFilter" aria-label="Filter by folder">
            <option value="">All folders</option>
            ${state.folders.map((folder) => `<option value="${folder.id}" ${folder.id === state.documentFolderId ? "selected" : ""}>${escapeHtml(folder.name)}</option>`).join("")}
          </select>
        </div>
        <span class="badge">${state.documents.length} item${state.documents.length === 1 ? "" : "s"}</span>
      </div>
      <article class="card"><div id="documentList" class="table-wrap">${state.documents.length ? documentTable(state.documents) : emptyState("file", "No documents found", state.documentSearch || state.documentFolderId ? "Try changing the search or folder filter." : "Upload your first document. It will be encrypted before storage.", "Upload document", "upload-document")}</div></article>`;

    bindCommonContentActions();
    bindDocumentActions();

    let timer;
    $("#documentSearch")?.addEventListener("input", (event) => {
      clearTimeout(timer);
      timer = setTimeout(async () => {
        state.documentSearch = event.target.value.trim();
        await refreshDocumentList();
      }, 280);
    });
    $("#documentFolderFilter")?.addEventListener("change", async (event) => {
      state.documentFolderId = event.target.value;
      await refreshDocumentList();
    });
  }

  async function refreshDocumentList() {
    const list = $("#documentList");
    if (!list) return;
    list.innerHTML = `<div class="empty-state"><span class="skeleton" style="width:62px;height:62px"></span></div>`;
    try {
      await loadDocuments();
      list.innerHTML = state.documents.length ? documentTable(state.documents) : emptyState("file", "No documents found", "Try changing the search or folder filter.", "Upload document", "upload-document");
      const badge = $(".toolbar > .badge");
      if (badge) badge.textContent = `${state.documents.length} item${state.documents.length === 1 ? "" : "s"}`;
      bindCommonContentActions();
      bindDocumentActions();
    } catch (error) {
      handleError(error);
      list.innerHTML = emptyState("alert", "Could not load documents", error.message || "Try again.");
    }
  }

  function bindDocumentActions() {
    $$('[data-document-action]', elements.content).forEach((button) => {
      button.onclick = async (event) => {
        event.preventDefault();
        event.stopPropagation();

        const document = state.documents.find(
          (item) => String(item.id).toLowerCase() === String(button.dataset.id).toLowerCase()
        );
        if (!document) {
          showToast("The selected document could not be found. Refreshing the dashboard.", "error");
          await refreshCoreData();
          return;
        }

        const action = button.dataset.documentAction;
        try {
          if (action === "preview") await previewDocument(document);
          else if (action === "download") await downloadDocument(document, button);
          else if (action === "integrity") await verifyDocument(document);
          else if (action === "rename") openRenameDocument(document);
          else if (action === "delete") await deleteDocument(document);
        } catch (error) {
          handleError(error);
        }
      };
    });
  }

  async function previewDocument(document) {
    openModal({
      title: document.fileName,
      eyebrow: "Secure document view",
      body: `<div class="document-preview-loading"><span class="skeleton" style="width:64px;height:64px"></span><p>Authorising, decrypting, and verifying the file…</p></div>`
    });
    elements.modal.classList.add("modal-wide");

    try {
      const extension = `.${String(document.fileName || "").split(".").pop()?.toLowerCase()}`;
      const previewable = [".pdf", ".txt", ".jpg", ".jpeg", ".png"].includes(extension);

      if (!previewable) {
        elements.modalBody.innerHTML = `
          <div class="document-preview-unavailable">
            <span class="empty-icon">${icon("file")}</span>
            <h3>Inline preview is not available for this file type</h3>
            <p>You can still view the item details here and download the decrypted file to open it in its native application.</p>
          </div>
          <div class="detail-list">
            ${detailRow("File name", document.fileName)}
            ${detailRow("Type", document.contentType || "File")}
            ${detailRow("Size", formatBytes(document.fileSizeBytes))}
            ${detailRow("Folder", document.folderName || "Unfiled")}
            ${detailRow("Uploaded", formatDate(document.createdAtUtc, true))}
          </div>
          <div class="modal-actions"><button class="button button-secondary" type="button" data-close-modal>Close</button><button class="button button-primary" type="button" data-preview-download>${icon("download")} Download to view</button></div>`;
        $("[data-close-modal]", elements.modalBody)?.addEventListener("click", closeModal);
        $("[data-preview-download]", elements.modalBody)?.addEventListener("click", async (event) => {
          await downloadDocument(document, event.currentTarget);
        });
        return;
      }

      const result = await api.blob(`/documents/${document.id}/preview`);
      const contentType = String(result.contentType || "").toLowerCase();

      if (contentType.startsWith("text/plain")) {
        const text = await result.blob.text();
        elements.modalBody.innerHTML = `
          <div class="document-preview-meta"><span>${icon("shield-check")} Integrity verified before preview</span><span>${escapeHtml(formatBytes(document.fileSizeBytes))}</span></div>
          <pre class="document-text-preview">${escapeHtml(text)}</pre>
          <div class="modal-actions"><button class="button button-secondary" type="button" data-close-modal>Close</button><button class="button button-primary" type="button" data-preview-download>${icon("download")} Download</button></div>`;
      } else {
        state.activePreviewUrl = URL.createObjectURL(result.blob);
        const viewer = contentType.startsWith("image/")
          ? `<div class="document-image-preview"><img src="${state.activePreviewUrl}" alt="Preview of ${escapeHtml(document.fileName)}" /></div>`
          : `<iframe class="document-preview-frame" src="${state.activePreviewUrl}" title="Preview of ${escapeHtml(document.fileName)}"></iframe>`;
        elements.modalBody.innerHTML = `
          <div class="document-preview-meta"><span>${icon("shield-check")} Integrity verified before preview</span><span>${escapeHtml(formatBytes(document.fileSizeBytes))}</span></div>
          ${viewer}
          <div class="modal-actions"><button class="button button-secondary" type="button" data-close-modal>Close</button><button class="button button-primary" type="button" data-preview-download>${icon("download")} Download</button></div>`;
      }

      $("[data-close-modal]", elements.modalBody)?.addEventListener("click", closeModal);
      $("[data-preview-download]", elements.modalBody)?.addEventListener("click", async (event) => {
        await downloadDocument(document, event.currentTarget);
      });
    } catch (error) {
      closeModal();
      handleError(error);
    }
  }

  async function downloadDocument(document, button) {
    setButtonLoading(button, true);
    try {
      const name = await api.download(`/documents/${document.id}/download`);
      showToast(`${name} downloaded successfully.`);
    } catch (error) {
      handleError(error);
    } finally {
      setButtonLoading(button, false);
    }
  }

  async function verifyDocument(document) {
    openModal({
      title: "Checking file integrity",
      eyebrow: "SHA-256 verification",
      body: `<div class="empty-state" style="min-height:210px"><span class="skeleton" style="width:64px;height:64px"></span><p>Decrypting and comparing file hashes…</p></div>`
    });
    try {
      const result = await api.get(`/documents/${document.id}/integrity`);
      elements.modalBody.innerHTML = `
        <div class="integrity-result ${result.isValid ? "valid" : "invalid"}">
          ${icon(result.isValid ? "shield-check" : "alert")}
          <h3>${result.isValid ? "File integrity confirmed" : "Integrity check failed"}</h3>
          <p>${result.isValid ? "The stored file matches its original SHA-256 hash." : "The file may have been modified or corrupted."}</p>
        </div>
        <div class="hash-comparison">
          <div class="hash-box"><span>Stored hash</span><code>${escapeHtml(result.storedHash)}</code></div>
          <div class="hash-box"><span>Current hash</span><code>${escapeHtml(result.currentHash)}</code></div>
        </div>
        <div class="modal-actions"><button class="button button-secondary" type="button" data-close-modal>Close</button></div>`;
      $("[data-close-modal]", elements.modalBody)?.addEventListener("click", closeModal);
    } catch (error) {
      closeModal();
      handleError(error);
    }
  }

  function openRenameDocument(document) {
    const baseName = document.fileName.replace(/\.[^/.]+$/, "");
    openModal({
      title: "Rename document",
      eyebrow: "Document management",
      body: `<form id="renameDocumentForm">
        <div class="field"><label for="newDocumentName">New file name</label><input id="newDocumentName" type="text" maxlength="220" value="${escapeHtml(baseName)}" required /></div>
        <p class="field-hint">The original file extension will be kept automatically.</p>
        <div class="modal-actions"><button class="button button-secondary" type="button" data-close-modal>Cancel</button><button class="button button-primary" type="submit">Save name</button></div>
      </form>`,
      onOpen: (body) => {
        $("[data-close-modal]", body).addEventListener("click", closeModal);
        $("#renameDocumentForm", body).addEventListener("submit", async (event) => {
          event.preventDefault();
          const submit = $("button[type='submit']", event.currentTarget);
          const name = $("#newDocumentName").value.trim();
          if (!name) return;
          setButtonLoading(submit, true);
          try {
            await api.put(`/documents/${document.id}/rename`, { newFileName: name });
            closeModal();
            showToast("Document renamed.");
            await navigate(state.route, { folderId: state.documentFolderId });
          } catch (error) { handleError(error); }
          finally { setButtonLoading(submit, false); }
        });
      }
    });
  }

  async function deleteDocument(document) {
    const accepted = await confirmAction({ title: "Delete document?", message: `“${document.fileName}” will be permanently removed from encrypted storage.`, acceptText: "Delete document" });
    if (!accepted) return;
    try {
      await api.delete(`/documents/${document.id}`);
      showToast("Document deleted.");
      await navigate(state.route, { folderId: state.documentFolderId });
    } catch (error) { handleError(error); }
  }

  async function openUploadModal() {
    try {
      if (!state.folders.length) state.folders = await api.get("/folders");
    } catch (error) { handleError(error); return; }

    openModal({
      title: "Upload a private document",
      eyebrow: "Encrypted storage",
      body: `<form id="uploadForm">
        <div id="dropZone" class="drop-zone">
          <input id="documentFile" type="file" accept="${config.ALLOWED_EXTENSIONS.join(",")}" required />
          <span class="drop-icon">${icon("upload")}</span>
          <h3>Drop a file here or click to browse</h3>
          <p>PDF, Word, text, JPG, and PNG files are accepted.</p>
          <small>Maximum file size: ${formatBytes(config.MAX_FILE_SIZE_BYTES)}</small>
        </div>
        <div id="selectedFile" class="selected-file hidden"></div>
        <div class="field" style="margin-top:17px"><label for="uploadFolder">Folder <span class="optional">Optional</span></label><select id="uploadFolder"><option value="">No folder</option>${state.folders.map((folder) => `<option value="${folder.id}" ${folder.id === state.documentFolderId ? "selected" : ""}>${escapeHtml(folder.name)}</option>`).join("")}</select></div>
        <div class="modal-actions"><button class="button button-secondary" type="button" data-close-modal>Cancel</button><button class="button button-primary" type="submit">${icon("lock")} Encrypt & upload</button></div>
      </form>`,
      onOpen: (body) => {
        const form = $("#uploadForm", body);
        const input = $("#documentFile", body);
        const zone = $("#dropZone", body);
        const selected = $("#selectedFile", body);
        $("[data-close-modal]", body).addEventListener("click", closeModal);
        ["dragenter", "dragover"].forEach((name) => zone.addEventListener(name, () => zone.classList.add("dragover")));
        ["dragleave", "drop"].forEach((name) => zone.addEventListener(name, () => zone.classList.remove("dragover")));
        input.addEventListener("change", () => {
          const file = input.files[0];
          if (!file) { selected.classList.add("hidden"); return; }
          selected.classList.remove("hidden");
          selected.innerHTML = `${icon("file")}<div><strong>${escapeHtml(file.name)}</strong><small>${formatBytes(file.size)}</small></div><span class="badge">Ready</span>`;
        });
        form.addEventListener("submit", async (event) => {
          event.preventDefault();
          const file = input.files[0];
          if (!file) { showToast("Choose a file to upload.", "warning"); return; }
          const extension = `.${file.name.split(".").pop()?.toLowerCase()}`;
          if (!config.ALLOWED_EXTENSIONS.includes(extension)) { showToast("This file type is not allowed.", "warning"); return; }
          if (file.size > config.MAX_FILE_SIZE_BYTES) { showToast("The selected file is larger than 10 MB.", "warning"); return; }
          const submit = $("button[type='submit']", form);
          setButtonLoading(submit, true);
          const formData = new FormData();
          formData.append("file", file);
          const folderId = $("#uploadFolder", body).value;
          if (folderId) formData.append("folderId", folderId);
          try {
            await api.post("/documents", formData);
            closeModal();
            showToast("Document encrypted and uploaded.");
            await navigate(state.route === "documents" ? "documents" : "dashboard", { folderId: state.documentFolderId });
          } catch (error) { handleError(error); }
          finally { setButtonLoading(submit, false); }
        });
      }
    });
  }

  async function renderFolders() {
    state.folders = await api.get("/folders");
    elements.content.innerHTML = `
      <div class="page-header"><div class="page-header-copy"><h2>Document folders</h2><p>Keep personal documents organised without changing their encrypted protection.</p></div><div class="page-actions"><button class="button button-primary" type="button" data-action="create-folder">${icon("plus")}<span>New folder</span></button></div></div>
      ${state.folders.length ? `<section class="folder-grid">${state.folders.map((folder) => `<article class="card folder-card">
        <div class="folder-card-head"><span class="folder-symbol">${icon("folder")}</span><div class="table-actions"><button class="action-button" type="button" data-folder-action="rename" data-id="${folder.id}" title="Rename">${icon("edit")}</button><button class="action-button danger" type="button" data-folder-action="delete" data-id="${folder.id}" title="Delete">${icon("trash")}</button></div></div>
        <h3>${escapeHtml(folder.name)}</h3><p>${folder.documentCount} document${folder.documentCount === 1 ? "" : "s"}</p>
        <div class="folder-card-footer"><span>Updated ${escapeHtml(formatDate(folder.updatedAtUtc))}</span><button class="button button-ghost button-compact" type="button" data-folder-action="open" data-id="${folder.id}">Open</button></div>
      </article>`).join("")}</section>` : `<article class="card">${emptyState("folder", "No folders yet", "Create folders to group related documents such as education, identity, finance, or work.", "Create folder", "create-folder")}</article>`}`;
    bindCommonContentActions();
    $$('[data-folder-action]', elements.content).forEach((button) => button.addEventListener("click", async () => {
      const folder = state.folders.find((item) => item.id === button.dataset.id);
      if (!folder) return;
      if (button.dataset.folderAction === "open") navigate("documents", { folderId: folder.id });
      if (button.dataset.folderAction === "rename") openFolderModal(folder);
      if (button.dataset.folderAction === "delete") await deleteFolder(folder);
    }));
  }

  function openFolderModal(folder = null) {
    const editing = Boolean(folder);
    openModal({
      title: editing ? "Rename folder" : "Create a folder",
      eyebrow: "Vault organisation",
      body: `<form id="folderForm"><div class="field"><label for="folderName">Folder name</label><input id="folderName" type="text" maxlength="120" placeholder="e.g. Identity documents" value="${escapeHtml(folder?.name || "")}" required /></div><div class="modal-actions"><button class="button button-secondary" type="button" data-close-modal>Cancel</button><button class="button button-primary" type="submit">${editing ? "Save changes" : "Create folder"}</button></div></form>`,
      onOpen: (body) => {
        $("[data-close-modal]", body).addEventListener("click", closeModal);
        $("#folderForm", body).addEventListener("submit", async (event) => {
          event.preventDefault();
          const name = $("#folderName", body).value.trim();
          if (!name) return;
          const submit = $("button[type='submit']", event.currentTarget);
          setButtonLoading(submit, true);
          try {
            if (editing) await api.put(`/folders/${folder.id}`, { name });
            else await api.post("/folders", { name });
            closeModal();
            showToast(editing ? "Folder renamed." : "Folder created.");
            await navigate(state.route === "folders" ? "folders" : "dashboard");
          } catch (error) { handleError(error); }
          finally { setButtonLoading(submit, false); }
        });
      }
    });
  }

  async function deleteFolder(folder) {
    const accepted = await confirmAction({
      title: "Delete folder?",
      message: `“${folder.name}” will be deleted. Its documents will remain safely stored as unfiled items.`,
      acceptText: "Delete folder"
    });
    if (!accepted) return;
    try {
      await api.delete(`/folders/${folder.id}`);
      showToast("Folder deleted. Documents remain in the vault.");
      await navigate("folders");
    } catch (error) { handleError(error); }
  }

  async function renderCategories() {
    state.categories = await api.get("/categories");
    elements.content.innerHTML = `
      <div class="page-header"><div class="page-header-copy"><h2>Credential categories</h2><p>Create, rename, and delete categories used to organise encrypted credential records.</p></div><div class="page-actions"><button class="button button-primary" type="button" data-action="create-category">${icon("plus")}<span>New category</span></button></div></div>
      ${state.categories.length ? `<section class="folder-grid">${state.categories.map((category) => `<article class="card folder-card">
        <div class="folder-card-head"><span class="folder-symbol">${icon("tag")}</span><div class="table-actions"><button class="action-button" type="button" data-category-action="rename" data-id="${category.id}" title="Rename">${icon("edit")}</button><button class="action-button danger" type="button" data-category-action="delete" data-id="${category.id}" title="Delete">${icon("trash")}</button></div></div>
        <h3>${escapeHtml(category.name)}</h3><p>${category.credentialCount} credential${category.credentialCount === 1 ? "" : "s"}</p>
        <div class="folder-card-footer"><span>Updated ${escapeHtml(formatDate(category.updatedAtUtc))}</span><button class="button button-ghost button-compact" type="button" data-category-action="open" data-name="${escapeHtml(category.name)}">View records</button></div>
      </article>`).join("")}</section>` : `<article class="card">${emptyState("tag", "No categories yet", "Create categories such as Education, Banking, Social, or Work to organise secure credentials.", "Create category", "create-category")}</article>`}`;

    bindCommonContentActions();
    $$('[data-category-action]', elements.content).forEach((button) => button.addEventListener("click", async () => {
      if (button.dataset.categoryAction === "open") {
        state.credentialSearch = button.dataset.name || "";
        await navigate("credentials");
        return;
      }
      const category = state.categories.find((item) => item.id === button.dataset.id);
      if (!category) return;
      if (button.dataset.categoryAction === "rename") openCategoryModal(category);
      if (button.dataset.categoryAction === "delete") await deleteCategory(category);
    }));
  }

  function openCategoryModal(category = null) {
    const editing = Boolean(category);
    openModal({
      title: editing ? "Rename category" : "Create a category",
      eyebrow: "Credential organisation",
      body: `<form id="categoryForm"><div class="field"><label for="categoryName">Category name</label><input id="categoryName" type="text" maxlength="100" placeholder="e.g. Education" value="${escapeHtml(category?.name || "")}" required /></div><p class="field-hint">Categories organise credential records only. Sensitive values remain AES-encrypted.</p><div class="modal-actions"><button class="button button-secondary" type="button" data-close-modal>Cancel</button><button class="button button-primary" type="submit">${editing ? "Save changes" : "Create category"}</button></div></form>`,
      onOpen: (body) => {
        $("[data-close-modal]", body).addEventListener("click", closeModal);
        $("#categoryForm", body).addEventListener("submit", async (event) => {
          event.preventDefault();
          const name = $("#categoryName", body).value.trim();
          if (!name) return;
          const submit = $("button[type='submit']", event.currentTarget);
          setButtonLoading(submit, true);
          try {
            if (editing) await api.put(`/categories/${category.id}`, { name });
            else await api.post("/categories", { name });
            closeModal();
            showToast(editing ? "Category renamed. Linked credentials were updated." : "Category created.");
            await navigate(state.route === "categories" ? "categories" : "dashboard");
          } catch (error) { handleError(error); }
          finally { setButtonLoading(submit, false); }
        });
      }
    });
  }

  async function deleteCategory(category) {
    const accepted = await confirmAction({
      title: "Delete category?",
      message: `“${category.name}” will be deleted. Linked credential records will remain encrypted and will become uncategorised.`,
      acceptText: "Delete category"
    });
    if (!accepted) return;
    try {
      await api.delete(`/categories/${category.id}`);
      showToast("Category deleted. Credential records remain safely stored.");
      await navigate("categories");
    } catch (error) { handleError(error); }
  }

  async function loadCredentials() {
    const query = state.credentialSearch ? `?search=${encodeURIComponent(state.credentialSearch)}` : "";
    state.credentials = await api.get(`/credentials${query}`);
  }

  async function renderCredentials() {
    await loadCredentials();
    elements.content.innerHTML = `
      <div class="page-header"><div class="page-header-copy"><h2>Secure credentials</h2><p>Store account details in encrypted form. Values remain masked until explicitly revealed.</p></div><div class="page-actions"><button class="button button-primary" type="button" data-action="create-credential">${icon("plus")}<span>Add credential</span></button></div></div>
      <div class="toolbar"><div class="search-box">${icon("search")}<input id="credentialSearch" type="search" placeholder="Search title or category…" value="${escapeHtml(state.credentialSearch)}" /></div><span class="badge">${state.credentials.length} record${state.credentials.length === 1 ? "" : "s"}</span></div>
      <div id="credentialList">${state.credentials.length ? `<section class="credential-grid">${credentialCards(state.credentials)}</section>` : `<article class="card">${emptyState("key", "No credentials found", state.credentialSearch ? "Try a different search." : "Add a secure credential record for a website, service, or account.", "Add credential", "create-credential")}</article>`}</div>`;
    bindCommonContentActions();
    bindCredentialActions();
    let timer;
    $("#credentialSearch")?.addEventListener("input", (event) => {
      clearTimeout(timer);
      timer = setTimeout(async () => {
        state.credentialSearch = event.target.value.trim();
        const list = $("#credentialList");
        list.innerHTML = `<div class="empty-state"><span class="skeleton" style="width:62px;height:62px"></span></div>`;
        try {
          await loadCredentials();
          list.innerHTML = state.credentials.length ? `<section class="credential-grid">${credentialCards(state.credentials)}</section>` : `<article class="card">${emptyState("key", "No credentials found", "Try a different search.", "Add credential", "create-credential")}</article>`;
          const badge = $(".toolbar > .badge");
          if (badge) badge.textContent = `${state.credentials.length} record${state.credentials.length === 1 ? "" : "s"}`;
          bindCommonContentActions();
          bindCredentialActions();
        } catch (error) { handleError(error); }
      }, 280);
    });
  }

  function credentialCards(items) {
    return items.map((credential) => `<article class="card credential-card">
      <div class="credential-card-head"><span class="badge primary credential-category">${icon("key")} ${escapeHtml(credential.category || "General")}</span><div class="table-actions"><button class="action-button" type="button" data-credential-action="reveal" data-id="${credential.id}" title="Reveal">${icon("eye")}</button><button class="action-button danger" type="button" data-credential-action="delete" data-id="${credential.id}" title="Delete">${icon("trash")}</button></div></div>
      <h3>${escapeHtml(credential.title)}</h3><p>Updated ${escapeHtml(formatDate(credential.updatedAtUtc))}</p>
      <div class="masked-value"><span>${escapeHtml(credential.secret)}</span>${icon("lock")}</div>
      <div class="credential-footer"><span class="badge success">Encrypted</span><button class="button button-ghost button-compact" type="button" data-credential-action="reveal" data-id="${credential.id}">${icon("eye")} Reveal</button></div>
    </article>`).join("");
  }

  function bindCredentialActions() {
    $$('[data-credential-action]', elements.content).forEach((button) => button.addEventListener("click", async () => {
      const credential = state.credentials.find((item) => item.id === button.dataset.id);
      if (!credential) return;
      if (button.dataset.credentialAction === "reveal") await revealCredential(credential.id);
      if (button.dataset.credentialAction === "delete") await deleteCredential(credential);
    }));
  }

  async function openCredentialForm(existing = null) {
    const editing = Boolean(existing);
    try {
      state.categories = await api.get("/categories");
    } catch (error) {
      handleError(error);
      return;
    }
    const categoryOptions = state.categories.map((category) => `<option value="${escapeHtml(category.name)}"></option>`).join("");
    openModal({
      title: editing ? "Edit credential" : "Add secure credential",
      eyebrow: "AES-encrypted record",
      body: `<form id="credentialForm">
        <div class="field-grid"><div class="field"><label for="credentialTitle">Title</label><input id="credentialTitle" type="text" maxlength="150" placeholder="e.g. Student portal" value="${escapeHtml(existing?.title || "")}" required /></div><div class="field"><label for="credentialCategory">Category <span class="optional">Optional</span></label><input id="credentialCategory" type="text" list="credentialCategoryOptions" maxlength="100" placeholder="Education" value="${escapeHtml(existing?.category || "")}" /><datalist id="credentialCategoryOptions">${categoryOptions}</datalist><p class="field-hint">Choose an existing category or type a new one. New names are added automatically.</p></div></div>
        <div class="field"><label for="credentialUsername">Username or email</label><input id="credentialUsername" type="text" maxlength="500" autocomplete="off" value="${escapeHtml(existing?.username || "")}" required /></div>
        <div class="field"><label for="credentialSecret">Password / secret</label><div class="input-wrap"><svg><use href="#icon-lock"></use></svg><input id="credentialSecret" type="password" maxlength="1000" autocomplete="new-password" value="${escapeHtml(existing?.secret || "")}" required /><button class="password-toggle" type="button" data-toggle-password="credentialSecret" aria-label="Show secret">${icon("eye")}</button></div></div>
        <div class="field"><label for="credentialWebsite">Website <span class="optional">Optional</span></label><input id="credentialWebsite" type="url" maxlength="500" placeholder="https://example.com" value="${escapeHtml(existing?.website || "")}" /></div>
        <div class="field"><label for="credentialNotes">Notes <span class="optional">Optional</span></label><textarea id="credentialNotes" maxlength="2000" placeholder="Recovery details or private notes…">${escapeHtml(existing?.notes || "")}</textarea></div>
        <div class="modal-actions"><button class="button button-secondary" type="button" data-close-modal>Cancel</button><button class="button button-primary" type="submit">${icon("lock")} ${editing ? "Save encrypted changes" : "Encrypt & save"}</button></div>
      </form>`,
      onOpen: (body) => {
        bindPasswordToggles(body);
        $("[data-close-modal]", body).addEventListener("click", closeModal);
        $("#credentialForm", body).addEventListener("submit", async (event) => {
          event.preventDefault();
          const payload = {
            title: $("#credentialTitle", body).value.trim(),
            category: $("#credentialCategory", body).value.trim() || null,
            username: $("#credentialUsername", body).value,
            secret: $("#credentialSecret", body).value,
            website: $("#credentialWebsite", body).value.trim() || null,
            notes: $("#credentialNotes", body).value.trim() || null
          };
          if (!payload.title || !payload.username || !payload.secret) { showToast("Title, username, and secret are required.", "warning"); return; }
          const submit = $("button[type='submit']", event.currentTarget);
          setButtonLoading(submit, true);
          try {
            if (editing) await api.put(`/credentials/${existing.id}`, payload);
            else await api.post("/credentials", payload);
            closeModal();
            showToast(editing ? "Credential updated securely." : "Credential encrypted and saved.");
            await navigate(state.route === "credentials" ? "credentials" : "dashboard");
          } catch (error) { handleError(error); }
          finally { setButtonLoading(submit, false); }
        });
      }
    });
  }

  async function revealCredential(id) {
    openModal({ title: "Decrypting credential", eyebrow: "Owner-only access", body: `<div class="empty-state" style="min-height:210px"><span class="skeleton" style="width:64px;height:64px"></span><p>Authorising and decrypting the selected record…</p></div>` });
    try {
      const item = await api.get(`/credentials/${id}/reveal`);
      elements.modalTitle.textContent = item.title;
      elements.modalEyebrow.textContent = "Decrypted credential";
      elements.modalBody.innerHTML = `
        <div class="detail-list">
          ${detailRow("Category", item.category || "General")}
          ${detailRow("Username", item.username, true)}
          ${detailRow("Secret", item.secret, true)}
          ${detailRow("Website", item.website || "—", Boolean(item.website))}
          ${detailRow("Notes", item.notes || "—", Boolean(item.notes))}
          ${detailRow("Updated", formatDate(item.updatedAtUtc, true))}
        </div>
        <div class="demo-note" style="margin-top:18px">${icon("info")}<span>This decrypted value is shown only after JWT authentication and ownership validation.</span></div>
        <div class="modal-actions"><button class="button button-secondary" type="button" data-close-modal>Close</button><button class="button button-primary" type="button" data-edit-credential>${icon("edit")} Edit</button></div>`;
      $("[data-close-modal]", elements.modalBody).addEventListener("click", closeModal);
      $("[data-edit-credential]", elements.modalBody).addEventListener("click", () => openCredentialForm(item));
      $$('[data-copy-value]', elements.modalBody).forEach((button) => button.addEventListener("click", async () => {
        try {
          await navigator.clipboard.writeText(decodeURIComponent(button.dataset.copyValue));
          showToast("Copied to clipboard.");
        } catch { showToast("Could not copy the value.", "error"); }
      }));
    } catch (error) { closeModal(); handleError(error); }
  }

  function detailRow(label, value, copy = false) {
    const text = String(value ?? "");
    return `<div class="detail-row"><span>${escapeHtml(label)}</span><div class="detail-value"><strong>${escapeHtml(text)}</strong>${copy ? `<button class="copy-button" type="button" data-copy-value="${encodeURIComponent(text)}" title="Copy">${icon("copy")}</button>` : ""}</div></div>`;
  }

  async function deleteCredential(credential) {
    const accepted = await confirmAction({ title: "Delete credential?", message: `“${credential.title}” will be permanently removed. The encrypted values cannot be recovered.`, acceptText: "Delete credential" });
    if (!accepted) return;
    try {
      await api.delete(`/credentials/${credential.id}`);
      showToast("Credential deleted.");
      await navigate("credentials");
    } catch (error) { handleError(error); }
  }

  async function renderProfile() {
    state.profile = await api.get("/profile");
    const profile = state.profile;
    elements.content.innerHTML = `
      <div class="page-header"><div class="page-header-copy"><h2>Account profile</h2><p>View and update your basic account details.</p></div></div>
      <section class="profile-layout">
        <article class="card profile-summary"><div class="profile-avatar-large">${escapeHtml(initials(profile.fullName))}</div><h3>${escapeHtml(profile.fullName)}</h3><p>${escapeHtml(profile.email)}</p><div class="profile-meta"><div class="profile-meta-row"><span>Role</span><strong>${escapeHtml(profile.role)}</strong></div><div class="profile-meta-row"><span>Member since</span><strong>${escapeHtml(formatDate(profile.createdAtUtc))}</strong></div><div class="profile-meta-row"><span>Access</span><strong class="badge success">Active</strong></div></div></article>
        <article class="card"><header class="card-header"><div><h3>Personal details</h3><p>Your email address is used as the login identity and cannot be changed here.</p></div></header><div class="card-body"><form id="profileForm" class="profile-form">
          <div class="field full"><label for="profileName">Full name</label><input id="profileName" type="text" maxlength="120" value="${escapeHtml(profile.fullName)}" required /></div>
          <div class="field"><label for="profileEmail">Email address</label><input id="profileEmail" type="email" value="${escapeHtml(profile.email)}" disabled /></div>
          <div class="field"><label for="profilePhone">Phone number <span class="optional">Optional</span></label><input id="profilePhone" type="tel" maxlength="30" value="${escapeHtml(profile.phoneNumber || "")}" /></div>
          <div class="modal-actions full" style="grid-column:1/-1"><button class="button button-primary" type="submit">Save profile</button></div>
        </form></div></article>
      </section>`;
    $("#profileForm")?.addEventListener("submit", async (event) => {
      event.preventDefault();
      const submit = $("button[type='submit']", event.currentTarget);
      const payload = { fullName: $("#profileName").value.trim(), phoneNumber: $("#profilePhone").value.trim() || null };
      if (!payload.fullName) { showToast("Full name is required.", "warning"); return; }
      setButtonLoading(submit, true);
      try {
        const updated = await api.put("/profile", payload);
        state.profile = updated;
        state.session.fullName = updated.fullName;
        const persistent = Boolean(localStorage.getItem(config.PERSISTENT_SESSION_KEY));
        api.saveSession(state.session, persistent);
        updateSessionUi();
        showToast("Profile updated.");
        await renderProfile();
      } catch (error) { handleError(error); }
      finally { setButtonLoading(submit, false); }
    });
  }

  async function renderAdmin() {
    elements.content.innerHTML = `
      <div class="page-header"><div class="page-header-copy"><h2>Administration</h2><p>Manage accounts and basic application settings without exposing users’ decrypted private content.</p></div></div>
      <div class="admin-tabs"><button class="admin-tab ${state.adminTab === "overview" ? "active" : ""}" type="button" data-admin-tab="overview">Overview</button><button class="admin-tab ${state.adminTab === "users" ? "active" : ""}" type="button" data-admin-tab="users">User accounts</button><button class="admin-tab ${state.adminTab === "settings" ? "active" : ""}" type="button" data-admin-tab="settings">Settings</button></div>
      <div id="adminContent">${loadingPage()}</div>`;
    $$('[data-admin-tab]').forEach((button) => button.addEventListener("click", async () => {
      state.adminTab = button.dataset.adminTab;
      await renderAdmin();
    }));
    if (state.adminTab === "overview") await renderAdminOverview();
    if (state.adminTab === "users") await renderAdminUsers();
    if (state.adminTab === "settings") await renderAdminSettings();
  }

  async function renderAdminOverview() {
    const data = await api.get("/admin/dashboard");
    $("#adminContent").innerHTML = `
      <section class="metrics-grid">
        ${metricCard("Total users", data.totalUsers, "user", "#246bfd", "#eaf1ff")}
        ${metricCard("Upload events", data.totalUploads, "upload", "#8a62db", "#f1eafd")}
        ${metricCard("Stored files", data.totalStoredFiles, "file", "#15966e", "#e6f7f1")}
        ${metricCard("Credentials", data.totalCredentialRecords, "key", "#d98416", "#fff4df")}
      </section>
      <article class="card"><header class="card-header"><div><h3>Privacy boundary</h3><p>Administrator privileges are deliberately limited.</p></div><span class="badge success">Enforced</span></header><div class="card-body"><div class="security-summary" style="margin:0"><h4>${icon("shield-check")} Admin-safe design</h4><ul><li>${icon("check")} Account metadata and aggregate counts are available</li><li>${icon("check")} User account status can be managed</li><li>${icon("check")} Decrypted documents and credential secrets are not exposed to administrators</li></ul></div><p class="field-hint" style="margin-top:15px">Dashboard generated ${escapeHtml(formatDate(data.generatedAtUtc, true))}.</p></div></article>`;
  }

  async function renderAdminUsers() {
    const users = await api.get("/admin/users");
    $("#adminContent").innerHTML = `<article class="card"><header class="card-header"><div><h3>User accounts</h3><p>Activate or deactivate application access.</p></div><span class="badge">${users.length} users</span></header><div class="table-wrap"><table class="data-table"><thead><tr><th>User</th><th>Role</th><th>Created</th><th>Status</th></tr></thead><tbody>${users.map((user) => `<tr><td><div class="user-cell"><span class="avatar">${escapeHtml(initials(user.fullName))}</span><div><strong>${escapeHtml(user.fullName)}</strong><small>${escapeHtml(user.email)}${user.phoneNumber ? ` · ${escapeHtml(user.phoneNumber)}` : ""}</small></div></div></td><td><span class="badge ${user.role === "Administrator" ? "primary" : ""}">${escapeHtml(user.role)}</span></td><td>${escapeHtml(formatDate(user.createdAtUtc))}</td><td><label class="switch" title="${user.isActive ? "Active" : "Inactive"}"><input type="checkbox" data-user-status="${user.id}" ${user.isActive ? "checked" : ""} ${user.id === state.session.userId ? "disabled" : ""} /><span></span></label></td></tr>`).join("")}</tbody></table></div></article>`;
    $$('[data-user-status]', $("#adminContent")).forEach((input) => input.addEventListener("change", async () => {
      const desired = input.checked;
      input.disabled = true;
      try {
        await api.patch(`/admin/users/${input.dataset.userStatus}/status`, { isActive: desired });
        showToast(`User account ${desired ? "activated" : "deactivated"}.`);
      } catch (error) {
        input.checked = !desired;
        handleError(error);
      } finally { input.disabled = false; }
    }));
  }

  async function renderAdminSettings() {
    const settings = await api.get("/admin/settings");
    $("#adminContent").innerHTML = `<article class="card"><header class="card-header"><div><h3>Application settings</h3><p>Basic key-value settings stored by the backend.</p></div><button class="button button-primary button-compact" type="button" id="addSettingButton">${icon("plus")} Add setting</button></header><div class="card-body"><div id="settingList" class="setting-list">${settings.length ? settings.map(settingRow).join("") : emptyState("settings", "No settings yet", "Create the first basic application setting.", "Add setting", "add-setting")}</div></div></article>`;
    $("#addSettingButton")?.addEventListener("click", () => openSettingModal());
    $("[data-action='add-setting']")?.addEventListener("click", () => openSettingModal());
    $$('[data-save-setting]').forEach((button) => button.addEventListener("click", async () => {
      const key = button.dataset.saveSetting;
      const input = document.querySelector(`[data-setting-value="${CSS.escape(key)}"]`);
      if (!input) return;
      setButtonLoading(button, true);
      try {
        await api.put(`/admin/settings/${encodeURIComponent(key)}`, { value: input.value });
        showToast(`Setting “${key}” saved.`);
      } catch (error) { handleError(error); }
      finally { setButtonLoading(button, false); }
    }));
  }

  function settingRow(setting) {
    return `<div class="setting-row"><span class="setting-key">${escapeHtml(setting.key)}</span><input type="text" maxlength="1000" value="${escapeHtml(setting.value)}" data-setting-value="${escapeHtml(setting.key)}" /><button class="button button-secondary button-compact" type="button" data-save-setting="${escapeHtml(setting.key)}">Save</button></div>`;
  }

  function openSettingModal() {
    openModal({
      title: "Add application setting",
      eyebrow: "Administration",
      body: `<form id="settingForm"><div class="field"><label for="settingKey">Setting key</label><input id="settingKey" type="text" maxlength="100" placeholder="e.g. SupportEmail" required /></div><div class="field"><label for="settingValue">Value</label><textarea id="settingValue" maxlength="1000" required></textarea></div><div class="modal-actions"><button class="button button-secondary" type="button" data-close-modal>Cancel</button><button class="button button-primary" type="submit">Save setting</button></div></form>`,
      onOpen: (body) => {
        $("[data-close-modal]", body).addEventListener("click", closeModal);
        $("#settingForm", body).addEventListener("submit", async (event) => {
          event.preventDefault();
          const key = $("#settingKey", body).value.trim();
          const value = $("#settingValue", body).value;
          if (!key || !value) return;
          const submit = $("button[type='submit']", event.currentTarget);
          setButtonLoading(submit, true);
          try {
            await api.put(`/admin/settings/${encodeURIComponent(key)}`, { value });
            closeModal();
            showToast("Setting added.");
            await renderAdmin();
          } catch (error) { handleError(error); }
          finally { setButtonLoading(submit, false); }
        });
      }
    });
  }

  function bindCommonContentActions() {
    $$('[data-route-link]', elements.content).forEach((button) => button.addEventListener("click", () => navigate(button.dataset.routeLink)));
    $$('[data-action="upload-document"]', elements.content).forEach((button) => button.addEventListener("click", openUploadModal));
    $$('[data-action="create-folder"]', elements.content).forEach((button) => button.addEventListener("click", () => openFolderModal()));
    $$('[data-action="create-category"]', elements.content).forEach((button) => button.addEventListener("click", () => openCategoryModal()));
    $$('[data-action="create-credential"]', elements.content).forEach((button) => button.addEventListener("click", () => openCredentialForm()));
  }

  function bindPasswordToggles(root = document) {
    $$('[data-toggle-password]', root).forEach((button) => {
      if (button.dataset.bound === "true") return;
      button.dataset.bound = "true";
      button.addEventListener("click", () => {
        const input = document.getElementById(button.dataset.togglePassword);
        if (!input) return;
        const show = input.type === "password";
        input.type = show ? "text" : "password";
        button.innerHTML = icon(show ? "eye-off" : "eye");
        button.setAttribute("aria-label", show ? "Hide password" : "Show password");
      });
    });
  }

  async function handleLogin(event) {
    event.preventDefault();
    clearFieldErrors(elements.loginForm);
    const email = $("#loginEmail").value.trim();
    const password = $("#loginPassword").value;
    let valid = true;
    if (!email || !/^\S+@\S+\.\S+$/.test(email)) { setFieldError("loginEmail", "Enter a valid email address."); valid = false; }
    if (!password) { setFieldError("loginPassword", "Enter your password."); valid = false; }
    if (!valid) return;

    const submit = $("button[type='submit']", elements.loginForm);
    setButtonLoading(submit, true);
    try {
      const response = await api.post("/auth/login", { email, password });
      state.session = response;
      api.saveSession(response, $("#rememberMe").checked);
      elements.loginForm.reset();
      setConnection(true);
      showApp();
      showToast("Welcome back. Your vault is ready.");
    } catch (error) { handleError(error); }
    finally { setButtonLoading(submit, false); }
  }

  async function handleRegister(event) {
    event.preventDefault();
    clearFieldErrors(elements.registerForm);
    const fullName = $("#registerName").value.trim();
    const email = $("#registerEmail").value.trim();
    const phoneNumber = $("#registerPhone").value.trim() || null;
    const password = $("#registerPassword").value;
    let valid = true;
    if (!fullName) { setFieldError("registerName", "Enter your full name."); valid = false; }
    if (!email || !/^\S+@\S+\.\S+$/.test(email)) { setFieldError("registerEmail", "Enter a valid email address."); valid = false; }
    if (password.length < 8) { setFieldError("registerPassword", "Password must contain at least 8 characters."); valid = false; }
    if (!valid) return;

    const submit = $("button[type='submit']", elements.registerForm);
    setButtonLoading(submit, true);
    try {
      const response = await api.post("/auth/register", { fullName, email, password, phoneNumber });
      state.session = response;
      api.saveSession(response, false);
      elements.registerForm.reset();
      setConnection(true);
      showApp();
      showToast("Your private vault account has been created.");
    } catch (error) { handleError(error); }
    finally { setButtonLoading(submit, false); }
  }

  async function logout() {
    const accepted = await confirmAction({ title: "Sign out?", message: "Your current browser session will be cleared. Stored vault data remains safe on the server.", acceptText: "Sign out", danger: false });
    if (!accepted) return;
    try { await api.post("/auth/logout", {}); } catch { /* Client-side token removal still completes logout. */ }
    showAuth();
    showToast("You have been signed out.", "info");
  }

  function bindGlobalEvents() {
    elements.loginTab.addEventListener("click", () => setAuthMode("login"));
    elements.registerTab.addEventListener("click", () => setAuthMode("register"));
    elements.loginForm.addEventListener("submit", handleLogin);
    elements.registerForm.addEventListener("submit", handleRegister);
    elements.quickUploadButton.addEventListener("click", openUploadModal);
    elements.profileMenuButton.addEventListener("click", () => navigate("profile"));
    elements.logoutButton.addEventListener("click", logout);
    elements.menuButton.addEventListener("click", openSidebar);
    elements.sidebarOverlay.addEventListener("click", closeSidebar);
    $$(".nav-item").forEach((item) => item.addEventListener("click", () => navigate(item.dataset.route)));
    elements.modalClose.addEventListener("click", closeModal);
    elements.modalBackdrop.addEventListener("click", (event) => { if (event.target === elements.modalBackdrop) closeModal(); });
    elements.confirmCancel.addEventListener("click", () => resolveConfirm(false));
    elements.confirmAccept.addEventListener("click", () => resolveConfirm(true));
    elements.confirmBackdrop.addEventListener("click", (event) => { if (event.target === elements.confirmBackdrop) resolveConfirm(false); });
    window.addEventListener("vault:unauthorized", () => showAuth("Your login expired. Please sign in again."));
    window.addEventListener("keydown", (event) => {
      if (event.key !== "Escape") return;
      if (!elements.confirmBackdrop.classList.contains("hidden")) resolveConfirm(false);
      else if (!elements.modalBackdrop.classList.contains("hidden")) closeModal();
      else closeSidebar();
    });

    const password = $("#registerPassword");
    password.addEventListener("input", () => {
      const value = password.value;
      let strength = 0;
      if (value.length >= 8) strength++;
      if (/[a-z]/.test(value) && /[A-Z]/.test(value)) strength++;
      if (/\d/.test(value)) strength++;
      if (/[^A-Za-z0-9]/.test(value)) strength++;
      $(".password-meter").dataset.strength = String(strength);
      $("#passwordHint").textContent = strength <= 1 ? "Use 8+ characters with a number and symbol." : strength === 2 ? "Password strength: fair" : strength === 3 ? "Password strength: good" : "Password strength: strong";
    });
    bindPasswordToggles();
  }

  function initialize() {
    bindGlobalEvents();
    if (state.session) showApp();
    else showAuth();
  }

  initialize();
})();

/**
 * Personal Digital Vault frontend configuration.
 * Change API_BASE_URL only when the backend uses a different address.
 */
window.VAULT_CONFIG = Object.freeze({
  API_BASE_URL: `${window.location.origin}/api`,
  APP_NAME: "Personal Digital Vault",
  MAX_FILE_SIZE_BYTES: 10 * 1024 * 1024,
  ALLOWED_EXTENSIONS: [".pdf", ".doc", ".docx", ".txt", ".jpg", ".jpeg", ".png"],
  SESSION_KEY: "pdv.session",
  PERSISTENT_SESSION_KEY: "pdv.session.persistent"
});

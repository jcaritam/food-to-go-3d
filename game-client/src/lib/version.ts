export const APP_VERSION = "v0.2"
export const BUILD_DATE = "2026-04-03"
export const VERSION_LABEL = import.meta.env.DEV
  ? "dev"
  : `${APP_VERSION} — ${BUILD_DATE}`

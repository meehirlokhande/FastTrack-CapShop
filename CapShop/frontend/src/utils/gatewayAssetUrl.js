/** Turn AuthService-relative paths (e.g. /avatars/...) into same-origin URLs via the gateway. */
export function gatewayAssetUrl(relativePath) {
  if (!relativePath) return null;
  if (relativePath.startsWith("http")) return relativePath;
  const p = relativePath.startsWith("/") ? relativePath : `/${relativePath}`;
  return `/gateway${p}`;
}

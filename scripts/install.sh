#!/bin/sh
# Install unity-cli managed binary.
# Usage: curl -fsSL https://raw.githubusercontent.com/akiojin/unity-cli/main/scripts/install.sh | sh
set -e

REPO="akiojin/unity-cli"
INSTALL_DIR="${HOME}/.unity/tools/unity-cli"
LINK_DIR="${HOME}/.local/bin"

# --- helpers ----------------------------------------------------------------

die() { echo "error: $*" >&2; exit 1; }

need() {
    command -v "$1" >/dev/null 2>&1 || die "required command not found: $1"
}

sha256_check() {
    if command -v sha256sum >/dev/null 2>&1; then
        sha256sum "$1" | cut -d ' ' -f1
    elif command -v shasum >/dev/null 2>&1; then
        shasum -a 256 "$1" | cut -d ' ' -f1
    else
        die "neither sha256sum nor shasum found"
    fi
}

# --- detect RID -------------------------------------------------------------

detect_rid() {
    os=$(uname -s)
    arch=$(uname -m)

    case "$os" in
        Darwin)
            case "$arch" in
                arm64|aarch64) echo "osx-arm64" ;;
                x86_64)        echo "osx-x64" ;;
                *)             die "unsupported macOS architecture: $arch" ;;
            esac
            ;;
        Linux)
            case "$arch" in
                aarch64|arm64) echo "linux-arm64" ;;
                x86_64)        echo "linux-x64" ;;
                *)             die "unsupported Linux architecture: $arch" ;;
            esac
            ;;
        *)
            die "unsupported OS: $os"
            ;;
    esac
}

# --- main -------------------------------------------------------------------

need curl

RID=$(detect_rid)
echo "Detected platform: ${RID}"

# 1. Fetch latest release tag
echo "Fetching latest release..."
TAG=$(curl -fsSL "https://api.github.com/repos/${REPO}/releases/latest" \
    | grep '"tag_name"' | head -1 | sed 's/.*"tag_name"[[:space:]]*:[[:space:]]*"\([^"]*\)".*/\1/')

[ -z "$TAG" ] && die "failed to determine latest release tag"
echo "Latest release: ${TAG}"

# 2. Download manifest
MANIFEST_URL="https://github.com/${REPO}/releases/download/${TAG}/unity-cli-manifest.json"
MANIFEST=$(curl -fsSL "$MANIFEST_URL") || die "failed to download manifest"

# 3. Extract asset URL and SHA256 for this RID
ASSET_URL=$(echo "$MANIFEST" | grep -A2 "\"${RID}\"" | grep '"url"' | head -1 \
    | sed 's/.*"url"[[:space:]]*:[[:space:]]*"\([^"]*\)".*/\1/')
EXPECTED_SHA=$(echo "$MANIFEST" | grep -A2 "\"${RID}\"" | grep '"sha256"' | head -1 \
    | sed 's/.*"sha256"[[:space:]]*:[[:space:]]*"\([^"]*\)".*/\1/')

[ -z "$ASSET_URL" ] && die "manifest has no asset for RID: ${RID}"
[ -z "$EXPECTED_SHA" ] && die "manifest has no sha256 for RID: ${RID}"

# 4. Download binary
DEST_DIR="${INSTALL_DIR}/${RID}"
mkdir -p "$DEST_DIR"
TMP="${DEST_DIR}/unity-cli.download"

echo "Downloading unity-cli..."
curl -fsSL -o "$TMP" "$ASSET_URL" || die "download failed"

# 5. Verify SHA256
ACTUAL_SHA=$(sha256_check "$TMP")
if [ "$ACTUAL_SHA" != "$EXPECTED_SHA" ]; then
    rm -f "$TMP"
    die "checksum mismatch: expected ${EXPECTED_SHA}, got ${ACTUAL_SHA}"
fi
echo "Checksum verified."

# 6. Install
BINARY="${DEST_DIR}/unity-cli"
mv -f "$TMP" "$BINARY"
chmod 755 "$BINARY"

# 7. Write VERSION
VERSION=$(echo "$TAG" | sed 's/^v//')
printf '%s\n' "$VERSION" > "${DEST_DIR}/VERSION"

# 8. Symlink
mkdir -p "$LINK_DIR"
ln -sf "$BINARY" "${LINK_DIR}/unity-cli"
echo "Installed unity-cli ${VERSION} -> ${LINK_DIR}/unity-cli"

# 9. Warn about cargo conflict
if [ -f "${HOME}/.cargo/bin/unity-cli" ]; then
    echo ""
    echo "WARNING: ${HOME}/.cargo/bin/unity-cli exists and may shadow the managed binary."
    echo "  Consider running: cargo uninstall unity-cli"
fi

# 10. PATH check
case ":${PATH}:" in
    *":${LINK_DIR}:"*) ;;
    *)
        echo ""
        echo "NOTE: ${LINK_DIR} is not in your PATH."
        echo "  Add the following to your shell profile:"
        echo "    export PATH=\"${LINK_DIR}:\$PATH\""
        ;;
esac

echo ""
echo "Done. Run 'unity-cli --version' to verify."

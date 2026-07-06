#!/usr/bin/env bash
set -euo pipefail

export DEBIAN_FRONTEND=noninteractive

ANDROID_SDK_ROOT="/usr/lib/android-sdk"
CMDLINE_TOOLS_DIR="${ANDROID_SDK_ROOT}/cmdline-tools/latest"
JAVA_21_HOME="/usr/local/sdkman/candidates/java/21.0.9-ms"
FLUTTER_HOME="/usr/local/flutter"

echo "[devcontainer] Installing system dependencies..."
sudo apt-get update
sudo apt-get install -y --no-install-recommends \
  android-sdk-platform-tools \
  usbutils \
  unzip \
  xz-utils \
  zip \
  clang \
  cmake \
  ninja-build \
  pkg-config \
  libgtk-3-dev \
  libglu1-mesa

echo "[devcontainer] Installing Android SDK command-line tools..."
if [[ ! -x "${CMDLINE_TOOLS_DIR}/bin/sdkmanager" ]]; then
  TMP_ZIP="$(mktemp /tmp/android-cmdline-tools-XXXXXX.zip)"
  TOOL_URLS=(
    "https://dl.google.com/android/repository/commandlinetools-linux-13114758_latest.zip"
    "https://dl.google.com/android/repository/commandlinetools-linux-12266719_latest.zip"
  )

  DOWNLOAD_OK=0
  for url in "${TOOL_URLS[@]}"; do
    if curl -fsSL "${url}" -o "${TMP_ZIP}"; then
      DOWNLOAD_OK=1
      break
    fi
  done

  if [[ "${DOWNLOAD_OK}" -ne 1 ]]; then
    echo "Failed to download Android command-line tools." >&2
    exit 1
  fi

  sudo mkdir -p "${ANDROID_SDK_ROOT}/cmdline-tools"
  sudo rm -rf "${ANDROID_SDK_ROOT}/cmdline-tools/latest"
  sudo mkdir -p "${CMDLINE_TOOLS_DIR}"
  sudo unzip -q "${TMP_ZIP}" -d /tmp/android-cmdline-tools
  sudo mv /tmp/android-cmdline-tools/cmdline-tools/* "${CMDLINE_TOOLS_DIR}/"
  rm -f "${TMP_ZIP}"
  rm -rf /tmp/android-cmdline-tools
fi

echo "[devcontainer] Installing Android SDK components..."
export ANDROID_HOME="${ANDROID_SDK_ROOT}"
export ANDROID_SDK_ROOT="${ANDROID_SDK_ROOT}"
export JAVA_HOME="${JAVA_21_HOME}"
export PATH="${JAVA_HOME}/bin:${CMDLINE_TOOLS_DIR}/bin:${ANDROID_SDK_ROOT}/platform-tools:${PATH}"

yes | sdkmanager --licenses >/dev/null
sdkmanager \
  "platform-tools" \
  "build-tools;36.0.0" \
  "platforms;android-36" \
  "ndk;28.2.13676358" >/dev/null

echo "[devcontainer] Installing Flutter SDK..."
if [[ ! -x "${FLUTTER_HOME}/bin/flutter" ]]; then
  sudo git clone --depth 1 -b stable https://github.com/flutter/flutter.git "${FLUTTER_HOME}"
fi

echo "[devcontainer] Writing profile exports..."
sudo tee /etc/profile.d/dnd-monorepo-env.sh >/dev/null <<EOF
export JAVA_HOME="${JAVA_21_HOME}"
export ANDROID_HOME="${ANDROID_SDK_ROOT}"
export ANDROID_SDK_ROOT="${ANDROID_SDK_ROOT}"
export FLUTTER_HOME="${FLUTTER_HOME}"
export PATH="\${FLUTTER_HOME}/bin:\${ANDROID_SDK_ROOT}/cmdline-tools/latest/bin:\${ANDROID_SDK_ROOT}/platform-tools:\${JAVA_HOME}/bin:\${PATH}"
EOF

echo "[devcontainer] Validating toolchain..."
dotnet --version
docker --version
java -version
flutter --version
sdkmanager --version
adb version

echo "[devcontainer] Bootstrap complete."

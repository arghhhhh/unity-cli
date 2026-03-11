use std::fs;
use std::path::{Path, PathBuf};
use std::thread::{self, JoinHandle};
use std::time::{Duration, SystemTime};

use crate::core::managed_binaries::{
    binary_path_for, detect_rid, download_latest_binary, fetch_latest_release, install_dir_for,
    read_local_version_for, ManagedBinary,
};

const THROTTLE_SECS: u64 = 4 * 60 * 60; // 4 hours

/// Spawn a background thread that checks for updates and installs the latest
/// binary if a newer version is available.  Returns `None` when the check is
/// skipped (opt-out env, throttle, or error resolving paths).
pub fn maybe_self_update() -> Option<JoinHandle<()>> {
    if std::env::var("UNITY_CLI_NO_AUTO_UPDATE").ok().as_deref() == Some("1") {
        tracing::debug!("self-update skipped: UNITY_CLI_NO_AUTO_UPDATE=1");
        return None;
    }

    let marker = last_check_path().ok()?;
    if is_recent(&marker) {
        tracing::debug!("self-update skipped: last check within throttle window");
        return None;
    }

    Some(thread::spawn(move || {
        match run_update(&marker) {
            Ok(()) => tracing::debug!("self-update check completed"),
            Err(e) => tracing::debug!("self-update failed: {e:#}"),
        }
    }))
}

/// Print a one-time warning if `~/.cargo/bin/unity-cli` exists, which could
/// shadow the managed binary.
pub fn warn_cargo_conflict() {
    if let Some(home) = dirs::home_dir() {
        let cargo_bin = if cfg!(target_os = "windows") {
            home.join(".cargo/bin/unity-cli.exe")
        } else {
            home.join(".cargo/bin/unity-cli")
        };
        if cargo_bin.exists() {
            eprintln!(
                "warning: {} exists and may shadow the managed binary. \
                 Consider running `cargo uninstall unity-cli`.",
                cargo_bin.display()
            );
        }
    }
}

fn last_check_path() -> anyhow::Result<PathBuf> {
    Ok(install_dir_for(ManagedBinary::UnityCli)?.join("LAST_UPDATE_CHECK"))
}

fn is_recent(path: &Path) -> bool {
    fs::metadata(path)
        .and_then(|m| m.modified())
        .ok()
        .and_then(|t| SystemTime::now().duration_since(t).ok())
        .is_some_and(|age| age < Duration::from_secs(THROTTLE_SECS))
}

fn touch(path: &Path) {
    if let Some(parent) = path.parent() {
        let _ = fs::create_dir_all(parent);
    }
    let _ = fs::write(path, b"");
}

fn run_update(marker: &Path) -> anyhow::Result<()> {
    let latest = fetch_latest_release(ManagedBinary::UnityCli);

    // Always touch the marker, even on failure, to prevent hammering the API.
    touch(marker);

    let latest = latest?;
    let local = read_local_version_for(ManagedBinary::UnityCli);
    if local.as_deref() == Some(latest.version.as_str()) {
        tracing::debug!(
            "self-update: already at latest version {} (rid={})",
            latest.version,
            detect_rid()
        );
        return Ok(());
    }

    tracing::debug!(
        "self-update: upgrading from {:?} to {} (rid={})",
        local,
        latest.version,
        detect_rid()
    );
    let dest = binary_path_for(ManagedBinary::UnityCli)?;
    download_latest_binary(ManagedBinary::UnityCli, &latest, &dest)?;
    tracing::debug!("self-update: successfully installed {}", latest.version);
    Ok(())
}

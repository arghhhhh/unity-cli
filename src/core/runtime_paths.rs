use std::fs;
use std::path::PathBuf;

use anyhow::{Context, Result};

use super::managed_binaries;

#[derive(Debug, Clone)]
pub struct RuntimePaths {
    tools_root: PathBuf,
}

impl RuntimePaths {
    pub fn discover() -> Result<Self> {
        Ok(Self {
            tools_root: managed_binaries::tools_root()?,
        })
    }

    pub fn run_root(&self) -> Result<PathBuf> {
        let dir = self.tools_root.join("run");
        fs::create_dir_all(&dir)
            .with_context(|| format!("Failed to create runtime root: {}", dir.display()))?;
        Ok(dir)
    }

    pub fn daemon_dir(&self, daemon_name: &str) -> Result<PathBuf> {
        let dir = self.run_root()?.join(daemon_name);
        fs::create_dir_all(&dir)
            .with_context(|| format!("Failed to create daemon runtime dir: {}", dir.display()))?;
        Ok(dir)
    }
}

#[cfg(test)]
mod tests {
    use super::RuntimePaths;

    #[test]
    fn daemon_dir_is_under_run_root() {
        let _guard = crate::test_env::env_lock()
            .lock()
            .unwrap_or_else(|poison| poison.into_inner());
        let root = tempfile::tempdir().expect("tempdir should succeed");
        std::env::set_var(
            "UNITY_CLI_TOOLS_ROOT",
            root.path()
                .to_str()
                .expect("temp path should be valid UTF-8"),
        );

        let paths = RuntimePaths::discover().expect("runtime paths should resolve");
        let daemon_dir = paths
            .daemon_dir("unityd")
            .expect("daemon dir should resolve");
        assert!(daemon_dir.ends_with("run/unityd"));
        assert!(daemon_dir.exists());

        std::env::remove_var("UNITY_CLI_TOOLS_ROOT");
    }
}

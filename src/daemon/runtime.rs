use std::fs;
use std::path::PathBuf;

use anyhow::Result;

use crate::core::runtime_paths::RuntimePaths;

#[derive(Debug, Clone)]
pub struct DaemonRuntimePaths {
    name: &'static str,
    paths: RuntimePaths,
}

impl DaemonRuntimePaths {
    pub fn new(name: &'static str) -> Result<Self> {
        Ok(Self {
            name,
            paths: RuntimePaths::discover()?,
        })
    }

    pub fn dir(&self) -> Result<PathBuf> {
        self.paths.daemon_dir(self.name)
    }

    pub fn pid_file(&self) -> Result<PathBuf> {
        Ok(self.dir()?.join(format!("{}.pid", self.name)))
    }

    #[cfg(unix)]
    pub fn socket_file(&self) -> Result<PathBuf> {
        Ok(self.dir()?.join(format!("{}.sock", self.name)))
    }

    pub fn cleanup(&self) {
        if let Ok(path) = self.pid_file() {
            let _ = fs::remove_file(path);
        }
        #[cfg(unix)]
        {
            if let Ok(path) = self.socket_file() {
                let _ = fs::remove_file(path);
            }
        }
    }
}

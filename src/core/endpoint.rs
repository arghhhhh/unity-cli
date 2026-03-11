use anyhow::Result;

use crate::core::config::{default_host, default_port};
use crate::core::instances;

#[derive(Debug, Clone, PartialEq, Eq)]
pub struct ResolvedEndpoint {
    pub host: String,
    pub port: u16,
}

pub fn resolve_endpoint(
    host_override: Option<String>,
    port_override: Option<u16>,
) -> Result<ResolvedEndpoint> {
    let host = host_override.map(|value| value.trim().to_string());
    let env_host = crate::core::config::read_env(&["UNITY_CLI_HOST"]);
    let env_port = crate::core::config::read_env_u16("UNITY_CLI_PORT");

    if let Some(port) = port_override.or(env_port) {
        return Ok(ResolvedEndpoint {
            host: host.or(env_host).unwrap_or_else(default_host),
            port,
        });
    }

    if host.is_none() && env_host.is_none() {
        if let Some((active_host, active_port)) = instances::active_endpoint()? {
            return Ok(ResolvedEndpoint {
                host: active_host,
                port: active_port,
            });
        }
    }

    Ok(ResolvedEndpoint {
        host: host.or(env_host).unwrap_or_else(default_host),
        port: default_port(),
    })
}

#[cfg(test)]
mod tests {
    use super::resolve_endpoint;

    #[test]
    fn cli_override_wins_over_active_instance() {
        let _guard = crate::test_env::env_lock()
            .lock()
            .unwrap_or_else(|poison| poison.into_inner());
        std::env::set_var("UNITY_CLI_HOST", "env-host");
        std::env::set_var("UNITY_CLI_PORT", "7777");
        let value = resolve_endpoint(Some("cli-host".to_string()), Some(9999))
            .expect("endpoint should resolve");
        assert_eq!(value.host, "cli-host");
        assert_eq!(value.port, 9999);
        std::env::remove_var("UNITY_CLI_HOST");
        std::env::remove_var("UNITY_CLI_PORT");
    }

    #[test]
    fn env_port_wins_when_no_cli_override() {
        let _guard = crate::test_env::env_lock()
            .lock()
            .unwrap_or_else(|poison| poison.into_inner());
        std::env::set_var("UNITY_CLI_HOST", "env-host");
        std::env::set_var("UNITY_CLI_PORT", "7777");
        let value = resolve_endpoint(None, None).expect("endpoint should resolve");
        assert_eq!(value.host, "env-host");
        assert_eq!(value.port, 7777);
        std::env::remove_var("UNITY_CLI_HOST");
        std::env::remove_var("UNITY_CLI_PORT");
    }
}

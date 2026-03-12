use std::collections::{HashMap, VecDeque};
use std::sync::{Mutex, OnceLock};

use serde::{Deserialize, Serialize};
use serde_json::{json, Value};

#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct TransportTiming {
    pub send_ms: f64,
    pub read_ms: f64,
    pub normalize_ms: f64,
    pub total_ms: f64,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct RemoteCommandTiming {
    #[serde(skip_serializing_if = "Option::is_none")]
    pub connect_ms: Option<f64>,
    pub transport: TransportTiming,
}

#[derive(Debug, Clone)]
pub struct CliCommandTiming {
    pub route: &'static str,
    pub success: bool,
    pub total_ms: f64,
    pub daemon_ipc_ms: Option<f64>,
    pub connect_ms: Option<f64>,
    pub unity_roundtrip_ms: Option<f64>,
    pub send_ms: Option<f64>,
    pub read_ms: Option<f64>,
    pub normalize_ms: Option<f64>,
}

#[derive(Debug, Clone)]
struct RecentCommand {
    timestamp: String,
    tool: String,
    route: String,
    success: bool,
    total_ms: f64,
}

#[derive(Debug, Clone, Default)]
struct NumericSummary {
    count: u64,
    total_ms: f64,
    last_ms: f64,
    max_ms: f64,
}

impl NumericSummary {
    fn record(&mut self, value: f64) {
        self.count += 1;
        self.total_ms += value;
        self.last_ms = value;
        self.max_ms = self.max_ms.max(value);
    }

    fn snapshot(&self) -> Value {
        json!({
            "count": self.count,
            "avgMs": if self.count == 0 {
                0.0
            } else {
                self.total_ms / self.count as f64
            },
            "lastMs": self.last_ms,
            "maxMs": self.max_ms
        })
    }
}

#[derive(Debug, Clone, Default)]
struct ToolSummary {
    count: u64,
    error_count: u64,
    route_counts: HashMap<String, u64>,
    total_ms: NumericSummary,
    daemon_ipc_ms: NumericSummary,
    connect_ms: NumericSummary,
    unity_roundtrip_ms: NumericSummary,
    send_ms: NumericSummary,
    read_ms: NumericSummary,
    normalize_ms: NumericSummary,
    last_route: String,
    last_success: bool,
    last_seen_at: String,
}

impl ToolSummary {
    fn record(&mut self, recent: &RecentCommand, timing: &CliCommandTiming) {
        self.count += 1;
        if !timing.success {
            self.error_count += 1;
        }
        *self.route_counts.entry(recent.route.clone()).or_insert(0) += 1;
        self.total_ms.record(timing.total_ms);
        if let Some(value) = timing.daemon_ipc_ms {
            self.daemon_ipc_ms.record(value);
        }
        if let Some(value) = timing.connect_ms {
            self.connect_ms.record(value);
        }
        if let Some(value) = timing.unity_roundtrip_ms {
            self.unity_roundtrip_ms.record(value);
        }
        if let Some(value) = timing.send_ms {
            self.send_ms.record(value);
        }
        if let Some(value) = timing.read_ms {
            self.read_ms.record(value);
        }
        if let Some(value) = timing.normalize_ms {
            self.normalize_ms.record(value);
        }
        self.last_route = recent.route.clone();
        self.last_success = timing.success;
        self.last_seen_at = recent.timestamp.clone();
    }

    fn snapshot(&self) -> Value {
        json!({
            "count": self.count,
            "errorCount": self.error_count,
            "routeCounts": self.route_counts,
            "lastRoute": self.last_route,
            "lastSuccess": self.last_success,
            "lastSeenAt": self.last_seen_at,
            "totalMs": self.total_ms.snapshot(),
            "daemonIpcMs": self.daemon_ipc_ms.snapshot(),
            "connectMs": self.connect_ms.snapshot(),
            "unityRoundtripMs": self.unity_roundtrip_ms.snapshot(),
            "sendMs": self.send_ms.snapshot(),
            "readMs": self.read_ms.snapshot(),
            "normalizeMs": self.normalize_ms.snapshot()
        })
    }
}

#[derive(Debug, Default)]
struct CliCommandTracker {
    per_tool: HashMap<String, ToolSummary>,
    recent: VecDeque<RecentCommand>,
}

impl CliCommandTracker {
    fn record(&mut self, tool_name: &str, timing: CliCommandTiming) {
        let recent = RecentCommand {
            timestamp: chrono_like_now(),
            tool: tool_name.to_string(),
            route: timing.route.to_string(),
            success: timing.success,
            total_ms: timing.total_ms,
        };
        let summary = self.per_tool.entry(tool_name.to_string()).or_default();
        summary.record(&recent, &timing);
        self.recent.push_back(recent);
        while self.recent.len() > 50 {
            self.recent.pop_front();
        }
    }

    fn snapshot(&self) -> Value {
        let per_tool = self
            .per_tool
            .iter()
            .map(|(tool, summary)| (tool.clone(), summary.snapshot()))
            .collect::<serde_json::Map<String, Value>>();
        let recent = self
            .recent
            .iter()
            .map(|entry| {
                json!({
                    "timestamp": entry.timestamp,
                    "tool": entry.tool,
                    "route": entry.route,
                    "success": entry.success,
                    "totalMs": entry.total_ms
                })
            })
            .collect::<Vec<_>>();
        json!({
            "perTool": per_tool,
            "recent": recent
        })
    }
}

fn tracker() -> &'static Mutex<CliCommandTracker> {
    static TRACKER: OnceLock<Mutex<CliCommandTracker>> = OnceLock::new();
    TRACKER.get_or_init(|| Mutex::new(CliCommandTracker::default()))
}

pub fn record_cli_tool_call(tool_name: &str, timing: CliCommandTiming) {
    if let Ok(mut tracker) = tracker().lock() {
        tracker.record(tool_name, timing);
    }
}

pub fn snapshot_value() -> Value {
    tracker()
        .lock()
        .map(|tracker| tracker.snapshot())
        .unwrap_or_else(|_| json!({ "error": "command stats tracker lock poisoned" }))
}

#[cfg(test)]
pub fn reset_for_tests() {
    if let Ok(mut tracker) = tracker().lock() {
        *tracker = CliCommandTracker::default();
    }
}

fn chrono_like_now() -> String {
    use std::time::{SystemTime, UNIX_EPOCH};

    match SystemTime::now().duration_since(UNIX_EPOCH) {
        Ok(duration) => format!("{}.{:09}Z", duration.as_secs(), duration.subsec_nanos()),
        Err(_) => "0.000000000Z".to_string(),
    }
}

#[cfg(test)]
mod tests {
    use super::{record_cli_tool_call, reset_for_tests, snapshot_value, CliCommandTiming};

    #[test]
    fn snapshot_groups_calls_by_tool_and_route() {
        reset_for_tests();
        record_cli_tool_call(
            "capture_screenshot",
            CliCommandTiming {
                route: "direct",
                success: true,
                total_ms: 12.0,
                daemon_ipc_ms: None,
                connect_ms: Some(1.0),
                unity_roundtrip_ms: Some(11.0),
                send_ms: Some(2.0),
                read_ms: Some(8.0),
                normalize_ms: Some(1.0),
            },
        );
        record_cli_tool_call(
            "capture_screenshot",
            CliCommandTiming {
                route: "daemon",
                success: false,
                total_ms: 20.0,
                daemon_ipc_ms: Some(4.0),
                connect_ms: Some(0.5),
                unity_roundtrip_ms: Some(15.5),
                send_ms: Some(3.0),
                read_ms: Some(11.0),
                normalize_ms: Some(1.5),
            },
        );

        let snapshot = snapshot_value();
        let capture = &snapshot["perTool"]["capture_screenshot"];
        assert_eq!(capture["count"], 2);
        assert_eq!(capture["errorCount"], 1);
        assert_eq!(capture["routeCounts"]["direct"], 1);
        assert_eq!(capture["routeCounts"]["daemon"], 1);
        assert_eq!(
            snapshot["recent"].as_array().map(|items| items.len()),
            Some(2)
        );
    }
}

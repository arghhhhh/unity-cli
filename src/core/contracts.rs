use serde::{Deserialize, Serialize};
use serde_json::Value;

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct BatchItem {
    pub tool: String,
    pub params: Value,
}

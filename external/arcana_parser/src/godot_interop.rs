use godot::builtin as gd;
use crate::{godot_interop, parser};
use crate::rule_nodes::RuleNode;

/// Example "parser": returns length and first line.
/// Replace with your real parsing entrypoint.
pub fn convert_success(result: RuleNode) -> gd::Dictionary {
    let mut out = gd::Dictionary::new();

    out
}

pub fn convert_failure(error: parser::Failure) -> gd::Dictionary {
    let mut out = gd::Dictionary::new();

    out
}

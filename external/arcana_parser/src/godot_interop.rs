use crate::faux_parser::{parser, rule_nodes::RuleNode};
use godot::builtin as gd;

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

use godot::builtin::{Dictionary, GString};
use crate::godot_interop;

/// Example "parser": returns length and first line.
/// Replace with your real parsing entrypoint.
pub fn parse(input: &str) -> Result<Dictionary, GString> {
    if input.trim().is_empty() {
        return Err("Input is empty".into());
    }

    let mut d = Dictionary::new();
    d.set("length", input.len() as i64);

    let first_line = input.lines().next().unwrap_or("");
    d.set("first_line", first_line);

    Ok(d)
}

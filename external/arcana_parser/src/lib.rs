use godot::builtin as gd;
use godot::classes::{IRefCounted, RefCounted};
use godot::init::{gdextension, ExtensionLibrary};
use godot::obj::Base;
use godot::register::{godot_api, GodotClass};

mod godot_interop;
mod ebnf_ast;
mod ebnf_ast_util;
mod ebnf_parser;
mod left_left_parser;
mod lexer;
mod rule_nodes;
mod token;
mod grammar;
mod rule_name_generator;
mod parser;
mod grammar_util;

struct ArcanaParserExt
{

}

#[gdextension]
unsafe impl ExtensionLibrary for ArcanaParserExt {}

#[derive(GodotClass)]
#[class(base=RefCounted)]
pub struct ArcanaParser {
    #[base]
    base: Base<RefCounted>,
}

#[godot_api]
impl IRefCounted for ArcanaParser {
    fn init(base: Base<RefCounted>) -> Self {
        Self { base }
    }
}

#[godot_api]
impl ArcanaParser {
    /// Parse user-provided text.
    ///
    /// Return format:
    /// { "ok": true,  "value": <something> }
    /// { "ok": false, "error": "..." }
    #[func]
    pub fn parse_text(&self, text: gd::GString) -> gd::Dictionary {
        let mut out = gd::Dictionary::new();

        // Convert Godot string -> Rust String (UTF-8)
        let input: String = text.to_string();

        // Replace this stub with your real parser call.
        match godot_interop::parse(&input) {
            Ok(value) => {
                out.set("ok", true);
                out.set("value", value); // Variant
            }
            Err(err) => {
                out.set("ok", false);
                out.set("error", err);
            }
        }

        out
    }
}
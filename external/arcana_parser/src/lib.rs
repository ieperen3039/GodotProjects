use crate::faux_parser::*;
use godot::builtin as gd;
use godot::classes::{IRefCounted, RefCounted};
use godot::init::{gdextension, ExtensionLibrary};
use godot::obj::Base;
use godot::register::{godot_api, GodotClass};

mod faux_parser;
mod godot_interop;

#[cfg(test)]
mod parser_tests;

struct ArcanaParserExt {}

#[gdextension]
unsafe impl ExtensionLibrary for ArcanaParserExt {}

#[derive(GodotClass)]
#[class(base=RefCounted)]
pub struct ArcanaParser {
    #[base]
    base: Base<RefCounted>,
    lexer: lexer::Lexer,
    parser: left_left_parser::Parser,
}

#[godot_api]
impl IRefCounted for ArcanaParser {
    fn init(base: Base<RefCounted>) -> Self {
        let grammar_definition = include_str!("../res/arcana.ebnf");

        let grammar = ebnf_parser::parse_ebnf(grammar_definition)
            .map(grammatificator::convert_to_grammar)
            .expect("invalid grammar");

        let lexer = lexer::Lexer::default();
        let parser = left_left_parser::Parser::new(grammar, None);

        Self { base, lexer, parser }
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
        // Convert Godot string -> Rust String (UTF-8)
        let input: String = text.to_string();

        let tokens = match self.lexer.read(&input) {
            Ok(t) => t,
            Err(char_idx) => {
                return godot_interop::convert_failure(parser::Failure::LexerError { char_idx });
            }
        };

        let program_ast = match self.parser.parse_program(&tokens) {
            Ok(ast) => ast,
            Err(mut errors) => return godot_interop::convert_failure(errors.pop().unwrap()),
        };

        godot_interop::convert_success(program_ast)
    }
}

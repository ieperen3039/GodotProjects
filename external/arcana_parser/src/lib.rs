use godot::builtin as gd;
use godot::classes::{IRefCounted, RefCounted};
use godot::init::{gdextension, ExtensionLibrary};
use godot::obj::Base;
use godot::register::{godot_api, GodotClass};
use crate::lexer::Lexer;

mod godot_interop;
mod ebnf_ast;
mod ebnf_parser;
mod left_left_parser;
mod lexer;
mod rule_nodes;
mod token;
mod grammar;
mod rule_name_generator;
mod parser;
mod grammar_util;
mod grammatificator;

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
        let grammar_definition = include_str!("../res/arcana.ebnf");

        let grammar = ebnf_parser::parse_ebnf(grammar_definition)
            .map(grammatificator::convert_to_grammar)
            .expect("invalid grammar");

        // TODO specify lexer parameters
        let lexer = Lexer::default();
        let parser = left_left_parser::Parser::new(grammar, None);

        // actual function starts here

        // Convert Godot string -> Rust String (UTF-8)
        let input: String = text.to_string();

        let tokens = match lexer.read(&input) {
            Ok(t) => t,
            Err(char_idx) => {
                return godot_interop::convert_failure(parser::Failure::LexerError { char_idx })
            }
        };

        let program_ast = match parser.parse_program(&tokens) {
            Ok(ast) => ast,
            Err(mut errors) => {
                return godot_interop::convert_failure(errors.pop().unwrap())
            }
        };

        godot_interop::convert_success(program_ast)
    }
}
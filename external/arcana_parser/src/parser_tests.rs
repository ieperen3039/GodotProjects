use std::fs::File;
use crate::faux_parser::parser::Failure;
use crate::faux_parser::{ebnf_parser, grammatificator, left_left_parser, lexer};

struct MockParser;

impl MockParser {
    pub fn parse(input: &str) -> Result<String, String> {
        let grammar_definition = include_str!("../res/arcana.ebnf");

        let grammar = ebnf_parser::parse_ebnf(grammar_definition)
            .map(grammatificator::convert_to_grammar)
            .expect("invalid grammar");

        let tokens = lexer::Lexer::default().read(&input)
            .map_err(|err| {
                Failure::LexerError { char_idx: err }.error_string(grammar_definition)
            })?;

        let parser = left_left_parser::Parser::new(grammar, File::create("trace.xml").ok());

        parser.parse_program(&tokens)
            .map(|n| format!("{:?}", n))
            .map_err(|v| {
                v.iter()
                    .map(|e| e.error_string(input))
                    .fold(String::new(), |a, s| a + "\n" + &s)
            })
    }
}

#[test]
fn test_spell_1() {
    let result = MockParser::parse("FARB");
    println!("{:?}", result);

    assert!(result.is_ok())
}

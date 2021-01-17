using System;
using System.Linq;
using System.Text;
using Irony.Parsing;
using SimpleJira.Impl.Ast;

namespace SimpleJira.Fakes.Impl.Jql.Parser
{
    internal static class JqlParser
    {
        public static IJqlClause Parse(string jql)
        {
            if (string.IsNullOrEmpty(jql))
                return null;
            var parser = CreateJqlParser();
            var parseTree = parser.Parse(jql.ToUpper());
            if (parseTree.Status != ParseTreeStatus.Parsed)
                throw new InvalidOperationException(FormatErrors(parseTree, parser.Context.TabWidth));
            return (IJqlClause) parseTree.Root.AstNode;
        }

        private static Irony.Parsing.Parser CreateJqlParser()
        {
            var grammar = new JqlGrammar();
            var language = new LanguageData(grammar);
            if (language.Errors.Count <= 0)
                return new Irony.Parsing.Parser(language);
            var b = new StringBuilder();
            foreach (var error in language.Errors)
                b.Append(error);
            throw new InvalidOperationException($"invalid grammar\r\n{b}");
        }

        private static string FormatErrors(ParseTree parseTree, int tabWidth)
        {
            var b = new StringBuilder();
            foreach (var message in parseTree.ParserMessages)
            {
                b.AppendLine(
                    $"{message.Level}: {message.Message} at {message.Location} in state {message.ParserState}");

                var theMessage = message;
                var lines = parseTree.SourceText.Replace("\t", new string(' ', tabWidth))
                    .Split(new[] {"\r\n"}, StringSplitOptions.None)
                    .Select((sourceLine, index) =>
                        index == theMessage.Location.Line
                            ? $"{sourceLine}\r\n{new string('_', theMessage.Location.Column)}|<-Here"
                            : sourceLine);
                foreach (var line in lines)
                    b.AppendLine(line);
            }

            return $"parse errors\r\n:{b}";
        }
    }
}
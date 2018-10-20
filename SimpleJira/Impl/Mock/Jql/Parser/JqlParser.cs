using System;
using System.Linq;
using System.Text;
using Irony.Parsing;
using SimpleJira.Impl.Mock.Jql.Expressions;

namespace SimpleJira.Impl.Mock.Jql.Parser
{
    internal static class JqlParser
    {
        public static IJqlClause Parse(string jql)
        {
            var parser = CreateJqlParser();
            var parseTree = parser.Parse(jql);
            if (parseTree.Status != ParseTreeStatus.Parsed)
                throw new InvalidOperationException(FormatErrors(parseTree, parser.Context.TabWidth));
            return (IJqlClause) parseTree.Root.AstNode;
        }

        private static Irony.Parsing.Parser CreateJqlParser()
        {
            var grammar = new JqlGrammar();
            var language = new LanguageData(grammar);
            if (language.Errors.Count > 0)
            {
                var b = new StringBuilder();
                foreach (var error in language.Errors)
                    b.Append(error);
                throw new InvalidOperationException($"invalid grammar\r\n{b}");
            }
            return new Irony.Parsing.Parser(language);
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
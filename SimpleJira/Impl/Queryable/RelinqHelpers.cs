using System;
using System.Collections;
using System.Linq;
using Remotion.Linq.Parsing.ExpressionVisitors.Transformation;
using Remotion.Linq.Parsing.Structure;
using Remotion.Linq.Parsing.Structure.NodeTypeProviders;
using SimpleJira.Interface.Metadata;

namespace SimpleJira.Impl.Queryable
{
    internal static class RelinqHelpers
    {
        private static IQueryParser queryParser;

        public static IQueryProvider CreateQueryProvider(IJiraMetadataProvider metadataProvider,
            Func<BuiltQuery, IEnumerable> execute)
        {
            if (queryParser == null)
                queryParser = CreateQueryParser();
            return new RelinqQueryProvider(queryParser, new RelinqQueryExecutor(metadataProvider, execute));
        }

        private static IQueryParser CreateQueryParser()
        {
            var nodeTypeProvider = new CompoundNodeTypeProvider(new INodeTypeProvider[]
            {
                MethodInfoBasedNodeTypeRegistry.CreateFromRelinqAssembly()
            });
            var transformerRegistry = ExpressionTransformerRegistry.CreateDefault();
            var expressionTreeParser = new ExpressionTreeParser(nodeTypeProvider,
                ExpressionTreeParser.CreateDefaultProcessor(transformerRegistry));
            return new QueryParser(expressionTreeParser);
        }
    }
}
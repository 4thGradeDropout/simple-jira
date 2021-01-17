using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using SimpleJira.Impl;
using SimpleJira.Impl.Helpers;
using SimpleJira.Interface.Metadata;

namespace SimpleJira.Fakes.Impl
{
    internal static class JiraMetadataProviderExtenstions
    {
        private static readonly ConcurrentDictionary<IJiraMetadataProvider, (Workflow workflow, Type type)[]> workflows
            =
            new ConcurrentDictionary<IJiraMetadataProvider, (Workflow workflow, Type type)[]>();

        public static (Workflow workflow, Type type)[] GetWorkflow(this IJiraMetadataProvider provider)
        {
            return workflows.GetOrAdd(provider, k =>
            {
                return provider.Issues.Where(x => x.Workflow != null)
                    .Select(x => (type: x.Type, workflow: x.Workflow))
                    .Select(x =>
                    {
                        var workflowBuilderType = typeof(WorkflowBuilder<>).MakeGenericType(x.type);
                        var workflowBuilder = workflowBuilderType.New();
                        var workflowBuilderBuildMethod =
                            workflowBuilderType.GetMethod("Build", BindingFlags.Public | BindingFlags.Instance);
                        var workflowType = x.workflow.GetType();
                        var workflowBuildMethod =
                            workflowType.GetMethod("Build", BindingFlags.Public | BindingFlags.Instance);
                        ReflectionHelpers.GetCompiledDelegate(workflowBuildMethod)(x.workflow, new[] {workflowBuilder});
                        return (workflow: (Workflow) ReflectionHelpers.GetCompiledDelegate(workflowBuilderBuildMethod)(
                            workflowBuilder,
                            ReflectionHelpers.noParameters), x.type);
                    })
                    .ToArray();
            });
        }
    }
}
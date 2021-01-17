using System;
using System.Linq.Expressions;

namespace SimpleJira.Interface.Issue
{
    public interface IScopeBuilder<TIssue> where TIssue : JiraIssue
    {
        /// <summary>
        /// Defines an issue's field constraint and its default value for custom implementation of <c>SimpleJira.Interface.Types.JiraIssue</c>.
        /// </summary>
        /// <typeparam name="TField">Type of issue's field</typeparam>
        /// <param name="expression">Property's access expression.</param>
        /// <param name="value">The default value of the property. Will be filled automatically when issue is creating.</param>
        /// <returns>
        /// 	Reference to the same builder to support fluent interface.
        /// </returns>
        IScopeBuilder<TIssue> Define<TField>(Expression<Func<TIssue, TField>> expression, TField value);
    }
}
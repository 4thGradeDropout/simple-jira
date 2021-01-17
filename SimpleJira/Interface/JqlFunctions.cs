using System;

namespace SimpleJira.Interface
{
    public static class JqlFunctions
    {
        /// <summary>
        /// Markup's function for supporting ~ operator in Linq Expressions.
        /// </summary>
        /// <param name="field">Reference to the field that should be matched using ~ operator.</param>
        /// <param name="pattern">Search pattern.</param>
        /// <exception cref="NotSupportedException">Throws when is used independently of Linq Expression.</exception>
        /// <returns>
        /// 	<c>true</c> if the field is satisfied with the search pattern otherwise <c>false</c>.
        /// </returns>
        /// <remarks>The function is not able to be used independently of Linq Expression.</remarks>
        public static bool Contains(string field, string pattern)
        {
            throw new NotSupportedException("this method is not implemented for public usage. Only for Linq-Provider");
        }
    }
}
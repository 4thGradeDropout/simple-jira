using System;

namespace SimpleJira.Impl.Queryable
{
    internal class CustomFieldHelpers
    {
        private const string customFieldPrefix = "customfield_";

        public static int? ExtractIdentifier(string customerField)
        {
            if (customerField.StartsWith(customFieldPrefix,
                StringComparison.InvariantCultureIgnoreCase))
            {
                var customId = customerField.Substring(customFieldPrefix.Length);
                return int.Parse(customId);
            }

            return null;
        }
    }
}
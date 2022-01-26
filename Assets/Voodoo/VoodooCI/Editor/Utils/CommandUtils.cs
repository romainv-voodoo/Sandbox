using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Voodoo.CI
{
    public static class CommandUtils
    {
        private static readonly Func<string, string, bool> StringComparer = (s1, s2) => s1.Contains(s2);

        private const StringComparison DefaultStringComparison = StringComparison.InvariantCultureIgnoreCase;

        public static string[] GetArguments()
        {
            return Environment.GetCommandLineArgs();
        }

        /// <summary>
        /// Tries to get the value (the next argument) of the given argument [key].
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <param name="argumentKey">The argument to use as a key.</param>
        /// <param name="argumentValue">The argument value to retrieve.</param>
        /// <param name="comparisonType">The string comparison to use.</param>
        /// <returns>Returns <c>true</c> if a valid argument value candidate was found, otherwise <c>false</c>.</returns>
        public static bool GetArgumentValue(string[] arguments, string argumentKey, out string argumentValue,
            StringComparison comparisonType = DefaultStringComparison)
        {
            var argumentsLength = arguments.Length;

            for (var i = 0; i < argumentsLength; i++)
            {
                var arg = arguments[i];

                if (!StringComparer(arg, argumentKey))
                    continue;

                var search = $"{argumentKey}=";

                if (arg.Contains(search))
                {
                    var idx = arg.IndexOf(search, StringComparison.Ordinal);
                    var val = arg.Substring(idx + search.Length);

                    if (!string.IsNullOrEmpty(val))
                    {
                        argumentValue = val;
                        Debug.Log($":: :: :: Argument: {argumentKey} : {argumentValue} :: :: ::");
                        return true;
                    }
                }

                if (i < argumentsLength - 1)
                {
                    argumentValue = arguments[i + 1];
                    return true;
                }

                break;
            }

            argumentValue = null;
            return false;
        }

        /// <summary>
        /// Checks whether the given argument was passed along with this command.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <param name="argument">The argument to check.</param>
        /// <returns>Returns <c>true</c> if the given argument was found, otherwise <c>false</c>.</returns>
        public static bool HasArgument(IEnumerable<string> arguments, string argument)
        {
            return arguments.Any(arg => StringComparer(arg, argument));
        }
    }
}
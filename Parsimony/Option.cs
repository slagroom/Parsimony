﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Parsimony
{
    /// <summary>
    /// The internal representation of an option within a set.
    /// </summary>
    internal class Option<TOptions, TValue> where TOptions : notnull
    {
        /// <summary>
        /// The option's name.
        /// </summary>
        public OptionName Name { get; }

        /// <summary>
        /// Creates a new <see cref="Option{TOptions, TValue}"/>.
        /// </summary>
        /// <param name="name">The option's name.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="name"/> is <c>null</c>.
        /// </exception>
        public Option(OptionName name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public bool CanParse(ParseContext<TOptions> context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return ParseNameAndValue(context.Input).NameAndValue != null;
        }

        /// <summary>
        /// Attempts to parse the target option from the <see cref="ParseContext{TOptions}.Input"/> of
        /// <paramref name="context"/>.
        /// </summary>
        /// <param name="context">The parse context.</param>
        /// <returns>A <see cref="ParseResult{TOptions, TError}"/>.</returns>
        public ParseResult<TOptions, string> Parse(ParseContext<TOptions> context)
        {
            return new ParseResult<TOptions, string>(context, "not implemented");
        }

        private NameAndValueResult ParseNameAndValue(IEnumerable<string> input)
        {
            var tokens = input.ToList();
            var shortName = Name.ShortName != null ? $"-{Name.ShortName}" : null;
            var longName = Name.LongName != null ? $"--{Name.LongName}" : null;

            // optionNames is guaranteed to have at least one value because a long or short name is required.
            var optionNames = new[] { shortName, longName }.Where(n => n != null).ToList();

            if (tokens.Count == 0)
                return new NameAndValueResult(null, input);

            // Standalone flag
            // If we find a short-name or long-name flag we're done.
            if (typeof(TValue) == typeof(bool) && optionNames.Contains(tokens[0]))
                return new NameAndValueResult(new NameAndValue(tokens[0], "true"), tokens.Skip(1));

            // Adjoined short-name flag
            // If we find a short-name flag on the front of a token then we need to extract the flag and leave the rest
            // of the short-name options in the token.
            if (typeof(TValue) == typeof(bool) && shortName != null && tokens[0].StartsWith(shortName))
            {
                tokens[0] = $"-{tokens[0].Substring(2)}";
                return new NameAndValueResult(new NameAndValue(shortName, "true"), tokens);
            }

            // Equals-joined long-name flags work the same as equals-joined long-name options and they're handled
            // together below.

            // NOTE: Some of the type guards below are redundant because of the flag hanlding above, but they're left
            // in place so that each of the expressions are valid on their own.

            // Space separated option and value
            // If we find a short-name or long-name option then we need to consume the next token to get the value.
            if (typeof(TValue) != typeof(bool) && optionNames.Contains(tokens[0]) && tokens.Count >= 2)
                return new NameAndValueResult(new NameAndValue(tokens[0], tokens[1]), tokens.Skip(2));

            // Adjoined short-name option and value
            // If we find a short-name option on the front of a token then the rest of the token is the value.
            if (typeof(TValue) != typeof(bool) && shortName != null && tokens[0].StartsWith(shortName) && tokens[0].Length > 2)
                return new NameAndValueResult(new NameAndValue(shortName, tokens[0].Substring(2)), tokens.Skip(1));

            // Equals-joined long-name option/flag and value
            // If we find a long-name option or flag joined to a value by an equals sign then everything after the
            // equals sign is the value.
            if (longName != null)
            {
                var prefix = $"{longName}=";
                if (tokens[0].StartsWith(prefix) && tokens[0].Length > prefix.Length)
                {
                    return new NameAndValueResult(
                        new NameAndValue(longName, tokens[0].Substring(prefix.Length)), tokens.Skip(1));
                }
            }

            return new NameAndValueResult(null, input);
        }

        private class NameAndValue
        {
            public string Name { get; }
            public string Value { get; }
            public NameAndValue(string name, string value)
            {
                Name = name ?? throw new ArgumentNullException(nameof(name));
                Value = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        private class NameAndValueResult
        {
            public NameAndValue? NameAndValue { get; }
            public IReadOnlyList<string> Input { get; }
            public NameAndValueResult(NameAndValue? nameAndValue, IEnumerable<string> input)
            {
                NameAndValue = nameAndValue;
                Input = input?.ToList() ?? throw new ArgumentNullException(nameof(input));
            }
        }
    }
}
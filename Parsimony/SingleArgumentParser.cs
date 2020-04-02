﻿using System;
using System.Linq;

namespace Parsimony
{
    /// <summary>
    /// A parser that consumes the next input token as an argument.
    /// </summary>
    public class SingleArgumentParser<TOptions>
        : IParser<TOptions>
        where TOptions : notnull
    {
        /// <summary>
        /// Parses the next argument from the input.
        /// </summary>
        /// <param name="state">The current parser state.</param>
        /// <returns>The new state.</returns>
        public ParserState<TOptions> Parse(ParserState<TOptions> state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            var input = state.Input.ToList();
            if (input.Count == 0)
                // TODO: Custom exception
                throw new Exception("Input was empty");
            var arguments = state.Result.Arguments.ToList();
            arguments.Add(input.First());
            input = input.Skip(1).ToList();
            var result = new ParseResult<TOptions>(state.Result.Options, arguments);
            return new ParserState<TOptions>(result, input);
        }
    }
}

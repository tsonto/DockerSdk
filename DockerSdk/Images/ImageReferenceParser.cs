using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace DockerSdk.Images
{
    /// <summary>
    /// Provides methods for parsing image reference strings.
    /// </summary>
    internal static class ImageReferenceParser
    {
        /// <summary>
        /// Tries to parse the input as a Docker image reference.
        /// </summary>
        /// <param name="input">The text to parse.</param>
        /// <param name="parsed">An object that indicates how the value was parsed, or null if parsing failed.</param>
        /// <returns>True if parsing succeeded; false otherwise.</returns>
        /// <exception cref="ArgumentException"><paramref name="input"/> is null or empty.</exception>
        public static bool TryParse(string input, [NotNullWhen(returnValue: true)] out DecomposedImageReference? parsed)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentException($"'{nameof(input)}' cannot be null or empty.", nameof(input));

            // ANTLR takes a ridiculous amount of code to do the common case of "parse and walk the tree".
            var charStream = new AntlrInputStream(input);
            var lexer = new Parser.ImageReferencesLexer(charStream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new Parser.ImageReferencesParser(tokenStream);
            var context = parser.referenceOnly();
            if (parser.NumberOfSyntaxErrors > 0)
            {
                parsed = null;
                return false;
            }
            var listener = new ImageReferenceListener();
            ParseTreeWalker.Default.Walk(listener, context);

            if (listener.Error != null)
            {
                parsed = null;
                return false;
            }

            // If there are multiple repository components and the first component has a dot but no
            // underscores, treat it as a host component instead. Other signs of a host component
            // (port number, uppercase letters) will already have set it.
            if (listener.Host is null && listener.NormalComponent.Count > 1)
            {
                var first = listener.NormalComponent.First();
                if (first.Contains('.') && !first.Contains('_'))
                {
                    listener.Host = first;
                    listener.NormalComponent.Remove(first);
                }
            }

            // Make the HostWithPort value.
            string? hostWithPort = listener.Host;
            if (hostWithPort is not null && listener.Port.HasValue)
                hostWithPort += ":" + listener.Port.Value.ToString(CultureInfo.InvariantCulture);

            // Make the RepositoryWithoutHost value.
            string? repositoryWithoutHost = null;
            if (listener.NormalComponent.Any())
                repositoryWithoutHost = string.Join('/', listener.NormalComponent);

            // Make the ShortId value.
            string? shortId = listener.ShortId;
            if (shortId is null && listener.LongId is not null)
                shortId = ImageId.Shorten(listener.LongId);

            // Create the output object.
            parsed = new(input)
            {
                Digest = listener.Digest,
                Host = listener.Host,
                HostWithPort = hostWithPort,
                LongId = listener.LongId,
                Port = listener.Port,
                Repository = listener.Repository,
                RepositoryWithoutHost = repositoryWithoutHost,
                ShortId = shortId,
                Tag = listener.Tag,
            };
            return true;
        }

        private class ImageReferenceListener : Parser.ImageReferencesBaseListener
        {
            public string? Digest;
            public string? Error;
            public string? Host;
            public string? LongId;
            public List<string> NormalComponent = new();
            public int? Port;
            public string? Repository;
            public string? ShortId;
            public string? Tag;

            public override void ExitDigest(Parser.ImageReferencesParser.DigestContext context)
                => Digest = context.GetText();

            public override void ExitHostname(Parser.ImageReferencesParser.HostnameContext context)
                => Host = context.GetText();

            public override void ExitLongId(Parser.ImageReferencesParser.LongIdContext context)
                => LongId = context.GetText();

            public override void ExitMediumId(Parser.ImageReferencesParser.MediumIdContext context)
                => LongId = "sha256:" + context.GetText();

            public override void ExitNormalComponent(Parser.ImageReferencesParser.NormalComponentContext context)
                => NormalComponent.Add(context.GetText());

            public override void ExitPort(Parser.ImageReferencesParser.PortContext context)
                => Port = int.Parse(context.GetText());

            public override void ExitRepository(Parser.ImageReferencesParser.RepositoryContext context)
                => Repository = context.GetText();

            public override void ExitShortId(Parser.ImageReferencesParser.ShortIdContext context)
                => ShortId = context.GetText();

            public override void ExitTag(Parser.ImageReferencesParser.TagContext context)
                => Tag = context.GetText();

            public override void VisitErrorNode(IErrorNode node)
            {
                Error = node.GetText();
            }
        }
    }
}

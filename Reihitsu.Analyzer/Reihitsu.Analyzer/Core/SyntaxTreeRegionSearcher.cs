using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Reihitsu.Analyzer.Core
{
    /// <summary>
    /// Searching for region elements
    /// </summary>
    public class SyntaxTreeRegionSearcher
    {
        #region Fields

        /// <summary>
        /// Could the start region be found?
        /// </summary>
        private bool _isStartFound;

        /// <summary>
        /// Search start point
        /// </summary>
        private SyntaxTrivia _startRegion;

        /// <summary>
        /// Found region element
        /// </summary>
        private SyntaxTrivia? _foundRegion;

        #endregion // Fields

        #region Public methods

        /// <summary>
        /// Searching the fitting region pair element
        /// </summary>
        /// <param name="node">Node of the region element</param>
        /// <param name="regionTrivia">Region trivia</param>
        /// <param name="matchingRegionTrivia">Fitting region trivia element</param>
        /// <returns>Could the region element be found?</returns>
        public bool SearchRegionPair(SyntaxNodeOrToken node, SyntaxTrivia regionTrivia, out SyntaxTrivia? matchingRegionTrivia)
        {
            matchingRegionTrivia = null;

            if (regionTrivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia) == false)
            {
                throw new NotSupportedException();
            }

            _startRegion = regionTrivia;
            _foundRegion = null;
            _isStartFound = false;

            if (node != null)
            {
                if (SearchChildNode(node)
                 || SearchParentNode(node))
                {
                    matchingRegionTrivia = _foundRegion;

                    return true;
                }
            }

            return false;
        }

        #endregion // Public methods

        #region Private methods

        /// <summary>
        /// Searching parent node
        /// </summary>
        /// <param name="syntaxNode">Parent node</param>
        /// <returns>Could the region element be found?</returns>
        private bool SearchParentNode(SyntaxNodeOrToken syntaxNode)
        {
            var isRegionFound = false;

            if (syntaxNode.Parent != null)
            {
                isRegionFound = syntaxNode.Parent
                                          .ChildNodesAndTokens()
                                          .Reverse()
                                          .SkipWhile(obj => obj != syntaxNode)
                                          .Skip(1)
                                          .Any(SearchChildNode)
                             || SearchParentNode(syntaxNode.Parent);
            }

            return isRegionFound;
        }

        /// <summary>
        /// Searching a child node
        /// </summary>
        /// <param name="syntaxNode">Child node</param>
        /// <returns>Could the region element be found?</returns>
        private bool SearchChildNode(SyntaxNodeOrToken syntaxNode)
        {
            bool isRegionFound;

            var childNodes = syntaxNode.ChildNodesAndTokens();

            if (childNodes.Count == 0)
            {
                isRegionFound = SearchTrivia(syntaxNode.GetTrailingTrivia()
                                                       .Reverse())
                             || SearchTrivia(syntaxNode.GetLeadingTrivia()
                                                       .Reverse());
            }
            else
            {
                isRegionFound = childNodes.Reverse()
                                          .Any(SearchChildNode);
            }

            return isRegionFound;
        }

        /// <summary>
        /// Search trivia
        /// </summary>
        /// <param name="syntaxTriviaList">List of trivia elements</param>
        /// <returns>Could the region element be found?</returns>
        private bool SearchTrivia(SyntaxTriviaList.Reversed syntaxTriviaList)
        {
            var isRegionFound = false;

            foreach (var trivia in syntaxTriviaList)
            {
                if (_isStartFound == false)
                {
                    if (trivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia))
                    {
                        if (trivia == _startRegion)
                        {
                            _isStartFound = true;
                            break;
                        }
                    }
                }
                else if (trivia.IsKind(SyntaxKind.RegionDirectiveTrivia))
                {
                    _foundRegion = trivia;

                    isRegionFound = true;
                }
            }

            return isRegionFound;
        }

        #endregion // Private methods
    }
}
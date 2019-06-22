﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pipelines.DotGraph
{
    public static class DotGraph
    {
        public static string FromPipeline(IGraphNode node)
        {
            var roots = GetRoots(node);

            var metadata = new NodeMetadata(roots);

            return $@"
digraph G {{ node [style=filled, shape=rec]

# Nodes
{DotGraphNodes.AppendNodeAndChildren(roots, metadata)}

# Formatting
{DotGraphFormatting.AppendFormatting(roots, metadata)}
{DotGraphRanking.AppendRankings(roots, metadata)}

}}
".Trim();
        }

        private static IEnumerable<IGraphNode> GetRoots(IGraphNode root)
        {
            var nodesToWalk = new List<IGraphNode> {root};

            var graphNodes = new HashSet<IGraphNode>();

            while (nodesToWalk.Any())
            {
                var node = nodesToWalk.First();
                nodesToWalk.Remove(node);

                if (node.GetType().GetGenericTypeDefinition() == typeof(InputPipe<>))
                {
                    graphNodes.Add(node);
                }
                else
                {
                    nodesToWalk.AddRange(node.Parents);
                }
            }

            return graphNodes;
        }


        public static StringBuilder ProcessTree(
            IGraphNode node,
            StringBuilder result,
            Action<IGraphNode, NodeMetadata, StringBuilder> processNode,
            Action<IGraphNode, IGraphNode, NodeMetadata, StringBuilder> processChild,
            NodeMetadata metadata)
        {
            if (metadata.IsNodeDataProcessed(node, processNode))
            {
                return result;
            }

            metadata.SetNodeDataProcessed(node, processNode);

            processNode(node, metadata, result);

            if (node is ISender sender)
            {
                foreach (var listener in sender.Children)
                {
                    processChild(node, listener.CheckForwarding(), metadata, result);
                    ProcessTree(listener.CheckForwarding(), result, processNode, processChild, metadata);
                }
            }

            return result;
        }
    }
}
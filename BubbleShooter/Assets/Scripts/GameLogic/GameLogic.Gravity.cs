using System.Collections.Generic;

namespace GameLogic
{
    public static class Gravity
    {
        /// <summary>
        /// Get all bubbles not connected to top (will fall)
        /// </summary>
        public static List<Bubble> GetDisconnectedBubbles(BubbleGrid grid)
        {
            if (grid == null)
            {
                return null;
            }

            // Step 1: Find all bubbles connected to top row (DFS)
            HashSet<Bubble> connectedToTop = new HashSet<Bubble>();

            // Start DFS from all bubbles in top row
            foreach (Bubble topBubble in grid.GetBubblesInRow(0))
            {
                if (!connectedToTop.Contains(topBubble))
                {
                    DFS(topBubble, connectedToTop, grid);
                }
            }

            // Step 2: All bubbles NOT in connectedToTop set will fall
            List<Bubble> fallingBubbles = new List<Bubble>();

            foreach (Bubble bubble in grid.GetAllBubbles())
            {
                if (!connectedToTop.Contains(bubble))
                {
                    fallingBubbles.Add(bubble);
                }
            }

            return fallingBubbles;
        }

        /// <summary>
        /// Depth-First Search to find all connected bubbles
        /// </summary>
        private static void DFS(Bubble current, HashSet<Bubble> visited, BubbleGrid grid)
        {
            if (current == null || visited.Contains(current))
                return;

            visited.Add(current);

            // Visit all 6 neighbors
            var neighbors = grid.GetNeighbors(current.Coordinate);

            foreach (Bubble neighbor in neighbors)
            {
                DFS(neighbor, visited, grid);
            }
        }
    }
}

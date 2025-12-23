using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace GameLogic
{
    public static class MatchDetector
    {
        /// <summary>
        /// Find all bubbles matching with the start bubble (BFS) or triggered by bombs
        /// Combines both bomb explosions and 3-match results
        /// </summary>
        public static ICollection<Bubble> FindMatchingCluster(Bubble startBubble, BubbleGrid grid)
        {
            if (startBubble == null || grid == null)
            {
                Debug.LogWarning("[MatchDetector] StartBubble or grid is null!");
                return null;
            }

            // Check for bomb triggers near the placed bubble
            HashSet<Bubble> totalCluster = FindBombTargets(startBubble, grid);

            // Normal color matching
            Queue<Bubble> queue = new Queue<Bubble>();
            HashSet<HexCoordinate> checkCoords = new HashSet<HexCoordinate>();
            List<Bubble> colorCluster = null;

            queue.Enqueue(startBubble);
            checkCoords.Add(startBubble.Coordinate);

            int depth = 1;
            while (queue.Count > 0)
            {
                Bubble current = queue.Dequeue();

                if (colorCluster == null)
                    colorCluster = new List<Bubble>() { current };
                else
                    colorCluster.Add(current);

                // Check all 6 neighbors
                var neighbors = grid.GetNeighborsWithSelf(current.Coordinate, depth, checkCoords);
                foreach (Bubble neighbor in neighbors)
                {
                    if (neighbor.IsBomb)
                        continue;

                    if (neighbor.ColorType == startBubble.ColorType)
                        queue.Enqueue(neighbor);
                }
            }

            // Add color match cluster if it meets minimum count
            if (colorCluster != null && colorCluster.Count >= IntDefine.MIN_MATCH_COUNT)
            {
                foreach (var bubble in colorCluster)
                {
                    totalCluster.Add(bubble);
                }
            }

            // Return combined results
            return totalCluster;
        }

        /// <summary>
        /// Find bombs within 1 tile of the placed bubble and collect all affected bubbles
        /// </summary>
        private static HashSet<Bubble> FindBombTargets(Bubble placedBubble, BubbleGrid grid)
        {
            // Check if the placed bubble itself is a bomb
            int depth = placedBubble.Type switch
            {
                BubbleType.Bomb => 1,
                BubbleType.LargeBomb => 2,
                _ => 1,
            };

            Debug.Log($"[MatchDetector] Placed bubble is {placedBubble.Type} at {placedBubble.Coordinate}");

            // Check all neighbors for bombs
            var affectBubbles = grid.GetNeighbors(placedBubble.Coordinate, depth).ToHashSet();

            // Add self
            affectBubbles.Add(placedBubble);

            // Collect all bubbles affected by chain explosions
            if (!CollectExplosionCluster(ref affectBubbles, grid))
            {
                // If not bomb range, reset affect bubble list
                affectBubbles.Clear();
            }

            return affectBubbles;
        }

        /// <summary>
        /// Recursively calculate all bubbles affected by bomb chain explosions
        /// </summary>
        private static bool CollectExplosionCluster(ref HashSet<Bubble> initialBubbles, BubbleGrid grid)
        {
            HashSet<HexCoordinate> checkBubbles = new HashSet<HexCoordinate>();
            Queue<Bubble> bombQueue = new Queue<Bubble>();
            bool isFindBomb = false;

            // Start with initial bombs
            foreach (Bubble bubble in initialBubbles)
            {
                if (bubble.IsBomb)
                {
                    bombQueue.Enqueue(bubble);
                    checkBubbles.Add(bubble.Coordinate);
                    isFindBomb = true;
                }
            }

            // Process chain explosions
            while (bombQueue.Count > 0)
            {
                Bubble currentBomb = bombQueue.Dequeue();
                int depth = currentBomb.Type == BubbleType.LargeBomb ? 2 : 1;

                Debug.Log($"[MatchDetector] Processing {currentBomb.Type} at {currentBomb.Coordinate} with radius {depth}");

                // Get all bubbles in explosion range
                var explosionRange = grid.GetNeighborsWithSelf(currentBomb.Coordinate, depth, checkBubbles);

                foreach (Bubble bubble in explosionRange)
                {
                    // Add affected bubble
                    initialBubbles.Add(bubble);

                    // If this bubble is also a bomb, add it to the queue for chain explosion
                    if (!bubble.IsBomb)
                        continue;

                    bombQueue.Enqueue(bubble);
                    Debug.Log($"[MatchDetector] Chain explosion: {bubble.Type} at {bubble.Coordinate}");
                }
            }

            Debug.Log($"[MatchDetector] Total explosion cluster: {initialBubbles.Count} bubbles");

            return isFindBomb;
        }
    }
}

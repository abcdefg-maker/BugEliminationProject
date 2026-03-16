using System.Collections.Generic;
using UnityEngine;

namespace BugElimination
{
    /// <summary>
    /// 基于网格的 A* 寻路器。
    /// 使用 Linecast 检测相邻格子之间是否有 EdgeCollider2D 墙壁阻隔。
    /// </summary>
    public class GridPathfinder
    {
        private int width, height;
        private float cellSize;
        private Vector2 origin;
        private LayerMask wallLayer;

        // 每个格子 8 个方向的通行状态（bit flag）
        // bit 0-7 对应 8 个方向，1 = 可通行，0 = 被墙阻挡
        private byte[,] passable;

        // 0-3: 四方向  4-7: 对角线
        private static readonly int[] dx = { 0, 1, 0, -1, 1, 1, -1, -1 };
        private static readonly int[] dy = { 1, 0, -1, 0, 1, -1, 1, -1 };
        private static readonly float[] moveCost = { 1f, 1f, 1f, 1f, 1.414f, 1.414f, 1.414f, 1.414f };
        // 每个方向的反方向索引
        private static readonly int[] opposite = { 2, 3, 0, 1, 5, 4, 7, 6 };

        public int Width => width;
        public int Height => height;
        public Vector2 Origin => origin;
        public float CellSize => cellSize;

        /// <summary>
        /// 构建寻路网格。center 为网格中心世界坐标，size 为网格覆盖范围。
        /// 使用 Linecast 检测 EdgeCollider2D 墙壁。
        /// </summary>
        public GridPathfinder(Vector2 center, Vector2 size, float cellSize, LayerMask wallLayer)
        {
            this.cellSize = cellSize;
            this.wallLayer = wallLayer;
            width = Mathf.CeilToInt(size.x / cellSize);
            height = Mathf.CeilToInt(size.y / cellSize);
            origin = center - size / 2f;

            passable = new byte[width, height];

            // 对每个格子，检测 8 个方向是否有墙阻隔
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector2 from = GridToWorld(x, y);
                    byte mask = 0;

                    for (int dir = 0; dir < 8; dir++)
                    {
                        int nx = x + dx[dir], ny = y + dy[dir];
                        if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                            continue;

                        Vector2 to = GridToWorld(nx, ny);

                        if (!HasStaticWall(from, to, wallLayer))
                        {
                            mask |= (byte)(1 << dir);
                        }
                    }

                    passable[x, y] = mask;
                }
            }
        }

        /// <summary>
        /// 用多条平行 Linecast 检测两点之间是否有静态墙壁。
        /// 加宽检测范围以模拟怪物的碰撞体宽度，防止路径紧贴墙角。
        /// </summary>
        private bool HasStaticWall(Vector2 from, Vector2 to, LayerMask mask)
        {
            Vector2 dir = to - from;
            // 垂直于移动方向的偏移，模拟怪物宽度
            Vector2 perp = new Vector2(-dir.y, dir.x).normalized * cellSize * 0.35f;

            // 中心线 + 左右两条平行线，共 3 条
            for (int i = -1; i <= 1; i++)
            {
                Vector2 offset = perp * i;
                var hits = Physics2D.LinecastAll(from + offset, to + offset, mask);
                foreach (var hit in hits)
                {
                    if (hit.collider.attachedRigidbody == null && !hit.collider.isTrigger)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 检查从格子 (x,y) 向方向 dir 是否可通行
        /// </summary>
        public bool CanPass(int x, int y, int dir)
        {
            if (x < 0 || x >= width || y < 0 || y >= height) return false;
            return (passable[x, y] & (1 << dir)) != 0;
        }

        public Vector2 GridToWorld(int gx, int gy)
        {
            return new Vector2(origin.x + (gx + 0.5f) * cellSize, origin.y + (gy + 0.5f) * cellSize);
        }

        public void WorldToGrid(Vector2 world, out int gx, out int gy)
        {
            gx = Mathf.Clamp(Mathf.FloorToInt((world.x - origin.x) / cellSize), 0, width - 1);
            gy = Mathf.Clamp(Mathf.FloorToInt((world.y - origin.y) / cellSize), 0, height - 1);
        }

        /// <summary>
        /// A* 寻路。返回从 startWorld 到 endWorld 的世界坐标路径点列表。
        /// 找不到路径时返回 null。
        /// </summary>
        public List<Vector2> FindPath(Vector2 startWorld, Vector2 endWorld)
        {
            WorldToGrid(startWorld, out int sx, out int sy);
            WorldToGrid(endWorld, out int ex, out int ey);

            if (sx == ex && sy == ey)
                return new List<Vector2> { GridToWorld(ex, ey) };

            float[,] gCost = new float[width, height];
            int[,] pX = new int[width, height];
            int[,] pY = new int[width, height];
            bool[,] closed = new bool[width, height];

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    gCost[x, y] = float.MaxValue;

            gCost[sx, sy] = 0;
            pX[sx, sy] = -1;
            pY[sx, sy] = -1;

            var open = new List<(int x, int y)> { (sx, sy) };

            while (open.Count > 0)
            {
                // 找 fCost 最低的节点
                int bestIdx = 0;
                float bestF = gCost[open[0].x, open[0].y] + Heuristic(open[0].x, open[0].y, ex, ey);
                for (int i = 1; i < open.Count; i++)
                {
                    float f = gCost[open[i].x, open[i].y] + Heuristic(open[i].x, open[i].y, ex, ey);
                    if (f < bestF)
                    {
                        bestF = f;
                        bestIdx = i;
                    }
                }

                var (cx, cy) = open[bestIdx];
                open.RemoveAt(bestIdx);

                if (cx == ex && cy == ey)
                    return BuildWorldPath(pX, pY, sx, sy, ex, ey);

                if (closed[cx, cy]) continue;
                closed[cx, cy] = true;

                for (int dir = 0; dir < 8; dir++)
                {
                    // 核心改动：用 CanPass 检测是否可通行，而不是检测目标格子是否可行走
                    if (!CanPass(cx, cy, dir)) continue;

                    int nx = cx + dx[dir], ny = cy + dy[dir];
                    if (closed[nx, ny]) continue;

                    // 对角线移动：两个相邻正交方向也必须可通行（防止穿墙角）
                    if (dir >= 4)
                    {
                        // dir 4=(1,1), 5=(1,-1), 6=(-1,1), 7=(-1,-1)
                        // 对角线需要检查两个正交方向都能通过
                        int ortho1 = -1, ortho2 = -1;
                        switch (dir)
                        {
                            case 4: ortho1 = 1; ortho2 = 0; break; // (+1,+1) 需要 (+1,0) 和 (0,+1)
                            case 5: ortho1 = 1; ortho2 = 2; break; // (+1,-1) 需要 (+1,0) 和 (0,-1)
                            case 6: ortho1 = 3; ortho2 = 0; break; // (-1,+1) 需要 (-1,0) 和 (0,+1)
                            case 7: ortho1 = 3; ortho2 = 2; break; // (-1,-1) 需要 (-1,0) 和 (0,-1)
                        }
                        if (!CanPass(cx, cy, ortho1) || !CanPass(cx, cy, ortho2))
                            continue;
                    }

                    float newG = gCost[cx, cy] + moveCost[dir];
                    if (newG < gCost[nx, ny])
                    {
                        gCost[nx, ny] = newG;
                        pX[nx, ny] = cx;
                        pY[nx, ny] = cy;
                        open.Add((nx, ny));
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 八方向启发式（Octile Distance）
        /// </summary>
        private float Heuristic(int ax, int ay, int bx, int by)
        {
            int ddx = Mathf.Abs(ax - bx);
            int ddy = Mathf.Abs(ay - by);
            return (ddx + ddy) + (1.414f - 2f) * Mathf.Min(ddx, ddy);
        }

        private List<Vector2> BuildWorldPath(int[,] pX, int[,] pY, int sx, int sy, int ex, int ey)
        {
            var path = new List<Vector2>();
            int cx = ex, cy = ey;

            int safety = width * height;
            while ((cx != sx || cy != sy) && safety-- > 0)
            {
                path.Add(GridToWorld(cx, cy));
                int px = pX[cx, cy], py = pY[cx, cy];
                cx = px;
                cy = py;
            }

            path.Reverse();
            return path;
        }
    }
}

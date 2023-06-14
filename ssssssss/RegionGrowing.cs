using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ssssssss
{
    internal class RegionGrowing
    {
        Bitmap bitmap;
        bool[,] visited;
        public RegionGrowing(Bitmap p4)
        {
            bitmap = p4;
            run();
        }
        public void run()
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            Console.WriteLine("width = " + width + "    height = " + height);
            for (int x = 0; x < width; x += 300)
            {
                for (int y = 0; y < height; y += 30)
                {
                    Console.WriteLine("x = " + x + "     y = " + y);
                    Color regionColor = bitmap.GetPixel(x, y);
                    RegionGrowingRun(bitmap, x, y, 100, regionColor);
                }
            }
            //Console.WriteLine("x = " + x + "     y = " + y);
            
            /*
            Color regionColor = bitmap.GetPixel(0, 0);
            RegionGrowingRun(bitmap, 0, 0, 100, regionColor);
            Color regionColor2 = bitmap.GetPixel(width-1, height-1);
            RegionGrowingRun(bitmap, width - 1, height - 1, 100, regionColor2);
            */
        }
        public Bitmap getBitmap()
        {
            return bitmap;
        }

        public void RegionGrowingRun(Bitmap image, int seedX, int seedY, int threshold, Color borderColor)
        {
            int width = image.Width;
            int height = image.Height;

            // 성장 영역을 저장할 배열
            visited = new bool[width, height];

            // 성장 영역의 픽셀을 저장할 큐
            Queue<Point> queue = new Queue<Point>();

            // 초기 시드 픽셀을 큐에 추가하고 방문 표시
            queue.Enqueue(new Point(seedX, seedY));
            visited[seedX, seedY] = true;

            // 성장 영역 색상
            Color regionColor = bitmap.GetPixel(seedX, seedY);

            while (queue.Count > 0)
            {
                Point pixel = queue.Dequeue();
                int x = pixel.X;
                int y = pixel.Y;

                // 현재 픽셀과 이웃한 8방향 픽셀을 검사
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        int nx = x + dx;
                        int ny = y + dy;

                        // 이미지 범위를 벗어나면 스킵
                        if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                            continue;

                        // 방문하지 않은 픽셀이고, 유사성 조건을 만족하는 경우
                        if (!visited[nx, ny] && IsSimilarColor(bitmap.GetPixel(nx, ny), regionColor, threshold))
                        {
                            // 성장 영역에 추가
                            visited[nx, ny] = true;

                            // 테두리 픽셀인 경우에만 색상을 변경
                            if (IsBorderPixel(nx, ny, width, height))
                            {
                                bitmap.SetPixel(nx, ny, borderColor);
                            }

                            // 큐에 추가
                            queue.Enqueue(new Point(nx, ny));
                        }
                    }
                }
            }
        }
        bool IsBorderPixel(int x, int y, int width, int height)
        {
            // 현재 픽셀 주변에 적어도 하나의 방문한 픽셀이 있는지 확인
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    int nx = x + dx;
                    int ny = y + dy;

                    // 현재 픽셀 자신은 제외
                    if (dx == 0 && dy == 0)
                        continue;

                    // 픽셀이 이미지 범위를 벗어나면 스킵
                    if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                        continue;

                    // 방문한 픽셀이 존재하면 테두리로 판단
                    if (visited[nx, ny])
                        return true;
                }
            }

            // 주변에 방문한 픽셀이 없으면 테두리가 아님
            return false;
        }
        private bool IsSimilarColor(Color color1, Color color2, int threshold)
        {
            int diffR = Math.Abs(color1.R - color2.R);
            int diffG = Math.Abs(color1.G - color2.G);
            int diffB = Math.Abs(color1.B - color2.B);

            return (diffR <= threshold && diffG <= threshold && diffB <= threshold);
        }
    }
}

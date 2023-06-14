using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ssssssss
{
    public partial class Form1 : Form
    {
        const int HISTO_WIDTH = 256;
        const int HISTO_HEIGHT = 256;
        //////////원본////////
        Bitmap origin;
        int originwidth;
        int originheight;
        byte[,] originArray;
        //////////원본///////
        Bitmap p1;
        Image img;
        Color color;
        double[,] v;
        int cluster = 6, coord = 3;
        bool[,] visited;
        List<Point> pixelLocations;
        int[] avg;
        public Form1()
        {
            InitializeComponent();
            this.Size = new Size(1450, 680);
            pictureBox1.Location = new Point(30, 30);
            pictureBox1.Size = new Size(256, 256);
            pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

            pictureBox2.Location = new Point(300, 30);
            pictureBox2.Size = new Size(256, 256);
            pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            pictureBox2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

            pictureBox3.Location = new Point(570, 30);
            pictureBox3.Size = new Size(256, 256);
            pictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            pictureBox3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

            pictureBox4.Location = new Point(30, 320);
            pictureBox4.Size = new Size(256, 256);
            pictureBox4.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            pictureBox4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

            pictureBox5.Location = new Point(30, 320);
            pictureBox5.Size = new Size(256, 256);
            pictureBox5.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            pictureBox5.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

            pictureBox7.Location = new Point(840, 30);
            pictureBox7.Size = new Size(256, 256);
            pictureBox7.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            pictureBox7.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

            label1.Location = new Point(130, 300);
            label2.Location = new Point(390, 300);
            label3.Location = new Point(660, 300);
        }
        private void 열기ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = "영상 파일 열기";
            openFileDialog1.Filter = "All Files(*.*) |*.*|Bitmap File(*.bmp) |*.bmp |Jpeg File(*.jpg) | *.jpg";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string strFilename = openFileDialog1.FileName;
                img = Image.FromFile(strFilename);
                pictureBox1.Image = new Bitmap(openFileDialog1.FileName);

                origin = (Bitmap)pictureBox1.Image;
                originwidth = origin.Width;
                originheight = origin.Height;
                originArray = BitmapToByteArray2D(origin);
            }
        }
        private void 퍼지스트레칭ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pictureBox2.Image = fuzzyst(origin);
        }


        //원본이미지에서 결함영역 라벨링하기
        public Bitmap originlabel(byte[,] byteArray, int width, int height, int[] y_avg)
        {
            int minY = y_avg[0];
            int maxY = y_avg[1];

            // 새 이미지 크기
            int newWidth = width;
            int newHeight = maxY - minY + 1;

            // 새이미지
            Bitmap labeledImg = new Bitmap(newWidth, newHeight);

            for (int i = 0; i < newWidth; i++)
            {
                for (int j = minY; j <= maxY; j++)
                {
                    Color pixelColor = Color.FromArgb(byteArray[j * width + i, 0], byteArray[j * width + i, 1], byteArray[j * width + i, 2]);
                    labeledImg.SetPixel(i, j - minY, pixelColor);
                }
            }

            return labeledImg;
        }
        // ROI 사이즈 계산
        public Size getimgSize(int width, int height, int[] y_avg)
        {
            int minY = y_avg[0];
            int maxY = y_avg[1];

            int newWidth = width;
            int newHeight = maxY - minY + 1;

            return new Size(newWidth, newHeight);
        }
        // 비결함 영역 핑크칠해보기~~~
        public Bitmap changetored(byte[,] byteArray, int width, int height, int[] y_avg)
        {
            Bitmap newbmp = new Bitmap(width, height);

            int pinkTopRange = y_avg[0];    // height[0]
            int pinkBottomRange = y_avg[1]; // height[1]

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Color originColor = Color.FromArgb(byteArray[j * width + i, 0], byteArray[j * width + i, 1], byteArray[j * width + i, 2]);

                    // 핑크색으로 바꿀 범위 확인
                    if (j < pinkTopRange || j > pinkBottomRange)
                    {
                        newbmp.SetPixel(i, j, Color.Pink);
                    }
                    else
                    {
                        newbmp.SetPixel(i, j, originColor);
                    }
                }
            }

            return newbmp;
        }
        private void 배경제거ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //check(퍼지스트레칭한 이미지)
            Bitmap strimg = (Bitmap)pictureBox2.Image;
            byte[,] tempArray = BitmapToByteArray2D(strimg);
            int strheight = strimg.Height;
            int strwidth = strimg.Width;

            //height평균값이용
            int[] y_avg = GetHeightAverage(strwidth, strheight, tempArray);

            //ROI영역 이미지 띄울 픽쳐박스 사이즈 조정
            Size extractedSize = getimgSize(originwidth, originheight, y_avg);

            // 결함영역 라벨링
            pictureBox4.Size = extractedSize;
            pictureBox5.Size = pictureBox4.Size;
            pictureBox6.Size = pictureBox4.Size;
            pictureBox5.Location = new Point(30, pictureBox4.Location.Y + pictureBox4.Height + 10);
            pictureBox6.Location = new Point(30, pictureBox5.Location.Y + pictureBox5.Height + 10);
            pictureBox4.Image = originlabel(originArray, originwidth, originheight, y_avg);

            //////////////////////////check_part///////////////////////////////
            
            //check(비결함영역 제거)
            pictureBox3.Image = changetored(tempArray, strwidth, strheight, y_avg);
        }
        public int[] GetHeightAverage(int width, int height, byte[,] arr)
        {
            byte[,] checkArray = arr;
            int[] topyArray = new int[width / 3];       // 위쪽 탐색한 y값 저장할 배열
            int[] bottomyArray = new int[width / 3];    // 아래쪽 탐색한 y값 저장할 배열
            int stop = 10;
            int sum = 0;
            int startWidth = width / 3;                 // 탐색시작위치
            int endWidth = 2 * width / 3;               // 탐색종료위치
            int k = 0;
            avg = new int[2];                     // 위아래 height 평균 저장할 배열

            // 탐색영역설정(위에서 밑으로 탐색할때)
            Console.WriteLine("위위위위위위위위위위");
            for (int i = startWidth; i < endWidth; i++)
            {
                for (int j = 0; j < height - 2; j++)
                {
                    if (Math.Abs(checkArray[j * width + i, 0] - checkArray[(j + 1) * width + i, 0]) >= stop)
                    {
                        topyArray[k] = j;
                        Console.WriteLine("topy찾았지롱~ : " + topyArray[k] + " | y개수 : " + k + " | yArray배열 길이 : " + topyArray.Length);
                        k++;
                        break;
                    }
                }
                // k 값이 yArray의 크기보다 커지는 것을 방지
                if (k >= topyArray.Length)
                    break;
            }

            for (int i = 0; i < topyArray.Length; i++)  // topheight평균
            {
                sum += topyArray[i];
            }
            avg[0] = sum / k;

            // 탐색영역설정(밑에서 위로 탐색할때)
            Console.WriteLine("밑밑밑밑밑밑밑밑밑");
            sum = 0;
            k = 0;
            for (int i = startWidth; i < endWidth; i++)
            {
                for (int j = height - 2; j > 0; j--)
                {
                    if (Math.Abs(checkArray[j * width + i, 0] - checkArray[(j - 1) * width + i, 0]) >= stop)
                    {
                        bottomyArray[k] = j;
                        Console.WriteLine("bottomy찾았지롱~ : " + bottomyArray[k] + " | y개수 : " + k + " | yArray배열 길이 : " + bottomyArray.Length);
                        k++;
                        break;
                    }
                }
                // k 값이 yArray의 크기보다 커지는 것을 방지
                if (k >= bottomyArray.Length)
                    break;
            }

            for (int i = 0; i < bottomyArray.Length; i++)   // bottomheight평균
            {
                sum += bottomyArray[i];
            }
            avg[1] = sum / k;

            Array.Sort(avg);        // 오름차순 정렬
            Console.WriteLine("avg[0] = " + avg[0] + " | avg[1] = " + avg[1] + " | 원본이미지 height = " + height);
            Console.WriteLine("[영상크기] height : " + height + ", w : " + width + ", wh : " + height * width);

            return avg;
        }
        //////////////////////////////////씨앗/////////////////////////////////////////////////////////////////////
        public Bitmap RegionGrowingRun(Bitmap image, int seedX, int seedY, int threshold, Color borderColor)     //씨앗뿌릴좌표(이미지, x값, y값, threshold, 칠할 색
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
            Color regionColor = image.GetPixel(seedX, seedY);

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
                        if (!visited[nx, ny] && IsSimilarColor(image.GetPixel(nx, ny), regionColor, threshold))
                        {
                            // 성장 영역에 추가
                            visited[nx, ny] = true;

                            // 테두리 픽셀인 경우에만 색상을 변경
                            if (IsBorderPixel(nx, ny, width, height))
                            {
                                image.SetPixel(nx, ny, borderColor);
                            }

                            // 큐에 추가
                            queue.Enqueue(new Point(nx, ny));
                        }
                    }
                }
            }
            return image;
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
        //////////////////////////////////씨앗/////////////////////////////////////////////////////////////////////
        public Bitmap fuzzyst(Bitmap strimg)
        {
            int x, y, r, g, b;

            int Xmean, adjustment, Imax, Imin, Imid;
            double cut, l_value, r_value;

            int alpha, beta;
            Bitmap origin = strimg;
            Bitmap result;
            int height = origin.Height;
            int width = origin.Width;
            int[] LUT = new int[256];           // 히스토그램 표시할 배열
            byte[,] tempArray = new byte[height * width, 3];
            tempArray.Initialize();
            tempArray = BitmapToByteArray2D(origin);
            for (int rgb = 0; rgb < 3; rgb++)   // 영상 전체 채널별 히스토 평균
            {
                r = g = b = 0;
                for (x = 0; x < width; x++)
                {
                    for (y = 0; y < height; y++)
                    {
                        color = origin.GetPixel(x, y);
                        if (rgb == 0)
                            r += color.R;
                        else if (rgb == 1)
                            g += color.G;
                        else
                            b += color.B;
                    }
                }
                r = r / (height * width);
                g = g / (height * width);
                b = b / (height * width);
                // rgb별 히스토그램 평균 설정
                if (rgb == 0)
                    Xmean = r;
                else if (rgb == 1)
                    Xmean = g;
                else
                    Xmean = b;

                // 소속함수
                if (Xmean > 128)    // 밝기 조정률
                {
                    adjustment = 255 - Xmean;
                }
                else
                {
                    adjustment = Xmean;
                }
                Imax = Xmean + adjustment;
                Imin = Xmean - adjustment;
                Imid = (Imax + Imin) / 2;
                // ****
                // a-cut
                cut = (double)(Imin / Imax);
              
                    cut = 0.5;
                

                Console.WriteLine("Imin : " + Imin + ", Imax : " + Imax + ", a-cut : " + cut + ", 밝기 조정률 : " + adjustment);

                l_value = (Imid - Imin) * cut + Imin;   // a-cut -> 상한, 하한 계산
                r_value = -(Imax - Imid) * cut + Imax;
                alpha = (int)l_value;
                beta = (int)r_value;

                Console.WriteLine("alpha : " + alpha + " || beta : " + beta);

                for (x = 0; x < alpha; x++) LUT[x] = 0;
                for (x = 255; x > beta; x--) LUT[x] = 255;
                // 스트레칭 -> alpha => 0, beta => 255
                for (x = alpha; x <= beta; x++)
                    LUT[x] = (int)((x - alpha) * 255.0 / (beta - alpha));
                for (y = 0; y < height; y++)
                    for (x = 0; x < width; x++)
                        tempArray[y * width + x, rgb] = (byte)LUT[tempArray[y * width + x, rgb]];
            }
            result = byteArray2DToBitmap(tempArray, width, height);
            return result;
        }
        
        private void kmeansToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //kmeans();
        }
        private void kmeans()
        {
            //이미지지지지지지
            Bitmap origin = (Bitmap)pictureBox4.Image;
            int height = origin.Height;
            int width = origin.Width;
            int data = height * width;
            int sumr, sumg, sumb, count;
            byte[,] imgArray = BitmapToByteArray2D(origin);
            byte[,] x;

            x = imgArray;

            Console.WriteLine("[영상크기] h : " + height + ", w : " + width + ", wh : " + data);
            double[,] d = new double[data, cluster];    //데이터 배열
            v = new double[cluster, coord];   //각 클러스터별 벡터 값 받을 배열
            double[,] v_OLD = new double[cluster, coord]; //이전 클러스터 벡터 값 저장 배열
            double[,] U = new double[data, cluster];

            #region

            for (int i = 0; i < data; i++)
            {
                for (int j = 0; j < cluster; j++)
                {
                    U[i, j] = 0;
                }
            }

            int c = 0;
            do
            {
                for (int j = 0; j < cluster; j++)
                {
                    U[c, j] = 1;
                    if (c < data - 1)
                    {
                        c++;
                    }
                }
            } while (c < data - 1);

            for (int i = 0; i < cluster; i++)
            {
                sumr = 0;
                sumg = 0;
                sumb = 0;
                count = 0;
                for (int j = 0; j < data; j++)
                {
                    if (U[j, i] == 1)
                    {
                        sumr += x[j, 0];
                        sumg += x[j, 1];
                        sumb += x[j, 2];
                        count++;
                    }
                }
                v[i, 0] = sumr / count;
                v[i, 1] = sumg / count;
                v[i, 2] = sumb / count;
            }
            #endregion
            double min, num;
            int v_count;
            do
            {
                for (int i = 0; i < data; i++)/*Initialize U*/  //소속행렬 초기화
                {
                    for (int j = 0; j < cluster; j++)
                    {
                        U[i, j] = 0;
                    }
                }
                v_count = 0;

                for (int i = 0; i < data; i++)
                {
                    min = 0;
                    count = 0;
                    for (int j = 0; j < cluster; j++)   //유클리디안 거리 계산
                    {
                        d[i, j] = Math.Sqrt(Math.Pow((v[j, 0] - x[i, 0]), 2) + Math.Pow((v[j, 1] - x[i, 1]), 2) + Math.Pow((v[j, 2] - x[i, 2]), 2));
                        if (j == 0)
                        {
                            //여기서 min은 뭐임?
                            //첫번째 거리계싼한 애를 min으로 두고
                            min = d[i, j];
                        }
                        else if (j > 0 && (d[i, j] < min))  //그 다음부터 min갱신
                        {
                            min = d[i, j];
                            count = j;
                        }
                    }
                    U[i, count] = 1;
                }
                //무게중심의 변화체크
                for (int i = 0; i < cluster; i++)
                {
                    for (int j = 0; j < coord; j++)
                    {
                        v_OLD[i, j] = v[i, j];
                    }
                }

                for (int i = 0; i < cluster; i++)
                {
                    for (int j = 0; j < coord; j++)
                    {
                        count = 0;
                        num = 0;
                        for (int k = 0; k < data; k++)
                        {
                            num = num + (U[k, i] * x[k, j]);//소속행렬*데이터
                            if (U[k, i] == 1)
                            {
                                count++;
                            }
                        }
                        if ((num / count).Equals(double.NaN)) //double이상의 상수(예외처리)
                        {
                            v[i, j] = 0;
                        }
                        else //새로운 중심벡터 계산
                        {
                            v[i, j] = num / count;
                        }
                    }
                }

                for (int i = 0; i < cluster; i++)
                {
                    for (int j = 0; j < coord; j++)
                    {
                        if (v_OLD[i, j] != v[i, j])
                            v_count++;
                    }
                }
            } while (v_count != 0);

            //ROI영역 → 클러스터링 결과(수정중*******)
            Bitmap result = new Bitmap(width, height);
            //int[] clusterColors = { 0, 50, 100, 160, 210, 250 };
            int[] clusterColors = { 0, 0, 100, 200, 0, 0 };

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    for (int k = 0; k < cluster; k++)
                    {
                        if (U[i * width + j, k] == 1)
                        {
                            result.SetPixel(j, i, Color.FromArgb(clusterColors[k], clusterColors[k], clusterColors[k]));
                            break;
                        }
                    }
                }
            }
            pictureBox5.Image = result;
        }
        private void 두번째퍼지스트레칭ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Bitmap p4 = (Bitmap)pictureBox4.Image;  //ROI영역 이미지
            //pictureBox5.Image = fuzzyst(p4);        //ROI이미지 퍼지스트레칭이미지

            //씨앗뿌리기
            RegionGrowing regionGrowing = new RegionGrowing(p4);
            pictureBox5.Image = regionGrowing.getBitmap();

            //결함영상 이진화
            Bitmap p5 = (Bitmap)pictureBox5.Image;

            byte[,] newArray = BitmapToByteArray2D(p5);
            int width = p5.Width;
            int height = p5.Height;
            Bitmap newbmp = new Bitmap(width, height);
            int total = 0;
            pixelLocations = new List<Point>();     //결함 위치 저장 리스트

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Color originColor = Color.FromArgb(newArray[j * width + i, 0], newArray[j * width + i, 1], newArray[j * width + i, 2]);
                    int grayscale = (originColor.R + originColor.G + originColor.B) / 3;
                    total += grayscale;
                }
            }
            int rgbavg = (int)(total / (width * height));
            Console.WriteLine("평균rgb = " + avg);
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Color originColor = Color.FromArgb(newArray[j * width + i, 0], newArray[j * width + i, 1], newArray[j * width + i, 2]);
                    int grayscale = (originColor.R + originColor.G + originColor.B) / 3;

                    if (grayscale < rgbavg)
                    {
                        newbmp.SetPixel(i, j, Color.Black);
                    }
                    else
                    {
                        newbmp.SetPixel(i, j, Color.White);
                        pixelLocations.Add(new Point(i, j+avg[0]));
                    }
                }
            }
            /*for (int x = 0; x < newBmp.Width; x++)
            {
                for (int y = 0; y < newBmp.Height; y++)
                {
                    Point location = pixelLocations[x, y];
                    Console.WriteLine("Pixel Location - X: " + location.X + ", Y: " + location.Y);
                }
            }*/
            foreach (Point location in pixelLocations)
            {
                Console.WriteLine("픽셀위치 - X: " + location.X + ", Y: " + location.Y);
            }
            pictureBox6.Image = newbmp;
        }
        //원본이미지에 구한 결함영역 위치 색칠(빨간)
        private void 이진화ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Bitmap resultImage = new Bitmap(originwidth, originheight);

            for (int x = 0; x < originwidth; x++)
            {
                for (int y = 0; y < originheight; y++)
                {
                    Color pixelColor = origin.GetPixel(x, y);
                    if (pixelLocations.Contains(new Point(x, y)))
                    {
                        resultImage.SetPixel(x, y, Color.Red);
                    }
                    else
                    {
                        resultImage.SetPixel(x, y, pixelColor);
                    }
                }
            }
            pictureBox7.Image = resultImage;
        }
        public byte[,] BitmapToByteArray2D(Bitmap bmp)
        {
            byte[,] bmpArray = new byte[bmp.Width * bmp.Height, 3];
            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    Color color = bmp.GetPixel(x, y);
                    bmpArray[y * bmp.Width + x, 0] = color.R;
                    bmpArray[y * bmp.Width + x, 1] = color.G;
                    bmpArray[y * bmp.Width + x, 2] = color.B;
                }
            }
            return bmpArray;
        }
        public Bitmap byteArray2DToBitmap(byte[,] byteArray, int width, int height)
        {
            Bitmap newbmp = new Bitmap(width, height);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Color newColor = Color.FromArgb(byteArray[y * width + x, 0], byteArray[y * width + x, 1], byteArray[y * width + x, 2]);
                    newbmp.SetPixel(x, y, newColor);
                }
            }
            return newbmp;
        }
    }
}

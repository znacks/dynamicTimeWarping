using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dtw_windowForm
{
    public partial class Form1 : Form
    {
        static Dictionary<string, float> dtw_matx = new Dictionary<string, float>();
        static float[] addition;
        static float[] mathX, mathY;
        static Dictionary<string, float> dtw_row = new Dictionary<string, float>();
        static bool tableEnable,graphEnable;
        static float jx = -1, finalDistance, idk;

        static DataTable dtwMatrixTable = new DataTable();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        static void DynamicTimeWarpingDistance(int i, int j)
        {
            float dist = Math.Abs(mathX[i] - mathY[j]);

            // first char
            if (j == 0 && i == 0)
            {
                dtw_matx.Add(key(0, 0), dist);
            }
            // first line
            else if (j == 0)
            {
                dtw_matx.Add(key(i, 0), dist);
                if (i != 0)
                {
                    addition = new float[]
                    {
                        dtw_matx[key(i-1,0)]
                    };
                }

                dtw_matx[key(i, 0)] = dist + addition.Min();
            }
            //rest of the line
            else if (j != 0)
            {
                dtw_matx.Add(key(i, j), dist);
                if (i != 0)
                {
                    addition = new float[]
                    {
                        dtw_matx[key(i-1,j)],
                        dtw_matx[key(i,j-1)],
                        dtw_matx[key(i-1,j-1)],
                    };
                }
                else
                {
                    addition = new float[]
                    {
                        dtw_matx[key(0,j-1)],
                    };
                }

                dtw_matx[key(i, j)] = dist + addition.Min();
            }

            if (tableEnable)
            {
                if (jx != j)
                {
                    dtwMatrixTable.Columns.AddRange(new DataColumn[]
                    {
                        new DataColumn(j.ToString()+"j",typeof(int))
                    });
                }

                while (idk < mathX.Length)
                {
                    dtwMatrixTable.Rows.Add();
                    idk++;
                }
            }

            jx = j;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            button1.Enabled = true;
            button2.Enabled = false;
            panel1.CreateGraphics().Clear(SystemColors.Control);

            dtw_matx.Clear();
            dtwMatrixTable.Columns.Clear();
            dtwMatrixTable.Clear();
            this.dataGridView1.Columns.Clear();

            this.dataGridView1.DataSource = null;
            this.dataGridView1.Refresh();

            idk = 0;
            finalDistance = 0;
        }

        float[] sampleSpace;
        private void solution_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            button2.Enabled = true;

            FindPath();

            float similarity;
            try
            {
                similarity = (EuclidianDist() * 100) / (EuclidianDist() + finalDistance);
            }
            catch (DivideByZeroException)
            {
                similarity = 100;
            }

            label1.Text = "Final Distance : " + finalDistance.ToString();
            label2.Text = "Similarity : " + similarity.ToString() + "%";

            if (tableEnable) dataGridView1["0j", 0].Selected = false;
        }

        void FindPath()
        {
            string[] inputArray1 = textBox1.Text.Split(new Char[] { ',' });
            string[] inputArray2 = textBox2.Text.Split(new Char[] { ',' });

            mathX = Array.ConvertAll(inputArray1, s => float.TryParse(s, out var x) ? x : -1);
            mathY = Array.ConvertAll(inputArray2, s => float.TryParse(s, out var x) ? x : -1);

            for (int j = 0; j < mathY.Length; j++)
            {
                for (int i = 0; i < mathX.Length; i++)
                {
                    DynamicTimeWarpingDistance(i, j);
                    this.dataGridView1.DataSource = dtwMatrixTable;
                    if (tableEnable) dataGridView1[j.ToString() + "j", i].Value = dtw_matx[key(i, j)];

                    PaintGraph(j, i, false);
                }
                if (tableEnable) dataGridView1.Columns[j.ToString() + "j"].Width = 25;
            }

            int jj = mathY.Length - 1, ii = mathX.Length - 1;

            while (true)
            {
                if (tableEnable) { var selectedGrid = dataGridView1[jj.ToString() + "j", ii].Style.BackColor = Color.Yellow; }
                PaintGraph(jj, ii,true);

                finalDistance += dtw_matx[key(ii, jj)];

                if (ii == 0 && jj == 0)
                {
                    sampleSpace = new float[]
                    {
                        dtw_matx[key(0,0)]
                    };

                    if (tableEnable) dataGridView1["0j", 0].Style.BackColor = Color.Yellow;

                    break;
                }
                else if (ii == 0)
                {
                    sampleSpace = new float[]
                    {
                        dtw_matx[key(0,jj-1)]
                    };
                }
                else if (jj == 0)
                {
                    sampleSpace = new float[]
                    {
                        dtw_matx[key(ii-1,0)]
                    };
                }
                else
                {
                    sampleSpace = new float[]
                    {
                        dtw_matx[key(ii-1,jj)],
                        dtw_matx[key(ii,jj-1)],
                        dtw_matx[key(ii-1,jj-1)]
                    };
                }

                var selectedIndex = sampleSpace.FindIndex(x => x == sampleSpace.Min());

                if (ii == 0)
                {
                    jj -= 1;
                }
                else if (jj == 0)
                {
                    ii -= 1;
                }
                else
                {
                    if (selectedIndex == 2)
                    {
                        ii -= 1;
                        jj -= 1;
                    }
                    else if (selectedIndex == 1)
                    {
                        jj -= 1;
                    }
                    else if (selectedIndex == 0)
                    {
                        ii -= 1;
                    }
                }
            }
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            tableEnable = tableBox.Checked;
        }

        private void graphBox_CheckedChanged(object sender, EventArgs e)
        {
            graphEnable = graphBox.Checked;
        }

        void PaintGraph(int i,int j,bool highlight)
        {
            if (graphEnable)
            {
                Graphics gObject = panel1.CreateGraphics();

                panel1.Width = 300;
                panel1.Height = 300;

                float stepX = panel1.Width / mathX.Length;
                float stepY = panel1.Height / mathY.Length;

                label3.Text = mathX.Length.ToString() + " x " + mathY.Length.ToString();

                if (highlight) gObject.FillRectangle(new SolidBrush(Color.Red), stepX * i, stepY * j, stepX, stepY);
                else gObject.FillRectangle(new SolidBrush(Color.White), stepX * i, stepY * j, stepX, stepY);
            }
        }

        float EuclidianDist()
        {
            float euclidianDist = 0;
            int ii = mathX.Length - 1, jj = mathY.Length - 1;

            while(ii != 0 && jj != 0)
            {
                euclidianDist += Math.Abs(mathX[ii] - mathY[jj]);

                ii--;
                jj--;
            }

            return euclidianDist;
        }

        static string key(int i, int j)
        {
            string value = i.ToString() + "i " + j.ToString() + "j";

            return value;
        }
    }
    
    static class ArrayExtensions
    {
        public static int FindIndex<T>(this T[] array, Predicate<T> match)
        {
            return Array.FindIndex(array, match);
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace WindowsFormsApplication2
{
    public partial class Form1 : Form
    {
        Substitution sub;
        char[] key;
        Random random = new Random();
        public List<Chromosome> population;
        public double elite = 0.1;
        public double MutationProbability = 0.4;
        public int max = 300;
        public int Pop_Size = 100;
        TextBox[] tb1 = new TextBox[26];
        TextBox[] tb2 = new TextBox[26];
 
        public Form1()
        {
            InitializeComponent();
            sub = new Substitution();
            comboBox1.Items.Add("Tournament");
            comboBox1.Items.Add("Roulette Wheel");
        }

        public void InitializePopulation(char[] key, string s)
        {
            population = new List<Chromosome>();
            for (int i = 0; i < Pop_Size; i++)
            {
                this.population.Add(new Chromosome(key, s));
            }
        }

        public char[] FrequencyAnalysis()
        {
            string alfabet = "abcdefghijklmnopqrstuvwxyz";
            string freqAlf = "etaoinshrdlcumwfgypbvkxjqz";
            ArrayList aL = new ArrayList();
            ArrayList aList = new ArrayList();
            string text = richTextBox2.Text;
            SortedDictionary<char, int> dict = new SortedDictionary<char, int>();
            for (int i = 0; i < text.Length; i++)
                for (int j = 0; j < alfabet.Length; j++)
                    if (text[i] == alfabet[j])
                        aL.Add(text[i]);
            for (int i = 0; i < aL.Count; i++)
                if (dict.ContainsKey((char)aL[i]))
                    dict[(char)aL[i]]++;
                else
                    dict.Add((char)aL[i], 1);

            var sortDict = dict.OrderByDescending(x => x.Value);

            foreach (KeyValuePair<char, int> kvp in sortDict)
            {
                aList.Add((char)kvp.Key);
            }
            char[] freqKey1 = new char[aList.Count];
            for (int i = 0; i < freqKey1.Length; i++)
            {
                freqKey1[i] = (char)aList[i];
            }
            char[] alf = alfabet.ToCharArray();
            IEnumerable<char> nums = alf.Except<char>(freqKey1);
            foreach (char c in nums)
                aList.Add(c);
            char[] freqKey = new char[aList.Count];
            for (int i = 0; i < freqKey.Length; i++)
            {
                freqKey[i] = (char)aList[i];
            }
            char[] newKey = new char[alfabet.Length];
            for (int i = 0; i < alfabet.Length; i++)
                for (int j = 0; j < freqKey.Length; j++)
                    if (alfabet[i] == freqAlf[j])
                        newKey[i] = freqKey[j];
            return newKey;
        }

        public char[] go(char[] key, string s)
        {
            InitializePopulation(key, s);
            if (checkBox1.Checked)
            {
                char[] freqKey = FrequencyAnalysis();
                Chromosome chrom = new Chromosome(freqKey, key, s);
                population.Remove(population[population.Count - 1]);
                population.Add(chrom);
            }
            chart1.ChartAreas[0].AxisX.Minimum = 0;
            chart1.ChartAreas[0].AxisX.Interval = 20;
            chart1.ChartAreas[0].AxisY.Minimum = 0;
            chart1.ChartAreas[0].AxisY.Maximum = 100;
            chart1.ChartAreas[0].AxisY.Interval = 20;
            DateTime a = DateTime.Now;
            for (int i = 0; i < max; i++)
            {
                population.Sort();
                double fit = population[0].fitness;
                chart1.Series[0].Points.AddXY(i, fit);
                chart1.Refresh();
                label10.Text = i.ToString();
                label10.Refresh();
                label11.Text = population[0].fitness.ToString()+"%";
                label11.Refresh();
                label12.Text = (DateTime.Now - a).ToString();
                label12.Refresh();
                richTextBox4.AppendText(("Generation " + i.ToString() + " has " + population[0].fitness.ToString()+"%" + Environment.NewLine));
                if (population[0].fitness == 0)
                {
                    break;
                }
                else
                {
                    if (comboBox1.SelectedIndex == 0)
                        population = Tournament(population, key, s);
                    if (comboBox1.SelectedIndex == 1)
                        population = RouletteWheel(population, key, s);
                }
            }
            string best_result = new string(population[0].gens);
            char[] res = population[0].gens;
            return res;
        }

        List<Chromosome> Tournament(List<Chromosome> population, char[] key, string sopen)
        {
            int esize = (int)(Pop_Size * elite);
            List<Chromosome> children = new List<Chromosome>();
            selectElite(population, children, esize);
            List<Chromosome> selection = new List<Chromosome>();
            Random r = new Random();
            for (int i = 0; i < Pop_Size; i++)
            {
                int f = r.Next(population.Count);
                if (population[i].fitness < population[f].fitness)
                    selection.Add(population[i]);
                else
                    selection.Add(population[f]);
            }
            for (int j = 0; j < selection.Count; j++)
            {
                Chromosome child1 = Cross(selection[j], selection[r.Next(selection.Count)], key, sopen);
                if (random.NextDouble() < MutationProbability)
                {
                    children.Add(Mutate(child1, key, sopen));
                }
                else
                {
                    children.Add(child1);
                }
            }

            if (children.Count < Pop_Size)
            {
                while (children.Count != Pop_Size)
                {
                    children.Add(population[esize]);
                    esize++;
                }
            }
            if (children.Count > Pop_Size)
            {
                while (children.Count != Pop_Size)
                {
                    int rem = 0;
                    children.Remove(children[children.Count - 1 - rem]);
                    rem++;
                }
            }
            return children;
        }

        List<Chromosome> RouletteWheel(List<Chromosome> population, char[] key, string sopen)
        {
            int esize = (int)(Pop_Size * elite);
            List<Chromosome> children = new List<Chromosome>();
            selectElite(population, children, esize);
            Random r = new Random();
            List<Chromosome> selection = new List<Chromosome>();
            double roulette = 0;
            for (int i = 0; i < population.Count; i++)
            {
                roulette += population[i].fitness;
            }
            double[] pCross = new double[population.Count];
            for (int i = 0; i < population.Count; i++)
            {
                pCross[i] = roulette / population[i].fitness;
            }
            int l = 0;
            for (int i = 0; i < pCross.Length; i++)
            {
                l += (int)pCross[i];
            }
            for (int i = 0; i < population.Count; i++)
            {
                double sum = 0;
                int w = 0;
                int Rand_Val = r.Next(0, l);
                while (sum <= Rand_Val && w < population.Count)
                {
                    sum += pCross[w];
                    w++;
                }
                selection.Add(population[w - 1]);
            }
            for (int i = 0; i < selection.Count; i++)
            {

                Chromosome child = Cross(selection[i], selection[r.Next(selection.Count)], key, sopen); ;
                if (random.NextDouble() < MutationProbability)
                    children.Add(Mutate(child, key, sopen));
                else
                    children.Add(child);
            }
            if (children.Count < Pop_Size)
            {
                while (children.Count != Pop_Size)
                {
                    children.Add(population[esize]);
                    esize++;
                }
            }
            if (children.Count > Pop_Size)
            {
                while (children.Count != Pop_Size)
                {
                    int rem = 0;
                    children.Remove(children[children.Count - 1 - rem]);
                    rem++;
                }
            }
            return children;
        }


        List<Chromosome> NextGeneration(List<Chromosome> population, char[] key, string s)
        {
            int esize = (int)(Pop_Size * elite);
            List<Chromosome> children = new List<Chromosome>();
            selectElite(population, children, esize);
            for (int i = 0; i < Pop_Size; i += 2)
            {
                Chromosome child = Cross(population[i], population[i + 1], key, s);
                if (random.NextDouble() < MutationProbability)
                    children.Add(Mutate(child, key, s));
                else
                    children.Add(child);
            }
            if (children.Count < Pop_Size)
            {
                while (children.Count != Pop_Size)
                {
                    children.Add(population[esize]);
                    esize++;
                }
            }
            return children;
        }

        private void selectElite(List<Chromosome> population, List<Chromosome> children, int esize)
        {
            for (int i = 0; i < esize; i++)
            {
                children.Add(population[i]);
            }
        }


        Chromosome Cross(Chromosome a, Chromosome b, char[] key, string s)
        {
            char[] pair = new char[a.gens.Length];
            int razryv = random.Next(1, pair.Length - 1);
            for (int i = 0; i < razryv; i++)
            {
                pair[i] = a.gens[i];
            }
            for (int i = razryv; i < pair.Length; i++)
            {
                pair[i] = b.gens[i];
            }
            char[] finalchild = Repair(pair, razryv);
            Chromosome result = new Chromosome(finalchild, key, s);
            return result;
        }

        char[] Repair(char[] child, int razr)
        {
            string alfabet2 = "abcdefghijklmnopqrstuvwxyz";
            char[] alf2 = alfabet2.ToCharArray();
            char[] alf3 = child;
            int razryv = razr;
            List<int> repair = new List<int>();
            for (int x = 0; x < razryv; x++)
            {
                for (int y = razryv; y < alf3.Length; y++)
                {
                    if (alf3[x] == alf3[y])
                    {
                        repair.Add(y);
                    }
                }
            }

            int[] repair2 = repair.ToArray<int>();
            repair.Clear();
            IEnumerable<char> nums = alf2.Except<char>(alf3);
            char[] alf4 = new char[alf2.Length];
            int k = 0;
            foreach (char c in nums)
            {
                alf4[k] = c;
                k++;
            }
            for (int w = 0; w < repair2.Length; w++)
            {
                alf3[repair2[w]] = alf4[w];
            }
            return alf3;
        }

        Chromosome Mutate(Chromosome a, char[] key, string s)
        {
            char[] mutPair = a.gens;
            int geneNum = random.Next(mutPair.Length);
            int geneNum2 = random.Next(mutPair.Length);
            char muttemp = mutPair[geneNum];
            mutPair[geneNum] = mutPair[geneNum2];
            mutPair[geneNum2] = muttemp;
            Chromosome MutResult = new Chromosome(mutPair, key, s);
            return MutResult;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            string str = richTextBox1.Text;
            char[] EncText = sub.EncodingText(str, key);
            string Encod = new string(EncText);
            richTextBox2.Text = Encod;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.elite = Convert.ToDouble(par1.Text);
            this.MutationProbability = Convert.ToDouble(par3.Text);
            this.max =  Convert.ToInt32(par4.Text);;
            this.Pop_Size = Convert.ToInt32(par5.Text);
            string str = richTextBox1.Text;
            chart1.Series[0].Points.Clear();
            label10.Text = null;
            label11.Text = null;
            label12.Text = null;
            richTextBox4.Text = null;
            backgroundWorker1.RunWorkerAsync();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            key = sub.KeyGenerate();
            string str = new string(key);          
            for (int i = 0; i < tb1.Length; i++)
            {
                Controls.Remove(tb1[i]);
                Controls.Remove(tb2[i]); 
                tb1[i] = new System.Windows.Forms.TextBox();
                tb1[i].Location = new System.Drawing.Point(120 + i * 20, 310);
                tb1[i].Name = "textBox" + i.ToString();
                tb1[i].Size = new System.Drawing.Size(20, 20);
                tb1[i].TabIndex = i;
                tb1[i].Text = " " + str[i].ToString();                
                Controls.Add(tb1[i]);
            }
            for (int i = 0; i < tb2.Length; i++)
            {
                Controls.Remove(tb2[i]);
                tb2[i] = new System.Windows.Forms.TextBox();
                tb2[i].Location = new System.Drawing.Point(120 + i * 20, 340);
                tb2[i].Name = "textBox" + i.ToString();
                tb2[i].Size = new System.Drawing.Size(20, 20);
                tb2[i].TabIndex = i;
                Controls.Add(tb2[i]);
            }
        }



        private void Form1_Load(object sender, EventArgs e)
        {
            label1.Text = "Plain Text: ";
            label2.Text = "Encrypted Text: ";
            label3.Text = "Decrypted Text: ";
            label4.Text = "Alphabet: ";
            label5.Text = "Generated Key: ";
            label6.Text = "Found Key: ";
            label7.Text = "Generation: ";
            label8.Text = "Fitness: ";
            label9.Text = "Time: ";
            label10.Text = " ";
            label11.Text = " ";
            label12.Text = " ";
            label13.Text = "Elite: ";
            label14.Text = "Selection Type: ";
            label15.Text = "Mutation Probability: ";
            label16.Text = "Max Generation: ";
            label17.Text = "Population Size: ";
            label18.Text = "GENETIC ALGORITHM PARAMETERS: ";
            par1.Text = "0,1";
            par3.Text = "0,4";
            par4.Text = "300";
            par5.Text = "100";
            comboBox1.SelectedIndex = 0;
            string alphabet = "abcdefghijklmnopqrstuvwxyz";
            TextBox[] tb = new TextBox[alphabet.Length];
            for (int i = 0; i < tb.Length; i++)
            {
                tb[i] = new System.Windows.Forms.TextBox();
                tb[i].Location = new System.Drawing.Point(120 + i * 20, 280);
                tb[i].Name = "textBox" + i.ToString();
                tb[i].Size = new System.Drawing.Size(20, 20);
                tb[i].TabIndex = i;
                tb[i].Text = " " + alphabet[i].ToString();
                Controls.Add(tb[i]);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = null;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            richTextBox2.Text = null;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            richTextBox3.Text = null;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Title = "Выберите файл";
            openFileDialog1.Filter = "Текстовые файлы|*.txt";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string s = openFileDialog1.FileName;
                FileStream stream = new FileStream(s, FileMode.Open);
                StreamReader reader = new StreamReader(stream);
                string str = reader.ReadToEnd();
                stream.Close();
                richTextBox1.Text = str;
            }
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void button8_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < tb1.Length; i++)
                if (tb1[i].Text != tb2[i].Text)
                    tb2[i].BackColor = Color.Red;
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            string str = richTextBox1.Text;
            char[] res = this.go(key, str);
            string Decod = new string(res);
            char[] check = richTextBox2.Text.ToCharArray();
            char[] rs = sub.DecodingText(check, res);
            string fin = new string(rs);
            richTextBox3.Text = fin;
            for (int i = 0; i < tb2.Length; i++)
            {
                tb2[i].Text = " " + Decod[i].ToString();
            }
        }

    }

    public class Substitution
    {
        public Substitution()
        {
        }
        public char[] KeyGenerate()
        {
            string alfabet = "abcdefghijklmnopqrstuvwxyz";
            char[] key = alfabet.ToCharArray();
            Random rand = new Random();
            for (int i = 0; i < key.Length; i++)
            {
                int j = rand.Next(key.Length);
                char tmp = key[i];
                key[i] = key[j];
                key[j] = tmp;
            }
            return key;
        }
        public char[] EncodingText(string str, char[] key1)
        {
            string alfabet = "abcdefghijklmnopqrstuvwxyz";
            char[] alf = alfabet.ToCharArray();
            char[] text = str.ToCharArray();
            char[] key = key1;
            int[] s = new int[text.Length];
            for (int x = 0; x < text.Length; x++)
                for (int y = 0; y < alf.Length; y++)
                {
                    if (text[x] == alf[y])
                        s[x] = y;
                }
            for (int x = 0; x < text.Length; x++)
                for (int y = 0; y < alf.Length; y++)
                {
                    if (text[x] == alf[y])
                        text[x] = key[s[x]];
                }
            return text;
        }
        public char[] DecodingText(char[] enctext, char[] key1)
        {
            string alfabet = "abcdefghijklmnopqrstuvwxyz";
            char[] alf = alfabet.ToCharArray();
            char[] text = enctext;
            char[] key = key1;
            int[] s = new int[text.Length];
            for (int x = 0; x < text.Length; x++)
                for (int y = 0; y < alf.Length; y++)
                {
                    if (text[x] == key[y])
                        s[x] = y;
                }
            for (int x = 0; x < text.Length; x++)
                for (int y = 0; y < alf.Length; y++)
                {
                    if (text[x] == alf[y])
                        text[x] = alf[s[x]];
                }
            return text;
        }
    }
    public class Chromosome : IComparable<Chromosome>
    {
        public char[] gens;
        static Random random;
        public double fitness;

        public Chromosome(char[] key, string s)
        {
            string alfabet = "abcdefghijklmnopqrstuvwxyz";
            char[] key1 = alfabet.ToCharArray();
            Random rand = new Random();
            for (int i = 0; i < key1.Length; i++)
            {
                int j = rand.Next(1, key1.Length - 2);
                j = j + 1;
                char tmp = key1[i];
                key1[i] = key1[j];
                key1[j] = tmp;
            }
            this.gens = key1;
            this.fitness = this.GetFitness(this, key, s);
        }
        public Chromosome(char[] genofond, char[] key, string s)
        {
            this.gens = genofond;
            this.fitness = this.GetFitness(this, key, s);
        }
        double GetFitness(Chromosome a, char[] key, string s)
        {
            Substitution sub1 = new Substitution();
            FileStream stream = new FileStream("myDictionary1.txt", FileMode.Open);
            StreamReader reader = new StreamReader(stream);
            string str = reader.ReadToEnd();
            stream.Close();
            string firsttext = s;
            str = s+str;
            string[] words = str.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            char[] encText = sub1.EncodingText(firsttext, key);
            char[] GotText = sub1.DecodingText(encText, this.gens);
            string GotTextStr = new string(GotText);
            string[] words2 = GotTextStr.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            int knownwords = 0;
            for (int x = 0; x < words2.Length; x++)
                for (int y = 0; y < words.Length; y++)
                {
                    if (words2[x] == words[y])
                    {
                        knownwords++;
                        break;
                    }
                }
            double result = 100 * (words2.Length - knownwords) / words2.Length;
            return result;
        }

        public int CompareTo(Chromosome obj)
        {
            if (this.fitness > obj.fitness)
                return 1;
            if (this.fitness < obj.fitness)
                return -1;
            else
                return 0;
        }
    }
}

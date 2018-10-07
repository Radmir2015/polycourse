using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace Polynomial
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            openFileDialog1.Filter = "Text files(*.txt)|*.txt|All files(*.*)|*.*";
            saveFileDialog1.Filter = "Text files(*.txt)|*.txt|All files(*.*)|*.*";
        }

        public class Polynom
        {
            private List<Node> nodes = new List<Node>();

            public object Clone()
            {
                return this.MemberwiseClone();
            }

            public Polynom() {}
            public Polynom(Node node) => Add(node);
            public Polynom(List<Node> nds) => nds.ForEach(x => Add(x));
            public Polynom(string expression)
            {
                string[] unprocessedNodes = Regex.Split(Regex.Replace(expression, @"(?<=[0-9a-zA-Z]+)([\+-])", " $1"), @"(?<=[0-9a-zA-Z]+)\s+(?=[\+-])").Select(x => x.Trim()).ToArray();
                unprocessedNodes = unprocessedNodes.Select(x => Regex.Replace(x, @"\s+", "")).ToArray();
                
                //Regex regex = new Regex(@"([\+-]?(?:\s*)\d*)\s*\*?\s*([a-zA-Z]?)\s*\^?\s*([\+-]?(?:\s*)\d*)");
                Regex regex = new Regex(@"([\+-]?\d*\.?\d*)\*?([a-zA-Z]?)\^?([\+-]?\d*)");

                //MessageBox.Show(string.Join(";", unprocessedNodes));

                foreach (string node in unprocessedNodes)
                {
                    Node temp = new Node();
                    MatchCollection matches = regex.Matches(node);

                    for (int i = 0; i < matches.Count - 1; i++)
                    {
                        Match match = matches[i];

                        //MessageBox.Show(match.Groups[1].Value + ";" + match.Groups[2].Value + ";" + match.Groups[3].Value);

                        if (match.Groups[1].Value == "" && match.Groups[2].Value == "" || unprocessedNodes.Any(x => x == ""))
                        {
                            MessageBox.Show("Fatal error");
                            break;
                        }

                        double tempK = (match.Groups[1].Value == "" || match.Groups[1].Value == "+")
                                    ? 1
                                    : (match.Groups[1].Value == "-"
                                        ? -1
                                        : StringToDouble(match.Groups[1].Value));

                        int tempPower = match.Groups[3].Value == ""
                                        ? (match.Groups[2].Value == ""
                                            ? 1
                                            : (match.Groups[2].Value == "-"
                                                ? -1
                                                : 1))
                                        : StringToInt(match.Groups[3].Value);

                        string tempLetter = match.Groups[2].Value;

                        if (temp.GetDict().Count == 0)
                            temp = new Node(tempK, tempPower, tempLetter);
                        else
                            temp.GetDict()[tempLetter] = tempPower;
                    }
                    Add(temp);
                }
            }

            bool CompareSets(Node aNode, Node bNode)
            {
                return CompareLetters(aNode, bNode)
                    && aNode.GetDict().Keys.All(x => aNode.GetPower(x) == bNode.GetPower(x));
            }

            bool CompareLetters(Node aNode, Node bNode)
            {
                HashSet<string> aLetters = new HashSet<string>(aNode.GetDict().Keys);
                HashSet<string> bLetters = new HashSet<string>(bNode.GetDict().Keys);
                
                return aLetters.SetEquals(bLetters);
            }

            int ComparePowers(Node aNode, Node bNode)
            {
                return aNode.GetDict().Values.Sum() - bNode.GetDict().Values.Sum();
            }

            public void Add(Node node)
            {
                Node temp = new Node(node.GetK(), node.GetCopiedDict());
                if (nodes.Any(x => CompareLetters(node, x) && ComparePowers(node, x) == 0)) // && nodes.Any(x => ComparePowers(node, x) == 0))
                {
                    int count = nodes.Count();
                    for (int i = 0; i < count; i++)
                        if (CompareSets(nodes[i], node))
                            nodes[i].SetK(nodes[i].GetK() + node.GetK());
                    //nodes = nodes.Where(x => x.GetK() != 0).ToList();
                }
                else
                    nodes.Insert(nodes.TakeWhile(x => ComparePowers(x, node) >= 0).Count(), temp);
                nodes = nodes.Where(x => x.GetK() != 0).ToList();
            }

            public void Add(List<Node> nds) => nds.ForEach(x => Add(x));

            public void Delete(Node node) => nodes.Remove(node);
            public void DeleteByPower(int power) => nodes.RemoveAll(x => x.GetFullPower() == power);
            public void Clear() => nodes.Clear();
            public int AmountOfSums() => nodes.Count();

            public List<Node> GetNodes() => nodes;

            public string GetRepresentation()
            {
                return nodes.Count() == 0 ? "0" :
                    string.Join(" + ", nodes.Select(x =>
                        (Math.Abs(x.GetK()) != 1 ? x.GetK().ToString() : (x.GetK() < 0 ? "-" : ""))
                        + string.Join(" * ", x.GetDict().Select(y =>
                            ((y.Key == "" && Math.Abs(x.GetK()) == 1)
                            ? Math.Abs(x.GetK()).ToString()
                            : "") +
                            y.Key + ((y.Value != 1)
                                ? ("^" + y.Value)
                                : "")))
                            )).Replace("+ -", "- ").Replace(",", ".");
            }

            public bool IsEmpty() => nodes.Count() == 0;

            public static Dictionary<string, int> MultiplyPowers(Dictionary<string, int> aNode, Dictionary<string, int> bNode)
            {
                Dictionary<string, int> lessLetters = aNode.Count(x => x.Key != "") > bNode.Count(x => x.Key != "") ? bNode : aNode;
                Dictionary<string, int> moreLetters = aNode.Count(x => x.Key != "") <= bNode.Count(x => x.Key != "") ? bNode : aNode;
                foreach (var x in lessLetters.Where(x => x.Key != ""))
                {
                    if (!moreLetters.ContainsKey(x.Key))
                        moreLetters[x.Key] = 0;
                    moreLetters[x.Key] += x.Value;
                }

                return moreLetters;
            }

            public static Polynom operator+(Polynom f, Polynom g)
            {
                Polynom res = new Polynom(f.nodes);
                res.Add(g.nodes);
                return res;
            }

            public static Polynom operator*(Polynom f, Polynom g)
            {
                return new Polynom((from fNode in f.nodes
                                    from gNode in g.nodes
                                    select new Node(fNode.GetK() * gNode.GetK(), MultiplyPowers(fNode.GetCopiedDict(), gNode.GetCopiedDict()))).ToList());
            }
        }

        public class Node
        {
            double k = 0;
            Dictionary<string, int> letterPower = new Dictionary<string, int>();

            public int GetPower(string letter = "x") => letterPower[letter];
            public void SetPower(int power, string letter = "x") => letterPower[letter] = power;

            public double GetK() => k;
            public void SetK(double k) => this.k = k;

            public string GetLetter() => letterPower.Keys.ElementAt(0);
            public void SetLetter(string letter) => letterPower.Add(letter, 0);

            public int GetFullPower() => letterPower.Values.Sum();

            public Dictionary<string, int> GetDict() => letterPower;
            public Dictionary<string, int> GetCopiedDict() => letterPower.ToDictionary(k => k.Key, k => k.Value);

            public Node() { }
            public Node(double k, Dictionary<string, int> dict)
            {
                this.k = k;
                letterPower = dict;
            }
            public Node(double k, int power, string letter = "x")
            {
                this.k = k;
                letterPower.Add(letter, power);
            }
        }
        
        public static int StringToInt(string str) => System.Convert.ToInt32(str);
        public static double StringToDouble(string str) => System.Convert.ToDouble(str.Replace(".", ","));

        Polynom p = new Polynom();
        Polynom f = new Polynom();
        Polynom result = new Polynom();

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                double k = StringToDouble(textBox1.Text);
                int power = StringToInt(textBox2.Text);

                var temp = (power == 0)
                ? new Node(k, 1, "")
                : new Node(k, power);

                if (radioButton1.Checked)
                {
                    p.Add(temp);
                    textBox3.Text = p.GetRepresentation();
                }

                if (radioButton2.Checked)
                {
                    f.Add(temp);
                    textBox4.Text = f.GetRepresentation();
                }

                if (radioButton3.Checked)
                {
                    p.Add(temp);
                    f.Add(temp);
                    textBox3.Text = p.GetRepresentation();
                    textBox4.Text = f.GetRepresentation();
                }
            }
            catch (Exception exep)
            {
                MessageBox.Show(exep.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            result.Clear();
            result = p + f;
            textBox5.Text = result.GetRepresentation();
            textBox6.Text = "+";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            result.Clear();
            result = p * f;
            textBox5.Text = result.GetRepresentation();
            textBox6.Text = "*";
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                p.Clear();
                p = new Polynom(textBox3.Text);
                textBox3.Text = p.GetRepresentation();
            }

            if (radioButton2.Checked)
            {
                f.Clear();
                f = new Polynom(textBox4.Text);
                textBox4.Text = f.GetRepresentation();
            }

            if (radioButton3.Checked)
            {
                p.Clear();
                f.Clear();
                p = new Polynom(textBox3.Text);
                f = new Polynom(textBox4.Text);
                textBox3.Text = p.GetRepresentation();
                textBox4.Text = f.GetRepresentation();
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
                f = new Polynom(p.GetNodes());

            if (radioButton2.Checked)
                p = new Polynom(f.GetNodes());

            if (radioButton3.Checked)
            {
                var temp = new Polynom(f.GetNodes());
                f = new Polynom(p.GetNodes());
                p = new Polynom(temp.GetNodes());
            }

            textBox3.Text = p.GetRepresentation();
            textBox4.Text = f.GetRepresentation();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (openFileDialog1.ShowDialog() == DialogResult.Cancel) return;
                string text = System.IO.File.ReadAllText(openFileDialog1.FileName);

                string[] split = Regex.Split(text, @"\s+");

                Polynom temp;

                if (new Regex("[a-zA-Z]").Match(text).Length > 0)
                    temp = new Polynom(string.Join(" ", split));
                else
                    temp = new Polynom(Enumerable.Range(0, split.Skip(1).Count()).Select(i =>
                        new Node(StringToDouble(split.ElementAt(i + 1)),
                                (StringToInt(split.ElementAt(0)) - i == 0)
                                    ? 1
                                    : StringToInt(split.ElementAt(0)) - i,
                                (StringToInt(split.ElementAt(0)) - i == 0)
                                    ? ""
                                    : "x")).ToList());

                if (radioButton1.Checked)
                {
                    p = new Polynom(temp.GetNodes());

                    textBox3.Text = p.GetRepresentation();
                }

                if (radioButton2.Checked)
                {
                    f = new Polynom(temp.GetNodes());

                    textBox4.Text = f.GetRepresentation();
                }

                if (radioButton3.Checked)
                {
                    p = new Polynom(temp.GetNodes());
                    f = new Polynom(temp.GetNodes());

                    textBox3.Text = p.GetRepresentation();
                    textBox4.Text = f.GetRepresentation();
                }
            }
            catch (Exception exep)
            {
                MessageBox.Show(exep.Message);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.Cancel) return;

            string filename = saveFileDialog1.FileName;
            string text = "";

            if (radioButton1.Checked)
                text = p.GetRepresentation();

            if (radioButton2.Checked)
                text = f.GetRepresentation();

            if (radioButton3.Checked)
                text = ((p.GetRepresentation() != "0" ? ("(" + p.GetRepresentation() + ")") : "") + " " + textBox6.Text + " " + (f.GetRepresentation() != "0" ? ("(" + f.GetRepresentation() + ")") : "") + " = " + result.GetRepresentation()).Trim();

            System.IO.File.WriteAllText(filename, text);
        }
    }
}

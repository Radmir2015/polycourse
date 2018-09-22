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
                Regex regex = new Regex(@"([\+-]?\d*)\*?([a-zA-Z]?)\^?([\+-]?\d*)");

                MessageBox.Show(string.Join(";", unprocessedNodes));

                foreach (string node in unprocessedNodes)
                {
                    Node temp = new Node();
                    MatchCollection matches = regex.Matches(node);

                    for (int i = 0; i < matches.Count - 1; i++)
                    {
                        Match match = matches[i];

                        MessageBox.Show(match.Groups[1].Value + ";" + match.Groups[2].Value + ";" + match.Groups[3].Value);

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
                                            ? 0
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

            //public void Add(Node node) {

            //    if (nodes.Any(x => node.GetLetter() == x.GetLetter()) && nodes.Any(x => node.GetPower() == x.GetPower()))
            //    {
            //        nodes = nodes.Select(x =>
            //            (node.GetPower() == x.GetPower() && node.GetLetter() == x.GetLetter())
            //            ? new Node(x.GetK() + node.GetK(), x.GetPower(), x.GetLetter())
            //            : x)
            //            .Where(x => x.GetK() != 0)
            //            .ToList();
            //    }
            //    else
            //        nodes.Insert(nodes.TakeWhile(x => x.GetPower() > node.GetPower()).Count(), node);
            //    //nodes.Sort((a, b) => b.GetPower() - a.GetPower());
            //}

            bool CompareSets(Node aNode, Node bNode)
            {
                return CompareLetters(aNode, bNode)
                    && aNode.GetDict().Keys.All(x => aNode.GetPower(x) == bNode.GetPower(x));
                //return CompareLetters(aNode, bNode) && ComparePowers(aNode, bNode) == 0 &&
                //aNode.All(x => bNode.First(y => y.GetLetter() == x.GetLetter()).GetPower() == x.GetPower());
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
                
                if (nodes.Any(x => CompareLetters(node, x)) && nodes.Any(x => ComparePowers(node, x) == 0))
                {
                    for (int i = 0; i < nodes.Count(); i++)
                    {
                        if (CompareSets(nodes[i], node))
                        {
                            //nodes[i] = new List<Node> { };
                            //nodes[i].ForEach(x => new Node(node.First(y => y.GetPower() == x.GetPower())));
                            nodes[i].SetK(nodes[i].GetK() + node.GetK());
                            //nodes[i][0].SetK(node[0].GetK() + nodes[i][0].GetK());
                        }
                    }

                    nodes = nodes.Where(x => x.GetK() != 0).ToList();

                    //nodes = nodes.Select(x =>
                    //    CompareSets(x, node)
                    //    ? new List<Node> { new Node(x.GetK() + node.ElementAt(0).GetK(), x.GetPower(), x.GetLetter()) }
                    //    : x)
                    //    .Where(x => x.GetK() != 0)
                    //    .ToList();
                }
                else
                    nodes.Insert(nodes.TakeWhile(x => ComparePowers(x, node) > 0).Count(), node);
                //nodes.Sort((a, b) => b.GetPower() - a.GetPower());
            }

            public void Add(List<Node> nds) => nds.ForEach(x => Add(x));

            public void Delete(Node node) => nodes.Remove(node);
            //public void DeleteByPower(Node node) => nodes.RemoveAll(x => x.GetPower() == node.GetPower());
            public void Clear() => nodes.Clear();
            public int AmountOfSums() => nodes.Count();

            //public string GetRepresentation() =>
            //    string.Join(" + ", nodes.Select(x =>
            //        ((x.GetK() == 1 && x.GetPower() != 0) ? "" : (x.GetK() == 1 ? "1" : x.GetK().ToString())) +
            //        ((x.GetPower() != 0)
            //        ? (x.GetLetter() + (x.GetPower() == 1 ? "" : ("^" + x.GetPower().ToString())))
            //        : ""))).Replace("+ -", "- ").Replace(",", ".");
            public string GetRepresentation()
            {
                return string.Join(" + ", nodes.Select(x =>
                    x.GetK() + string.Join(" * ", x.GetDict().Select(y => y.Key + "^" + y.Value))
                ));
            }

            public bool IsEmpty() => nodes.Count() == 0;

            public static Polynom operator+(Polynom f, Polynom g)
            {
                //return new Polynom(f.nodes.Select(x =>
                //!g.nodes.FirstOrDefault(y => y.GetPower() == x.GetPower()).Equals(default(Node))
                //? new Node(x.GetK() + g.nodes.FirstOrDefault(y => y.GetPower() == x.GetPower()).GetK(), x.GetPower())
                //: x).ToList());

                Polynom res = new Polynom();
                res.Add(f.nodes);
                res.Add(g.nodes);
                return res;
            }

            //public static Polynom operator*(Polynom f, Polynom g)
            //{
            //    return new Polynom((from fNode in f.nodes
            //                       from gNode in g.nodes
            //                       select new Node(fNode.GetK() * gNode.GetK(), fNode.GetPower() + gNode.GetPower())).ToList());
            //}
        }

        public class Node
        {
            //int power = 0;
            double k = 0;
            Dictionary<string, int> letterPower = new Dictionary<string, int>();
            //string letter = "x";

            public int GetPower(string letter = "x") => letterPower[letter];
            public void SetPower(int power, string letter = "x") => letterPower[letter] = power;

            public double GetK() => k;
            public void SetK(double k) => this.k = k;

            public string GetLetter() => letterPower.Keys.ElementAt(0);
            public void SetLetter(string letter) => letterPower.Add(letter, 0);

            public Dictionary<string, int> GetDict() => letterPower;

            public Node() { }

            public Node(double k, int power, string letter = "x")
            {
                this.k = k;
                //this.power = power;
                //this.letter = letter;
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

                Node n = new Node(k, power);
                p.Add(new List<Node> { n });

                textBox3.Text = p.GetRepresentation();
            }
            catch (Exception exep)
            {
                MessageBox.Show(exep.Message);
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                double k = StringToDouble(textBox1.Text);
                int power = StringToInt(textBox2.Text);

                Node n = new Node(k, power);
                f.Add(new List<Node> { n });

                textBox4.Text = f.GetRepresentation();
            }
            catch (Exception exep)
            {
                MessageBox.Show(exep.Message);
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            //MessageBox.Show(p.GetRepresentation());
            //MessageBox.Show(f.GetRepresentation());
            result.Clear();
            result = p + f;
            textBox5.Text = result.GetRepresentation();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //result = p * f;
            textBox5.Text = result.GetRepresentation();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            p.Clear();
            p = new Polynom(textBox3.Text);
            textBox3.Text = p.GetRepresentation(); //match.Groups[1].Value + " " + match.Groups[2].Value + " " + match.Groups[3].Value;

        }

        private void button6_Click(object sender, EventArgs e)
        {
            f.Clear();
            f = new Polynom(textBox4.Text);
            textBox4.Text = f.GetRepresentation();
        }
    }
}

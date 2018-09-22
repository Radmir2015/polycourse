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
            private List<List<Node>> nodes = new List<List<Node>>();

            public Polynom() {}
            public Polynom(List<Node> node) => Add(node);
            public Polynom(List<List<Node>> nds) => nds.ForEach(x => Add(x));
            public Polynom(string expression)
            {
                string[] unprocessedNodes = Regex.Split(Regex.Replace(expression, @"(?<=[0-9a-zA-Z]+)([\+-])", " $1"), @"(?<=[0-9a-zA-Z]+)\s+(?=[\+-])").Select(x => x.Trim()).ToArray();
                unprocessedNodes = unprocessedNodes.Select(x => Regex.Replace(x, @"\s+", "")).ToArray();
                
                //Regex regex = new Regex(@"([\+-]?(?:\s*)\d*)\s*\*?\s*([a-zA-Z]?)\s*\^?\s*([\+-]?(?:\s*)\d*)");
                Regex regex = new Regex(@"([\+-]?\d*)\*?([a-zA-Z]?)\^?([\+-]?\d*)");

                MessageBox.Show(string.Join(";", unprocessedNodes));

                foreach (string node in unprocessedNodes)
                {
                    List<Node> temp = new List<Node>();
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
                        temp.Add(new Node((match.Groups[1].Value == "" || match.Groups[1].Value == "+") ? 1 : (match.Groups[1].Value == "-" ? -1 : StringToDouble(match.Groups[1].Value)), match.Groups[3].Value == "" ? (match.Groups[2].Value == "" ? 0 : (match.Groups[2].Value == "-" ? -1 : 1)) : StringToInt(match.Groups[3].Value), match.Groups[2].Value));
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

            bool CompareSets(List<Node> aNode, List<Node> bNode)
            {
                return CompareLetters(aNode, bNode) && ComparePowers(aNode, bNode) == 0 &&
                aNode.All(x => bNode.First(y => y.GetLetter() == x.GetLetter()).GetPower() == x.GetPower());
            }

            bool CompareLetters(List<Node> aNode, List<Node> bNode)
            {
                HashSet<string> aLetters = new HashSet<string>();
                HashSet<string> bLetters = new HashSet<string>();
                aNode.ForEach(x => aLetters.Add(x.GetLetter()));
                bNode.ForEach(x => bLetters.Add(x.GetLetter()));

                return aLetters.SetEquals(bLetters);
            }

            int ComparePowers(List<Node> aNode, List<Node> bNode)
            {
                return aNode.Sum(x => x.GetPower()) - bNode.Sum(x => x.GetPower());
            }

            public void Add(List<Node> node)
            {
                
                if (nodes.Any(x => CompareLetters(node, x)) && nodes.Any(x => ComparePowers(node, x) == 0))
                {
                    for (int i = 0; i < nodes.Count(); i++)
                    {
                        if (CompareSets(nodes[i], node))
                        {
                            //nodes[i] = new List<Node> { };
                            //nodes[i].ForEach(x => new Node(node.First(y => y.GetPower() == x.GetPower())));
                            nodes[i][0].SetK(node[0].GetK() + nodes[i][0].GetK());
                        }
                    }

                    nodes = nodes.Where(x => x[0].GetK() != 0).ToList();

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

            public void Add(List<List<Node>> nds) => nds.ForEach(x => Add(x));

            public void Delete(List<Node> node) => nodes.Remove(node);
            //public void DeleteByPower(Node node) => nodes.RemoveAll(x => x.GetPower() == node.GetPower());
            public void Clear() => nodes.Clear();

            //public string GetRepresentation() =>
            //    string.Join(" + ", nodes.Select(x =>
            //        ((x.GetK() == 1 && x.GetPower() != 0) ? "" : (x.GetK() == 1 ? "1" : x.GetK().ToString())) +
            //        ((x.GetPower() != 0)
            //        ? (x.GetLetter() + (x.GetPower() == 1 ? "" : ("^" + x.GetPower().ToString())))
            //        : ""))).Replace("+ -", "- ").Replace(",", ".");
            public string GetRepresentation()
            {
                return string.Join(" + ", nodes.Select(x =>
                    x[0].GetK() + string.Join(" * ", x.Select(y => y.GetLetter() + "^" + y.GetPower()))
                ));
            }

            public bool IsEmpty() => nodes.Count() == 0;

            public static Polynom operator+(Polynom f, Polynom g)
            {
                //return new Polynom(f.nodes.Select(x =>
                //!g.nodes.FirstOrDefault(y => y.GetPower() == x.GetPower()).Equals(default(Node))
                //? new Node(x.GetK() + g.nodes.FirstOrDefault(y => y.GetPower() == x.GetPower()).GetK(), x.GetPower())
                //: x).ToList());

                Polynom res = new Polynom(f.nodes);
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
            int power = 0;
            double k = 0;
            string letter = "x";

            public int GetPower() => power;
            public void SetPower(int power) => this.power = power;

            public double GetK() => k;
            public void SetK(double k) => this.k = k;

            public string GetLetter() => letter;
            public void SetLetter(string letter) => this.letter = letter;

            public Node(double k, int power, string letter = "x")
            {
                this.k = k;
                this.power = power;
                this.letter = letter;
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

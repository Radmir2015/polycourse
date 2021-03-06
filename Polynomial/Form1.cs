﻿using System;
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
            public Polynom(string expression, bool calculatingPowers = true)
            {
                string[] unprocessedNodes = Regex.Split(Regex.Replace(expression, @"(?<=[0-9a-zA-Z]+)([\+-])", " $1"), @"(?<=[0-9a-zA-Z]+)\s+(?=[\+-])").Select(x => x.Trim()).ToArray();
                unprocessedNodes = unprocessedNodes.Select(x => Regex.Replace(x, @"\s+", "")).ToArray();
                
                Regex regex = new Regex(@"([\+-]?\d*\.?\d*)\*?([a-zA-Z]?)\^?([\+-]?\d*)");

                foreach (string node in unprocessedNodes)
                {
                    Node temp = new Node();
                    MatchCollection matches = regex.Matches(node);

                    for (int i = 0; i < matches.Count - 1; i++)
                    {
                        Match match = matches[i];

                        if (match.Groups[1].Value == "" && match.Groups[2].Value == "" || unprocessedNodes.Any(x => x == ""))
                        {
                            MessageBox.Show("Parse error");
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

                        if (match.Groups[2].Value != "" && match.Groups[3].Value == "0")
                        {
                            if (temp.GetDict().Count > 0)
                                continue;
                            tempLetter = "";
                            tempPower = 0;
                        }

                        if (calculatingPowers && match.Groups[2].Value == "")
                        {
                            tempK = Math.Sign(tempK) * Math.Pow(Math.Abs(tempK), tempPower);
                            tempPower = 0;
                        }

                        if (temp.GetDict().Count == 0)
                            temp = new Node(tempK, tempPower, tempLetter);
                        else
                        {
                            if (temp.GetDict().Count == 1 && temp.GetDict().ContainsKey("") && temp.GetDict()[""] == 0)
                                temp.GetDict().Remove("");
                            temp.GetDict()[tempLetter] = tempPower;
                        }
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
                if (nodes.Any(x => CompareSets(node, x)))
                {
                    Node found = nodes.First(x => CompareSets(node, x));
                    found.SetK(found.GetK() + node.GetK());
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
                        (Math.Abs(x.GetK()) != 1 ? ((decimal)x.GetK()).ToString() : (x.GetK() < 0 ? "-" : ""))
                        + string.Join(" * ", x.GetDict().Select(y =>
                            ((y.Key == "" && Math.Abs(x.GetK()) == 1)
                            ? Math.Abs(x.GetK()).ToString()
                            : "") +
                            y.Key + ((y.Value != 1 && y.Value != 0)
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
                    if (moreLetters[x.Key] == 0) moreLetters.Remove(x.Key);
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

        private bool isUnary = false;
        private string lastOperation = "";
        private int unaryPolynom = 0;

        private List<string> prepareLetters()
        {
            List<string> temp = new HashSet<string>(
                        (radioButton1.Checked ? p : radioButton2.Checked ? f : result)
                        .GetNodes().Select(x =>
                            x.GetCopiedDict().Keys.ToList())
                            .SelectMany(i => i).Where(x => x != "")).ToList();
            label7.Text = "(" + string.Join(", ", temp) + ") =";

            return temp;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                double k = StringToDouble(textBox1.Text);
                int power = StringToInt(textBox2.Text);

                var temp = (power == 0)
                ? new Node(k, 0, "")
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

                prepareLetters();
            }
            catch (Exception exep)
            {
                MessageBox.Show(exep.Message, "Добавление узла");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            result.Clear();
            if (radioButton1.Checked)
            {
                isUnary = true;
                unaryPolynom = 1;
                result = new Polynom(new Node(1, 0, "")) * p;
                textBox6.Text = "+ (унарный)";
            }
            if (radioButton2.Checked)
            {
                isUnary = true;
                unaryPolynom = 2;
                result = new Polynom(new Node(1, 0, "")) * f;
                textBox6.Text = "+ (унарный)";
            }
            if (radioButton3.Checked)
            {
                isUnary = false;
                unaryPolynom = 0;
                result = p + f;
                textBox6.Text = "+ (бинарный)";
            }
            lastOperation = "+";
            prepareLetters();
            textBox5.Text = result.GetRepresentation();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            result.Clear();
            if (radioButton1.Checked)
            {
                isUnary = true;
                unaryPolynom = 1;
                result = new Polynom(new Node(-1, 0, "")) * p;
                textBox6.Text = "- (унарный)";
            }
            if (radioButton2.Checked)
            {
                isUnary = true;
                unaryPolynom = 2;
                result = new Polynom(new Node(-1, 0, "")) * f;
                textBox6.Text = "- (унарный)";
            }
            if (radioButton3.Checked)
            {
                isUnary = false;
                unaryPolynom = 0;
                result = p + new Polynom(new Node(-1, 0, "")) * f;
                textBox6.Text = "- (бинарный)";
            }
            lastOperation = "-";
            prepareLetters();
            textBox5.Text = result.GetRepresentation();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            result.Clear();
            result = p * f;
            prepareLetters();
            textBox5.Text = result.GetRepresentation();
            textBox6.Text = "* (бинарный)";
            lastOperation = "*";
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

            prepareLetters();
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

            prepareLetters();

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

                prepareLetters();
            }
            catch (Exception exep)
            {
                MessageBox.Show(exep.Message, "Открытие файла");
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
            {
                if (isUnary)
                    text = lastOperation + "(" + ((unaryPolynom == 1)
                            ? p.GetRepresentation()
                            : (unaryPolynom == 2)
                            ? f.GetRepresentation() : "") + ") = " + result.GetRepresentation();
                else
                    text = ((p.GetRepresentation() != "0" ? ("(" + p.GetRepresentation() + ")") : "") + " " + lastOperation + " " + (f.GetRepresentation() != "0" ? ("(" + f.GetRepresentation() + ")") : "") + " = " + result.GetRepresentation()).Trim();
            }

            System.IO.File.WriteAllText(filename, text);
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            prepareLetters();
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            prepareLetters();
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            prepareLetters();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            try
            {
                Polynom temp = radioButton1.Checked ? p : radioButton2.Checked ? f : result;
                temp.DeleteByPower(StringToInt(textBox8.Text));
            }
            catch (Exception exep)
            {
                MessageBox.Show(exep.Message, "Удаление узлов по степени");
            }
            if (radioButton1.Checked)
                textBox3.Text = p.GetRepresentation();

            if (radioButton2.Checked)
                textBox4.Text = f.GetRepresentation();

            if (radioButton3.Checked)
                textBox5.Text = result.GetRepresentation();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            if (radioButton3.Checked)
            {
                p.Clear();
                f.Clear();
            }
            else
                (radioButton1.Checked ? p : f).Clear();

            textBox3.Text = p.GetRepresentation();
            textBox4.Text = f.GetRepresentation();
            textBox5.Text = result.GetRepresentation();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            result.Clear();

            textBox5.Text = result.GetRepresentation();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            Polynom temp = radioButton1.Checked ? p : radioButton2.Checked ? f : result;
            string answer = "f" + label7.Text + " " + temp.GetRepresentation() + "\n";

            try
            {

                List<string> letters = prepareLetters();
                List<double> nums = textBox7.Text.Split(' ').Select(StringToDouble).ToList();

                if (letters.Count() > nums.Count()) return;

                answer += "f(" + string.Join(", ", nums) + ") = " + letters.Aggregate(temp.GetRepresentation(), (x, y) =>
                        x.Replace(y, "(" + nums[letters.IndexOf(y)].ToString() + ")")) + " = ";

                answer += temp.GetNodes().Select(x =>
                        x.GetK() * x.GetCopiedDict().Select(kv =>
                            kv.Key == ""
                            ? 1
                            : Math.Pow(nums[letters.IndexOf(kv.Key)], kv.Value)).Sum()).Sum().ToString();

                MessageBox.Show(answer, "Значение функции в точке");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ввод аргументов функции");
            }
        }
    }
}

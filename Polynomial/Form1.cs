using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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

            public Polynom() {}
            public Polynom(Node node) => Add(node);
            public Polynom(List<Node> nds) => nds.ForEach(x => Add(x));

            public void Add(Node node) {
                if (nodes.Any(x => node.GetPower() == x.GetPower()))
                {
                    nodes = nodes.Select(x =>
                        (node.GetPower() == x.GetPower())
                        ? new Node(x.GetK() + node.GetK(), x.GetPower())
                        : x)
                        .Where(x => x.GetK() != 0)
                        .ToList();
                }
                else
                    nodes.Insert(nodes.TakeWhile(x => x.GetPower() > node.GetPower()).Count(), node);
                //nodes.Sort((a, b) => b.GetPower() - a.GetPower());
            }

            public void Add(List<Node> nds) => nds.ForEach(x => Add(x));

            public void Delete(Node node) => nodes.Remove(node);
            public void DeleteByPower(Node node) => nodes.RemoveAll(x => x.GetPower() == node.GetPower());

            public string GetRepresentation(string letter = "x") =>
                string.Join(" + ", nodes.Select(x =>
                    x.GetK().ToString() +
                    ((x.GetPower() != 0)
                    ? letter + "^" + x.GetPower().ToString()
                    : ""))).Replace("+ -", "- ").Replace(",", ".");

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

            public static Polynom operator*(Polynom f, Polynom g)
            {
                return new Polynom((from fNode in f.nodes
                                   from gNode in g.nodes
                                   select new Node(fNode.GetK() * gNode.GetK(), fNode.GetPower() + gNode.GetPower())).ToList());
            }
        }

        public class Node
        {
            int power = 0;
            double k = 0;

            public int GetPower() => power;
            public void SetPower(int power) => this.power = power;

            public double GetK() => k;
            public void SetK(double k) => this.k = k;

            public Node(double k, int power)
            {
                this.k = k;
                this.power = power;
            }
        }
        
        public int StringToInt(string str) => System.Convert.ToInt32(str);
        public double StringToDouble(string str) => System.Convert.ToDouble(str.Replace(".", ","));

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
                p.Add(n);

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
                f.Add(n);

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
            result = p * f;
            textBox5.Text = result.GetRepresentation();
        }
    }
}

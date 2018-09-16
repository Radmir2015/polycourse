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

            public void add(Node node) => nodes.Add(node);
            public void delete(Node node) => nodes.Remove(node);
            public void deleteByPower(Node node) => nodes.RemoveAll(x => x.power == node.power);

            public string getRepresentation(string letter = "x") =>
                string.Join(" + ", nodes.Select(x => x.k.ToString() + letter + "^" + x.power.ToString()));
        }

        public class Node
        {
            public int power = 0;
            public double k = 0;

            public Node(double k, int power)
            {
                this.k = k;
                this.power = power;
            }
        }
        
        public int stringToInt(string str) => System.Convert.ToInt32(str);

        Polynom p = new Polynom();

        private void button1_Click(object sender, EventArgs e)
        {
            Node n = new Node(stringToInt(textBox1.Text), stringToInt(textBox2.Text));
            p.add(n);

            textBox3.Text = p.getRepresentation();
        }
    }
}

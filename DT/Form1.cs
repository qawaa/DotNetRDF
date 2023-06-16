using System.Data;
using System.Drawing.Imaging;
using System.Xml.Linq;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Inference;
using VDS.RDF.Writing;

namespace DT
{
    public partial class Form1 : Form
    {
        DTree dtree; 
        string fileName = "";
        public Form1()
        {
            InitializeComponent();
            textBox1.Text = "одно";
            textBox2.Text = "-1";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Обработка сообщения от кнопки "Загрузка ДД".
            // Загрузка ДД с запросом имени файла и отображение.
            // Вводятся заданные в окне имя корня и глубина.
            string current_directory = Directory.GetCurrentDirectory();

            int sn = 1;
           
            openFileDialog1.FileName = "";
            openFileDialog1.Filter = "(*.ttl)| *.ttl";

            if (openFileDialog1.ShowDialog() ==
                DialogResult.OK && openFileDialog1.FileName.Length > 0)
            {
                fileName = openFileDialog1.FileName;
                Directory.SetCurrentDirectory(current_directory);
            }
            else return;

            dtree = new DTree();

            // Загрузка дерева (графа) в формате turtle из файла.
            FileLoader.Load(dtree, fileName);

            show_tree();
        }

        public void Построение_дерева_с_заданной_вершиной_заданной_глубины(TreeNode node, int глубина, int sn)
        {
            // Построение дерева treeView
            if (node == null) return;

            // Выборка всех триплетов с субъектом node
            IUriNode uri = dtree.CreateUriNode(":" + node.Text);
            IEnumerable<Triple> triples = dtree.GetTriplesWithSubject(uri);

            foreach (Triple triple in triples)
            {
                string? notion = dtree.DName(triple.Object);
                string? predicate = dtree.DName(triple.Predicate);
                string? subject = dtree.DName(triple.Subject);
                
                if (глубина > 0 || глубина < 0)
                {
                    TreeNode дочерняя_node = new TreeNode("", 0, 0);
                    дочерняя_node.Nodes.Clear();
                    дочерняя_node.Text = notion;

                    Построение_дерева_с_заданной_вершиной_заданной_глубины(дочерняя_node, глубина - 1, sn + 1);
                    //дочерняя_node.Text += " ← <" + predicate + ">. " + subject + "          " + sn.ToString();
                    дочерняя_node.Text = subject + ".<" + predicate + "> → " + дочерняя_node.Text +"          " + sn.ToString();
                    //дочерняя_node.Text += " = <" + predicate + ">. " + subject;
                    node.Nodes.Add(дочерняя_node);

                }
            }

        }

        public void show_tree()
        {
            // Загрузка ДД из введенного в диалоге пути к файлу и отображение ДД.
            int sn = 1;

            if (fileName == "") return;
            FileLoader.Load(dtree, fileName);

            // Отображение через TreeView
            treeView1.BeginUpdate();
            treeView1.Nodes.Clear();
            //string имя_вершины = "одно";
            string имя_вершины = textBox1.Text;
            //int глубина = -1;
            int глубина = int.Parse(textBox2.Text);
            TreeNode node = new TreeNode("", 0, 0);
            node.Text = имя_вершины;

            Построение_дерева_с_заданной_вершиной_заданной_глубины(node, глубина, sn + 1);
            //Построение_дерева_с_заданной_вершиной_заданной_глубины(node, -1);
            treeView1.Nodes.Add(node);
            treeView1.ExpandAll();
            treeView1.EndUpdate();
            Show();
        }
       
        private void button2_Click_1(object sender, EventArgs e)
        {
            // Обработка сообщения от кнопки "Поддерево ДД".
            // Отображение поддерева ДД из файла с ранее введенным именем.
            // Вводятся заданные в окне имя корня и глубина.
            show_tree();
        }
    }
}
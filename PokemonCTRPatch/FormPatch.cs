using System;
using System.IO;
using System.Windows.Forms;

namespace PokemonCTR
{
    public partial class FormPatch : Form
    {
        public FormPatch()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Title = "原始ROM",
                Filter = "Nintendo DS ROM文件|*.nds|所有文件|*.*"
            };
            ofd.ShowDialog();
            textBox1.Text = ofd.FileName;
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Title = "补丁包",
                Filter = "补丁包|*.zip|所有文件|*.*"
            };
            ofd.ShowDialog();
            textBox2.Text = ofd.FileName;
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog
            {
                Title = "输出ROM",
                Filter = "Nintendo DS ROM文件|*.nds"
            };
            sfd.ShowDialog();
            textBox3.Text = sfd.FileName;
        }

        private void TextBox_DragDrop(object sender, DragEventArgs e)
        {
            string filePath = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
            if (CheckIfFileExists(filePath))
            {
                ((TextBox)sender).Text = filePath;
            }
        }

        private void TextBox_DragEnter(object sender, DragEventArgs e)
        {

            if (e.Data.GetDataPresent(DataFormats.FileDrop) && ((Array)e.Data.GetData(DataFormats.FileDrop)).Length == 1)
            {
                e.Effect = DragDropEffects.Link;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }
    }
}

using System;
using System.Windows.Forms;

namespace HeicToJpgShell
{
    public partial class OverwriteDialog : Form
    {
        public bool Overwrite { get; private set; }
        public bool ApplyToAll { get; private set; }

        public OverwriteDialog(string fileName)
        {
            InitializeComponent();
            lblMessage.Text = L10n.Format("OverwriteMsg", fileName);
        }

        private void BtnYes_Click(object sender, EventArgs e)
        {
            Overwrite = true;
            ApplyToAll = chkApplyToAll.Checked;
            this.DialogResult = DialogResult.Yes;
            this.Close();
        }

        private void BtnNo_Click(object sender, EventArgs e)
        {
            Overwrite = false;
            ApplyToAll = chkApplyToAll.Checked;
            this.DialogResult = DialogResult.No;
            this.Close();
        }
    }
}

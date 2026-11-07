using LOGIN_E;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LOGIN_E
{
    public partial class LoginMainForm : Form
    {
        private Form _current = null;

        public LoginMainForm()
        {
            InitializeComponent();
            pnlHost.Dock = DockStyle.Fill;
            ShowSignIn();
        }

        private void ShowChild(Form child)
        {
            if (_current != null)
            {
                _current.Close();
                _current.Dispose();
                _current = null;
            }

            _current = child;

            child.TopLevel = false;
            child.FormBorderStyle = FormBorderStyle.None;
            child.Dock = DockStyle.Fill;

            pnlHost.Controls.Clear();
            pnlHost.Controls.Add(child);
            child.Show();
        }

        private void ShowSignIn()
        {
            var f = new Login();

            f.ForgotPasswordClicked += () => ShowForgot();
            f.RegisterClicked += () => ShowRegister();

            f.LoginSuccess += (nextForm) =>
            {
                this.Hide();           // ẩn LoginMainForm
                nextForm.FormClosed += (_, __) =>
                {
                    this.Show();       // đóng role-form thì hiện login lại (tuỳ bạn)
                    ShowSignIn();      // reset về SignIn sạch sẽ
                };
                nextForm.Show();
            };
            ShowChild(f);
        }

        private void ShowForgot()
        {
            var f = new ForgotPassword();
            f.BackToSignin += () => ShowSignIn();
            ShowChild(f);
        }

        private void ShowRegister()
        {
            var f = new RegisterCustomer();
            f.BackToSignin += () => ShowSignIn();
            ShowChild(f);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
            Dispose();
        }
    }
}

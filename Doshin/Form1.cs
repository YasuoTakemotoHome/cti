using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Doshin
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Cti1_Load(object sender, EventArgs e)
        {
            //cti1.start();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            /*
            CTIClient.CTIClient cti = new CTIClient.CTIClient();
            */
            /*cti.oct1 = 192;
            cti.oct2 = 168;
            cti.oct3 = 100;
            cti.oct4From = 10;
            cti.oct4To = 20;*/
            CTIService.CTI cti = new CTIService.CTI();


            cti.start();





        }

        private void Button1_Click(object sender, EventArgs e)
        {
            /*
            CTIClient.CTIClient cti = new CTIClient.CTIClient();
            cti.port = 2001;
            */

        }
    }
}

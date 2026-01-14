using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Spire.Pdf;

using System.Runtime.InteropServices;


namespace AnyPrintConsole
{
    public partial class Form1 : Form


    {
        private Process onScreenKeyboardproc;
        string ffile;
        int codelenght;
        string filePath;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //string[] files = Directory.GetFiles("C:/AnyPrintFolder/FilesToPrint");
            //foreach (string file in files) 
            //{
            //    string filename = Path.GetFileName(file); ;
            //    textBox3.Text = textBox3.Text +"\r\n"+ filename; 
            //}
          
            
            textBox4.Text = "";


            string code6 = textBox2.Text;
           codelenght = code6.Length; 

            

            if ( codelenght == 6)
            {
                //string[] files = Directory.GetFiles("C:/AnyPrintFolder/FilesToPrint", textBox1.Text  + "*.pdf");
                string[] files = Directory.GetFiles("C:/AnyPrintFolder/FilesToPrint", textBox2.Text + "*.pdf");
                foreach (string file in files)
                {
                    string filename = Path.GetFileName(file); ;
                    //ffile = "\r\n"+ filename; 
                    ffile = filename;

                }
              
            }
           


            if (ffile != null)
                            {
                filePath = @"C:/AnyPrintFolder/FilesToPrint/" + ffile;
                textBox4.Text = ffile.Remove(0, 7);
                textBox4.Select();
                               
            }
            else

            {
                MessageBox.Show("The Print Code is NOT VALID");
            }

        }

        private void CallKeyboard()
        {
            // TRo Close All Open On Screen ketboard
            Process[] oslProcessesArry = Process.GetProcessesByName("TabTip");
            foreach (Process onScreenProcess in oslProcessesArry)
            {
                onScreenProcess.Kill();
            }

            //To Open keyboard

            string progFiles = @"C:\Program Files\Common Files\microsoft shared\ink";
            string onScreenKeyboardPath = System.IO.Path.Combine(progFiles, "TabTip.exe");
            onScreenKeyboardproc = System.Diagnostics.Process.Start(onScreenKeyboardPath);
        }

       

        private void textBox2_Enter(object sender, EventArgs e)
        {
            CallKeyboard();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            BackColor = Color.FromArgb(230, 226, 223);
        }


        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox4.Text !="")
            {             //Create PdfDocument object
                PdfDocument pdf = new PdfDocument();

                //Load a PDF file
                //pdf.LoadFromFile(@"C:\AnyPrintFolder\FilesToPrint\PDFCopy.pdf");
                //Directory.GetFiles("C:/AnyPrintFolder/FilesToPrint", textBox1.Text + "*.pdf");
                pdf.LoadFromFile(@"C:\AnyPrintFolder\FilesToPrint\" + ffile + "");


                //Print with default printer 
                pdf.Print();

                textBox2.Text = "";
                textBox4.Text = "";



               

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    //Console.WriteLine($"File '{filePath}' deleted successfully.");
                }

                ffile = null;
            }
          
        }

        private void textBox2_Leave(object sender, EventArgs e)
        {
            Process[] oslProcessesArry = Process.GetProcessesByName("TabTip");
            foreach (Process onScreenProcess in oslProcessesArry)
            {
                onScreenProcess.Kill();
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            foreach (System.Diagnostics.Process onProcess in System.Diagnostics.Process.GetProcessesByName("TabTip")) 
            {
                onProcess.Kill();
            }
        }

       
    }
}

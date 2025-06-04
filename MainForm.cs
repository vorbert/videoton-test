using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static GetID.GetID;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace VideotonTestApp
{
    public partial class MainForm : Form
    {
        delegate void SetTextCallback(string text, bool isError);
        private Thread formThread = null;

        GetID.GetID getId = new GetID.GetID();

        public MainForm()
        {
            InitializeComponent();

            getId.ValueChanged += GetIdValueChanged;
            getId.ErrorChanged += GetIdErrorChanged;
        }

        /**
         * Handling OnErrorChanged event in GetID class
         */
        private void GetIdErrorChanged(object sender, PropertyChangedEventArgs e)
        {
            // thread is added so I could access the Ui elements. Pls see further down!
            this.formThread = new Thread(new ThreadStart(this.addErrorToTextBox));

            this.formThread.Start();
        }

        /**
         * Handling OnValueChanged event in GetID class
         */
        private void GetIdValueChanged(object sender, PropertyChangedEventArgs e)
        {
            // thread is added so I could access the Ui elements. Pls see further down!
            this.formThread =  new Thread(new ThreadStart(this.addToTextBox));

            this.formThread.Start();
        }

        /**
         * Go button click event handling
         */
        private void goBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if(MsgTextBox.Text.Length > 0)
                {
                    MsgTextBox.Clear();
                    saveBtn.Enabled = false;
                }

                getId.Go();
                goBtn.Enabled = false;
                stopBtn.Enabled = true;

                if (getId.Running)
                {
                    MsgTextBox.AppendText("A GetId szolgáltatás elindult!\r\n");
                }
                else
                {
                    MsgTextBox.AppendText("A GetId szolgáltatás nem fut! Kérjük lépjen kapcsolatba az adminisztrátorral!\r\n");
                    getId.Stop();
                    goBtn.Enabled = true;
                    stopBtn.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                getId.Stop();
                goBtn.Enabled = true;
                stopBtn.Enabled = false;
                MsgTextBox.AppendText($"{ex.Message}\r\n");
            }
        }

        /**
         * Sopt button click event handlig
         */
        private void stopBtn_Click(object sender, EventArgs e)
        {
            this.stopGetIdRunning();
        }

        /**
         * created a reusable stop method
         */
        private void stopGetIdRunning()
        {
            getId.Stop();

            if (!getId.Running && stopBtn.Enabled)
            {
                MsgTextBox.AppendText("A GetId szolgáltatás leállt!\r\n");
                goBtn.Enabled = true;
                stopBtn.Enabled = false;
                saveBtn.Enabled = true;
            }
            else
            {
                MsgTextBox.AppendText("A GetId szolgáltatást nem sikerült leállítani! Kérjük lépjen kapcsolatba az adminisztrátorral!\r\n");
            }
        }

        /**
         * Handle the error text, and push it to the UI textbox element
         */
        private void addErrorToTextBox()
        {
            this.setText($"A GetId szolgáltatásban hiba lépett fel! Kérjük lépjen kapcsolatba az adminisztrátorral! \r\nHibakód: {getId.ErrorMessage}\r\n", false);
        }


        /**
         * Do the string quality verdict, also push the text to the UI textbox element
         */
        private void addToTextBox()
        {
            string pattern = @"(^[A-Z]{1})([0-9]{4})";
            string verdict;

            if(Regex.IsMatch(getId.Value, pattern))
            {
                verdict= "- MEGFELELŐ";
            } 
            else
            {
                verdict = "- NEM MEGFELELŐ";
            }

            this.setText($"{getId.Value} {verdict}\r\n", true);
        }

        /**
         * I added this method here so it could be called back, if needed. I did this, because I couldn't acces the UI elements 
         */
        private void setText(string text, bool isError)
        {
            if (this.MsgTextBox.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(setText);
                this.Invoke(d, new object[] { text, isError });
            }
            else
            {
                if(getId.Running)
                {
                    this.MsgTextBox.Text += text;
                    this.MsgTextBox.SelectionStart = this.MsgTextBox.Text.Length;

                    // hadling stop on error, or if we run out of storage space
                    if (this.MsgTextBox.Text.Length >= (this.MsgTextBox.MaxLength - 200) || !isError)
                    {
                        this.stopGetIdRunning();
                    }
                }                
            }
        }

        /**
         * Handleled save data to file button and saved string value from the MsgTextBox to .xtx file
         */
        private void saveBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if(MsgTextBox.Text.Length > 0)
                {
                    SaveFileDialog file = new SaveFileDialog();
                    file.FileName = "codes.txt";
                    file.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                    file.ShowDialog();

                    using (StreamWriter filewrite = new StreamWriter(file.FileName))
                    {
                        using (StringReader reader = new StringReader(MsgTextBox.Text))
                        {
                            string line;
                            while ((line = reader.ReadLine()) != null)
                            {
                                filewrite.WriteLine(line);
                            }
                            MessageBox.Show("Adatmentés sikeres!");
                        }
                    }   
                } 
                else
                {
                    MessageBox.Show("Hiba! Nincs elég adat a mentéshez!");
                }
            } catch (Exception ex)
            {
                MessageBox.Show($"Adat mentés közben hiba történt. \r\nError: {ex.Message}");
            }
        }
    }
}

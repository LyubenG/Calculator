using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Calculator
{
    public partial class Calculator : Form
    {
        bool comaClickedOnce = false;
        List<string> history = new List<string>();
        double memoryValue = 0;
        //Making the program movable. (only piece of code that is not mine, got it from stackoverflow)
        // https://stackoverflow.com/questions/1592876/make-a-borderless-form-movable
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        public Calculator()
        {
            InitializeComponent();
        }

        private void btnClear(object sender, EventArgs e)
        {
            //Clearing if there is any input.
            tbResult.Text = "0";
            disableEnableButtons(true, true, true);
        }

        private void btnOperators(object sender, EventArgs e)
        {

            //Checking which operator button was clicked and if the equesion is finished.
            Button buttonClicked = (Button)sender;
            string operation = buttonClicked.Text;
            comaClickedOnce = false;

            if (buttonClicked.Text.Equals("-") && tbResult.Text.Equals("0"))
            {
                tbResult.Text = "-";
                return;
            }

            if (tbResult.Text == "0" && operation != "√")
            {
                return;
            }

            //Disabling operator buttons if a button is already clicked.
            if (operation != "√")
            {
                //Checking if the square root button was clicked, if not we disable everything besides that button.
                disableEnableButtons(false, true, true);
            }
            else
            {
                disableEnableButtons(false, false, false);
            }

            //Changing how symbols if the square root symbol is used.
            if (buttonClicked.Text == "√" && tbResult.Text == "0")
            {
                tbResult.Text = $"{operation}";
            }
            else
            {
                if (buttonClicked.Text == "√")
                {
                    tbResult.Text = $"{tbResult.Text}{operation}";
                }
                else
                {
                    tbResult.Text = $"{tbResult.Text} {operation} ";
                }
            }
        }

        private void btnNumbers_Click(object sender, EventArgs e)
        {
            if (!comaClickedOnce) disableEnableButtons(true, true, true);
            else disableEnableButtons(true, true, false);
            //Removing default textbox value -> "0"
            if (tbResult.Text.Equals("0"))
            {
                tbResult.Text = "";
            }
            Button buttonClicked = (Button)sender;

            //Disabling the coma if it is pressed once so user can't add mulitple comas.
            if (buttonClicked.Text == ".")
            {
                comaClickedOnce = true;
                disableEnableButtons(false, false, false);
            }
            tbResult.Text = tbResult.Text + buttonClicked.Text;
        }

        private void btnEquals_Click(object sender, EventArgs e)
        {
            if (tbResult.Text.Length > 0)
            {
                double value = 0;
                string operation = "";
                //Getting all numbers and operations.
                string[] tokens = tbResult.Text.Split();

                double nextNumber = 0;
                for (int i = 0; i <= tokens.Length; i += 2)
                {
                    //Setting the very first number as a base value.
                    if (i == 0)
                        value = Calculate(tokens, i, 0);

                    if (i < tokens.Length - 1) operation = tokens[i + 1];
                    if (i + 2 <= tokens.Length)
                    {
                        //Getting the n-th value and then depending on the operation we * || + || / || - to the base value.
                        nextNumber = Calculate(tokens, i, i + 2);
                        switch (operation)
                        {
                            case "+": value += nextNumber; break;
                            case "-": value -= nextNumber; break;
                            case "÷": value /= nextNumber; break;
                            case "×": value *= nextNumber; break;
                        }
                    }
                }
                if (tbResult.Text.Contains("+") || tbResult.Text.Contains("-") || tbResult.Text.Contains("÷")
                    || tbResult.Text.Contains("×") || tbResult.Text.Contains("√"))
                    history.Add($"{tbResult.Text} = {value}");
                tbResult.Text = value.ToString();
                value = 0;
            }
        }

        private static double Calculate(string[] tokens, int i, int index)
        {
            double value;
            {
                //Checking if the equasion contains √ and then deciding which type is it (3√5 / √5 / √5√5 etc).
                //If there is no √ we just take the value.
                if (!tokens[index].Contains("√")) value = double.Parse(tokens[index]);
                else if (tokens[index].IndexOf("√") == 0 && tokens[index].IndexOf("√") == tokens[index].LastIndexOf("√"))
                {
                    value = Math.Sqrt(double.Parse(tokens[index].Substring(1)));
                }
                else
                {
                    if (tokens[index].IndexOf("√") != tokens[index].LastIndexOf("√"))
                    {
                        value = 1;
                        string[] sqrtTokens = tokens[index].Split(new string[] { "√" }, StringSplitOptions.RemoveEmptyEntries);
                        if (tokens[index].IndexOf("√") == 0)
                        {
                            for (int j = 0; j < sqrtTokens.Length; j++)
                            {
                                sqrtTokens[j] = Math.Sqrt(double.Parse(sqrtTokens[j])).ToString();
                                value *= double.Parse(sqrtTokens[j]);
                            }
                        }
                        else
                        {
                            int firstSqrt = tokens[index].IndexOf("√");
                            for (int j = firstSqrt; j < sqrtTokens.Length; j++)
                            {
                                sqrtTokens[j] = Math.Sqrt(double.Parse(sqrtTokens[j])).ToString();
                                value *= double.Parse(sqrtTokens[j]);
                            }
                            value *= double.Parse(sqrtTokens[0]);
                        }
                    }
                    else
                    {
                        int indexOfSqrt = tokens[index].IndexOf("√");
                        double firstValue = double.Parse(tokens[index].Substring(0, indexOfSqrt));
                        double secondValue = double.Parse(tokens[index].Substring(indexOfSqrt + 1));
                        value = firstValue * Math.Sqrt(secondValue);
                    }
                }
            }
            return value;
        }

        private void disableEnableButtons(bool trueFalse, bool sqrtRoot, bool coma)
        {
            //Disabling or enabling buttons so there are no errors.
            btnPlusMinus.Enabled = trueFalse;
            btnSquare.Enabled = trueFalse;
            btnDevision.Enabled = trueFalse;
            btnPlus.Enabled = trueFalse;
            btnSubtraction.Enabled = trueFalse;
            btnSqrtRoot.Enabled = sqrtRoot;
            btnMultiply.Enabled = trueFalse;
            btnEquals.Enabled = trueFalse;
            btnComa.Enabled = coma;
            btnMemoryClear.Enabled = trueFalse;
            btnMemoryMinus.Enabled = trueFalse;
            btnMemoryPlus.Enabled = trueFalse;
            btnMemoryRecall.Enabled = trueFalse;
            btnMemoryStore.Enabled = trueFalse;
            btnOneX.Enabled = trueFalse;
        }

        private void btnPlusMinus_Click(object sender, EventArgs e)
        {
            //The number multiplied by -1.
            if (!tbResult.Text.Contains(" ") && !tbResult.Text.Contains("√"))
            {
                double currentValue = double.Parse(tbResult.Text);
                double newValue = currentValue * -1;
                tbResult.Text = newValue.ToString();
            }
            else
            {
                MessageBox.Show("Incorrect input! Try hitting the '=' button first.");
            }
        }

        private void btnSquare_Click(object sender, EventArgs e)
        {
            //Multiplying a number by itself.
            if (!tbResult.Text.Contains(" ") && !tbResult.Text.Contains("√") && !tbResult.Text.Equals("."))
            {
                double value = double.Parse(tbResult.Text);
                tbResult.Text = (value * value).ToString();
            }
        }

        private void btnOneX_Click(object sender, EventArgs e)
        {
            //Dividing 1 by the current number.
            if (!tbResult.Text.Contains(" ") && !tbResult.Text.Contains("√"))
            {
                if (tbResult.Text.Equals("0")) return; 
                double currentValue = 1 / double.Parse(tbResult.Text);
                tbResult.Text = currentValue.ToString();
            }
            else
            {
                MessageBox.Show("Incorrect input! Try hitting the '=' button first.");
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Showing about info.
            MessageBox.Show("Calculator made by Lyuben Georgiev" + Environment.NewLine + "STD 3A Group" + Environment.NewLine +
                "2001681081", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Exiting the application.
            Application.Exit();
        }

        private void Calculator_Load(object sender, EventArgs e)
        {
            //Removing the default top menu.
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
        }

        private void historyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string message = "";
            for (int i = 0; i < history.Count; i++)
            {
                //Adding previous equasions into the history.
                string lastHistory = history.ElementAt(i);
                message += $"{i + 1}) {lastHistory} {Environment.NewLine}";
            }
            //If there are no previous history.
            if (history.Count == 0) message = "No Previous History!";

            MessageBox.Show(message, "Previous History");
        }

        private void msMainMenu_MouseDown(object sender, MouseEventArgs e)
        {
            //Checking if the top menu is held by the mouse.
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void minimizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Minimazing the program.
            this.WindowState = FormWindowState.Minimized;
        }

        private void btnMemoryStore_Click(object sender, EventArgs e)
        {
            if (!tbResult.Text.Contains(" ") && !tbResult.Text.Contains("√") && !tbResult.Text.Equals(""))
                memoryValue = double.Parse(tbResult.Text);
            else
            {
                MessageBox.Show("Incorrect input! Try hitting the '=' button first.");
                return;
            }

            //Checking if there is a square root symbol before saving the number.
            if (tbResult.Text.Contains("√"))
            {
                int indexOfSqrt = tbResult.Text.IndexOf("√");
                //Checking if it's the √5 type of square root or 5√5 type.
                if (indexOfSqrt == 0)
                {
                    memoryValue = Math.Sqrt(double.Parse(tbResult.Text.Substring(1)));
                    return;
                }
                else
                {
                    double firstValue = double.Parse(tbResult.Text.Substring(0, indexOfSqrt));
                    double secondValue = double.Parse(tbResult.Text.Substring(indexOfSqrt + 1));
                    memoryValue = firstValue * (Math.Sqrt(secondValue));
                    return;
                }
            }
        }

        private void btnMemoryPlus_Click(object sender, EventArgs e)
        {
            //Adding the memory value to the calculator's textbox.
            if (!tbResult.Text.Contains(" ") && !tbResult.Text.Contains("√"))
                tbResult.Text = (memoryValue + double.Parse(tbResult.Text)).ToString();
        }

        private void btnMemoryClear_Click(object sender, EventArgs e)
        {
            //Resetting the memory value.
            memoryValue = 0;
        }

        private void btnMemoryRecall_Click(object sender, EventArgs e)
        {
            //Adding the memory value to the calculator's textbox as a text.
            if (memoryValue != 0)
            {
                if (tbResult.Text != "0") tbResult.Text += $"{memoryValue}"; else tbResult.Text = memoryValue.ToString();
            }
        }

        private void btnMemoryMinus_Click(object sender, EventArgs e)
        {
            //Subtracting the memory value to the calculator's textbox
            if (!tbResult.Text.Contains(" ") && !tbResult.Text.Contains("√"))
                tbResult.Text = (double.Parse(tbResult.Text) - memoryValue).ToString();
        }

        private void btnClearEntry_Click(object sender, EventArgs e)
        {
            //Deleting the last entered symbol.
            if (tbResult.Text.Length > 1)
                tbResult.Text = tbResult.Text.Substring(0, tbResult.Text.Length - 1);
            else tbResult.Text = "0"; disableEnableButtons(true, true, true);
        }
    }
}
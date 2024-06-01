using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
namespace 像素颜色拾取器
{
    public partial class colortool : Form
    {
        private GlobalKeyboardHook _globalKeyboardHook;

        [DllImport("user32.dll")]
        static extern bool GetCursorPos(out Point lpPoint);

        [DllImport("gdi32.dll")]
        static extern uint GetPixel(IntPtr hdc, int nXPos, int nYPos);

        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
        public colortool()
        {
            InitializeComponent();
            this.TopMost = true;
            _globalKeyboardHook = new GlobalKeyboardHook();
            _globalKeyboardHook.KeyDown += GlobalKeyboardHook_KeyDown;
            richTextBox1.ScrollBars = RichTextBoxScrollBars.Vertical;
            richTextBox1.WordWrap = false;

        }

        private void InitializeComponent()
        {
            textBox1 = new TextBox();
            richTextBox1 = new RichTextBox();
            SuspendLayout();
            // 
            // textBox1
            // 
            textBox1.BorderStyle = BorderStyle.None;
            textBox1.Enabled = false;
            textBox1.Location = new Point(12, 12);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(148, 16);
            textBox1.TabIndex = 0;
            textBox1.Text = "按F键拾取像素坐标和颜色";
            textBox1.TextChanged += textBox1_TextChanged;
            // 
            // richTextBox1
            // 
            richTextBox1.BackColor = Color.White;
            richTextBox1.ForeColor = Color.Black;
            richTextBox1.ImeMode = ImeMode.On;
            richTextBox1.Location = new Point(12, 34);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.ReadOnly = true;
            richTextBox1.Size = new Size(466, 194);
            richTextBox1.TabIndex = 1;
            richTextBox1.Text = "";
            richTextBox1.TextChanged += richTextBox1_TextChanged;
            // 
            // colortool
            // 
            ClientSize = new Size(490, 240);
            Controls.Add(richTextBox1);
            Controls.Add(textBox1);
            FormBorderStyle = FormBorderStyle.Fixed3D;
            Name = "colortool";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "像素拾取器";
            Load += colortool_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        private TextBox textBox1;

        private void colortool_Load(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private RichTextBox richTextBox1;

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }
        private void GlobalKeyboardHook_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F)
            {
                Point cursorPosition;
                GetCursorPos(out cursorPosition);

                IntPtr hdc = GetDC(IntPtr.Zero);
                uint pixel = GetPixel(hdc, cursorPosition.X, cursorPosition.Y);
                ReleaseDC(IntPtr.Zero, hdc);

                Color color = Color.FromArgb((int)(pixel & 0x000000FF),
                                             (int)(pixel & 0x0000FF00) >> 8,
                                             (int)(pixel & 0x00FF0000) >> 16);

                string colorInfo = $"X: {cursorPosition.X}, Y: {cursorPosition.Y}, rgb ({color.R}, {color.G}, {color.B})";

                AppendColorText(richTextBox1, colorInfo, color);
            }
        }


        private void AppendColorText(RichTextBox richTextBox, string text, Color color)
        {
            // Append the text
            richTextBox.SelectionStart = richTextBox.TextLength;
            richTextBox.SelectionLength = 0;
            richTextBox.SelectionColor = richTextBox.ForeColor;
            richTextBox.AppendText(text);

            // Append a space
            richTextBox.SelectionStart = richTextBox.TextLength;
            richTextBox.SelectionLength = 0;
            richTextBox.AppendText("  ");

            // Append the color block
            richTextBox.SelectionStart = richTextBox.TextLength;
            richTextBox.SelectionLength = 0;
            richTextBox.SelectionBackColor = color;
            richTextBox.AppendText("       "); // Two spaces to make the block more visible

            // Reset the selection color and back color
            richTextBox.SelectionStart = richTextBox.TextLength;
            richTextBox.SelectionLength = 0;
            richTextBox.SelectionBackColor = richTextBox.BackColor;
            richTextBox.SelectionColor = richTextBox.ForeColor;

            // Append a new line
            richTextBox.AppendText(Environment.NewLine);

            // Scroll to the caret to show the latest text
            richTextBox.ScrollToCaret();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _globalKeyboardHook.Dispose();
            base.OnFormClosed(e);
        }
    }

    public class GlobalKeyboardHook : IDisposable
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;

        private LowLevelKeyboardProc _proc;
        private IntPtr _hookID = IntPtr.Zero;

        public event KeyEventHandler KeyDown;

        public GlobalKeyboardHook()
        {
            _proc = HookCallback;
            _hookID = SetHook(_proc);
        }

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Keys key = (Keys)vkCode;
                KeyDown?.Invoke(this, new KeyEventArgs(key));
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        public void Dispose()
        {
            UnhookWindowsHookEx(_hookID);
        }



    }
}

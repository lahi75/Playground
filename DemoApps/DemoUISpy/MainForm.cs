using Phoebit.Input;
using Phoebit.Input.Touch;
using System;
using System.Drawing;
using System.Windows.Forms;
using Phoebit.UIAutomation;
using Phoebit.Desktop;
using System.Runtime.InteropServices;

namespace CaptureDemo
{
    public partial class MainForm : Form
    {
        // hooks for global key, touch and mouse events
        readonly MouseHook mouseHook = new MouseHook();
        readonly KeyboardHook keyboardHook = new KeyboardHook();
        readonly ITouchPanel touchScreen = new TabletTouchPanel();

        // ui spy to sniff out the desktop
        readonly UISpy uiSpy = new UISpy();

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            CaptureScreen();

            // Capture the events            
            mouseHook.MouseMove += MouseHook_MouseMove;
            mouseHook.LeftButtonDown += MouseHook_LeftButtonDown;
            keyboardHook.KeyDown += KeyboardHook_KeyDown;

            //Installing the Mouse Hooks
            mouseHook.Install();
            //Installing the Keyboard Hooks
            keyboardHook.Install();

            // install touch panel if available
            if (touchScreen.TouchpanelFound)
            {
                touchScreen.TouchDown += TouchScreen_TouchDown;
                touchScreen.Enable = true;
            }
        }
        
        private void KeyboardHook_KeyDown(KeyboardHook.VKeys key)
        {
            keyLabel.Text = "Last Key: " + KeyboardHook.Keycode2String((uint)key);

            // examine if we edit an edit box             
            if (uiSpy.IsFocusOnEditControl())
            {
                Point defPnt = new Point();
                GetCursorPos(ref defPnt);
                keyLabel.Text += "\nEdit: " + uiSpy.GetBounds().ToString();
            }
            else
                keyLabel.Text += "\nno Edit";
        }

        private void MouseHook_MouseMove(MouseHook.MSLLHOOKSTRUCT mouseStruct)
        {
            mouseLabel.Text = "Mouse Pos: " + mouseStruct.pt.x + "," + mouseStruct.pt.y;            
        }
      
        private void MouseHook_LeftButtonDown(MouseHook.MSLLHOOKSTRUCT mouseStruct)
        {
            // on mouse click, sniff out which element was clicked at
            SniffDesktopElement(new Point(mouseStruct.pt.x, mouseStruct.pt.y));
        }

        private void TouchScreen_TouchDown(object sender, MouseEventArgs e)
        {
            touchLabel.Text = "Touch Pos: " + e.X + "," + e.Y;
        }        

        private void SniffDesktopElement(Point location)
        {            
            uiSpy.StartSearch(location, Screen.PrimaryScreen.Bounds);

            UIElement element = new UIElement();         

            if (uiSpy.WaitElement(out element))
            {
                Aoi a = new Aoi(element.ControlType, (System.Drawing.Rectangle)element.Bounds, element.name);
                uiElementLabel.Text = a.GetAoiTypeName().ToString() + "\n" + a.Bounds.ToString();                 
            }                                                   
        }

        private void CaptureScreen()
        {
            Bitmap bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(0, 0, 0, 0, Screen.PrimaryScreen.Bounds.Size);
            }
            screenBox.Image = bmp;
        }

        private void ScreenBox_Click(object sender, EventArgs e)
        {
            CaptureScreen();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            mouseHook.MouseMove -= new MouseHook.MouseHookCallback(MouseHook_MouseMove);
            mouseHook.Uninstall();

            keyboardHook.KeyDown -= new KeyboardHook.KeyboardHookCallback(KeyboardHook_KeyDown);
            keyboardHook.Uninstall();

            // remove touch screen handler if they exist
            touchScreen.Enable = false;
            if (touchScreen.Enable == true)
            {
                touchScreen.TouchDown -= new MouseEventHandler(TouchScreen_TouchDown);
                touchScreen.Enable = false;
            }
        }

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(ref Point lpPoint);
    }    
}

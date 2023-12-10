// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Threading;
using System.Windows.Forms;

// ----------------------------------------------------------------------------

namespace Phoebit.Input.Touch
{
    /// <summary>
    /// Touchscreen Modes for event detection
    /// </summary>
    public enum TouchScreenMode
    {
        /// <summary>
        /// Touchscreen disabled
        /// </summary>
        None = 0,
        /// <summary>
        /// Touchscreen use Mouse-Rawdata for event detection
        /// </summary>
        Raw = 1,
        /// <summary>
        /// Touchscreen use Tablet PC calls for event detection (requires XP Tablet Edition or Vista or higher)
        /// </summary>
        Tablet = 2,
        /* 
        /// <summary>
        /// Touchscreen use Windows Touch API for event detection (requires Windows 7 or higher)
        /// </summary>
        TouchAPI = 3,  // Not implemented right now
         */
    }


    /// <summary>
    /// Interface of Touchpanel classes
    /// </summary>
    public interface ITouchPanel
    {
                
        /// <summary>
        /// Touch Screen Click Down Event        
        /// </summary>
        event MouseEventHandler TouchDown;

        /// <summary>
        /// Touch Screen Click Up Event        
        /// </summary>
        event MouseEventHandler TouchUp;

        /// <summary>        
        /// Touch Screen move position event
        /// </summary>
        event MouseEventHandler TouchMove;       

        /// <summary>
        /// Enable or disable touch screen support
        /// </summary>
        bool Enable { get; set; }

        /// <summary>
        /// True if a touchpanel was found, otherwise false
        /// </summary>
        bool TouchpanelFound { get; }

        void SimulateTouchDown();
        void SimulateTouchMove();
        void SimulateTouchUp();
    }


    /// <summary>
    /// This class encapsulates a Touchpanel. Touchpanel data is collected by intercepting mouse raw data.
    /// </summary>
    //[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    public class RawTouchpanel : NativeWindow , ITouchPanel
    {        
        private InputDevice _touchpanel = null;
     
        /// <summary>
        /// Touch Screen Click Down event
        /// </summary>
        public event MouseEventHandler TouchDown;

        /// <summary>
        /// Touch Screen Click Up event
        /// </summary>
        public event MouseEventHandler TouchUp;
        
        /// <summary>
        /// Touch Screen move event
        /// </summary>
        public event MouseEventHandler TouchMove;        

        /// <summary>
        /// Returns true if a touchpanel was found, otherwise false
        /// </summary>
        public bool TouchpanelFound
        {
            get
            {
                return (_touchpanel != null);
            }
        }

        /// <summary>
        /// Enable or Disable Touchscreen Events
        /// </summary>
        public bool Enable { get; set; }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="hwnd">Handle of window, which wants to receive the touchpanel events</param>
        /// <param name="deviceName">Name of Touchpanel Device</param>
        /// <param name="clickDelay">specifies the delay between MouseDown and TouchDown Event</param>
        public RawTouchpanel(IntPtr hwnd, String deviceName)         
        {            
            if (hwnd == IntPtr.Zero)
                return;

            Enable = false;

            // Set window Handle
            this.AssignHandle(hwnd);
           
            List<DeviceInfo> devices;
            
            // Get connected touchpanel devices
            if (GetTouchpanelDevices(out devices))
            {
                foreach (DeviceInfo device in devices)
                {
                    if (device.DeviceName == deviceName)
                    {
                        try
                        {
                            _touchpanel = new InputDevice(Handle, device);
                        }
                        catch (ApplicationException)
                        {
                            // can't register tochpanel device
                            _touchpanel = null;
                            break;
                        }

                        // Register MouseDown and Mouseup events
                        _touchpanel.MouseDown += new MouseEventHandler(Touchpanel_MouseDown);
                        _touchpanel.MouseUp += new MouseEventHandler(Touchpanel_MouseUp);
                        _touchpanel.MouseMove += new MouseEventHandler(Touchpanel_MouseMove);                        
                        
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Intercept WM_INPUT messages
        /// </summary>
        /// <param name="message">Message</param>
        protected override void WndProc(ref Message message)
        {
            if (_touchpanel != null && Enable)
            {
                _touchpanel.ProcessMessage(message);
            }
            base.WndProc(ref message);
        }
        
        /// <summary>
        /// Event Handler for Mouse Down Event
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">MouseEventArgs</param>
        private void Touchpanel_MouseDown(object sender, MouseEventArgs e)
        {                   
            // Raw Data doesn't contain position data ... so set current position                        
            OnTouchDown(new MouseEventArgs(e.Button, e.Clicks, Cursor.Position.X, Cursor.Position.Y, e.Delta));    
        }

        /// <summary>
        /// Event Handler for Mouse Up event
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">MouseEventArgs</param>
        private void Touchpanel_MouseUp(object sender, MouseEventArgs e)
        {
            OnTouchUp(new MouseEventArgs(e.Button, e.Clicks, Cursor.Position.X, Cursor.Position.Y, e.Delta));    
        }

        /// <summary>
        /// Event Handler for Mouse Move event
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">MouseEventArgs</param>
        private void Touchpanel_MouseMove(object sender, MouseEventArgs e)
        {
            OnTouchMove(new MouseEventArgs(e.Button, e.Clicks, Cursor.Position.X, Cursor.Position.Y, e.Delta));
        }    
        
        /// <summary>
        /// Raise Touch Click Event
        /// </summary>
        /// <param name="e">MouseEventArgs</param>
        protected void OnTouchDown(MouseEventArgs e)
        {
            if (TouchDown != null)
            {                
                TouchDown(this, e);
            }
        }

        /// <summary>
        /// Raise Touch Click Event
        /// </summary>
        /// <param name="e">MouseEventArgs</param>
        protected void OnTouchUp(MouseEventArgs e)
        {
            if (TouchUp != null)
            {
                TouchUp(this, e);
            }
        }

        /// <summary>
        /// Raise Touch Click Event
        /// </summary>
        /// <param name="e">MouseEventArgs</param>
        protected void OnTouchMove(MouseEventArgs e)
        {
            if (TouchMove != null)
            {
                TouchMove(this, e);
            }
        }

        public void SimulateTouchDown()
        {
            if (TouchDown != null)
            {
                TouchDown(this, new MouseEventArgs(MouseButtons.Left, 0, Cursor.Position.X/2, Cursor.Position.Y/2, 0));
            }
        }

        public void SimulateTouchMove()
        {
            if (TouchMove != null)
            {
                TouchMove(this, new MouseEventArgs(MouseButtons.Left, 0, Cursor.Position.X/2, Cursor.Position.Y/2, 0));
            }
        }

        public void SimulateTouchUp()
        {
            if (TouchUp != null)
            {
                TouchUp(this, new MouseEventArgs(MouseButtons.Left, 0, Cursor.Position.X/2, Cursor.Position.Y/2, 0));
            }
        }

        /// <summary>
        /// Returns a List of Touchpaneldevices
        /// </summary>
        /// <param name="devices"></param>
        /// <returns>true on success, otherwise false</returns>
        public static bool GetTouchpanelDevices(out List<DeviceInfo> devices)
        {
            if (InputDevice.GetInputDevices(out devices))
            {
                for (int idx = 0; idx < devices.Count; idx++)
                {
                    // Touchpanel device is Mouse device ... if mouse device is HID-Device use TabletTouchPanel-Class
                    if (devices[idx].DeviceType != DeviceType.Mouse)
                    {
                        devices.RemoveAt(idx);
                        idx--;
                    }
                }
                return true;
            }
            else
                return false;
        }
    }

    /// <summary>
    /// This class encapsulates a Touchpanel. Use this TouchPanel class for Windows Vista and Windows XP Tablet Edition
    /// Allways set Enabled = false before creating a new instance of the class.
    /// </summary>
    //[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    public class TabletTouchPanel : ITouchPanel
    {                       
        private const uint MI_WP_SIGNATURE = 0xFF515700;
        private const uint SIGNATURE_MASK = 0xFFFFFF00;

        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_LBUTTONUP = 0x0202;
        private const int WM_MOUSEMOVE = 0x0200;
 
        private const int WH_MOUSE_LL = 14;

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int code, HookProc func, IntPtr hInstance, int threadID);

        [DllImport("user32.dll")]
        private static extern int UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern int CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32")]
        private static extern uint GetMessageExtraInfo();
               
        private delegate int HookProc(int code, IntPtr wParam, IntPtr lParam);
        private HookProc ProcessMessageDelegate = null;
        private IntPtr _hook = IntPtr.Zero;
        private bool _enable = false;

        /// <summary>
        /// Touch Screen Click Down Event
        /// </summary>
        public event MouseEventHandler TouchDown;

        /// <summary>
        /// Touch Screen Click Up Event
        /// </summary>
        public event MouseEventHandler TouchUp;

        /// <summary>
        /// Touch Screen Move Event
        /// </summary>
        public event MouseEventHandler TouchMove;
        
        /// <summary>
        /// Returns true if hook for touchpanel is installed, otherwise false
        /// </summary>
        public bool TouchpanelFound
        {
            get
            {
                return (!Enable || (Enable && _hook != IntPtr.Zero));
            }
        }        

        /// <summary>
        /// Enable or Disable Touchscreen Events
        /// </summary>
        public bool Enable 
        {
            get
            {
                return _enable;
            }
            set
            {
                if (value)
                {
                    if (_hook == IntPtr.Zero)
                    {
                        _hook = SetWindowsHookEx(WH_MOUSE_LL, this.ProcessMessageDelegate, GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName), 0);
                    }
                }
                else
                {
                    if (_hook != IntPtr.Zero)
                    {
                        UnhookWindowsHookEx(_hook);
                        _hook = IntPtr.Zero;
                    }
                }
                _enable = value;
            }        
        }

        /// <summary>
        /// Ctor
        /// </summary>
        public TabletTouchPanel()         
        {
            ProcessMessageDelegate = new HookProc(ProcessMessages);            
        }
       
        /// <summary>
        /// Destructor. Unhook the low level mouse events
        /// </summary>
        ~TabletTouchPanel()
        {
            //Unhook low level mouse events 
            Enable = false;
        }

        /// <summary>
        /// Checks whether event is a touch or a mouse event
        /// </summary>
        /// <param name="extraInfo">extraInfo databyte</param>
        /// <returns>true if event is a touch event, otherwise false</returns>
        private bool IsTouchEvent(uint extraInfo)
        {
            return (((extraInfo) & SIGNATURE_MASK) == MI_WP_SIGNATURE);
        }

        /// <summary>
        /// Intercept WM_LBUTTONDOWN and WM_LBUTTONUP and WM_MOUSEMOVE messages
        /// </summary>        
        private int ProcessMessages(int code, IntPtr wParam, IntPtr lParam)
        {
            if (code < 0 || !Enable)
                return CallNextHookEx(IntPtr.Zero, code, wParam, lParam);

            MSLLHOOKSTRUCT hookData = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));

            // Process Mouse down and Mouse up messages
            switch (wParam.ToInt32())
            {
                case WM_LBUTTONDOWN:
                    if (IsTouchEvent(hookData.dwExtraInfo))
                    {
                        OnTouchDown(new MouseEventArgs(MouseButtons.Left, 0, hookData.pt.x, hookData.pt.y, 0));
                    }
                    break;
                case WM_LBUTTONUP:                    
                    if (IsTouchEvent(hookData.dwExtraInfo))
                    {                    
                        OnTouchUp(new MouseEventArgs(MouseButtons.Left, 0, hookData.pt.x, hookData.pt.y, 0));
                    }
                    break;

                case WM_MOUSEMOVE:                    
                    if (IsTouchEvent(hookData.dwExtraInfo))
                    {                                                
                        OnTouchMove(new MouseEventArgs(MouseButtons.None, 0, hookData.pt.x, hookData.pt.y, 0));
                    }
                    break;
            }

            //return the value returned by CallNextHookEx
            return CallNextHookEx(IntPtr.Zero, code, wParam, lParam);
        }       
      
        /// <summary>
        /// Raise Touch Click Event
        /// </summary>
        /// <param name="e">MouseEventArgs</param>
        protected void OnTouchDown(MouseEventArgs e)
        {
            if (TouchDown != null)
            {
                TouchDown(this, e);
            }
        }

        /// <summary>
        /// Raise Touch Click Event
        /// </summary>
        /// <param name="e">MouseEventArgs</param>
        protected void OnTouchUp(MouseEventArgs e)
        {
            if (TouchUp != null)
            {
                TouchUp(this, e);
            }
        }

        /// <summary>
        /// Raise Touch move Event
        /// </summary>
        /// <param name="e">MouseEventArgs</param>
        protected void OnTouchMove(MouseEventArgs e)
        {
            if (TouchMove != null)
            {
                TouchMove(this, e);
            }
        }


        public void SimulateTouchDown()
        {
            if (TouchDown != null)
            {
                TouchDown(this, new MouseEventArgs(MouseButtons.Left, 0, Cursor.Position.X, Cursor.Position.Y, 0));
            }
        }

        public void SimulateTouchMove()
        {
            if (TouchMove != null)
            {
                TouchMove(this, new MouseEventArgs(MouseButtons.Left, 0, Cursor.Position.X, Cursor.Position.Y, 0));
            }
        }

        public void SimulateTouchUp()
        {
            if (TouchUp != null)
            {
                TouchUp(this, new MouseEventArgs(MouseButtons.Left, 0, Cursor.Position.X, Cursor.Position.Y, 0));
            }
        }
    }
}

// ----------------------------------------------------------------------------
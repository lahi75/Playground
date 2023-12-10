// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

#pragma warning disable CA1416

// ----------------------------------------------------------------------------

namespace Phoebit.Input.Touch
{
    /// <summary>
    /// Device-types that are supported by the InputDevice-Class
    /// </summary>
    public enum DeviceType
    {
        /// <summary>
        /// Mouse input device
        /// </summary>
        Mouse = 0,
        /// <summary>
        /// Keyboard input device
        /// </summary>
        Keyboard = 1,
        /// <summary>
        /// HID input device
        /// </summary>
        HID = 2,
        /// <summary>
        /// Unknown input device
        /// </summary>
        Unknown = 3,
    }

    /// <summary>
    /// Class encapsulating the information about a Input device    
    /// </summary>   
    public class DeviceInfo
    {
        /// <summary>
        /// Get the name of the input device.
        /// </summary>
        public string DeviceName { get; private set; }

        /// <summary>
        /// Get the type of the input device.
        /// </summary>
        public DeviceType DeviceType { get; private set; }

        /// <summary>
        /// Get the handle of the input device.
        /// </summary>
        public IntPtr DeviceHandle { get; private set; }

        /// <summary>
        /// Get the description of the input device.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Get the short description of the input device.
        /// </summary>
        public string ShortDescription { get; private set; }

        /// <summary>
        /// Get the top level HID of the input device.
        /// </summary>
        public ushort UsagePage { get; private set; }

        /// <summary>
        /// Get the Usage ID, which indicates the precise device type.
        /// </summary>
        public ushort UsageID { get; private set; }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="name">Name of Device</param>
        /// <param name="type">Type of Device</param>
        /// <param name="handle">Handle of Device</param>
        /// <param name="description">Device Description</param>
        /// <param name="shortDescription">Short Description contains only the Name</param>
        /// <param name="usagePage">The top level HID</param>
        /// <param name="usageID"> This ID indicates the precise device type</param>
        public DeviceInfo(string name, DeviceType type, IntPtr handle, string description, string shortDescription, ushort usagePage, ushort usageID)
        {            
            DeviceName = name;
            DeviceType = type;
            DeviceHandle = handle;
            Description = description;
            ShortDescription = shortDescription;
            UsagePage = usagePage;
            UsageID = usageID;
        }
    }

    /// <summary>
    /// Class encapsulating an input Device. An Input device can be a keyboard, a mouse or a HID-device
    /// </summary>
    public class InputDevice
    {       
        [DllImport("User32.dll")]
        extern static uint GetRawInputDeviceList(IntPtr pRawInputDeviceList, ref uint uiNumDevices, uint cbSize);

        [DllImport("User32.dll")]
        extern static uint GetRawInputDeviceInfo(IntPtr hDevice, uint uiCommand, IntPtr pData, ref uint pcbSize);

        [DllImport("User32.dll")]
        extern static bool RegisterRawInputDevices(RAWINPUTDEVICE[] pRawInputDevice, uint uiNumDevices, uint cbSize);

        [DllImport("User32.dll")]
        extern static uint GetRawInputData(IntPtr hRawInput, uint uiCommand, IntPtr pData, ref uint pcbSize, uint cbSizeHeader);


        // The following constants are defined in Windows.h
        private const int RIDEV_INPUTSINK = 0x00000100;
        private const int RID_INPUT = 0x10000003;

        private const int FAPPCOMMAND_MASK = 0xF000;
        private const int FAPPCOMMAND_MOUSE = 0x8000;
        private const int FAPPCOMMAND_OEM = 0x1000;

        private const int RIM_TYPEMOUSE = 0;
        private const int RIM_TYPEKEYBOARD = 1;
        private const int RIM_TYPEHID = 2;

        private const int RIDI_DEVICENAME = 0x20000007;
        private const int RIDI_DEVICEINFO = 0x2000000b;

        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_SYSKEYUP = 0x0105;

        private const int WM_INPUT = 0x00FF;
        private const int VK_OEM_CLEAR = 0xFE;
        private const int VK_LAST_KEY = VK_OEM_CLEAR; // this is a made up value used as a sentinel

        private const int RI_MOUSE_LEFT = 0x003;  // Left Mouse Button
        private const int RI_MOUSE_RIGHT = 0x00C; // Right Mouse Button
        private const int RI_MOUSE_MIDDLE = 0x030; // Middle Mouse Button

        private const int RI_MOUSE_DOWN = 0x015; // Mouse Button Down
        private const int RI_MOUSE_UP = 0x02A;   // Mouse Button Up


        private DeviceInfo _device = null;

        /// <summary>
        /// The event raised when InputDevice detects that a mouse button was pressed
        /// </summary>
        public event MouseEventHandler MouseDown;

        /// <summary>
        /// The event raised when InputDevice detects that a mouse button was released
        /// </summary>
        public event MouseEventHandler MouseUp;

        /// <summary>
        /// The event raised when InputDevice detects that a mouse is moved
        /// </summary>
        public event MouseEventHandler MouseMove;

        /// <summary>
        /// The event raised when InputDevice detects that a key was pressed
        /// </summary>
        public event KeyEventHandler KeyDown;

        /// <summary>
        /// The event raised when InputDevice detects that a key was released
        /// </summary>
        public event KeyEventHandler KeyUp;

        /// <summary>
        /// Ctor. Registers the raw input devices for the calling window.
        /// </summary>
        /// <param name="hwnd">Handle of the window listening for events</param>
        /// <param name="device">Input device you want to register</param>
        public InputDevice(IntPtr hwnd, DeviceInfo device)
        {
            _device = device;

            //Create an array of the raw input device we want to listen to.
            //RIDEV_INPUTSINK determines that the window will continue to receive messages even when it doesn't have the focus.
            RAWINPUTDEVICE[] rid = new RAWINPUTDEVICE[1];

            rid[0].usUsagePage = device.UsagePage;
            rid[0].usUsage = device.UsageID;
            rid[0].dwFlags = RIDEV_INPUTSINK;
            rid[0].hwndTarget = hwnd;

            if (!RegisterRawInputDevices(rid, (uint)rid.Length, (uint)Marshal.SizeOf(rid[0])))
            {
                throw new ApplicationException("Failed to register raw input device.");
            }
        }

        /// <summary>
        /// Processes WM_INPUT messages to retrieve information about any keyboard or mouse events that occur.
        /// </summary>
        /// <param name="message">The WM_INPUT message to process.</param>
        private void ProcessInputCommand(Message message)
        {
            if (_device == null)
                return;

            uint dwSize = 0;

            // call to GetRawInputData sets the value of dwSize, which can then be used to allocate the appropriate amount of memory, storing the pointer in "buffer".
            GetRawInputData(message.LParam, RID_INPUT, IntPtr.Zero, ref dwSize, (uint)Marshal.SizeOf(typeof(RAWINPUTHEADER)));

            IntPtr buffer = Marshal.AllocHGlobal((int)dwSize);

            try
            {
                // Check that buffer points to something, and if so, call GetRawInputData again to fill the allocated memory with information about the input
                if (buffer != IntPtr.Zero && GetRawInputData(message.LParam, RID_INPUT, buffer, ref dwSize, (uint)Marshal.SizeOf(typeof(RAWINPUTHEADER))) == dwSize)
                {
                    // Store the message information in "raw", then check that the input comes from the requested input device.
                    RAWINPUT raw = (RAWINPUT)Marshal.PtrToStructure(buffer, typeof(RAWINPUT));

                    if (raw.header.hDevice == _device.DeviceHandle)
                    {
                        // message from mouse device
                        if (raw.header.dwType == RIM_TYPEMOUSE)
                        {                                                        
                            MouseButtons mouseButton = MouseButtons.None;

                            ushort flags = raw.mouse.buttonsStr.usButtonFlags;

                            // detect the pressed/released button
                            if ((flags & RI_MOUSE_LEFT) != 0)
                                mouseButton = MouseButtons.Left;
                            else
                                if ((flags & RI_MOUSE_RIGHT) != 0)
                                    mouseButton = MouseButtons.Right;
                                else
                                    if ((flags & RI_MOUSE_MIDDLE) != 0)
                                        mouseButton = MouseButtons.Middle;
                         

                            // Send Mouse Down or Mouse Up Event
                            if ((flags & RI_MOUSE_DOWN) != 0)
                            {
                                // Send Mouse down event
                                if (MouseDown != null)
                                    MouseDown(this, new MouseEventArgs(mouseButton, 0, raw.mouse.lLastX, raw.mouse.lLastY, 0));
                            }
                            else
                                if ((flags & RI_MOUSE_UP) != 0)
                                {
                                    // Send Mouse up event
                                    if (MouseUp != null)
                                        MouseUp(this, new MouseEventArgs(mouseButton, 0, raw.mouse.lLastX, raw.mouse.lLastY, 0));
                                }


                            ///// TODO: handle mouse move
                            if (MouseMove != null)
                                MouseMove(this, new MouseEventArgs(mouseButton, 0, raw.mouse.lLastX, raw.mouse.lLastY, 0));

                        }
                        else
                        {
                            //message from HID device
                            if (raw.header.dwType == RIM_TYPEHID)
                            {                                
                                // not implemented
                                
                                // implement actions for HID Devices here
                            }
                            else
                            {
                                //message from keyboard device
                                if (raw.header.dwType == RIM_TYPEKEYBOARD)
                                {
                                    // Filter for Key Down events and then retrieve information about the keystroke
                                    if (raw.keyboard.Message == WM_KEYDOWN || raw.keyboard.Message == WM_KEYUP || raw.keyboard.Message == WM_SYSKEYDOWN || raw.keyboard.Message == WM_SYSKEYUP)
                                    {
                                        ushort key = raw.keyboard.VKey;

                                        // On most keyboards, "extended" keys such as the arrow or page keys return two codes - the key's own code, and an "extended key" flag,
                                        // which translates to 255. This flag isn't useful to us, so it can be disregarded.
                                        if (key > VK_LAST_KEY)
                                        {
                                            return;
                                        }

                                        Keys keyArg = (Keys)Enum.Parse(typeof(Keys), Enum.GetName(typeof(Keys), key));

                                        if (raw.keyboard.Message == WM_KEYDOWN || raw.keyboard.Message == WM_SYSKEYDOWN)
                                        {
                                            if (KeyDown != null)
                                                KeyDown(this, new KeyEventArgs(keyArg));
                                        }
                                        else
                                        {
                                            if (raw.keyboard.Message == WM_KEYUP || raw.keyboard.Message == WM_SYSKEYUP)
                                            {
                                                if (KeyUp != null)
                                                    KeyUp(this, new KeyEventArgs(keyArg));
                                            }
                                        }                                       
                                    }
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        /// <summary>
        /// Filters Windows messages for WM_INPUT messages and calls ProcessInputCommand if necessary.
        /// </summary>
        /// <param name="message">The Windows message.</param>
        public void ProcessMessage(Message message)
        {
            switch (message.Msg)
            {
                case WM_INPUT:
                    ProcessInputCommand(message);
                    break;
            }
        }

        /// <summary>
        /// Converts a RAWINPUTDEVICELIST dwType value to a string describing the device type.
        /// </summary>
        /// <param name="device">A dwType value (RIM_TYPEMOUSE, RIM_TYPEKEYBOARD or RIM_TYPEHID).</param>
        /// <returns>A string representation of the input value.</returns>
        private static DeviceType GetDeviceType(int device)
        {
            DeviceType deviceType;
            switch (device)
            {
                case RIM_TYPEMOUSE:
                    deviceType = DeviceType.Mouse;
                    break;
                case RIM_TYPEKEYBOARD:
                    deviceType = DeviceType.Keyboard;
                    break;
                case RIM_TYPEHID:
                    deviceType = DeviceType.HID;
                    break;
                default:
                    deviceType = DeviceType.Unknown;
                    break;
            }
            return deviceType;
        }

        /// <summary>
        /// Iterates through the list provided by GetRawInputDeviceList and adding them to deviceList.
        /// </summary>
        /// <returns>true on success, otherwise false</returns>
        public static bool GetInputDevices(out List<DeviceInfo> deviceList)
        {
            deviceList = new List<DeviceInfo>();

            uint deviceCount = 0;
            int dwSize = (Marshal.SizeOf(typeof(RAWINPUTDEVICELIST)));

            // Get the number of raw input devices in the list, then allocate sufficient memory and get the entire list
            if (GetRawInputDeviceList(IntPtr.Zero, ref deviceCount, (uint)dwSize) == 0)
            {
                IntPtr pRawInputDeviceList = Marshal.AllocHGlobal((int)(dwSize * deviceCount));
                GetRawInputDeviceList(pRawInputDeviceList, ref deviceCount, (uint)dwSize);

                // Iterate through the list, discarding undesired items and retrieving further information on keyboard devices
                for (int i = 0; i < deviceCount; i++)
                {
                  //  DeviceInfo deviceInfo;
                    string fullDeviceName;
                    uint pcbSize = 0;

                    RAWINPUTDEVICELIST rid = (RAWINPUTDEVICELIST)Marshal.PtrToStructure(new IntPtr((pRawInputDeviceList.ToInt32() + (dwSize * i))), typeof(RAWINPUTDEVICELIST));

                    GetRawInputDeviceInfo(rid.hDevice, RIDI_DEVICENAME, IntPtr.Zero, ref pcbSize);                    

                    if (pcbSize > 0)
                    {
                        IntPtr pData = Marshal.AllocHGlobal((int)pcbSize);
                        GetRawInputDeviceInfo(rid.hDevice, RIDI_DEVICENAME, pData, ref pcbSize);
                        fullDeviceName = (string)Marshal.PtrToStringAnsi(pData);

                        // Drop the "root" keyboard and mouse devices used for Terminal Services and the Remote Desktop
                        if (fullDeviceName.ToUpper().Contains("ROOT"))
                        {
                            continue;
                        }

                        bool hidInformation = false;

                        // If the device is identified in the list as a keyboard, Mouse or HID device, create a DeviceInfo object to store information about it
                        if (rid.dwType == RIM_TYPEMOUSE || rid.dwType == RIM_TYPEHID || rid.dwType == RIM_TYPEKEYBOARD)
                        {
                            string deviceName = (string)Marshal.PtrToStringAnsi(pData);
                            IntPtr deviceHandle = rid.hDevice;
                            DeviceType deviceType = GetDeviceType(rid.dwType);
                            ushort usageID = 0;
                            ushort usagePage = 0;

                            // Check the Registry to retrieve a more friendly description.
                            string deviceDescription = GetDeviceDescription(fullDeviceName);
                            string shortDescription = deviceDescription;

                            // try to split the device description string
                            string[] splittedDesc = deviceDescription.Split(new string[] { "%;" }, StringSplitOptions.None);

                            if (splittedDesc.Length == 2)
                            {
                                shortDescription = splittedDesc[1];
                            }  

                            // Get HID-Information
                            if (rid.dwType == RIM_TYPEHID)
                            {                               
                                pcbSize = 0;
                                GetRawInputDeviceInfo(rid.hDevice, RIDI_DEVICEINFO, IntPtr.Zero, ref pcbSize);

                                if (pcbSize > 0)
                                {
                                    pcbSize = (uint)Marshal.SizeOf(typeof(RID_DEVICE_INFO));

                                    RID_DEVICE_INFO ridInfo = new RID_DEVICE_INFO();
                                    ridInfo.cbSize = (int)pcbSize;

                                    IntPtr pInfoData = Marshal.AllocHGlobal((int)pcbSize);

                                    Marshal.StructureToPtr(ridInfo, pInfoData, false);

                                    GetRawInputDeviceInfo(rid.hDevice, RIDI_DEVICEINFO, pInfoData, ref pcbSize);

                                    ridInfo = (RID_DEVICE_INFO)Marshal.PtrToStructure(pInfoData, typeof(RID_DEVICE_INFO));

                                    usagePage = ridInfo.hid.usUsagePage;
                                    usageID = ridInfo.hid.usUsage;
                                    
                                    hidInformation = true;

                                    Marshal.FreeHGlobal(pInfoData);                                    
                                }
                            }
                            else
                            {
                                hidInformation = GetDeviceHID(rid.dwType, ref usagePage, ref usageID);
                            }

                            // if HID-Information received successfully, add Device to list
                            if (hidInformation)
                            {
                                deviceList.Add(new DeviceInfo(deviceName, deviceType, deviceHandle, deviceDescription, shortDescription, usagePage, usageID));
                            }

                        }
                        Marshal.FreeHGlobal(pData);
                    }
                }

                Marshal.FreeHGlobal(pRawInputDeviceList);

                return true;

            }
            else
                return false;
        }

        /// <summary>
        /// Get HID of Device
        /// </summary>
        /// <param name="deviceType">device type. Should be RIM_TYPEKEYBOARD or RIM_TYPEMOUSE</param>
        /// <param name="usagePage">reference to usagePage</param>
        /// <param name="usageID">reference to usageID</param>
        /// <returns>true on success, otherwise false</returns>
        private static bool GetDeviceHID(int deviceType, ref ushort usagePage, ref ushort usageID)
        {            
            switch (deviceType)
            {
                case RIM_TYPEKEYBOARD:
                    usagePage = 0x01;
                    usageID = 0x06;
                    return true;
                case RIM_TYPEMOUSE:
                    usagePage = 0x01;
                    usageID = 0x02;
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Reads the Registry to retrieve a friendly description of the device.
        /// </summary>
        /// <param name="item">The device name to search for, as provided by GetRawInputDeviceInfo.</param>
        /// <returns>The device description stored in the Registry entry's DeviceDesc value.</returns>
        private static string GetDeviceDescription(string item)
        {
            // Example Device Identification string
            // @"\??\ACPI#PNP0303#3&13c0b0c5&0#{884b96c3-56ef-11d1-bc8c-00a0c91405dd}";

            // remove the \??\
            item = item.Substring(4);

            string[] split = item.Split('#');

            string id_01 = split[0];    // ACPI (Class code)
            string id_02 = split[1];    // PNP0303 (SubClass code)
            string id_03 = split[2];    // 3&13c0b0c5&0 (Protocol code)
            //The final part is the class GUID and is not needed here

            //Open the appropriate key as read-only so no permissions are needed.
            RegistryKey regKey = Registry.LocalMachine;

            string key = string.Format(@"System\CurrentControlSet\Enum\{0}\{1}\{2}", id_01, id_02, id_03);

            regKey = regKey.OpenSubKey(key, false);

            //Retrieve the desired information and set isKeyboard
            string deviceDesc = (string)regKey.GetValue("DeviceDesc");

            return deviceDesc;
        }
    }
}

// ----------------------------------------------------------------------------
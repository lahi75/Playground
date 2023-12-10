// ----------------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;

// ----------------------------------------------------------------------------

namespace Phoebit.Input.Touch
{
    #region RAW Types

    /// <summary>
    /// The RAWINPUTDEVICELIST structure contains information about a raw input device
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct RAWINPUTDEVICELIST
    {
        public IntPtr hDevice;
        [MarshalAs(UnmanagedType.U4)]
        public int dwType;
    }

    /// <summary>
    /// The RAWINPUT structure contains the raw input from a device.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    internal struct RAWINPUT
    {
        [FieldOffset(0)]
        public RAWINPUTHEADER header;
        [FieldOffset(16)]
        public RAWMOUSE mouse;
        [FieldOffset(16)]
        public RAWKEYBOARD keyboard;
        [FieldOffset(16)]
        public RAWHID hid;
    }

    /// <summary>
    /// The RAWINPUTHEADER structure contains the header information that is part of the raw input data.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct RAWINPUTHEADER
    {
        [MarshalAs(UnmanagedType.U4)]
        public int dwType;
        [MarshalAs(UnmanagedType.U4)]
        public int dwSize;
        public IntPtr hDevice;
        [MarshalAs(UnmanagedType.U4)]
        public int wParam;
    }

    /// <summary>
    /// The RAWHID structure describes the format of the raw input from a Human Interface Device (HID).
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct RAWHID
    {
        [MarshalAs(UnmanagedType.U4)]
        public int dwSizHid;
        [MarshalAs(UnmanagedType.U4)]
        public int dwCount;
    }

    /// <summary>
    /// The BUTTONSSTR structure contains information about button flags and data
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct BUTTONSSTR
    {
        [MarshalAs(UnmanagedType.U2)]
        public ushort usButtonFlags;
        [MarshalAs(UnmanagedType.U2)]
        public ushort usButtonData;
    }

    /// <summary>
    /// The RAWMOUSE structure contains information about the state of the mouse.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    internal struct RAWMOUSE
    {
        [MarshalAs(UnmanagedType.U2)]
        [FieldOffset(0)]
        public ushort usFlags;
        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(4)]
        public uint ulButtons;
        [FieldOffset(4)]
        public BUTTONSSTR buttonsStr;
        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(8)]
        public uint ulRawButtons;
        [MarshalAs(UnmanagedType.I4)]
        [FieldOffset(12)]
        public int lLastX;
        [MarshalAs(UnmanagedType.I4)]
        [FieldOffset(16)]
        public int lLastY;
        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(20)]
        public uint ulExtraInformation;
    }

    /// <summary>
    /// The RAWKEYBOARD structure contains information about the state of the keyboard.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct RAWKEYBOARD
    {
        [MarshalAs(UnmanagedType.U2)]
        public ushort MakeCode;
        [MarshalAs(UnmanagedType.U2)]
        public ushort Flags;
        [MarshalAs(UnmanagedType.U2)]
        public ushort Reserved;
        [MarshalAs(UnmanagedType.U2)]
        public ushort VKey;
        [MarshalAs(UnmanagedType.U4)]
        public uint Message;
        [MarshalAs(UnmanagedType.U4)]
        public uint ExtraInformation;
    }

    /// <summary>
    /// The RAWINPUTDEVICE structure defines information for the raw input devices.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct RAWINPUTDEVICE
    {
        [MarshalAs(UnmanagedType.U2)]
        public ushort usUsagePage;
        [MarshalAs(UnmanagedType.U2)]
        public ushort usUsage;
        [MarshalAs(UnmanagedType.U4)]
        public int dwFlags;
        public IntPtr hwndTarget;
    }

    #endregion

    #region RID Types

    /// <summary>
    /// The RID_DEVICE_INFO structure defines the raw input data coming from any device. 
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    internal struct RID_DEVICE_INFO
    {
        [FieldOffset(0)]
        public int cbSize;
        [FieldOffset(16)]
        public RID_DEVICE_INFO_MOUSE mouse;
        [FieldOffset(16)]
        public RID_DEVICE_INFO_KEYBOARD keyboard;
        [FieldOffset(16)]
        public RID_DEVICE_INFO_HID hid;
    }

    /// <summary>
    /// Defines the raw input data coming from the specified mouse.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct RID_DEVICE_INFO_MOUSE
    {
        [MarshalAs(UnmanagedType.U4)]
        public int dwType;
        [MarshalAs(UnmanagedType.U4)]
        public int dwSubType;
        [MarshalAs(UnmanagedType.U4)]
        public int dwKeyboardMode;
        [MarshalAs(UnmanagedType.Bool)]
        public bool fHasHorizontalWheel;
    }

    /// <summary>
    /// The RID_DEVICE_INFO_KEYBOARD structure defines the raw input data coming from the specified keyboard.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct RID_DEVICE_INFO_KEYBOARD
    {
        [MarshalAs(UnmanagedType.U4)]
        public int dwType;
        [MarshalAs(UnmanagedType.U4)]
        public int dwSubType;
        [MarshalAs(UnmanagedType.U4)]
        public int dwKeyboardMode;
        [MarshalAs(UnmanagedType.U4)]
        public int dwNumberOfFunctionKeys;
        [MarshalAs(UnmanagedType.U4)]
        public int dwNumberOfIndicators;
        [MarshalAs(UnmanagedType.U4)]
        public int dwNumberOfKeysTotal;
    }

    /// <summary>
    /// The RID_DEVICE_INFO_HID structure defines the raw input data coming from the specified Human Interface Device (HID).
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct RID_DEVICE_INFO_HID
    {
        [MarshalAs(UnmanagedType.U4)]
        public int dwVendorId;
        [MarshalAs(UnmanagedType.U4)]
        public int dwProductId;
        [MarshalAs(UnmanagedType.U4)]
        public int dwVersionNumber;
        [MarshalAs(UnmanagedType.U2)]
        public ushort usUsagePage;
        [MarshalAs(UnmanagedType.U2)]
        public ushort usUsage;
    }

    #endregion

    #region TABLET TYPES

    [StructLayout(LayoutKind.Sequential)]
    internal struct POINT
    {
        [MarshalAs(UnmanagedType.I4)]  
        public int x;
        [MarshalAs(UnmanagedType.I4)]
        public int y;
    }

    /// <summary>
    /// The MSLLHOOKSTRUCT structure contains information about a low-level mouse input event. 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct MSLLHOOKSTRUCT
    {
        public POINT pt;
        [MarshalAs(UnmanagedType.U4)]
        public int mouseData;
        [MarshalAs(UnmanagedType.U4)]
        public int flags;
        [MarshalAs(UnmanagedType.U4)]
        public int time;
        [MarshalAs(UnmanagedType.U4)]
        public uint dwExtraInfo;
    }

    #endregion
}

// ----------------------------------------------------------------------------
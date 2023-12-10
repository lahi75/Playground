// ----------------------------------------------------------------------------

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

// ----------------------------------------------------------------------------

namespace Phoebit.Desktop
{    
    /// <summary>
    /// Provides functions to mess around with the primary screen
    /// capture the entire screen, or a particular window, and save it to a file
    /// retrieve rectangles if there are controls below a coordinate
    /// draw rectangles and images on the screen
    /// </summary>
    public class ScreenCapture
    {
        /// <summary>
        /// draw a frame on the given rectangle directly on the screen
        /// </summary>            
        public void FrameRect(Pen pen, Rectangle rect)
        {
            IntPtr handle = User32.GetDesktopWindow();
            IntPtr hdcSrc = User32.GetWindowDC(handle);            
            Graphics grs = Graphics.FromHdc(hdcSrc);

            grs.DrawRectangle(pen, rect);
            
            User32.ReleaseDC(handle, hdcSrc);
            grs.Dispose();            
        }

        /// <summary>
        /// draw the given image direcly on the rectangle on the screen
        /// </summary>            
        public void DrawImage(Image img, Rectangle rect)
        {          
            IntPtr handle = User32.GetDesktopWindow();
            IntPtr hdcSrc = User32.GetWindowDC(handle);
            Graphics grs = Graphics.FromHdc(hdcSrc);
            
            grs.DrawImage(img, rect.Left, rect.Top);            

            User32.ReleaseDC(handle, hdcSrc);
            grs.Dispose();         
        }

        /// <summary>
        /// get the rectangle of the handle at the given position
        /// </summary>            
        public Rectangle GetWindowAtPoint(Point p)
        {
            User32.RECT windowRect = new User32.RECT();
            User32.POINT point = new User32.POINT();
            point.x = p.X;
            point.y = p.Y;

            // get the window handle from the specified position
            IntPtr hWnd = User32.WindowFromPoint(point);            
            
            // get the windows rectangle from the handle
            User32.GetWindowRect(hWnd, ref windowRect);

            //User32.ScreenToClient(hWnd, ref windowRect);            

            // return the rectangle of the handle at the given position
            return new Rectangle( windowRect.left, windowRect.top, windowRect.right-windowRect.left, windowRect.bottom - windowRect.top );
        }

        /// <summary>
        /// retrieve the screen coordinates of the control
        /// </summary>        
        public Rectangle GetWindowFromHandle(IntPtr handle)
        {
            User32.RECT windowRect = new User32.RECT();
            // get the windows rectangle from the handle
            User32.GetWindowRect(handle, ref windowRect);

            // return the rectangle of the handle at the given position
            return new Rectangle(windowRect.left, windowRect.top, windowRect.right - windowRect.left, windowRect.bottom - windowRect.top);
        }       
      
        /// <summary>
        /// move the given window to the top
        /// </summary>        
        public void SetForegroundWindow(IntPtr handle)
        {
            User32.SetForegroundWindow(handle);
        }

        /// <summary>
        /// Finds the window handle 
        /// </summary>
        /// <param name="windowName"></param>
        /// <returns></returns>
        public IntPtr FindWindow(string windowName)
        {
            return User32.FindWindow(null, windowName);
        }

        /// <summary>
        /// Creates an Image object containing a screen shot of the entire desktop
        /// </summary>            
        public Image CaptureImageScreen()
        {
            return CaptureWindow(User32.GetDesktopWindow());
        }

        /// <summary>
        /// Creates an Image object containing a screen shot of the entire desktop
        /// </summary>            
        public Bitmap CaptureBitmapScreen()
        {
            return CaptureBitmapWindow(User32.GetDesktopWindow());
        }

        /// <summary>
        /// create a Bitmap object in the given size from the entire desktop
        /// </summary>
        /// <param name="size">desired size of the bitmap</param> 
        /// <<param name="captureHighQuality">captures in high quality (slow) if true, otherwise not</param>
        public Bitmap CaptureBitmapScreen(Size size, bool captureHighQuality)
        {
            return CaptureBitmapWindow(User32.GetDesktopWindow(), size, captureHighQuality);
        }

        /// <summary>
        /// capture the given part of the screen
        /// </summary>  
        public Image CaptureWindow(Rectangle rect)
        {
            return CaptureWindow(User32.GetDesktopWindow(), rect.Left, rect.Top, rect.Width, rect.Height);
        }

        /// <summary>
        /// Creates an Image object containing a screen shot of a specific window
        /// </summary>
        /// <param name="handle">The handle to the window. (In windows forms, this is obtained by the Handle property)</param>            
        Image CaptureWindow(IntPtr handle)
        {            
            // get the size
            User32.RECT windowRect = new User32.RECT();
            User32.GetWindowRect(handle, ref windowRect);
            int width = windowRect.right - windowRect.left;
            int height = windowRect.bottom - windowRect.top;

            return CaptureWindow(handle, 0, 0, width, height);            
        }

        /// <summary>
        /// Creates an bitmap object containing a screen shot of a specific window
        /// </summary>
        /// <param name="handle">The handle to the window. (In windows forms, this is obtained by the Handle property)</param>        
        Bitmap CaptureBitmapWindow(IntPtr handle)
        {
            // get the size
            User32.RECT windowRect = new User32.RECT();
            User32.GetWindowRect(handle, ref windowRect);
            int width = windowRect.right - windowRect.left;
            int height = windowRect.bottom - windowRect.top;

            return CaptureBitmapWindow(handle, 0, 0, width, height);            
        }

        Bitmap CaptureBitmapWindow(IntPtr handle, Size size, bool captureHighQuality)
        {
            // get the size
            User32.RECT windowRect = new User32.RECT();
            User32.GetWindowRect(handle, ref windowRect);
            int width = windowRect.right - windowRect.left;
            int height = windowRect.bottom - windowRect.top;

            return CaptureBitmapWindow(handle, 0, 0, width, height, size.Width, size.Height, captureHighQuality);
        }

        Image CaptureWindow(IntPtr handle, int left, int top, int width, int height)
        {
            // get te hDC of the target window
            IntPtr hdcSrc = User32.GetWindowDC(handle);
            // create a device context we can copy to
            IntPtr hdcDest = GDI32.CreateCompatibleDC(hdcSrc);
            // create a bitmap we can copy it to,
            // using GetDeviceCaps to get the width/height
            IntPtr hBitmap = GDI32.CreateCompatibleBitmap(hdcSrc, width, height);
            // select the bitmap object
            IntPtr hOld = GDI32.SelectObject(hdcDest, hBitmap);
            // bitblt over
            GDI32.BitBlt(hdcDest, 0, 0, width, height, hdcSrc, left, top, GDI32.SRCCOPY);
            // restore selection
            GDI32.SelectObject(hdcDest, hOld);
            // clean up
            GDI32.DeleteDC(hdcDest);
            User32.ReleaseDC(handle, hdcSrc);
            // get a .NET image object for it
            Image img = Image.FromHbitmap(hBitmap);
            // free up the Bitmap object
            GDI32.DeleteObject(hBitmap);
            return img;
        }

        Bitmap CaptureBitmapWindow(IntPtr handle, int left, int top, int width, int height)
        {
            // get te hDC of the target window
            IntPtr hdcSrc = User32.GetWindowDC(handle);
            // create a device context we can copy to
            IntPtr hdcDest = GDI32.CreateCompatibleDC(hdcSrc);
            // create a bitmap we can copy it to,
            // using GetDeviceCaps to get the width/height
            IntPtr hBitmap = GDI32.CreateCompatibleBitmap(hdcSrc, width, height);
            // select the bitmap object
            IntPtr hOld = GDI32.SelectObject(hdcDest, hBitmap);
            // bitblt over
            GDI32.BitBlt(hdcDest, 0, 0, width, height, hdcSrc, left, top, GDI32.SRCCOPY);
            // restore selection
            GDI32.SelectObject(hdcDest, hOld);
            // clean up
            GDI32.DeleteDC(hdcDest);
            User32.ReleaseDC(handle, hdcSrc);
            // get a .NET image object for it
            Bitmap img = Bitmap.FromHbitmap(hBitmap);
            // free up the Bitmap object
            GDI32.DeleteObject(hBitmap);
            return img;
        }

        Bitmap CaptureBitmapWindow(IntPtr handle, int left, int top, int width, int height, int targetWidth, int targetHeight, bool captureHighQuality)
        {
            // get te hDC of the target window
            IntPtr hdcSrc = User32.GetWindowDC(handle);
            // create a device context we can copy to
            IntPtr hdcDest = GDI32.CreateCompatibleDC(hdcSrc);
            // create a bitmap we can copy it to,
            // using GetDeviceCaps to get the width/height
            IntPtr hBitmap = GDI32.CreateCompatibleBitmap(hdcSrc, targetWidth, targetHeight);
            // select the bitmap object
            IntPtr hOld = GDI32.SelectObject(hdcDest, hBitmap);

            //GDI32.HALFTONE is slow but high quality. GDI32.COLORONCOLOR is fast
            int iStrechMode = captureHighQuality ? GDI32.HALFTONE : GDI32.COLORONCOLOR;
                        
            // bitblt over
            GDI32.SetStretchBltMode(hdcDest, iStrechMode);
                    
            GDI32.StretchBlt(hdcDest, 0, 0, targetWidth, targetHeight, hdcSrc, left, top, width, height, GDI32.SRCCOPY);
            // restore selection
            GDI32.SelectObject(hdcDest, hOld);
            // clean up
            GDI32.DeleteDC(hdcDest);
            User32.ReleaseDC(handle, hdcSrc);
            // get a .NET image object for it
            Bitmap img = Bitmap.FromHbitmap(hBitmap);
            // free up the Bitmap object
            GDI32.DeleteObject(hBitmap);
            return img;
        }

        /// <summary>
        /// Captures a screen shot of a specific window, and saves it to a file
        /// </summary>            
        void CaptureWindowToFile(IntPtr handle, string filename, ImageFormat format)
        {
            Image img = CaptureWindow(handle);
            img.Save(filename, format);
        }
        
        /// <summary>
        /// Captures a screen shot of the entire desktop, and saves it to a file
        /// </summary>            
        public void CaptureScreenToFile(string filename, ImageFormat format)
        {
            Image img = CaptureImageScreen();
            img.Save(filename, format);
        }

        #region Imports
        /// <summary>
        /// Helper class containing Gdi32 API functions
        /// </summary>
        private class GDI32
        {
            public const int SRCCOPY = 0x00CC0020; // BitBlt dwRop parameter
            public const int COLORONCOLOR = 3; //SetStretchBltMode iStretchMode parameter
            public const int HALFTONE = 4; //SetStretchBltMode iStretchMode parameter

            [DllImport("gdi32.dll")]
            public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest,
                int nWidth, int nHeight, IntPtr hObjectSource,
                int nXSrc, int nYSrc, int dwRop);
            [DllImport("gdi32.dll")]
            public static extern bool StretchBlt(IntPtr hObject, int nXDest, int nYDest,
                int nWidth, int nHeight, IntPtr hObjectSource,
                int nXSrc, int nYSrc, int nWidthSrc, int nHeighSrc, int dwRop);
            [DllImport("gdi32.dll")]
            public static extern int SetStretchBltMode(IntPtr hDC, int iStretchMode );
            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth,
                int nHeight);
            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleDC(IntPtr hDC);
            [DllImport("gdi32.dll")]
            public static extern bool DeleteDC(IntPtr hDC);
            [DllImport("gdi32.dll")]
            public static extern bool DeleteObject(IntPtr hObject);
            [DllImport("gdi32.dll")]
            public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
        }

        /// <summary>
        /// Helper class containing User32 API functions
        /// </summary>
        private class User32
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct RECT
            {
                public int left;
                public int top;
                public int right;
                public int bottom;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct POINT
            {
                public int x;
                public int y;                
            }

            [DllImport("user32.dll")]
            public static extern IntPtr GetDesktopWindow();
            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowDC(IntPtr hWnd);
            [DllImport("user32.dll")]
            public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);
            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);                        
            [DllImport("user32.dll")]
            public static extern IntPtr WindowFromPoint(POINT point);
            [DllImport("user32.dll")]
            public static extern bool ScreenToClient(IntPtr hWnd, ref RECT rect);
            [DllImport("user32.dll")]
            public static extern bool ScreenToClient(IntPtr hWnd, ref POINT rect);
            [DllImport("user32.dll")]
            public static extern bool ClientToScreen(IntPtr hWnd, ref POINT rect);
            [DllImport("user32.dll")]
            public static extern bool SetForegroundWindow(IntPtr hwnd);
            [DllImport("user32.dll")]
            public static extern IntPtr FindWindow(string className, string windowName);
        }
        #endregion
    }    
}

// ----------------------------------------------------------------------------
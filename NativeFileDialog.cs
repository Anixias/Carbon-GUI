/*_________
 /         \ tinyfiledialogsTest.cs v3.8.3 [Nov 1, 2020] zlib licence
 |tiny file| C# bindings created [2015]
 | dialogs | Copyright (c) 2014 - 2020 Guillaume Vareille http://ysengrin.com
 \____  ___/ http://tinyfiledialogs.sourceforge.net
      \|     git clone http://git.code.sf.net/p/tinyfiledialogs/code tinyfd
         ____________________________________________
	    |                                            |
	    |   email: tinyfiledialogs at ysengrin.com   |
	    |____________________________________________|

If you like tinyfiledialogs, please upvote my stackoverflow answer
https://stackoverflow.com/a/47651444

- License -
 This software is provided 'as-is', without any express or implied
 warranty.  In no event will the authors be held liable for any damages
 arising from the use of this software.
 Permission is granted to anyone to use this software for any purpose,
 including commercial applications, and to alter it and redistribute it
 freely, subject to the following restrictions:
 1. The origin of this software must not be misrepresented; you must not
 claim that you wrote the original software.  If you use this software
 in a product, an acknowledgment in the product documentation would be
 appreciated but is not required.
 2. Altered source versions must be plainly marked as such, and must not be
 misrepresented as being the original software.
 3. This notice may not be removed or altered from any source distribution.
*/

using System;
using System.Runtime.InteropServices;

namespace NativeServices
{
    class NativeFileDialog
    {
        private const string dll32 = "tinyfiledialogs32.dll";
        private const string dll64 = "tinyfiledialogs64.dll";
        
        
        [DllImport(dll32, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tinyfd_beep")]
            private static extern void Beep_x32();
        [DllImport(dll64, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tinyfd_beep")]
            private static extern void Beep_x64();
        
        
        
        // cross platform UTF8
        [DllImport(dll32, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tinyfd_notifyPopup")]
            private static extern int NotifyPopup_x32(string aTitle, string aMessage, string aIconType);
        [DllImport(dll64, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tinyfd_notifyPopup")]
            private static extern int NotifyPopup_x64(string aTitle, string aMessage, string aIconType);
        
        [DllImport(dll32, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tinyfd_messageBox")]
            private static extern int MessageBox_x32(string aTitle, string aMessage, string aDialogType, string aIconType, int aDefaultButton);
        [DllImport(dll64, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tinyfd_messageBox")]
            private static extern int MessageBox_x64(string aTitle, string aMessage, string aDialogType, string aIconType, int aDefaultButton);
        
        [DllImport(dll32, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tinyfd_inputBox")]
            private static extern IntPtr InputBox_x32(string aTitle, string aMessage, string aDefaultInput);
        [DllImport(dll64, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tinyfd_inputBox")]
            private static extern IntPtr InputBox_x64(string aTitle, string aMessage, string aDefaultInput);
        
        [DllImport(dll32, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tinyfd_saveFileDialog")]
            private static extern IntPtr SaveFileDialog_x32(string aTitle, string aDefaultPathAndFile, int aNumOfFilterPatterns, string[] aFilterPatterns, string aSingleFilterDescription);
        [DllImport(dll64, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tinyfd_saveFileDialog")]
            private static extern IntPtr SaveFileDialog_x64(string aTitle, string aDefaultPathAndFile, int aNumOfFilterPatterns, string[] aFilterPatterns, string aSingleFilterDescription);
        
        [DllImport(dll32, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tinyfd_openFileDialog")]
            private static extern IntPtr OpenFileDialog_x32(string aTitle, string aDefaultPathAndFile, int aNumOfFilterPatterns, string[] aFilterPatterns, string aSingleFilterDescription, int aAllowMultipleSelects);
        [DllImport(dll64, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tinyfd_openFileDialog")]
            private static extern IntPtr OpenFileDialog_x64(string aTitle, string aDefaultPathAndFile, int aNumOfFilterPatterns, string[] aFilterPatterns, string aSingleFilterDescription, int aAllowMultipleSelects);
        
        [DllImport(dll32, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tinyfd_selectFolderDialog")]
            private static extern IntPtr SelectFolderDialog_x32(string aTitle, string aDefaultPathAndFile);
        [DllImport(dll64, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tinyfd_selectFolderDialog")]
            private static extern IntPtr SelectFolderDialog_x64(string aTitle, string aDefaultPathAndFile);
        
        [DllImport(dll32, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tinyfd_colorChooser")]
            private static extern IntPtr ColorChooser_x32(string aTitle, string aDefaultHexRGB, byte[] aDefaultRGB, byte[] aoResultRGB);
        [DllImport(dll64, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tinyfd_colorChooser")]
            private static extern IntPtr ColorChooser_x64(string aTitle, string aDefaultHexRGB, byte[] aDefaultRGB, byte[] aoResultRGB);
        
        

        // windows only utf16
        [DllImport(dll32, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tinyfd_notifyPopupW")]
            private static extern int NotifyPopupW_x32(string aTitle, string aMessage, string aIconType);
        [DllImport(dll64, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tinyfd_notifyPopupW")]
            private static extern int NotifyPopupW_x64(string aTitle, string aMessage, string aIconType);
        
        [DllImport(dll32, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tinyfd_messageBoxW")]
            private static extern int MessageBoxW_x32(string aTitle, string aMessage, string aDialogType, string aIconType, int aDefaultButton);
        [DllImport(dll64, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tinyfd_messageBoxW")]
            private static extern int MessageBoxW_x64(string aTitle, string aMessage, string aDialogType, string aIconType, int aDefaultButton);
        
        [DllImport(dll32, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tinyfd_inputBoxW")]
            private static extern IntPtr InputBoxW_x32(string aTitle, string aMessage, string aDefaultInput);
        [DllImport(dll64, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tinyfd_inputBoxW")]
            private static extern IntPtr InputBoxW_x64(string aTitle, string aMessage, string aDefaultInput);
        
        [DllImport(dll32, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tinyfd_saveFileDialogW")]
            private static extern IntPtr SaveFileDialogW_x32(string aTitle, string aDefaultPathAndFile, int aNumOfFilterPatterns, string[] aFilterPatterns, string aSingleFilterDescription);
        [DllImport(dll64, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tinyfd_saveFileDialogW")]
            private static extern IntPtr SaveFileDialogW_x64(string aTitle, string aDefaultPathAndFile, int aNumOfFilterPatterns, string[] aFilterPatterns, string aSingleFilterDescription);
        
        [DllImport(dll32, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tinyfd_openFileDialogW")]
            private static extern IntPtr OpenFileDialogW_x32(string aTitle, string aDefaultPathAndFile, int aNumOfFilterPatterns, string[] aFilterPatterns, string aSingleFilterDescription, int aAllowMultipleSelects);
        [DllImport(dll64, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tinyfd_openFileDialogW")]
            private static extern IntPtr OpenFileDialogW_x64(string aTitle, string aDefaultPathAndFile, int aNumOfFilterPatterns, string[] aFilterPatterns, string aSingleFilterDescription, int aAllowMultipleSelects);
        
        [DllImport(dll32, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tinyfd_selectFolderDialogW")]
            private static extern IntPtr SelectFolderDialogW_x32(string aTitle, string aDefaultPathAndFile);
        [DllImport(dll64, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tinyfd_selectFolderDialogW")]
            private static extern IntPtr SelectFolderDialogW_x64(string aTitle, string aDefaultPathAndFile);
        
        [DllImport(dll32, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tinyfd_colorChooserW")]
            private static extern IntPtr ColorChooserW_x32(string aTitle, string aDefaultHexRGB, byte[] aDefaultRGB, byte[] aoResultRGB);
        [DllImport(dll64, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tinyfd_colorChooserW")]
            private static extern IntPtr ColorChooserW_x64(string aTitle, string aDefaultHexRGB, byte[] aDefaultRGB, byte[] aoResultRGB);
        
        // cross platform
        [DllImport(dll32, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tinyfd_getGlobalChar")]
            private static extern IntPtr GetGlobalChar_x32(string aCharVariableName);
        [DllImport(dll64, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tinyfd_getGlobalChar")]
            private static extern IntPtr GetGlobalChar_x64(string aCharVariableName);
        
        [DllImport(dll32, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tinyfd_getGlobalInt")]
            private static extern int GetGlobalInt_x32(string aIntVariableName);
        [DllImport(dll64, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tinyfd_getGlobalInt")]
            private static extern int GetGlobalInt_x64(string aIntVariableName);
        
        [DllImport(dll32, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tinyfd_setGlobalInt")]
            private static extern int SetGlobalInt_x32(string aIntVariableName, int aValue);
        [DllImport(dll64, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tinyfd_setGlobalInt")]
            private static extern int SetGlobalInt_x64(string aIntVariableName, int aValue);
        
        // ******** a complicated way to access tinyfd's global variables
        // [DllImport("kernel32.dll", SetLastError = true)] internal static extern IntPtr GetProcAddress(IntPtr hModule, string procName);
        // [DllImport("kernel32.dll", SetLastError = true)] internal static extern IntPtr LoadLibrary(string lpszLib);
        
        public const string IconTypeInfo = "Info";
        public const string IconTypeWarning = "Warning";
        public const string IconTypeError = "Error";
        public const string IconTypeQuestion = "Question";
        
        public const string DialogTypeOK = "ok";
        public const string DialogTypeOKCancel = "okcancel";
        public const string DialogTypeYesNo = "yesno";
        public const string DialogTypeYesNoCancel = "yesnocancel";
        
        public enum DialogButton
        {
            Cancel = 0,
            No = 0,
            Ok = 1,
            Yes = 1,
            NoTriplet = 2
        }
        
        // Helpers
        private static string StringFromAnsi(IntPtr ptr) // for UTF-8/char
        {
            return Marshal.PtrToStringAnsi(ptr);
        }

        private static string StringFromUni(IntPtr ptr) // for UTF-16/wchar_t
        {
            return Marshal.PtrToStringUni(ptr);
        }
		
		private static string GetString(IntPtr ptr)
		{
			if (Environment.Is64BitProcess)
			{
				return StringFromUni(ptr);
			}
			
			return StringFromAnsi(ptr);
		}
        
        private static bool IsPointerValid(IntPtr ptr)
        {
            return !(ptr == IntPtr.Zero || ptr == null);
        }
        
        // --- Custom Naming Conventions (with optional parameters included)
        public static void Beep()
        {
            if (Environment.Is64BitProcess)
            {
                Beep_x64();
            }
            else
            {
                Beep_x32();
            }
        }
        
        public static void NotifyPopup(string title, string message, string iconType = IconTypeInfo)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (Environment.Is64BitProcess)
                {
                    NotifyPopupW_x64(title, message, iconType);
                }
                else
                {
                    NotifyPopupW_x32(title, message, iconType);
                }
            }
            else
            {
                if (Environment.Is64BitProcess)
                {
                    NotifyPopup_x64(title, message, iconType);
                }
                else
                {
                    NotifyPopup_x32(title, message, iconType);
                }
            }
        }
        
        public static DialogButton MessageBox(string title, string message, string dialogType, string iconType, DialogButton defaultButton = DialogButton.Ok)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (Environment.Is64BitProcess)
                {
                    return (DialogButton)MessageBoxW_x64(title, message, dialogType, iconType, (int)defaultButton);
                }
                else
                {
                    return (DialogButton)MessageBoxW_x32(title, message, dialogType, iconType, (int)defaultButton);
                }
            }
            else
            {
                if (Environment.Is64BitProcess)
                {
                    return (DialogButton)MessageBox_x64(title, message, dialogType, iconType, (int)defaultButton);
                }
                else
                {
                    return (DialogButton)MessageBox_x32(title, message, dialogType, iconType, (int)defaultButton);
                }
            }
        }
        
        public static string InputBox(string title, string message, string defaultInput)
        {
            IntPtr ptr;
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (Environment.Is64BitProcess)
                {
                    ptr = InputBoxW_x64(title, message, defaultInput);
                }
                else
                {
                    ptr = InputBoxW_x32(title, message, defaultInput);
                }
            }
            else
            {
                if (Environment.Is64BitProcess)
                {
                    ptr = InputBox_x64(title, message, defaultInput);
                }
                else
                {
                    ptr = InputBox_x32(title, message, defaultInput);
                }
            }
            
            if (!IsPointerValid(ptr))
            {
                return null;
            }
            
            var str = GetString(ptr);
            return str;
        }
        
        public static string SaveFileDialog(string title, string defaultPath, string[] filters = null, string filterDescription = "Files")
        {
            var count = 0;
            if (filters != null)
            {
                count = filters.Length;
            }
            
            IntPtr ptr;
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (Environment.Is64BitProcess)
                {
                    ptr = SaveFileDialogW_x64(title, defaultPath, count, filters, filterDescription);
                }
                else
                {
                    ptr = SaveFileDialogW_x32(title, defaultPath, count, filters, filterDescription);
                }
            }
            else
            {
                if (Environment.Is64BitProcess)
                {
                    ptr = SaveFileDialog_x64(title, defaultPath, count, filters, filterDescription);
                }
                else
                {
                    ptr = SaveFileDialog_x32(title, defaultPath, count, filters, filterDescription);
                }
            }
            
            if (!IsPointerValid(ptr))
            {
                return null;
            }
            
            var str = GetString(ptr);
            return str;
        }
        
        public static string[] OpenFileDialog(string title, string defaultPath, string[] filters = null, string filterDescription = "Files", bool allowMultiple = false)
        {
            var count = 0;
            if (filters != null)
            {
                count = filters.Length;
            }
            
            IntPtr ptr;
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (Environment.Is64BitProcess)
                {
                    ptr = OpenFileDialogW_x64(title, defaultPath, count, filters, filterDescription, allowMultiple ? 2 : 0);
                }
                else
                {
                    ptr = OpenFileDialogW_x32(title, defaultPath, count, filters, filterDescription, allowMultiple ? 2 : 0);
                }
            }
            else
            {
                if (Environment.Is64BitProcess)
                {
                    ptr = OpenFileDialog_x64(title, defaultPath, count, filters, filterDescription, allowMultiple ? 2 : 0);
                }
                else
                {
                    ptr = OpenFileDialog_x32(title, defaultPath, count, filters, filterDescription, allowMultiple ? 2 : 0);
                }
            }
            
            if (!IsPointerValid(ptr))
            {
                return null;
            }
            
            var str = GetString(ptr);
            return str.Split('|');
        }
        
        public static string SelectFolderDialog(string title, string defaultPath)
        {
            IntPtr ptr;
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (Environment.Is64BitProcess)
                {
                    ptr = SelectFolderDialogW_x64(title, defaultPath);
                }
                else
                {
                    ptr = SelectFolderDialogW_x32(title, defaultPath);
                }
            }
            else
            {
                if (Environment.Is64BitProcess)
                {
                    ptr = SelectFolderDialog_x64(title, defaultPath);
                }
                else
                {
                    ptr = SelectFolderDialog_x32(title, defaultPath);
                }
            }
            
            if (!IsPointerValid(ptr))
            {
                return null;
            }
            
            var str = GetString(ptr);
            return str;
        }
        
        public static byte[] ColorPicker(string title, byte[] defaultColor)
        {
            byte[] outputColor = (byte[])defaultColor.Clone();
            
            IntPtr ptr;
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (Environment.Is64BitProcess)
                {
                    ptr = ColorChooserW_x64(title, null, defaultColor, outputColor);
                }
                else
                {
                    ptr = ColorChooserW_x32(title, null, defaultColor, outputColor);
                }
            }
            else
            {
                if (Environment.Is64BitProcess)
                {
                    ptr = ColorChooser_x64(title, null, defaultColor, outputColor);
                }
                else
                {
                    ptr = ColorChooser_x32(title, null, defaultColor, outputColor);
                }
            }
            
            if (!IsPointerValid(ptr))
            {
                return null;
            }
            
            //var str = StringFromAnsi(ptr);
            //return str;
            
            return outputColor;
        }
    }
}

/*
namespace ConsoleApplication1
{
    class tinyfiledialogsTest
    {
        private static string stringFromAnsi(IntPtr ptr) // for UTF-8/char
        {
            return System.Runtime.InteropServices.Marshal.PtrToStringAnsi(ptr);
        }

        private static string stringFromUni(IntPtr ptr) // for UTF-16/wchar_t
        {
            return System.Runtime.InteropServices.Marshal.PtrToStringUni(ptr);
        }

        static void Main(string[] args)
        {
            // ******** a simple way to access tinyfd's global variables
            IntPtr lTheVersionText = tinyfd.getGlobalChar("version");
            string lTheVersionString = stringFromAnsi(lTheVersionText);
            tinyfd.messageBox("version", lTheVersionString, "ok", "info", 1);

            // cross platform utf-8
            IntPtr lTheInputText = tinyfd.inputBox("input box", "gimme a string", "A text to input");
            string lTheInputString = stringFromAnsi(lTheInputText);
            int lala = tinyfd.messageBox("a message box char", lTheInputString, "ok", "warning", 1);

            // windows only utf-16
            IntPtr lAnotherInputTextW = tinyfd.inputBoxW("input box", "gimme another string", "Another text to input");
            string lAnotherInputString = stringFromUni(lAnotherInputTextW);
            int lili = tinyfd.messageBoxW("a message box wchar_t", lAnotherInputString, "ok", "info", 1);

            tinyfd.notifyPopupW("there is no warning (even if it is a warning icon)", lTheVersionString, "warning");

            tinyfd.beep();

            // ******** a complicated way to access tinyfd's global variables (uncomment the 2 lines in the class tinyfd above)
            // IntPtr DLL = tinyfd.LoadLibrary(tinyfd.mDllLocation);
            // if (DLL != IntPtr.Zero)
            // {
            //    IntPtr lVersionAddr = tinyfd.GetProcAddress(DLL, "version");
            //    string lVersion = stringFromAnsi(lVersionAddr);
            //    IntPtr lForceConsoleAddr = tinyfd.GetProcAddress(DLL, "forceConsole");
            //    if (lForceConsoleAddr != IntPtr.Zero)
            //    {
            //        int lForceConsoleValue = Marshal.ReadInt32(lForceConsoleAddr);
            //        tinyfd.notifyPopup(lVersion, lForceConsoleValue.ToString(), "info");
            //        Marshal.WriteInt32(lForceConsoleAddr, 0);
            //    }
            // }
        }
    }
}
*/
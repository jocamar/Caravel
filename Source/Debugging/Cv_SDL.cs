#region License
/* SDL2# - C# Wrapper for SDL2
 *
 * Copyright (c) 2013-2016 Ethan Lee.
 *
 * This software is provided 'as-is', without any express or implied warranty.
 * In no event will the authors be held liable for any damages arising from
 * the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 * claim that you wrote the original software. If you use this software in a
 * product, an acknowledgment in the product documentation would be
 * appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not be
 * misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source distribution.
 *
 * Ethan "flibitijibibo" Lee <flibitijibibo@flibitijibibo.com>
 *
 */

 /*
 * Adapted by João Marinheiro - 03/06/2018
 * Removed unneeded code and changed namespaces.
 */
#endregion

#region Using Statements
using System;
using System.Runtime.InteropServices;
#endregion

namespace Caravel.Debugging.SDLWrapper
{
	public static class SDL
	{
		#region SDL2# Variables

		private const string nativeLibName = "SDL2.dll";

		#endregion

		#region UTF8 Marshaling

		internal static byte[] UTF8_ToNative(string s)
		{
			if (s == null)
			{
				return null;
			}

			// Add a null terminator. That's kind of it... :/
			return System.Text.Encoding.UTF8.GetBytes(s + '\0');
		}

		internal static unsafe string UTF8_ToManaged(IntPtr s, bool freePtr = false)
		{
			if (s == IntPtr.Zero)
			{
				return null;
			}

			/* We get to do strlen ourselves! */
			byte* ptr = (byte*) s;
			while (*ptr != 0)
			{
				ptr++;
			}

			/* TODO: This #ifdef is only here because the equivalent
			 * .NET 2.0 constructor appears to be less efficient?
			 * Here's the pretty version, maybe steal this instead:
			 *
			string result = new string(
				(sbyte*) s, // Also, why sbyte???
				0,
				(int) (ptr - (byte*) s),
				System.Text.Encoding.UTF8
			);
			 * See the CoreCLR source for more info.
			 * -flibit
			 */
#if NETSTANDARD2_0
			/* Modern C# lets you just send the byte*, nice! */
			string result = System.Text.Encoding.UTF8.GetString(
				(byte*) s,
				(int) (ptr - (byte*) s)
			);
#else
			/* Old C# requires an extra memcpy, bleh! */
			byte[] bytes = new byte[ptr - (byte*) s];
			Marshal.Copy(s, bytes, 0, bytes.Length);
			string result = System.Text.Encoding.UTF8.GetString(bytes);
#endif

			/* Some SDL functions will malloc, we have to free! */
			if (freePtr)
			{
				SDL_free(s);
			}
			return result;
		}

		#endregion

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr SDL_malloc(IntPtr size);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void SDL_free(IntPtr memblock);

		#region SDL_messagebox.h

		[Flags]
		public enum SDL_MessageBoxFlags : uint
		{
			SDL_MESSAGEBOX_ERROR =		0x00000010,
			SDL_MESSAGEBOX_WARNING =	0x00000020,
			SDL_MESSAGEBOX_INFORMATION =	0x00000040
		}

		[Flags]
		public enum SDL_MessageBoxButtonFlags : uint
		{
			SDL_MESSAGEBOX_BUTTON_RETURNKEY_DEFAULT = 0x00000001,
			SDL_MESSAGEBOX_BUTTON_ESCAPEKEY_DEFAULT = 0x00000002
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct INTERNAL_SDL_MessageBoxButtonData
		{
			public SDL_MessageBoxButtonFlags flags;
			public int buttonid;
			public IntPtr text; /* The UTF-8 button text */
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_MessageBoxButtonData
		{
			public SDL_MessageBoxButtonFlags flags;
			public int buttonid;
			public string text; /* The UTF-8 button text */
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_MessageBoxColor
		{
			public byte r, g, b;
		}

		public enum SDL_MessageBoxColorType
		{
			SDL_MESSAGEBOX_COLOR_BACKGROUND,
			SDL_MESSAGEBOX_COLOR_TEXT,
			SDL_MESSAGEBOX_COLOR_BUTTON_BORDER,
			SDL_MESSAGEBOX_COLOR_BUTTON_BACKGROUND,
			SDL_MESSAGEBOX_COLOR_BUTTON_SELECTED,
			SDL_MESSAGEBOX_COLOR_MAX
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_MessageBoxColorScheme
		{
			[MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct, SizeConst = (int)SDL_MessageBoxColorType.SDL_MESSAGEBOX_COLOR_MAX)]
				public SDL_MessageBoxColor[] colors;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct INTERNAL_SDL_MessageBoxData
		{
			public SDL_MessageBoxFlags flags;
			public IntPtr window;				/* Parent window, can be NULL */
			public IntPtr title;				/* UTF-8 title */
			public IntPtr message;				/* UTF-8 message text */
			public int numbuttons;
			public IntPtr buttons;
			public IntPtr colorScheme;			/* Can be NULL to use system settings */
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_MessageBoxData
		{
			public SDL_MessageBoxFlags flags;
			public IntPtr window;				/* Parent window, can be NULL */
			public string title;				/* UTF-8 title */
			public string message;				/* UTF-8 message text */
			public int numbuttons;
			public SDL_MessageBoxButtonData[] buttons;
			public SDL_MessageBoxColorScheme? colorScheme;	/* Can be NULL to use system settings */
		}

		[DllImport(nativeLibName, EntryPoint = "SDL_ShowMessageBox", CallingConvention = CallingConvention.Cdecl)]
		private static extern int INTERNAL_SDL_ShowMessageBox([In()] ref INTERNAL_SDL_MessageBoxData messageboxdata, out int buttonid);

		/* Ripped from Jameson's LpUtf8StrMarshaler */
		private static IntPtr INTERNAL_AllocUTF8(string str)
		{
			if (string.IsNullOrEmpty(str))
			{
				return IntPtr.Zero;
			}
			byte[] bytes = System.Text.Encoding.UTF8.GetBytes(str + '\0');
			IntPtr mem = SDL.SDL_malloc((IntPtr) bytes.Length);
			Marshal.Copy(bytes, 0, mem, bytes.Length);
			return mem;
		}

		public static unsafe int SDL_ShowMessageBox([In()] ref SDL_MessageBoxData messageboxdata, out int buttonid)
		{
			var data = new INTERNAL_SDL_MessageBoxData()
			{
				flags = messageboxdata.flags,
				window = messageboxdata.window,
				title = INTERNAL_AllocUTF8(messageboxdata.title),
				message = INTERNAL_AllocUTF8(messageboxdata.message),
				numbuttons = messageboxdata.numbuttons,
			};

			var buttons = new INTERNAL_SDL_MessageBoxButtonData[messageboxdata.numbuttons];
			for (int i = 0; i < messageboxdata.numbuttons; i++)
			{
				buttons[i] = new INTERNAL_SDL_MessageBoxButtonData()
				{
					flags = messageboxdata.buttons[i].flags,
					buttonid = messageboxdata.buttons[i].buttonid,
					text = INTERNAL_AllocUTF8(messageboxdata.buttons[i].text),
				};
			}

			if (messageboxdata.colorScheme != null)
			{
				data.colorScheme = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(SDL_MessageBoxColorScheme)));
				Marshal.StructureToPtr(messageboxdata.colorScheme.Value, data.colorScheme, false);
			}

			int result;
			fixed (INTERNAL_SDL_MessageBoxButtonData* buttonsPtr = &buttons[0])
			{
				data.buttons = (IntPtr)buttonsPtr;
				result = INTERNAL_SDL_ShowMessageBox(ref data, out buttonid);
			}

			Marshal.FreeHGlobal(data.colorScheme);
			for (int i = 0; i < messageboxdata.numbuttons; i++)
			{
				SDL_free(buttons[i].text);
			}
			SDL_free(data.message);
			SDL_free(data.title);

			return result;
		}

		/* window refers to an SDL_Window* */
		[DllImport(nativeLibName, EntryPoint = "SDL_ShowSimpleMessageBox", CallingConvention = CallingConvention.Cdecl)]
		private static extern int INTERNAL_SDL_ShowSimpleMessageBox(
			SDL_MessageBoxFlags flags,
			byte[] title,
			byte[] message,
			IntPtr window
		);
		public static int SDL_ShowSimpleMessageBox(
			SDL_MessageBoxFlags flags,
			string title,
			string message,
			IntPtr window
		) {
			return INTERNAL_SDL_ShowSimpleMessageBox(
				flags,
				UTF8_ToNative(title),
				UTF8_ToNative(message),
				window
			);
		}

		#endregion

	}
}

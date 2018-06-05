using Microsoft.Xna.Framework;
using Caravel.Debugging.SDLWrapper;
using System;

namespace Caravel.Debugging
{
    public class Cv_DebugDialog
    {
        public enum Cv_ErrorDialogResult
        {
            CV_LOGERROR_RETRY,
            CV_LOGERROR_ABORT,
            CV_LOGERROR_IGNORE
        }

        public enum Cv_MessageType
        {
            CV_MESSAGE_WARNING,
            CV_MESSAGE_INFO,
            CV_MESSAGE_ERROR,
            CV_MESSAGE_OTHER
        };

        public enum Cv_ButtonType
        {
            CV_BUTTON_IGNORE,
            CV_BUTTON_CONTINUE,
            CV_BUTTON_ABORT,
            CV_BUTTON_RETRY,
            CV_BUTTON_CANCEL,
            CV_BUTTON_QUIT,
            CV_BUTTON_ACCEPT,
            CV_BUTTON_REJECT
        };

        public struct Cv_MessageBoxParams
        {
            public Cv_MessageType messageType;
            public Cv_ButtonType[] buttons;
            public Cv_ButtonType defaultButton;
            public string title;
            public string message;
            public Color bgColor;
            public Color textColor;
            public Color btBorderColor;
            public Color btSelectedColor;
            public Color btBgColor;
        };

        public static bool ShowMessageBox(Cv_MessageBoxParams mBoxParams, out Cv_ButtonType result)
        {
            var colorScheme = new SDL.SDL_MessageBoxColorScheme();

            var sdlBgColor = new SDL.SDL_MessageBoxColor();
            sdlBgColor.r = mBoxParams.bgColor.R;
            sdlBgColor.g = mBoxParams.bgColor.G;
            sdlBgColor.b = mBoxParams.bgColor.B;

            var sdlTextColor = new SDL.SDL_MessageBoxColor();
            sdlTextColor.r = mBoxParams.textColor.R;
            sdlTextColor.g = mBoxParams.textColor.G;
            sdlTextColor.b = mBoxParams.textColor.B;

            var sdlBorderColor = new SDL.SDL_MessageBoxColor();
            sdlBorderColor.r = mBoxParams.btBorderColor.R;
            sdlBorderColor.g = mBoxParams.btBorderColor.G;
            sdlBorderColor.b = mBoxParams.btBorderColor.B;

            var sdlBtBgColor = new SDL.SDL_MessageBoxColor();
            sdlBtBgColor.r = mBoxParams.btBgColor.R;
            sdlBtBgColor.g = mBoxParams.btBgColor.G;
            sdlBtBgColor.b = mBoxParams.btBgColor.B;

            var sdlBtSelectedColor = new SDL.SDL_MessageBoxColor();
            sdlBtSelectedColor.r = mBoxParams.btSelectedColor.R;
            sdlBtSelectedColor.g = mBoxParams.btSelectedColor.G;
            sdlBtSelectedColor.b = mBoxParams.btSelectedColor.B;

            colorScheme.colors = new SDL.SDL_MessageBoxColor[] {
                sdlBgColor, sdlTextColor, sdlBorderColor, sdlBtBgColor, sdlBtSelectedColor
            };

            SDL.SDL_MessageBoxFlags flags;
            
            switch (mBoxParams.messageType)
            {
                case Cv_MessageType.CV_MESSAGE_ERROR:
                    flags = SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR;
                    break;
                case Cv_MessageType.CV_MESSAGE_WARNING:
                    flags = SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_WARNING;
                    break;
                default:
                    flags = SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_INFORMATION;
                    break;
            }

            var btArray = new SDL.SDL_MessageBoxButtonData[mBoxParams.buttons.Length];

            for(var i = 0; i < mBoxParams.buttons.Length; i++)
            {
                btArray[i] = new SDL.SDL_MessageBoxButtonData();
                if (mBoxParams.buttons[i] == mBoxParams.defaultButton)
                {
                    btArray[i].flags = SDL.SDL_MessageBoxButtonFlags.SDL_MESSAGEBOX_BUTTON_RETURNKEY_DEFAULT;
                }
                else
                {
                    btArray[i].flags = 0;
                }

                btArray[i].text = GetButtonText(mBoxParams.buttons[i]);
                btArray[i].buttonid = (int) mBoxParams.buttons[i];
            }

            var messageBoxData = new SDL.SDL_MessageBoxData();
            messageBoxData.flags = flags;
            messageBoxData.window = IntPtr.Zero;
            messageBoxData.title = mBoxParams.title;
            messageBoxData.message = mBoxParams.message;
            messageBoxData.numbuttons = mBoxParams.buttons.Length;
            messageBoxData.buttons = btArray;
            messageBoxData.colorScheme = colorScheme;

            // show the dialog box
            int sdlResult;
            if (SDL.SDL_ShowMessageBox(ref messageBoxData, out sdlResult) < 0) {
                result = Cv_ButtonType.CV_BUTTON_ABORT;
                return false;
            }

            result = (Cv_ButtonType) sdlResult;
            return true;
        }

        private static string GetButtonText(Cv_ButtonType btType)
        {
            switch (btType) {
                case Cv_ButtonType.CV_BUTTON_IGNORE:    return "Ignore";
                case Cv_ButtonType.CV_BUTTON_CONTINUE:  return "Continue";
                case Cv_ButtonType.CV_BUTTON_ABORT:     return "Abort";
                case Cv_ButtonType.CV_BUTTON_RETRY:     return "Retry";
                case Cv_ButtonType.CV_BUTTON_CANCEL:    return "Cancel";
                case Cv_ButtonType.CV_BUTTON_QUIT:      return "Quit";
                case Cv_ButtonType.CV_BUTTON_ACCEPT:    return "Accept";
                default: return "Reject";
            }
        }
    }
}
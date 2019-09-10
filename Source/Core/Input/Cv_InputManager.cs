using System;
using System.Collections.Generic;
using System.Xml;
using Caravel.Debugging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Caravel.Core.Input
{
    public class Cv_InputManager
    {
		public enum Cv_MouseAction {
			Left,
			Right,
			Middle,
			X1,
			X2,
			MouseWheelUp,
			MouseWheelDown,
			MouseMove
		}

		public struct Cv_MouseValues {
			public Vector2 MousePos;
			public int MouseWheelVal;
		}

        public struct Cv_GamepadValues
        {
            public Vector2 RightThumbstick;
            public Vector2 LeftThumbstick;
            public float RightTrigger;
            public float LeftTrigger;
        }

		public static Cv_InputManager Instance
        {
            get; private set;
        }

        //Declare Dictionary Variables
        private Dictionary<string, Keys>[] m_BindedKeys = new [] {
			new Dictionary<string, Keys>(),
			new Dictionary<string, Keys>(),
			new Dictionary<string, Keys>(),
			new Dictionary<string, Keys>()
		};

		private Dictionary<string, Buttons>[] m_BindedButtons = new [] {
			new Dictionary<string, Buttons>(),
			new Dictionary<string, Buttons>(),
			new Dictionary<string, Buttons>(),
			new Dictionary<string, Buttons>()
		};

		private Dictionary<string, Cv_MouseAction>[] m_BindedMouseActions = new [] {
			new Dictionary<string, Cv_MouseAction>(),
			new Dictionary<string, Cv_MouseAction>(),
			new Dictionary<string, Cv_MouseAction>(),
			new Dictionary<string, Cv_MouseAction>()
		};

		//Declare Keyboard State Variables
		private KeyboardState m_KeyboardState;     //Stores the current Keyboard state
		private KeyboardState m_LastKeyboardState; //Stores the previous Keyboard state

		//Declare Mouse State Variables
		private MouseState m_MouseState;     //Stores the current Keyboard state
		private MouseState m_LastMouseState; //Stores the previous Keyboard state

		//Declare GamePad variables
		private GamePadState[] m_GamePadStates;        //Stores an array of the current game pad states
		private GamePadState[] m_LastGamePadStates;    //Stores an array of the previous game pad states

        public Cv_MouseValues GetMouseValues()
		{
			var mouseVals = new Cv_MouseValues();
			mouseVals.MousePos = new Vector2(m_MouseState.X, m_MouseState.Y);
			mouseVals.MouseWheelVal = m_MouseState.ScrollWheelValue;
			return mouseVals;
		}

		public Cv_GamepadValues GetGamepadValues(Cv_Player player)
        {
            var gamepadVals = new Cv_GamepadValues();
            var gamepadState = m_GamePadStates[player-1];
            gamepadVals.LeftThumbstick = gamepadState.ThumbSticks.Left;
            gamepadVals.RightThumbstick = gamepadState.ThumbSticks.Right;
            gamepadVals.LeftTrigger = gamepadState.Triggers.Left;
            gamepadVals.RightTrigger = gamepadState.Triggers.Right;
            return gamepadVals;
        }

        public void BindCommand(Cv_Player player, string command, Keys key)
		{
			m_BindedKeys[player-1][command] = key;
		}

		public void BindCommand(Cv_Player player, string command, Buttons button)
		{
			m_BindedButtons[player-1][command] = button;
        }

		public void BindCommand(Cv_Player player, string command, Cv_MouseAction button)
		{
			m_BindedMouseActions[player-1][command] = button;
        }

        public void UnbindCommand(Cv_Player player, string command)
        {
            m_BindedKeys[player-1].Remove(command);
            m_BindedButtons[player-1].Remove(command);
            m_BindedMouseActions[player-1].Remove(command);
        }

        public bool IsCommandBound(Cv_Player player, string command)
        {
            if (m_BindedKeys[player-1].ContainsKey(command)
                || m_BindedMouseActions[player-1].ContainsKey(command)
                || m_BindedButtons[player-1].ContainsKey(command))
                return true;

            return false;
        }

		public bool CommandDeactivated(string command, Cv_Player player)
		{
			//Determine if the Keyboard Input was released
			if (m_BindedKeys[player-1].ContainsKey(command)
				&& KeyReleased(m_BindedKeys[player-1][command]))
			{
				//return true (no need to check game pad)
				return true;
			}

			if (m_BindedMouseActions[player-1].ContainsKey(command)
				&& MouseButtonReleased(m_BindedMouseActions[player-1][command]))
			{
				return true;
			}

			//Determine if the passed player's game pad input was released
			if (m_BindedButtons[player-1].ContainsKey(command)
				&& ButtonReleased(m_BindedButtons[player-1][command], player))
			{
				return true;
			}

			//Otherwise return false
			return false;
		}

		public bool CommandActivated(string command, Cv_Player player)
		{
			//Determine if the Keyboard input was pressed
			if (m_BindedKeys[player-1].ContainsKey(command)
				&& KeyPressed(m_BindedKeys[player-1][command]))
			{
				//Return true (no need to check game pad)
				return true;
			}

			if (m_BindedMouseActions[player-1].ContainsKey(command)
				&& MouseButtonPressed(m_BindedMouseActions[player-1][command]))
			{
				return true;
			}

			//Determine if the passed player's game pad input was pressed
			if (m_BindedButtons[player-1].ContainsKey(command)
				&& ButtonPressed(m_BindedButtons[player-1][command], player))
			{
				return true;
			}

			//Othwerwise return false
			return false;
		}

		public bool CommandActive(string command, Cv_Player player)
		{
			//Determine if the Keyboard input is held down
			if (m_BindedKeys[player-1].ContainsKey(command)
				&& KeyDown(m_BindedKeys[player-1][command]))
			{
				//return true (no need to check game pad)
				return true;
			}

			if (m_BindedMouseActions[player-1].ContainsKey(command)
				&& m_BindedMouseActions[player-1][command] == Cv_MouseAction.MouseWheelDown && MouseWheelDown())
			{
				return true;
			}

			if (m_BindedMouseActions[player-1].ContainsKey(command)
				&& m_BindedMouseActions[player-1][command] == Cv_MouseAction.MouseWheelUp && MouseWheelUp())
			{
				return true;
			}

			if (m_BindedMouseActions[player-1].ContainsKey(command)
				&& m_BindedMouseActions[player-1][command] == Cv_MouseAction.MouseMove && MouseMoved())
			{
				return true;
			}
				
			if (m_BindedMouseActions[player-1].ContainsKey(command)
				&& MouseButtonDown(m_BindedMouseActions[player-1][command]))
			{
				return true;
			}

			//Determine if the active player's game pad input is held down
			if (m_BindedButtons[player-1].ContainsKey(command)
				&& ButtonDown(m_BindedButtons[player-1][command], player))
			{
				return true;
			}

			//otherwise return false
			return false;
		}

		internal Cv_InputManager()
		{
			Instance = this;

			//Setup the current keyboard state
			m_KeyboardState = Keyboard.GetState();

			//Setup the current mouse state
			m_MouseState = Mouse.GetState();

			//Setup game pad states array
			m_GamePadStates = new GamePadState[Enum.GetValues(typeof(PlayerIndex)).Length];

			//Loop through each player
			foreach (PlayerIndex index in Enum.GetValues(typeof(PlayerIndex)))
			{
				//Get the current player's game pad state
				m_GamePadStates[(int)index] = GamePad.GetState(index);
			}
		}

		internal bool Initialize()
		{
            LoadXML();
            return true;
		}

		internal void OnUpdate(float time, float elapsedTime)
		{
            //Set the previous keyboard state
            m_LastKeyboardState = m_KeyboardState;

            //Get the current keyboard state
            m_KeyboardState = Keyboard.GetState();

            //Set the previous mouse state
            m_LastMouseState = m_MouseState;

            //Get the current mouse state
            m_MouseState = Mouse.GetState();

            //Get the previous game pad states
            m_LastGamePadStates = (GamePadState[])m_GamePadStates.Clone();

            //Loop through each player
            foreach (PlayerIndex index in Enum.GetValues(typeof(PlayerIndex)))
            {
                //Get the current player's game pad state
                m_GamePadStates[(int)index] = GamePad.GetState(index);
            }
        }

        private bool KeyReleased(Keys key)
		{
			//Determine whether the parsed key is released or not
			return m_KeyboardState.IsKeyUp(key) && m_LastKeyboardState.IsKeyDown(key);
		}

		private bool KeyPressed(Keys key)
		{
			//Determine whether the parsed key is pressed or not
			return m_KeyboardState.IsKeyDown(key) && m_LastKeyboardState.IsKeyUp(key);
		}

		private bool KeyDown(Keys key)
		{
			//Determine whether a key is currently down
			return m_KeyboardState.IsKeyDown(key);
		}

		private bool MouseButtonReleased(Cv_MouseAction action)
		{
			//Determine whether the parsed button is released or not
			return Action2Button(m_MouseState, action) == ButtonState.Released && Action2Button(m_LastMouseState, action) == ButtonState.Pressed;
		}

		private bool MouseButtonPressed(Cv_MouseAction action)
		{
			//Determine whether the parsed button is pressed or not
			return Action2Button(m_MouseState, action) == ButtonState.Pressed && Action2Button(m_LastMouseState, action) == ButtonState.Released;
		}

		private bool MouseButtonDown(Cv_MouseAction action)
		{
			//Determine whether a button is currently down
			return Action2Button(m_MouseState, action) == ButtonState.Pressed;
		}

		private bool MouseWheelDown()
		{
			return m_MouseState.ScrollWheelValue < m_LastMouseState.ScrollWheelValue;
		}

		private bool MouseWheelUp()
		{
			return m_MouseState.ScrollWheelValue > m_LastMouseState.ScrollWheelValue;
		}

		private bool MouseMoved()
		{
			return (m_MouseState.X != m_LastMouseState.X || m_MouseState.Y != m_LastMouseState.Y);
		}

		private bool ButtonReleased(Buttons button, Cv_Player index)
		{
			//Determine whether the button has been released
			return m_GamePadStates[index-1].IsButtonUp(button) && m_LastGamePadStates[index-1].IsButtonDown(button);
		}

		private bool ButtonPressed(Buttons button, Cv_Player index)
		{
			//Determine whether the button has been pressed
			return m_GamePadStates[index-1].IsButtonDown(button) && m_LastGamePadStates[index-1].IsButtonUp(button);
		}

		private bool ButtonDown(Buttons button, Cv_Player index)
		{
			//Determine whether the button is down
			return m_GamePadStates[index-1].IsButtonDown(button);
		}

		private static ButtonState Action2Button(MouseState state, Cv_MouseAction action)
		{
			switch (action) 
			{
			case Cv_MouseAction.Left:
				return state.LeftButton;
			case Cv_MouseAction.Middle:
				return state.MiddleButton;
			case Cv_MouseAction.Right:
				return state.RightButton;
			case Cv_MouseAction.X1:
				return state.XButton1;
			case Cv_MouseAction.X2:
				return state.XButton2;
			default:
				return ButtonState.Released;
			}
		}

        private void LoadXML()
        {
            if (CaravelApp.Instance.ControlBindingsLocation == null || CaravelApp.Instance.ControlBindingsLocation == "")
            {
                Cv_Debug.Log("Input", "No control bindings to load.");
                return;
            }

            foreach (var playerBindings in m_BindedButtons)
            {
                playerBindings.Clear();
            }

            foreach (var playerBindings in m_BindedKeys)
            {
                playerBindings.Clear();
            }

            foreach (var playerBindings in m_BindedMouseActions)
            {
                playerBindings.Clear();
            }

            XmlDocument doc = new XmlDocument();
            doc.Load(CaravelApp.Instance.ControlBindingsLocation);

            var root = doc.FirstChild;

            var bindings = root.SelectNodes("Bindings");

            foreach (XmlElement playerBindings in bindings)
            {
                var player = int.Parse(playerBindings.Attributes["player"].Value);
                Cv_Player pIndex;

                switch(player)
                {
                    case 1:
                        pIndex = Cv_Player.One;
                        break;
                    case 2:
                        pIndex = Cv_Player.Two;
                        break;
                    case 3:
                        pIndex = Cv_Player.Three;
                        break;
                    case 4:
                        pIndex = Cv_Player.Four;
                        break;
                    default:
                        pIndex = Cv_Player.One;
                        break;
                }

                var commands = playerBindings.SelectNodes("Command");

                foreach (XmlElement command in commands)
                {
                    var commandId = command.Attributes["id"].Value;
                    var inputType = command.Attributes["inputType"].Value;
                    var bindedTo = command.Attributes["bindedTo"].Value;

                    if (inputType == "keyboard")
                    {
                        Keys key;
                        if (Enum.TryParse(bindedTo, out key))
                        {
                            BindCommand(pIndex, commandId, key);
                        }
                    }
                    else if (inputType == "mouse")
                    {
                        Cv_MouseAction mouseAction;
                        if (Enum.TryParse(bindedTo, out mouseAction))
                        {
                            BindCommand(pIndex, commandId, mouseAction);
                        }
                    }
                    else if (inputType == "gamepad")
                    {
                        Buttons button;
                        if (Enum.TryParse(bindedTo, out button))
                        {
                            BindCommand(pIndex, commandId, button);
                        }
                    }
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Xml;
using Caravel.Core.Events;
using Caravel.Debugging;
using Microsoft.Xna.Framework;

namespace Caravel.Core.Entity
{
    public abstract class Cv_EntityComponent
    {
        public enum Cv_ComponentID
        {
            INVALID_COMPONENT = 0
        }

        public virtual Cv_ComponentID ID
        {
            get
            {
                if (m_ID == Cv_ComponentID.INVALID_COMPONENT)
                {
                    m_ID = GetID(GetType());
                }

                return m_ID;
            }
        }

        protected internal Cv_Entity Owner
        {
            get; internal set;
        }

        protected Cv_ListenerList Events;

        private static Dictionary<Type, Cv_ComponentID> m_ComponentIds = new Dictionary<Type, Cv_ComponentID>();
        private Cv_ComponentID m_ID = Cv_ComponentID.INVALID_COMPONENT;

        public static Cv_ComponentID GetID<ComponentType>() where ComponentType : Cv_EntityComponent
        {
            lock(m_ComponentIds)
            {
                Cv_ComponentID componentID;
                if (!m_ComponentIds.TryGetValue(typeof(ComponentType), out componentID))
                {
                    componentID = (Cv_ComponentID) typeof(ComponentType).Name.GetHashCode();
                    m_ComponentIds.Add(typeof(ComponentType), componentID);
                }

                return componentID;
            }
        }

        public static Cv_ComponentID GetID(string componentName)
        {
            return (Cv_ComponentID) componentName.GetHashCode();
        }

        public static string GetComponentName<ComponentType>() where ComponentType : Cv_EntityComponent
        {
            return typeof(ComponentType).Name;
        }

        public static string GetComponentName(Cv_EntityComponent component)
        {
            return component.GetType().Name;
        }

        public virtual XmlElement VToXML()
        {
            var info = CaravelApp.Instance.Logic.GetComponentInfo(this.ID);

            var elems = info.SelectNodes("Element");

            var doc = new XmlDocument();
            var componentElement = doc.CreateElement(info.Attributes["name"].Value);

            foreach (XmlElement elem in elems)
            {
                var nodeName = elem.Attributes["name"].Value;
                var type = elem.Attributes["type"].Value;
                string[] fieldNames = elem.Attributes["fieldNames"].Value.Split(',');
                string[] propertyNames = elem.Attributes["propertyNames"].Value.Split(',');

                var propertyElement = doc.CreateElement(nodeName);

                if (type == "int")
                {
                    if (propertyNames.Length == 1 && fieldNames.Length == 2)
                    {
                        //This is a vector2
                        PropertyInfo prop = GetType().GetProperty(propertyNames[0], BindingFlags.Public | BindingFlags.Instance);
                        if(null != prop && prop.CanWrite)
                        {
                            var propVec = (Vector2) prop.GetValue(this);
                            propertyElement.SetAttribute(fieldNames[0], ((int)propVec.X).ToString());
                            propertyElement.SetAttribute(fieldNames[1], ((int)propVec.Y).ToString());
                        }
                    }
                    else if (propertyNames.Length == 1 && fieldNames.Length == 3)
                    {
                        //This is a vector3
                        PropertyInfo prop = GetType().GetProperty(propertyNames[0], BindingFlags.Public | BindingFlags.Instance);
                        if(null != prop && prop.CanWrite)
                        {
                            var propVec = (Vector3) prop.GetValue(this);
                            propertyElement.SetAttribute(fieldNames[0], ((int)propVec.X).ToString());
                            propertyElement.SetAttribute(fieldNames[1], ((int)propVec.Y).ToString());
                            propertyElement.SetAttribute(fieldNames[2], ((int)propVec.Z).ToString());
                        }
                    }
                    else
                    {
                        for (var i = 0; i < fieldNames.Length; i++)
                        {
                            PropertyInfo prop = GetType().GetProperty(propertyNames[i], BindingFlags.Public | BindingFlags.Instance);
                            if(null != prop && prop.CanWrite)
                            {
                                propertyElement.SetAttribute(fieldNames[i], ((int)prop.GetValue(this)).ToString());
                            }
                        }
                    }
                }
                else if (type == "float")
                {
                    if (propertyNames.Length == 1 && fieldNames.Length == 2)
                    {
                        //This is a vector2
                        PropertyInfo prop = GetType().GetProperty(propertyNames[0], BindingFlags.Public | BindingFlags.Instance);
                        if(null != prop && prop.CanWrite)
                        {
                            var propVec = (Vector2) prop.GetValue(this);
                            propertyElement.SetAttribute(fieldNames[0], propVec.X.ToString());
                            propertyElement.SetAttribute(fieldNames[1], propVec.Y.ToString());
                        }
                    }
                    else if (propertyNames.Length == 1 && fieldNames.Length == 3)
                    {
                        //This is a vector3
                        PropertyInfo prop = GetType().GetProperty(propertyNames[0], BindingFlags.Public | BindingFlags.Instance);
                        if(null != prop && prop.CanWrite)
                        {
                            var propVec = (Vector3) prop.GetValue(this);
                            propertyElement.SetAttribute(fieldNames[0], propVec.X.ToString());
                            propertyElement.SetAttribute(fieldNames[1], propVec.Y.ToString());
                            propertyElement.SetAttribute(fieldNames[2], propVec.Z.ToString());
                        }
                    }
                    else
                    {
                        for (var i = 0; i < fieldNames.Length; i++)
                        {
                            PropertyInfo prop = GetType().GetProperty(propertyNames[i], BindingFlags.Public | BindingFlags.Instance);
                            if(null != prop && prop.CanWrite)
                            {
                                propertyElement.SetAttribute(fieldNames[i], prop.GetValue(this).ToString());
                            }
                        }
                    }
                }
                else
                {
                    for (var i = 0; i < fieldNames.Length; i++)
                    {
                        PropertyInfo prop = GetType().GetProperty(propertyNames[i], BindingFlags.Public | BindingFlags.Instance);
                        if(null != prop && prop.CanWrite)
                        {
                            propertyElement.SetAttribute(fieldNames[i], prop.GetValue(this).ToString());
                        }
                    }
                }

                componentElement.AppendChild(propertyElement);
            }

            return componentElement;
        }

        public virtual bool VInitialize(XmlElement componentData)
        {
            var info = CaravelApp.Instance.Logic.GetComponentInfo(this.ID);

            var elems = info.SelectNodes("Element");

            foreach (XmlElement elem in elems)
            {
                var nodeName = elem.Attributes["name"].Value;
                var type = elem.Attributes["type"].Value;
                string[] fieldNames = elem.Attributes["fieldNames"].Value.Split(',');
                string[] propertyNames = elem.Attributes["propertyNames"].Value.Split(',');
                var node = componentData.SelectNodes(nodeName).Item(0);

                if (node == null)
                {
                    continue;
                }

                if (type == "int")
                {
                    if (propertyNames.Length == 1 && fieldNames.Length == 2)
                    {
                        //This is a vector2
                        var fieldValue1 = int.Parse(node.Attributes[fieldNames[0]].Value);
                        var fieldValue2 = int.Parse(node.Attributes[fieldNames[1]].Value);

                        PropertyInfo prop = GetType().GetProperty(propertyNames[0], BindingFlags.Public | BindingFlags.Instance);
                        if(null != prop && prop.CanWrite)
                        {
                            prop.SetValue(this, new Vector2(fieldValue1, fieldValue2), null);
                        }
                        else {
                            Cv_Debug.Error("Property " + propertyNames[0] + " does not exist in component " + GetComponentName(this) + ".");
                        }
                    }
                    else if (propertyNames.Length == 1 && fieldNames.Length == 3)
                    {
                        //This is a vector3
                        var fieldValue1 = int.Parse(node.Attributes[fieldNames[0]].Value);
                        var fieldValue2 = int.Parse(node.Attributes[fieldNames[1]].Value);
                        var fieldValue3 = int.Parse(node.Attributes[fieldNames[2]].Value);

                        PropertyInfo prop = GetType().GetProperty(propertyNames[0], BindingFlags.Public | BindingFlags.Instance);
                        if(null != prop && prop.CanWrite)
                        {
                            prop.SetValue(this, new Vector3(fieldValue1, fieldValue2, fieldValue3), null);
                        }
                        else {
                            Cv_Debug.Error("Property " + propertyNames[0] + " does not exist in component " + GetComponentName(this) + ".");
                        }
                    }
                    else
                    {
                        for (var i = 0; i < fieldNames.Length; i++)
                        {
                            var fieldValue = node.Attributes[fieldNames[i]].Value;

                            PropertyInfo prop = GetType().GetProperty(propertyNames[i], BindingFlags.Public | BindingFlags.Instance);
                            if(null != prop && prop.CanWrite)
                            {
                                prop.SetValue(this, int.Parse(fieldValue), null);
                            }
                            else {
                                Cv_Debug.Error("Property " + propertyNames[i] + " does not exist in component " + GetComponentName(this) + ".");
                            }
                        }
                    }
                }
                else if (type == "float")
                {
                    if (propertyNames.Length == 1 && fieldNames.Length == 2)
                    {
                        //This is a vector2
                        var fieldValue1 = float.Parse(node.Attributes[fieldNames[0]].Value, CultureInfo.InvariantCulture);
                        var fieldValue2 = float.Parse(node.Attributes[fieldNames[1]].Value, CultureInfo.InvariantCulture);

                        PropertyInfo prop = GetType().GetProperty(propertyNames[0], BindingFlags.Public | BindingFlags.Instance);
                        if(null != prop && prop.CanWrite)
                        {
                            prop.SetValue(this, new Vector2(fieldValue1, fieldValue2), null);
                        }
                        else {
                            Cv_Debug.Error("Property " + propertyNames[0] + " does not exist in component " + GetComponentName(this) + ".");
                        }
                    }
                    else if (propertyNames.Length == 1 && fieldNames.Length == 3)
                    {
                        //This is a vector3
                        var fieldValue1 = float.Parse(node.Attributes[fieldNames[0]].Value, CultureInfo.InvariantCulture);
                        var fieldValue2 = float.Parse(node.Attributes[fieldNames[1]].Value, CultureInfo.InvariantCulture);
                        var fieldValue3 = float.Parse(node.Attributes[fieldNames[2]].Value, CultureInfo.InvariantCulture);

                        PropertyInfo prop = GetType().GetProperty(propertyNames[0], BindingFlags.Public | BindingFlags.Instance);
                        if(null != prop && prop.CanWrite)
                        {
                            prop.SetValue(this, new Vector3(fieldValue1, fieldValue2, fieldValue3), null);
                        }
                        else {
                            Cv_Debug.Error("Property " + propertyNames[0] + " does not exist in component " + GetComponentName(this) + ".");
                        }
                    }
                    else
                    {
                        for (var i = 0; i < fieldNames.Length; i++)
                        {
                            var fieldValue = node.Attributes[fieldNames[i]].Value;

                            PropertyInfo prop = GetType().GetProperty(propertyNames[i], BindingFlags.Public | BindingFlags.Instance);
                            if(null != prop && prop.CanWrite)
                            {
                                prop.SetValue(this, float.Parse(fieldValue, CultureInfo.InvariantCulture), null);
                            }
                            else {
                                Cv_Debug.Error("Property " + propertyNames[i] + " does not exist in component " + GetComponentName(this) + ".");
                            }
                        }
                    }
                }
                else if (type == "boolean")
                {
                    for (var i = 0; i < fieldNames.Length; i++)
                    {
                        var fieldValue = node.Attributes[fieldNames[i]].Value;

                        PropertyInfo prop = GetType().GetProperty(propertyNames[i], BindingFlags.Public | BindingFlags.Instance);
                        if(null != prop && prop.CanWrite)
                        {
                            prop.SetValue(this, bool.Parse(fieldValue), null);
                        }
                        else {
                            Cv_Debug.Error("Property " + propertyNames[i] + " does not exist in component " + GetComponentName(this) + ".");
                        }
                    }
                }
                else if (type == "file" || type == "string")
                {
                    for (var i = 0; i < fieldNames.Length; i++)
                    {
                        var fieldValue = node.Attributes[fieldNames[i]].Value;

                        PropertyInfo prop = GetType().GetProperty(propertyNames[i], BindingFlags.Public | BindingFlags.Instance);
                        if(null != prop && prop.CanWrite)
                        {
                            prop.SetValue(this, fieldValue, null);
                        }
                        else {
                            Cv_Debug.Error("Property " + propertyNames[i] + " does not exist in component " + GetComponentName(this) + ".");
                        }
                    }
                }
                else
                {
                    Cv_Debug.Error("Invalid data type defined for component element [" + GetComponentName(this) + "]");
                    return false;
                }
            }

            return true;
        }

        public abstract bool VPostInitialize();

		public abstract void VPostLoad();

        public abstract void VOnChanged();

        public abstract void VOnDestroy();

        protected internal abstract void VOnUpdate(float elapsedTime);

        internal void OnDestroy()
        {
            VOnDestroy();
            Events.Dispose();
        }

        internal static Cv_ComponentID GetID(Type componentType)
        {
            lock(m_ComponentIds)
            {
                Cv_ComponentID componentID;
                if (!m_ComponentIds.TryGetValue(componentType, out componentID))
                {
                    componentID = (Cv_ComponentID) componentType.Name.GetHashCode();
                    m_ComponentIds.Add(componentType, componentID);
                }

                return componentID;
            }
        }
    }
}

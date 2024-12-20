﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Linq.Extensions;
using UnityEngine;

namespace UnityEditor.Extensions
{

    public abstract class AMainAttributePropertyDrawer<T> : PropertyDrawer where T : PropertyAttribute
    {
        public new T attribute => base.attribute as T;
        static readonly Type kPropertyDrawerType = typeof(PropertyDrawer);
        private PropertyDrawer m_SubPropertyDrawer;
        PropertyDrawer GetSubPropertyDrawer(SerializedProperty _property)
        {
            if (m_SubPropertyDrawer != null)
                return m_SubPropertyDrawer;

            FieldInfo targetField = _property.GetFieldInfo(out var parentObject);
            IEnumerable<Attribute> attributes = targetField.GetCustomAttributes();
            int order = attribute.order + 1;
            if (order >= attributes.Count())
                return null;

            Attribute nextAttribute = attributes.ElementAt(order);
            Type attributeType = nextAttribute.GetType();
            Type propertyDrawerType = (Type)Type.GetType("UnityEditor.ScriptAttributeUtility,UnityEditor").GetMethod("GetDrawerTypeForType", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { attributeType });
            m_SubPropertyDrawer = (PropertyDrawer)Activator.CreateInstance(propertyDrawerType);
            kPropertyDrawerType.GetField("m_FieldInfo", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(m_SubPropertyDrawer, targetField);
            kPropertyDrawerType.GetField("m_Attribute", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(m_SubPropertyDrawer, nextAttribute);
            m_SubPropertyDrawer.attribute.order = order;
            return m_SubPropertyDrawer;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var customDrawerHeight = GetSubPropertyDrawer(property)?.GetPropertyHeight(property, label);
            return customDrawerHeight ?? EditorGUI.GetPropertyHeight(property, label, true);
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var customDrawer = GetSubPropertyDrawer(property);
            if (customDrawer != null)
            {
                customDrawer.OnGUI(position, property, label);
                return;
            }
            EditorGUI.PropertyField(position, property, label, true);
        }
    }
}
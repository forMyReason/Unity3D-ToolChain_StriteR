﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;
using System.Linq;
using System.Linq.Extensions;
using Object = System.Object;

namespace UnityEditor.Extensions
{
    [CustomPropertyDrawer(typeof(ConditionAttribute),true)]
    public class ConditionAttributeDrawer:AAttributePropertyDrawer<ConditionAttribute> 
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!(attribute as ConditionAttribute).IsPropertyVisible( property))
                return 0;

            return base.GetPropertyHeight(property, label);
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!(attribute as ConditionAttribute).IsPropertyVisible( property))
                return;
            
            base.OnGUI(position, property, label);
        }
    }

    public static class UConditionAttribute
    {
        static bool Equals(this ConditionAttribute.ConditionFieldParameters parameter, object _comparer)
        {
            if (_comparer is Enum)
            {
                if(_comparer.GetType().GetCustomAttributes().OfType<FlagsAttribute>().Any())
                    return parameter.refValue?.Any(p=>((Enum)_comparer).HasFlag((Enum)p))??false;
            }
            
            return parameter.refValue?.Contains(_comparer) ??  _comparer == null;
        }
        

        static bool IsVisible(this ConditionAttribute _attribute, Func<IEnumerable<(FieldInfo, object)>> _getFields)
        {
            if (_attribute.m_Conditions==null || _attribute.m_Conditions.Length==0) return true;
            var fields = _getFields();
            return _attribute.Condition switch {
                ConditionAttribute.EConditionAction.AlwaysVisible => true,
                ConditionAttribute.EConditionAction.AnyEquals => _attribute.m_Conditions.Any(condition =>
                    fields.Any(p => p.Item1.Name == condition.fieldName && Equals(condition, p.Item2))),
                ConditionAttribute.EConditionAction.NonAnyEquals => !_attribute.m_Conditions.Any(condition =>
                    fields.Any(p => p.Item1.Name == condition.fieldName && Equals(condition, p.Item2))),
                ConditionAttribute.EConditionAction.AllEquals => _attribute.m_Conditions.All(condition =>
                    fields.Any(p => p.Item1.Name == condition.fieldName && Equals(condition, p.Item2))),
                ConditionAttribute.EConditionAction.NonAllEquals => _attribute.m_Conditions.All(condition =>
                    fields.Any(p => p.Item1.Name == condition.fieldName && Equals(condition, p.Item2))),
                _ => throw new InvalidEnumArgumentException()
            };
        }
        public static bool IsPropertyVisible(this ConditionAttribute _attribute,SerializedProperty _property)=>IsVisible(_attribute,()=>_property.AllRelativeFields());
        public static bool IsElementVisible(this ConditionAttribute _attribute,Object _target)=>IsVisible(_attribute,()=>_target.GetType().GetAllFields().Select(p=>(p,p.GetValue(_target))));
    }
}
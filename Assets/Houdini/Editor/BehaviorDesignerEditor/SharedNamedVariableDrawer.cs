using BehaviorDesigner.Editor;
using BehaviorDesigner.Runtime;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomObjectDrawer(typeof(NamedVariable))]
public class SharedNamedVariableDrawer : ObjectDrawer
{
	private static string[] variableNames;

	public override void OnGUI(GUIContent label)
	{
		NamedVariable namedVariable = this.value as NamedVariable;
		EditorGUILayout.BeginVertical();
		if (FieldInspector.DrawFoldout(namedVariable.GetHashCode(), label))
		{
			EditorGUI.indentLevel=(EditorGUI.indentLevel + 1);
			if (variableNames == null)
			{
				List<Type> list = VariableInspector.FindAllSharedVariableTypes(true);
				variableNames = new string[list.Count];
				for (int i = 0; i < list.Count; i++)
				{
					variableNames[i] = list[i].Name.Remove(0, 6);
				}
			}
			int num = 0;
			string value = namedVariable.type.Remove(0, 6);
			for (int j = 0; j < variableNames.Length; j++)
			{
				if (variableNames[j].Equals(value))
				{
					num = j;
					break;
				}
			}
			namedVariable.name = EditorGUILayout.TextField("Name", namedVariable.name);
			int num2 = EditorGUILayout.Popup("Type", num, variableNames, BehaviorDesignerUtility.SharedVariableToolbarPopup, new GUILayoutOption[0]);
			Type type = VariableInspector.FindAllSharedVariableTypes(true)[num2];
			if (num2 != num)
			{
				num = num2;
				namedVariable.value = (Activator.CreateInstance(type) as SharedVariable);
			}
			GUILayout.Space(3f);
			namedVariable.type = "Shared" + SharedNamedVariableDrawer.variableNames[num];
			namedVariable.value = FieldInspector.DrawSharedVariable(null, new GUIContent("Value"), null, type, namedVariable.value);
			EditorGUI.indentLevel=(EditorGUI.indentLevel - 1);
		}
		EditorGUILayout.EndVertical();
	}
}

using BehaviorDesigner.Runtime.Tasks;
using System.Reflection;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
	public class ObjectDrawer
	{
		protected FieldInfo fieldInfo;

		protected ObjectDrawerAttribute attribute;

		protected object value;

		protected Task task;

		public FieldInfo FieldInfo
		{
			get
			{
				return fieldInfo;
			}
			set
			{
				this.fieldInfo = value;
			}
		}

		public ObjectDrawerAttribute Attribute
		{
			get
			{
				return this.attribute;
			}
			set
			{
				this.attribute = value;
			}
		}

		public object Value
		{
			get
			{
				return this.value;
			}
			set
			{
				this.value = value;
			}
		}

		public Task Task
		{
			get
			{
				return task;
			}
			set
			{
				task = value;
			}
		}

		public virtual void OnGUI(GUIContent label)
		{
		}
	}
}
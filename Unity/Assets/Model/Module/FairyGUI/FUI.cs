﻿using System;
using System.Collections.Generic;
using System.Linq;
using FairyGUI;

namespace ETModel
{
	[ObjectSystem]
	public class FUIAwakeSystem : AwakeSystem<FUI, GObject>
	{
		public override void Awake(FUI self, GObject gObject)
		{
			self.GObject = gObject;
		}
	}
	
	public sealed class FUI: Entity
	{
		public GObject GObject;
		
		public Dictionary<string, FUI> children = new Dictionary<string, FUI>();

		public string Name
		{
			get
			{
				return this.GObject.name;
			}
		}

		public bool IsWindow
		{
			get
			{
				return this.GObject is GWindow;
			}
		}
		
		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}
			
			base.Dispose();
			
			// 从父亲中删除自己
			this.GetParent<FUI>().Remove(this.Name);

			// 删除所有的孩子
			foreach (FUI ui in this.children.Values)
			{
				ui.Dispose();
			}
			children.Clear();
			
			// 删除自己的UI
			this.GObject.Dispose();
			this.GObject = null;
		}

		public void Add(FUI ui)
		{
			if (!(this.GObject is GComponent gComponent))
			{
				throw new Exception($"this ui is not GComponent, so has not child, {this.Name}");
			}
			this.children.Add(ui.Name, ui);
			gComponent.AddChild(ui.GObject);
			ui.Parent = this;
		}

		public void Remove(string name)
		{
			if (this.IsDisposed)
			{
				return;
			}
			FUI ui;
			if (!this.children.TryGetValue(name, out ui))
			{
				return;
			}
			
			if (!(this.GObject is GComponent gComponent))
			{
				throw new Exception($"this ui is not GComponent, so has not child, {this.Name}");
			}

			gComponent.RemoveChild(ui.GObject, false);
			
			this.children.Remove(name);
			ui.Dispose();
		}

		public void RemoveChildren()
		{
			foreach (var child in this.children.Values.ToArray())
			{
				child.Dispose();
			}
			this.children.Clear();
		}

		/// <summary>
		/// 根据child的名字自动获取child的FUI类，如果child没有FUI，则给它创建一个
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public FUI Get(string name)
		{
			FUI child;
			if (this.children.TryGetValue(name, out child))
			{
				return child;
			}

			if (!(this.GObject is GComponent gComponent))
			{
				throw new Exception($"this ui is not GComponent, so has not child, {this.Name}");
			}

			GComponent childComponent = gComponent.GetChild(name).asCom;
			child = ComponentFactory.Create<FUI, GComponent>(childComponent);
			this.Add(child);
			
			return child;
		}

		public bool Visible
		{
			get
			{
				return this.GObject.visible;
			}
			set
			{
				this.GObject.visible = value;
			}
		}

		public void Show()
		{
			if (this.GObject.visible)
			{
				return;
			}
			this.GObject.visible = true;

			if (this.GObject is GWindow gWindow)
			{
				gWindow.Show();
			}
		}

		public void Hide()
		{
			if (!this.GObject.visible)
			{
				return;
			}
			this.GObject.visible = true;

			if (this.GObject is GWindow gWindow)
			{
				gWindow.Hide();
			}
		}
	}
}
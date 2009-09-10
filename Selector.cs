/*
	PixClip - The easy, sleek and fast screen clipping tool (Linux version).
	Copyright © 2009 António Maria Torre do Valle
	http://www.pixclip.net
	
	
	This file is part of PixClip Linux.

	PixClip Linux is free software: you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	PixClip Linux is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with PixClip Linux.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using Gtk;
using Gdk;

namespace PixClip {

	public class Selector: Gtk.Window {

		bool bSupportsAlpha = false;

		bool bSelecting = false;
		Gdk.Point ptSelectionStart;
		Gdk.Point ptSelectionCurrent;
		public Gdk.Rectangle rectSelection;
		
		public Selector() : base (Gtk.WindowType.Popup) {
			Console.WriteLine("selector: starting");
			this.Name = "PixClipSelector";
			this.Title = "PixClip";
			this.AllowShrink = true;
			this.Decorated = false;
			this.SkipPagerHint = true;
			this.SkipTaskbarHint = true;
			
			this.TypeHint = WindowTypeHint.Normal;

			this.DoubleBuffered = true;

			this.Modal = true;
			this.KeepBelow = false;
			this.KeepAbove = true;
			this.Gravity = Gravity.NorthWest;

			this.AddEvents((int) Gdk.EventMask.KeyPressMask);
			this.AddEvents((int) Gdk.EventMask.ButtonPressMask);
			this.AddEvents((int) Gdk.EventMask.ButtonReleaseMask);
			this.AddEvents((int) Gdk.EventMask.PointerMotionMask);

			/*this.DeleteEvent += OnDeleteEvent;*/
			this.ExposeEvent += OnExposeEvent;
			this.KeyPressEvent += OnKeyPressEvent;
			this.ScreenChanged += OnScreenChanged;
			this.ButtonPressEvent += OnButtonPressEvent;
			this.ButtonReleaseEvent += OnButtonReleaseEvent;
			this.MotionNotifyEvent += OnMotionNotifyEvent;

			Gdk.Rectangle rectLayout = GetScreenLayout();

			this.DefaultHeight = rectLayout.Height;
			this.DefaultWidth = rectLayout.Width;
			this.Stick();

			OnScreenChanged(this, null);
			this.ShowAll();
			
			this.GdkWindow.Cursor = new Gdk.Cursor(Gdk.CursorType.Tcross);
			
			Gdk.Keyboard.Grab(this.GdkWindow, true, Gtk.Global.CurrentEventTime);
			Gtk.Grab.Add(this);
			
			Console.WriteLine("selector: started");
		}

		Gdk.Rectangle GetScreenLayout() {
			Gdk.Rectangle rectLayout = new Gdk.Rectangle(0, 0, 0, 0);
			Console.WriteLine("selector: detected screen with " + this.Screen.NMonitors + " monitors");
			for(int i = 0; i < this.Screen.NMonitors; i++) {
				rectLayout = this.Screen.GetMonitorGeometry(i).Union(rectLayout);
			}
			return rectLayout;
		}
		
		/*static void OnDeleteEvent(object obj, DeleteEventArgs args) {
			Console.WriteLine("selector: selector window deleted");
			Application.Quit ();
			args.RetVal = true;
		}*/
		
		static void DrawRectangle (Cairo.Context gr, Gdk.Rectangle rectData) {
	        gr.Save ();

			gr.Rectangle(new Cairo.Rectangle(rectData.X - 0.5, rectData.Y - 0.5, rectData.Width, rectData.Height)); 
	        
	        gr.ClosePath ();
	        gr.Restore ();
	    }
	    
		void CloseSelector() {
			Gtk.Grab.Remove(this);
			Gdk.Keyboard.Ungrab(Gtk.Global.CurrentEventTime);
			this.Destroy();
		}
		
		void OnExposeEvent(object o, ExposeEventArgs args) {
			Widget win = (Widget) o;
			using(Cairo.Context ctx = CairoHelper.Create(win.GdkWindow)) {
				if(bSupportsAlpha) {
					ctx.SetSourceRGBA(1.0, 1.0, 1.0, 0.4);
				} else {
					ctx.SetSourceRGB(1.0, 1.0, 1.0);
				}

				ctx.Operator = Cairo.Operator.Source;
				ctx.Paint();

				DrawRectangle (ctx, rectSelection);
			    
			    ctx.Color = new Cairo.Color(1.0, 1.0, 1.0, 0.0);
			    ctx.FillPreserve ();
			    ctx.Color = new Cairo.Color(0, 0, 0, 0.8);
				ctx.LineWidth = 1.0;
				ctx.LineCap = Cairo.LineCap.Square;
				ctx.LineJoin = Cairo.LineJoin.Miter;
				ctx.Stroke();
			}
		}

		void OnScreenChanged(object o, ScreenChangedArgs args) {
			Console.WriteLine("selector: screen changed");
			Widget win = (Widget) o;
			Gdk.Colormap tempMap = win.Screen.RgbaColormap;
			if(tempMap == null) {
				tempMap = win.Screen.RgbColormap;
				bSupportsAlpha = false;
			} else {
				bSupportsAlpha = true;
			}
			win.Colormap = tempMap;
		}

		void OnKeyPressEvent(object o, KeyPressEventArgs args) {
			if(args.Event.Key == Gdk.Key.Escape) {
				Console.WriteLine("selector: quit selection");
				CloseSelector();
			}
		}

		protected virtual void OnButtonPressEvent(object o, Gtk.ButtonPressEventArgs args) {
			if(args.Event.Button == 1) {
				ptSelectionStart.X = (int) args.Event.X;
				ptSelectionStart.Y = (int) args.Event.Y;
				bSelecting = true;
				Console.WriteLine("selector: start selection");
			}
		}
		
		protected virtual void OnButtonReleaseEvent (object o, Gtk.ButtonReleaseEventArgs args) {
			if(bSelecting) {
				if(args.Event.Button == 1 && bSelecting) {
					bSelecting = false;
					Console.WriteLine("selector: selected rect - w=" + rectSelection.Width + " x h=" + rectSelection.Height);
					CloseSelector();
				} else if (args.Event.Button == 3) {
					bSelecting = false;
					rectSelection = new Rectangle(0, 0, 0, 0);
					this.QueueDraw();
					Console.WriteLine("selector: selection cancelled");
				}
			}
		}
		
		protected virtual void OnMotionNotifyEvent (object o, Gtk.MotionNotifyEventArgs args) {
			if(bSelecting) {
				ptSelectionCurrent.X = (int) args.Event.X;
				ptSelectionCurrent.Y = (int) args.Event.Y;
				rectSelection.X = ptSelectionStart.X < ptSelectionCurrent.X ? ptSelectionStart.X : ptSelectionCurrent.X;
				rectSelection.Y = ptSelectionStart.Y < ptSelectionCurrent.Y ? ptSelectionStart.Y : ptSelectionCurrent.Y;
				rectSelection.Width = ptSelectionCurrent.X > ptSelectionStart.X ? ptSelectionCurrent.X - ptSelectionStart.X : ptSelectionStart.X - ptSelectionCurrent.X;
				rectSelection.Height = ptSelectionCurrent.Y > ptSelectionStart.Y ? ptSelectionCurrent.Y - ptSelectionStart.Y : ptSelectionStart.Y - ptSelectionCurrent.Y;
				this.QueueDraw();
			}
		}
	}
}

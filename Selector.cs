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

		public Selector() : base (Gtk.WindowType.Toplevel) {
			Console.WriteLine("selector: starting");
			this.Name = "PixClipSelector";
			this.Title = "PixClip";
			this.AllowShrink = true;
			this.Decorated = false;
			this.SkipPagerHint = true;
			this.SkipTaskbarHint = true;
			
			this.TypeHint = WindowTypeHint.Splashscreen;

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
			Console.WriteLine("selector: started");
		}

		Gdk.Rectangle GetScreenLayout() {
			Gdk.Rectangle rectLayout = new Gdk.Rectangle(0, 0, 0, 0);
			if(this.Screen.NMonitors == 1) {
				Console.WriteLine("selector: single monitor screen detected");
				this.Fullscreen();
			} else {
				Console.WriteLine("selector: detected screen with " + this.Screen.NMonitors + " monitors");
				for(int i = 0; i < this.Screen.NMonitors; i++) {
					rectLayout = this.Screen.GetMonitorGeometry(i).Union(rectLayout);
				}
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
				this.Destroy();
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
				if(args.Event.Button == 1) {
					bSelecting = false;
					Console.WriteLine("selector: selected rect - w=" + rectSelection.Width + " x h=" + rectSelection.Height);
					this.Destroy();
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
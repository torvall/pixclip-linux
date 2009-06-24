using System;
using Gdk;

namespace PixClip {
	
	public static class Capture {
		
		public static Gdk.Pixbuf CaptureImage(Gdk.Rectangle rectSelection) {
			Window winRoot = Gdk.Screen.Default.RootWindow;
			Gdk.Pixbuf pix = new Gdk.Pixbuf(Colorspace.Rgb, true, 8, rectSelection.Width, rectSelection.Height);
			pix.GetFromDrawable(winRoot, winRoot.Colormap, rectSelection.X, rectSelection.Y, 0, 0, rectSelection.Width, rectSelection.Height);
			Console.WriteLine("capture: image captured");
			return pix;
		}
	}
}

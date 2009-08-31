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
using Gdk;

namespace PixClip {
	
	public static class Capture {
		
		public static Gdk.Pixbuf CaptureImage(Gdk.Rectangle rectSelection) {
			Window winRoot = Gdk.Screen.Default.RootWindow;
			Gdk.Pixbuf pix = new Gdk.Pixbuf(Colorspace.Rgb, true, 8, rectSelection.Width - 1, rectSelection.Height - 1);
			pix.GetFromDrawable(winRoot, winRoot.Colormap, rectSelection.X, rectSelection.Y, 0, 0, rectSelection.Width - 1, rectSelection.Height - 1);
			Console.WriteLine("capture: image captured");
			return pix;
		}
	}
}

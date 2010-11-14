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
using System.Threading;
using Gtk;
using Gdk;

namespace PixClip {
	
	public class MainClass {

		private static StatusIcon trayIcon;
		private static AboutDialog aboutDialog;
		
		public static Gdk.Rectangle rectSelection;
		private static bool bSelecting = false;
		
		private static Pixbuf pixClip;
		
		public static void Main (string[] args) {
			Console.WriteLine("main: PixClip starting");
			Application.Init();
			
			trayIcon = new StatusIcon(new Pixbuf("PixClip.ico"));
			trayIcon.Visible = true;
			
			trayIcon.Activate += OnTrayIconActivate;
			trayIcon.PopupMenu += OnTrayIconPopup;

			trayIcon.Tooltip = "PixClip Linux";

			Application.Run();
			Console.WriteLine("main: ended");
		}

		static void GetClip() {
			// TODO: Do not offer to save clip after selection and popup a clickable tooltip instead.
			FileChooserDialog fcd = new FileChooserDialog("PixClip - Save clip as...", null, FileChooserAction.Save, "Cancel", ResponseType.Cancel, "Save", ResponseType.Accept);
			FileFilter fltJpg = new FileFilter();
			fltJpg.AddMimeType("image/jpeg");
			fltJpg.Name = "JPEG image format";
			fcd.SetFilename("clip.jpg");
			fcd.SetCurrentFolder(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop));
			fcd.AddFilter(fltJpg);
			
			Console.WriteLine("main: selecting save target");
			if (fcd.Run() == (int) ResponseType.Accept) {
				// TODO: Add quality setting to options form.
				// http://library.gnome.org/devel/gdk-pixbuf/stable/gdk-pixbuf-file-saving.html
				string [] keys = {"quality"};
				string [] values = {"90"};
				pixClip.Savev(fcd.Filename, "jpeg", keys, values);
				Console.WriteLine("main: image saved");
			} else {
				Console.WriteLine("main: image save canceled");
			}
			fcd.Destroy();

			Clipboard clip = Gtk.Clipboard.Get(Atom.Intern("CLIPBOARD", false));
			clip.Image = pixClip;
			Console.WriteLine("main: image added to the clipboard");
		}
		
		static void OnTrayIconActivate(object sender, EventArgs e) {
			if(!bSelecting) {
				Console.WriteLine("main: capture process started");
				bSelecting = true;
				Selector mainSelect = new Selector();
				mainSelect.AppPaintable = true;
				
				mainSelect.Destroyed += OnMainSelectDestroyed;
			}
		}

		static void OnMainSelectDestroyed(object sender, EventArgs e) {
			if(bSelecting) {
				bSelecting = false;
				Selector mainSelect = (Selector) sender;
				rectSelection = mainSelect.rectSelection;
				mainSelect.Dispose();
				if(rectSelection.Width > 0 && rectSelection.Height > 0) {
					Console.WriteLine("main: selected rect - w=" + rectSelection.Width + " x h=" + rectSelection.Height);
					pixClip = Capture.CaptureImage(rectSelection);
					GetClip();
				}
				Console.WriteLine("main: capture process ended");
			}
		}

		static void OnTrayIconPopup(object o, EventArgs args) {
			if(!bSelecting) {
				Console.WriteLine("main: icon menu called");
				Menu popupMenu = new Menu();
	
				ImageMenuItem menuItemSaveImage = new ImageMenuItem("Save image...");
				Gtk.Image imgSaveImage = new Gtk.Image(Stock.SaveAs, IconSize.Menu);
				menuItemSaveImage.Image = imgSaveImage;
				menuItemSaveImage.TooltipText = "Save last clipped image to file";
				if(pixClip == null) {
					menuItemSaveImage.State = StateType.Insensitive;
				}
				popupMenu.Add(menuItemSaveImage);

				menuItemSaveImage.Activated += OnSaveImageActivated;
				
				SeparatorMenuItem sepMnit1 = new SeparatorMenuItem();
				popupMenu.Add(sepMnit1);
				
				ImageMenuItem menuItemCaptureScreen = new ImageMenuItem("Capture screen");
				Gtk.Image imgCaptureScreen = new Gtk.Image(Stock.Fullscreen, IconSize.Menu);
				menuItemCaptureScreen.Image = imgCaptureScreen;
				menuItemCaptureScreen.TooltipText = "Capture entire screen, including all monitors you may have";
				popupMenu.Add(menuItemCaptureScreen);

				menuItemCaptureScreen.Activated += OnCaptureScreenActivated;

				SeparatorMenuItem sepMnit2 = new SeparatorMenuItem();
				popupMenu.Add(sepMnit2);
				
				ImageMenuItem menuItemAbout = new ImageMenuItem("About...");
				Gtk.Image imgAbout = new Gtk.Image(Stock.About, IconSize.Menu);
				menuItemAbout.Image = imgAbout;
				menuItemAbout.TooltipText = "Information about this application";
				popupMenu.Add(menuItemAbout);

				menuItemAbout.Activated += OnAboutDialogActivated;
				
				ImageMenuItem menuItemQuit = new ImageMenuItem("Quit");
				Gtk.Image imgQuit = new Gtk.Image(Stock.Quit, IconSize.Menu);
				menuItemQuit.Image = imgQuit;
				menuItemQuit.TooltipText = "Exit PixClip";
				popupMenu.Add(menuItemQuit);
				
				menuItemQuit.Activated += delegate {
					Console.WriteLine("main: PixClip closing");
					Application.Quit();
				};
				
				popupMenu.ShowAll();
				popupMenu.Popup();
			}
		}

		static void OnSaveImageActivated(object sender, EventArgs e) {
			Console.WriteLine("main: save image command from menu");
			GetClip();
		}
			
		static void OnCaptureScreenActivated(object sender, EventArgs e) {
			Console.WriteLine("main: capturing screen");
			Thread.Sleep(1000);
			pixClip = Capture.CaptureScreen();
			GetClip();
		}
			
		static void OnAboutDialogActivated(object sender, EventArgs e) {
			Console.WriteLine("main: about form invoked");
			aboutDialog = new AboutDialog();
			
			aboutDialog.ProgramName = AppInfo.sProgramName;
			aboutDialog.Version = AppInfo.sVersion + " alpha";
			aboutDialog.Comments = AppInfo.sComments;
			aboutDialog.WrapLicense = true;
			aboutDialog.License = AppInfo.sLicense;
			//aboutDialog.Authors = new string[] { "António Maria Torre do Valle" };
			//aboutDialog.Logo = TODO: Add PixClip logo in about dialog.
			AboutDialog.SetUrlHook(delegate(Gtk.AboutDialog dialog, string link) {
				Gnome.Url.Show(link);
			});
			aboutDialog.Website = AppInfo.sWebsite;
			aboutDialog.Copyright = AppInfo.sCopyright;
			aboutDialog.Response += OnAboutDialogClose;
			
			aboutDialog.Run();
		}
		
		static void OnAboutDialogClose(object sender, ResponseArgs e) {
			if (e.ResponseId == ResponseType.Cancel || e.ResponseId == ResponseType.DeleteEvent) {
				aboutDialog.Destroy();
				Console.WriteLine("main: about form closed");
			}
		}
	}
}

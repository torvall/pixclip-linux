using System;
using Gtk;
using Gdk;

namespace PixClip {
	
	public class MainClass {
	
		private static StatusIcon trayIcon;
		private static AboutDialog aboutDialog;
		
		public static Gdk.Rectangle rectSelection;
		private static bool bSelecting = false;
		
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

		static void OnTrayIconActivate(object sender, EventArgs e) {
			if(!bSelecting) {
				Console.WriteLine("main: capture process started");
				bSelecting = true;
				Selector mainSelect = new Selector();
				mainSelect.AppPaintable = true;
				
				mainSelect.Hidden += OnMainSelectHidden;
			}
		}

		static void OnMainSelectHidden(object sender, EventArgs e) {
			rectSelection = ((Selector) sender).rectSelection;
			if(rectSelection.Width > 0 && rectSelection.Height > 0) {
				Console.WriteLine("main: selected rect - w=" + rectSelection.Width + " x h=" + rectSelection.Height);
				Pixbuf pixClip = Capture.CaptureImage(rectSelection);

				FileChooserDialog fcd = new FileChooserDialog("PixClip - Save clip as...", null, FileChooserAction.Save, "Cancel", ResponseType.Cancel, "Save", ResponseType.Accept);
				FileFilter fltJpg = new FileFilter();
				fltJpg.AddMimeType("image/jpeg");
				fltJpg.Name = "JPEG image format";
				fcd.AddFilter(fltJpg);
			 	
				fcd.SetFilename("clip.jpg");
				fcd.SetCurrentFolder(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop));
				
				Console.WriteLine("main: selecting save target");
				if (fcd.Run() == (int) ResponseType.Accept) {
					pixClip.Save(fcd.Filename, "jpeg");
					Console.WriteLine("main: image saved");
				} else {
					Console.WriteLine("main: image save canceled");
				}
				fcd.Destroy();
			}
			bSelecting = false;
			Console.WriteLine("main: capture process ended");
		}

		static void OnTrayIconPopup(object o, EventArgs args) {
			if(!bSelecting) {
				Console.WriteLine("main: icon menu called");
				Menu popupMenu = new Menu();
	
				ImageMenuItem menuItemAbout = new ImageMenuItem ("About");
				Gtk.Image appimg1 = new Gtk.Image(Stock.About, IconSize.Menu);
				menuItemAbout.Image = appimg1;
				popupMenu.Add(menuItemAbout);
				
				menuItemAbout.Activated += OnHelloAboutActivated;
				
				ImageMenuItem menuItemQuit = new ImageMenuItem ("Quit");
				Gtk.Image appimg2 = new Gtk.Image(Stock.Quit, IconSize.Menu);
				menuItemQuit.Image = appimg2;
				popupMenu.Add(menuItemQuit);
				
				menuItemQuit.Activated += delegate {
					Console.WriteLine("main: PixClip closing");
					Application.Quit();
				};
				popupMenu.ShowAll();
				popupMenu.Popup();
			}
		}

		static void OnHelloAboutActivated(object sender, EventArgs e) {
			if(!bSelecting) {
				Console.WriteLine("main: about form invoked");
				aboutDialog = new AboutDialog();
				
				aboutDialog.ProgramName = "PixClip Linux";
				aboutDialog.Version = "0.01 alpha";
				aboutDialog.Comments = "Easy, sleek and fast screen clipping, now also on Linux!";
				aboutDialog.License = "Free Software";
				aboutDialog.Authors = new string[] { "António Maria Torre do Valle" };
				aboutDialog.Website = "http://www.pixclip.net";
				aboutDialog.Response += OnHelloAboutClose;
				
				aboutDialog.Run();
			}
		}
		
		static void OnHelloAboutClose(object sender, ResponseArgs e) {
			if (e.ResponseId==ResponseType.Cancel || e.ResponseId==ResponseType.DeleteEvent) {
				aboutDialog.Destroy();
				Console.WriteLine("main: about form closed");
			}
		}
	}
}

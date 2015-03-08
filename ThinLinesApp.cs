using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ThinLines
{
  public class ThinLinesApp : IExternalApplication
  {
    #region Windows API, get from pinvoke.net

    [DllImport( "user32.dll", SetLastError = true )]
    static extern IntPtr FindWindowEx( IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow );

    [DllImport( "user32.dll" )]
    [return: MarshalAs( UnmanagedType.Bool )]
    public static extern bool EnumChildWindows( IntPtr window, EnumWindowProc callback, IntPtr i );

    public delegate bool EnumWindowProc( IntPtr hWnd, IntPtr parameter );

    public static bool EnumWindow( IntPtr handle, IntPtr pointer )
    {
      GCHandle gch = GCHandle.FromIntPtr( pointer );
      List<IntPtr> list = gch.Target as List<IntPtr>;
      if( list != null )
      {
        list.Add( handle );
      }

      return true;
    }

    public static List<IntPtr> GetChildWindows( IntPtr parent )
    {
      List<IntPtr> result = new List<IntPtr>();
      GCHandle listHandle = GCHandle.Alloc( result );
      try
      {
        EnumWindowProc childProc = new EnumWindowProc( EnumWindow );
        EnumChildWindows( parent, childProc, GCHandle.ToIntPtr( listHandle ) );
      }
      finally
      {
        if( listHandle.IsAllocated )
          listHandle.Free();
      }
      return result;
    }
    #endregion

    public Result OnShutdown( UIControlledApplication a )
    {
      return Result.Succeeded;
    }

    public Result OnStartup( UIControlledApplication a )
    {
      string tabName = "LineTools";
      string panelName = "LineTools";
      string buttonThinName = "Thin";
      string buttonThickName = "Thick";
      string buttonToggleName = "Toggle";

      try
      {
        List<RibbonPanel> panels = a.GetRibbonPanels( tabName );
      }
      catch
      {
        a.CreateRibbonTab( tabName );
      }

      RibbonPanel panelViewExport = a.CreateRibbonPanel( tabName, panelName );
      panelViewExport.Name = panelName;
      panelViewExport.Title = panelName;

      PushButtonData buttonThin = new PushButtonData( buttonThinName, buttonThinName, System.Reflection.Assembly.GetExecutingAssembly().Location, typeof( Command_ThinLines ).FullName );
      buttonThin.ToolTip = buttonThinName;
      ImageSource iconThin = GetIconSource( Images.Thin );
      buttonThin.LargeImage = iconThin;
      buttonThin.Image = Thumbnail( iconThin );
      panelViewExport.AddItem( buttonThin );

      PushButtonData buttonThick = new PushButtonData( buttonThickName, buttonThickName, System.Reflection.Assembly.GetExecutingAssembly().Location, typeof( Command_ThickLines ).FullName );
      buttonThick.ToolTip = buttonThickName;
      ImageSource iconThick = GetIconSource( Images.Thick );
      buttonThick.LargeImage = iconThick;
      buttonThick.Image = Thumbnail( iconThick );
      panelViewExport.AddItem( buttonThick );

      PushButtonData buttonToggle = new PushButtonData( buttonToggleName, buttonToggleName, System.Reflection.Assembly.GetExecutingAssembly().Location, typeof( Command_ToggleLineThickness ).FullName );
      buttonToggle.ToolTip = buttonToggleName;
      ImageSource iconToggle = GetIconSource( Images.ToggleLineThickness );
      buttonToggle.LargeImage = iconToggle;
      buttonToggle.Image = Thumbnail( iconToggle );
      panelViewExport.AddItem( buttonToggle );

      return Result.Succeeded;
    }

    public static ImageSource GetIconSource( Bitmap bmp )
    {
      BitmapSource icon = Imaging.CreateBitmapSourceFromHBitmap( bmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions() );
      return (System.Windows.Media.ImageSource) icon;
    }

    public static ImageSource Thumbnail( ImageSource source )
    {
      Rect rect = new Rect( 0, 0, 16, 16 );
      DrawingVisual drawingVisual = new DrawingVisual();
      using( DrawingContext drawingContext = drawingVisual.RenderOpen() )
      {
        drawingContext.DrawImage( source, rect );
      }

      RenderTargetBitmap resizedImage = new RenderTargetBitmap( (int) rect.Width, (int) rect.Height, 96, 96, PixelFormats.Default );
      resizedImage.Render( drawingVisual );

      return resizedImage;
    }

    public static AutomationElement GetThinLinesButton()
    {
      IntPtr revitHandle = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
      IntPtr outerToolFrame = FindWindowEx( revitHandle, IntPtr.Zero, "AdImpApplicationFrame", "AdImpApplicationFrame" );
      IntPtr innerToolFrame = GetChildWindows( outerToolFrame )[0];

      AutomationElement innerToolFrameElement = AutomationElement.FromHandle( innerToolFrame );

      PropertyCondition typeRibbonCondition = new PropertyCondition( AutomationElement.ControlTypeProperty, ControlType.Custom );
      AutomationElement lowestPanel = innerToolFrameElement.FindFirst( TreeScope.Children, typeRibbonCondition );

      PropertyCondition nameRibbonCondition = new PropertyCondition( AutomationElement.AutomationIdProperty, "ID_THIN_LINES_RibbonItemControl" );
      AndCondition andCondition = new AndCondition( typeRibbonCondition, nameRibbonCondition );
      AutomationElement buttonContainer = lowestPanel.FindFirst( TreeScope.Children, andCondition );

      PropertyCondition typeButtonCondition = new PropertyCondition( AutomationElement.ControlTypeProperty, ControlType.Button );
      PropertyCondition nameButtonCondition = new PropertyCondition( AutomationElement.AutomationIdProperty, "ID_THIN_LINES" );
      AndCondition andConditionButton = new AndCondition( typeButtonCondition, nameButtonCondition );
      AutomationElement button = buttonContainer.FindFirst( TreeScope.Children, andConditionButton );

      return button;
    }

    public static void SetThinLines( UIApplication app, bool makeThin )
    {
      bool isAlreadyThin = IsThinLines();

      if( makeThin != isAlreadyThin ) // switch TL state by invoking PostableCommand.ThinLines
      {
        RevitCommandId commandId = RevitCommandId.LookupPostableCommandId( PostableCommand.ThinLines );

        if( app.CanPostCommand( commandId ) )
        {
          app.PostCommand( commandId );
        }
      }
    }

    public static bool IsThinLines()
    {
      AutomationElement button = GetThinLinesButton();

      TogglePattern togglePattern = button.GetCurrentPattern( TogglePattern.Pattern ) as TogglePattern;

      string state = togglePattern.Current.ToggleState.ToString().ToUpper();

      return ( state == "ON" );
    }
  }
}

using Autodesk.Revit.UI;

namespace ThinLines
{
  [Autodesk.Revit.Attributes.Transaction( 
    Autodesk.Revit.Attributes.TransactionMode.ReadOnly )]
  [Autodesk.Revit.Attributes.Regeneration( 
    Autodesk.Revit.Attributes.RegenerationOption.Manual )]
  public class Command_ThinLines : IExternalCommand
  {
    public Result Execute( 
      ExternalCommandData commandData, 
      ref string message, 
      Autodesk.Revit.DB.ElementSet elements )
    {
      ThinLinesApp.SetThinLines( commandData.Application, true );
      return Result.Succeeded;
    }
  }
}
using Autodesk.Revit.UI;

namespace ThinLines
{
	[Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.ReadOnly)]
	[Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
	public class Command_ToggleLineThickness : IExternalCommand
	{
		public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
		{
			// get ThinLies state and set it to its opposite value

			ThinLinesApp.SetThinLines(commandData.Application, !ThinLinesApp.IsThinLines());

			// you also could toggle the ThinLines state by just invoking PostableCommand.ThinLines

			/*
			RevitCommandId commandId = RevitCommandId.LookupPostableCommandId(PostableCommand.ThinLines);

			if (commandData.Application.CanPostCommand(commandId))
			{
				commandData.Application.PostCommand(commandId);
			}
			*/

			return Result.Succeeded;
		}
	}
}

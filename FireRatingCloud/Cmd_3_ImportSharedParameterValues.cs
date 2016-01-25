#region Namespaces
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
#endregion // Namespaces

namespace FireRatingCloud
{
  /// <summary>
  /// Import updated FireRating parameter values 
  /// from external database.
  /// </summary>
  [Transaction( TransactionMode.Manual )]
  public class Cmd_3_ImportSharedParameterValues
    : IExternalCommand
  {
    public Result Execute(
      ExternalCommandData commandData,
      ref string message,
      ElementSet elements )
    {
      UIApplication uiapp = commandData.Application;
      Application app = uiapp.Application;
      Document doc = uiapp.ActiveUIDocument.Document;

      Guid paramGuid;
      if( !Util.GetSharedParamGuid( app, out paramGuid ) )
      {
        message = "Shared parameter GUID not found.";
        return Result.Failed;
      }

      // Determine custom project identifier.

      string project_id = Util.GetProjectIdentifier( doc );

      // Get all doors referencing this project.

      string query = "doors/project/" + project_id;

      List<DoorData> doors = Util.Get( query );

      if( null != doors && 0 < doors.Count )
      {
        using( Transaction t = new Transaction( doc ) )
        {
          t.Start( "Import Fire Rating Values" );

          // Retrieve element unique id and 
          // FireRating parameter values.

          foreach( DoorData d in doors )
          {
            string uid = d._id;
            Element e = doc.GetElement( uid );

            if( null == e )
            {
              message = string.Format(
                "Error retrieving element for "
                + "unique id {0}.", uid );

              return Result.Failed;
            }

            Parameter p = e.get_Parameter( paramGuid );

            if( null == p )
            {
              message = string.Format(
                "Error retrieving shared parameter on "
                + " element with unique id {0}.", uid );

              return Result.Failed;
            }
            object fire_rating = d.firerating;

            p.Set( (double) fire_rating );
          }
          t.Commit();
        }
      }
      return Result.Succeeded;
    }
  }
}
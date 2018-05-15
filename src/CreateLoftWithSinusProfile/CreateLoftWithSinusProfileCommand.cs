using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace AI.CreateLoftWithSinusProfile
{
    [Transaction(TransactionMode.Manual)]
    public class CreateLoftWithSinusProfileCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiapp = commandData.Application;
            var uidoc = uiapp.ActiveUIDocument;
            var doc = uidoc.Document;

            const double scale = 0.2;
            const double length = 4.0*Math.PI;
            const int steps = 8;

            var profile = CreateProfile(scale);

            var profiles = Enumerable
                .Range(0, steps + 1)
                .Select(x => x*length/steps)
                .Select(CreateSinusTransform)
                .Select(x => profile.CreateTransformed(x))
                .ToList();

            using (var transaction = new Transaction(doc, "create loft form with sinus path"))
            {
                transaction.Start();

                var solid = GeometryCreationUtilities.CreateLoftGeometry(profiles,
                    new SolidOptions(ElementId.InvalidElementId, ElementId.InvalidElementId));

                var directShape = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_GenericModel));

                directShape.AppendShape(new GeometryObject[] {solid});

                transaction.Commit();
            }

            return Result.Succeeded;
        }

        private static Transform CreateSinusTransform(double point)
        {
            var translation = Transform.CreateTranslation(new XYZ(point, Math.Sin(point), 0));

            var rotation = Transform.CreateRotation(XYZ.BasisZ, Math.Cos(point));

            return translation*rotation;
        }

        private static CurveLoop CreateProfile(double scale)
        {
            var a = new XYZ(0, -1, -1);
            var b = new XYZ(0, 1, -1);
            var c = new XYZ(0, 1, 0);
            var d = new XYZ(0, 0, 1);
            var e = new XYZ(0, -1, 1);

            var curvesList = new List<Curve>
                {
                    Line.CreateBound(scale*a, scale*b),
                    Line.CreateBound(scale*b, scale*c),
                    Line.CreateBound(scale*c, scale*d),
                    Line.CreateBound(scale*d, scale*e),
                    Line.CreateBound(scale*e, scale*a)
                };

            return CurveLoop.Create(curvesList);
        }
    }
}

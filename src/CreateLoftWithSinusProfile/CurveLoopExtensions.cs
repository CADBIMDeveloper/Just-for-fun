using System.Linq;
using Autodesk.Revit.DB;

namespace AI.CreateLoftWithSinusProfile
{
    public static class CurveLoopExtensions
    {
        public static CurveLoop CreateTransformed(this CurveLoop curveLoop, Transform transform)
        {
            var curves = curveLoop
                .Select(x => x.CreateTransformed(transform))
                .ToList();

            return CurveLoop.Create(curves);
        }
    }
}
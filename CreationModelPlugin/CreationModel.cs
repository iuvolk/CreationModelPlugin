using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using System.Reflection;

namespace CreationModelPlugin
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CreationModel : IExternalCommand

    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;

            List<Level> listLevel = LevelUtils.GetLevels(doc);
            Level level1 = LevelUtils.GetLevelByName(doc, "Уровень 1");
            Level level2 = LevelUtils.GetLevelByName(doc, "Уровень 2");


            Transaction transaction = new Transaction(doc, "Create model");
            transaction.Start();

            var walls = WallsUtils.AddWall(doc, level1, level2);

            AddDoor(doc, level1, walls[0]);

            for (int i = 1; i < walls.Count; i++)
            {
                AddWindows(doc, level1, walls[i]);
            }

            //AddRoof(doc, level2, walls);

            AddExtrusionRoof(doc, level2, walls);

            transaction.Commit();


            #region

            //// Сколько размещено экземпляров стен в проекте
            //var res1 = new FilteredElementCollector(doc)
            //    .OfClass(typeof(Wall))
            //    //.Cast<Wall>()
            //    .OfType<Wall>()
            //    .ToList();

            //// Сколько существует типов стен всего в проекте
            //var res2 = new FilteredElementCollector(doc)
            //   .OfClass(typeof(WallType))
            //   //.Cast<WallType>()
            //   .OfType<WallType>()
            //   .ToList();

            //// Сколько размещено экземпляров загружаемых семейств в проекте
            //var res3 = new FilteredElementCollector(doc)
            //    .OfClass(typeof(FamilyInstance))
            //    //.Cast<FamilyInstance>()
            //    .OfType<FamilyInstance>()
            //    .ToList();

            //// Сколько размещено экземпляров дверей (относятся к загружаемым семействам) в проекте
            //var res4 = new FilteredElementCollector(doc)
            //    .OfClass(typeof(FamilyInstance))
            //    .OfCategory(BuiltInCategory.OST_Doors)
            //    //.Cast<FamilyInstance>()
            //    .OfType<FamilyInstance>()
            //    .ToList();

            //// Сколько размещено экземпляров дверей (относятся к загружаемым семействам) в проекте типа "36" x 84""
            //var res5 = new FilteredElementCollector(doc)
            //    .OfClass(typeof(FamilyInstance))
            //    .OfCategory(BuiltInCategory.OST_Doors)
            //    //.Cast<FamilyInstance>()
            //    .OfType<FamilyInstance>()
            //    .Where(x => x.Name.Equals("36\" x 84\""))
            //    .ToList();

            //// Сколько размещено экземпляров всех семейств всего в проекте
            //var res6 = new FilteredElementCollector(doc)
            //    .WhereElementIsNotElementType()
            //    .ToList();
            #endregion

            return Result.Succeeded;
        }


        private void AddExtrusionRoof(Document doc, Level level2, List<Wall> walls)
        {
            RoofType roofType = new FilteredElementCollector(doc)
               .OfClass(typeof(RoofType))
               .OfType<RoofType>()
               .Where(x => x.Name.Equals("Типовой - 400мм"))
               .Where(x => x.FamilyName.Equals("Базовая крыша"))
               .FirstOrDefault();

            double wallWidth = walls[0].Width;
            double dt = wallWidth / 2;


            double startPoint = (walls[0].Location as LocationCurve).Curve.GetEndPoint(0).X - dt;
            double endPoint = (walls[0].Location as LocationCurve).Curve.GetEndPoint(1).X + dt;

            double y1Point = (walls[0].Location as LocationCurve).Curve.GetEndPoint(0).Y - dt;
            double y2Point = (walls[1].Location as LocationCurve).Curve.GetEndPoint(0).Y - (walls[0].Location as LocationCurve).Curve.GetEndPoint(1).Y;
            double y3Point = (walls[1].Location as LocationCurve).Curve.GetEndPoint(1).Y + dt;


            CurveArray curveArray = new CurveArray();
            curveArray.Append(Line.CreateBound(new XYZ(0, y1Point, level2.Elevation), new XYZ(0, y2Point, level2.Elevation + level2.Elevation / 2)));
            curveArray.Append(Line.CreateBound(new XYZ(0, y2Point, level2.Elevation + level2.Elevation / 2), new XYZ(0, y3Point, level2.Elevation)));

            ReferencePlane plane = doc.Create.NewReferencePlane(new XYZ(0, 0, 0), new XYZ(0, 0, 20), new XYZ(0, 20, 0), doc.ActiveView);
            doc.Create.NewExtrusionRoof(curveArray, plane, level2, roofType, startPoint, endPoint);

        }
        private void AddRoof(Document doc, Level level2, List<Wall> walls)
        {
            RoofType roofType = new FilteredElementCollector(doc)
                .OfClass(typeof(RoofType))
                .OfType<RoofType>()
                .Where(x => x.Name.Equals("Типовой - 400мм"))
                .Where(x => x.FamilyName.Equals("Базовая крыша"))
                .FirstOrDefault();

            double wallWidth = walls[0].Width;
            double dt = wallWidth / 2;
            List<XYZ> points = new List<XYZ>();
            points.Add(new XYZ(-dt, -dt, 0));
            points.Add(new XYZ(dt, -dt, 0));
            points.Add(new XYZ(dt, dt, 0));
            points.Add(new XYZ(-dt, dt, 0));
            points.Add(new XYZ(-dt, -dt, 0));

            Application application = doc.Application;
            CurveArray footprint = application.Create.NewCurveArray();

            for (int i = 0; i < 4; i++)
            {
                LocationCurve curve = walls[i].Location as LocationCurve;
                XYZ p1 = curve.Curve.GetEndPoint(0);
                XYZ p2 = curve.Curve.GetEndPoint(1);
                Line line = Line.CreateBound(p1 + points[i], p2 + points[i + 1]);
                footprint.Append(line);
            }
            ModelCurveArray footPrintToModelCurveMapping = new ModelCurveArray();
            FootPrintRoof footprintRoof = doc.Create.NewFootPrintRoof(footprint, level2, roofType, out footPrintToModelCurveMapping);

            #region
            //ModelCurveArrayIterator iterator = footPrintToModelCurveMapping.ForwardIterator();
            //iterator.Reset();
            //while (iterator.MoveNext())
            //{
            //    ModelCurve modelCurve = iterator.Current as ModelCurve;
            //    footprintRoof.set_DefinesSlope(modelCurve, true);
            //    footprintRoof.set_SlopeAngle(modelCurve, 0.5);
            //}
            #endregion

            foreach (ModelCurve m in footPrintToModelCurveMapping)
            {
                footprintRoof.set_DefinesSlope(m, true);
                footprintRoof.set_SlopeAngle(m, 0.5);
            }
        }

        private void AddDoor(Document doc, Level level1, Wall wall)
        {
            FamilySymbol doorType = new FilteredElementCollector(doc)
                 .OfClass(typeof(FamilySymbol))
                 .OfCategory(BuiltInCategory.OST_Doors)
                 .OfType<FamilySymbol>()
                 .Where(x => x.Name.Equals("0915 x 2134 мм"))
                 .Where(x => x.FamilyName.Equals("Одиночные-Щитовые"))
                 .FirstOrDefault();

            LocationCurve hostCurve = wall.Location as LocationCurve;
            XYZ point1 = hostCurve.Curve.GetEndPoint(0);
            XYZ point2 = hostCurve.Curve.GetEndPoint(1);
            XYZ point = (point1 + point2) / 2;

            if (!doorType.IsActive)
                doorType.Activate();

            doc.Create.NewFamilyInstance(point, doorType, wall, level1, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
        }

        private void AddWindows(Document doc, Level level1, Wall wall)
        {
            FamilySymbol windowType = new FilteredElementCollector(doc)
                 .OfClass(typeof(FamilySymbol))
                 .OfCategory(BuiltInCategory.OST_Windows)
                 .OfType<FamilySymbol>()
                 .Where(x => x.Name.Equals("0406 x 0610 мм"))
                 .Where(x => x.FamilyName.Equals("Фиксированные"))
                 .FirstOrDefault();

            LocationCurve hostCurve = wall.Location as LocationCurve;
            XYZ point1 = hostCurve.Curve.GetEndPoint(0);
            XYZ point2 = hostCurve.Curve.GetEndPoint(1);
            XYZ point = (point1 + point2) / 2;

            if (!windowType.IsActive)
                windowType.Activate();

            var window = doc.Create.NewFamilyInstance(point, windowType, wall, level1, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
            window.get_Parameter(BuiltInParameter.INSTANCE_SILL_HEIGHT_PARAM).Set(6);
        }
    }

}

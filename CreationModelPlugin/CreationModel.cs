using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            if(!doorType.IsActive)
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

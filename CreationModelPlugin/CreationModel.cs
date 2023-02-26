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

            WallsUtils.AddWall(doc, level1, level2);

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
        
    }

}

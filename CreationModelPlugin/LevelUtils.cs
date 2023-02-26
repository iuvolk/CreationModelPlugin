using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreationModelPlugin
{
    public class LevelUtils
    {
        public static List<Level> GetLevels(Document doc)
        {
            List<Level> listLevel = new FilteredElementCollector(doc)
            .OfClass(typeof(Level))
            .OfType<Level>()
            .ToList();
            return listLevel;
        }

        public static Level GetLevelByName(Document doc, string levelName)
        {
            Level level = LevelUtils.GetLevels(doc)
                .Where(x => x.Name.Equals(levelName))
                .FirstOrDefault();

            return level;
        }
    }
}

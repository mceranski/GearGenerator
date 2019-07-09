using System.Linq;
using System.Windows;

namespace GearGenerator.Models
{
    public class Tooth
    {
        public Point[] PrimaryPoints;
        public Point[] MirrorPoints;

        public Point[] AllPoints => PrimaryPoints.Union(MirrorPoints).ToArray();
    }
}
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GearGenerator
{
    public class Gear
    {
        //The number of teeth on the gear
        public int Teeth { get; set; }
        public double PressureAngle { get; set; }
        public double PressureRadians => DegreesToRadians(PressureAngle);
        public double DiametralPitch { get; set; }

        public double PitchDiameter => Teeth / DiametralPitch;
        public double PitchRadius => PitchDiameter / 2d;

        public double BaseDiameter => PitchDiameter * Math.Cos(PressureRadians);
        public double BaseRadius => BaseDiameter / 2d;
        public double BaseCircumference => Math.PI * BaseDiameter;

        
        // The 1/20th of the Base Circle Radius (FCB) is an arbitrary division, which yields a very close approximation; you can use whatever fraction you think will yield a good result
        public double NCBDegrees = 20;
        public double FCB => BaseRadius / NCBDegrees;
        public double ACBDegrees => 360d / NCBDegrees;
        public double ToothSpacingDegrees => 360d / Teeth;

        public double Addendum => 1d / DiametralPitch;

        //the dedendum (d) is computed differently for other pressure angles; see Machinery's Handbook for the correct formula
        public double Dedendum => 1.157d / DiametralPitch;

        //The outside diameter shows the size of the circle that surrounds the teeth.
        public double OutsideDiameter => PitchDiameter + 2 * Addendum;
        public double OutsideRadius => OutsideDiameter / 2d;

        public double RootDiameter => PitchDiameter - 2 * Dedendum;
        public double RootRadius => RootDiameter / 2d;

        static double DegreesToRadians(double degrees)
        {
            return degrees * 0.0174533;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            var properties = typeof(Gear).GetProperties().Where(x => x.CanRead).ToArray();
            foreach (var property in properties)
            {
                sb.AppendLine($"{string.Join(" ",Regex.Split(property.Name, @"(?<!^)(?=[A-Z])"))} = {property.GetValue(this)}");
            }

            return sb.ToString();
        }
    }
}

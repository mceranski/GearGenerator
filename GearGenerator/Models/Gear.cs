using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GearGenerator
{
    //http://www.gearseds.com/files/6.3.1_Gear_Terms_Lesson_rev3.pdf
    public class Gear
    {
        public int Teeth { get; set; }
        public double PressureAngle { get; set; }
        public double PressureRadians => DegreesToRadians(PressureAngle);

        public double DiametralPitch => Teeth / PitchDiameter;
        public double PitchDiameter { get; set; }

        public double PitchRadius => PitchDiameter / 2d;

        public double BaseDiameter => (PitchDiameter * Math.Cos(PressureRadians));
        public double BaseRadius => BaseDiameter / 2d;
        public double BaseCircumference => Math.PI * BaseDiameter;

        
        public double NCB => BaseCircumference / FCB;
        // The 1/20th of the Base Circle Radius (FCB) is an arbitrary division, which yields a very close approximation; you can use whatever fraction you think will yield a good result
        public double FCB => BaseRadius / 20d;
        public double ACBDegrees => 360d / NCB;
        public double ToothSpacingDegrees => ( 360d / Teeth );

        //The radial distance from the pitch circle to the top of the gear tooth
        public double Addendum => 1d / DiametralPitch;

        //the dedendum (d) is computed differently for other pressure angles; see Machinery's Handbook for the correct formula
        //The radial distance from the pitch circle to the bottom of the tooth
        public double Dedendum => 1.157 / DiametralPitch;

        //The outside diameter shows the size of the circle that surrounds the teeth.
        public double OutsideDiameter => (Teeth + 2) / DiametralPitch;
        public double OutsideRadius => OutsideDiameter / 2d;

        public double RootDiameter => (Teeth -2) / DiametralPitch;
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

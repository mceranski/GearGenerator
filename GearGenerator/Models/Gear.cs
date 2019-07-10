using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GearGenerator
{
    //http://www.gearseds.com/files/6.3.1_Gear_Terms_Lesson_rev3.pdf
    //Drawing a gear: http://www.cartertools.com/involute.html
    //Gear Generator Online: https://geargenerator.com
    public class Gear
    {
        //The number of teeth the gear will have
        public int NumberOfTeeth { get; set; }

        //The pressure angle figures into the geometry or form of the gear tooth. It refers to the angle through which forces are transmitted between meshing gears.
        public double PressureAngle { get; set; }

        public double PressureRadians => DegreesToRadians(PressureAngle);

        //The Diametral Pitch describes the gear tooth size. The Diametral Pitch is expressed as the number of teeth per inch of Pitch Diameter. Larger gears have fewer teeth per inch of
        //Diametral Pitch. Another way of saying this; Gear teeth size varies inversely with Diametral Pitch.
        public double DiametralPitch => NumberOfTeeth / PitchDiameter;

        //The diameter of the Pitch Circle from which the gear is designed. An imaginary circle, which will contact the pitch circle of another gear when in mesh.
        public double PitchDiameter { get; set; }

        public double PitchRadius => PitchDiameter / 2d;

        //The circle used to form the involute section of the gear tooth
        public double BaseDiameter => (PitchDiameter * Math.Cos(PressureRadians));
        public double BaseRadius => BaseDiameter / 2d;
        public double BaseCircumference => Math.PI * BaseDiameter;

        
        public double NCB => BaseCircumference / FCB;
        // The 1/20th of the Base Circle Radius (FCB) is an arbitrary division, which yields a very close approximation; you can use whatever fraction you think will yield a good result
        public double FCB => BaseRadius / 20d;
        public double ACBDegrees => 360d / NCB;
        public double ToothSpacingDegrees => ( 360d / NumberOfTeeth );

        //The radial distance from the pitch circle to the top of the gear tooth
        public double Addendum => 1d / DiametralPitch;

        //not sure if this is correct, this is the calculate for the radius at the bottom of the tooth
        public double FilletRadius => .3 / DiametralPitch;

        //the dedendum (d) is computed differently for other pressure angles; see Machinery's Handbook for the correct formula
        //The radial distance from the pitch circle to the bottom of the tooth
        public double Dedendum => 1.157 / DiametralPitch;

        //The outside diameter shows the size of the circle that surrounds the teeth.
        public double OutsideDiameter => (NumberOfTeeth + 2) / DiametralPitch;
        public double OutsideRadius => OutsideDiameter / 2d;

        //The diameter at the Bottom of the tooth
        public double RootDiameter => (NumberOfTeeth -2) / DiametralPitch;
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

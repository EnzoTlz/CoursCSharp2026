using System;
using System.Collections.Generic;
using System.Text;

namespace ZooPrjt
{
    public class Chicken : Animal
    {
        public Chicken(string name, string description, int age) : base(name, description, age)
        {
        }
        public override void Speak()
        {
            Console.WriteLine("Cluck Cluck");
        }
    }
}

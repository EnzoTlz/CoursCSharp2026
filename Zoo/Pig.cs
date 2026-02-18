using System;
using System.Collections.Generic;
using System.Text;

namespace ZooPrjt
{
    public class Pig : Animal
    {
        public Pig(string name, string description, int age) : base(name, description, age)
        {

        }

        public override void Speak()
        {
            Console.WriteLine("Oink Oink");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace ZooPrjt
{
    public abstract class Animal
    {
        public string mName { get; set; }
        public string mDescription { get; set; }
        public int mAge { get; set; }

        public Animal(string name, string description, int age)
        {
            mName = name;
            mDescription = description;
            mAge = age;
        }

        public abstract void Speak();
    }
}

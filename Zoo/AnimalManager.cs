using System;
using System.Collections.Generic;
using System.Text;

namespace ZooPrjt
{
    public class AnimalManager : ICompareAnimal
    {
        public AnimalManager()
        {
        }

        public Animal AnimalComparer(Animal a, Animal b)
        {
            Animal result = a.mAge < b.mAge ? b : a;
            return result;
        }
    }

}

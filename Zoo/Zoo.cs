using System;
using System.Collections.Generic;
using System.Text;

namespace ZooPrjt
{
    public class Zoo( List<Animal> animals)
    {
        private List<Animal> mAnimals = animals;
        public void AddAnimal(Animal animal)
        {
            mAnimals.Add(animal);
        }
        public void ShowAnimals()
        {
            foreach (var animal in mAnimals)
            {
                Console.WriteLine($"Name: {animal.mName}, Description: {animal.mDescription}");
            }
        }

        public Animal GetAnimal(int id) 
        { 
            if (id < 0 || id >= mAnimals.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(id), "Invalid animal ID.");
            }
            return mAnimals[id];
        }

        public void killAnimal(Animal animal)
        {
            if (mAnimals.Contains(animal))
            {
                mAnimals.Remove(animal);
                Console.WriteLine($"{animal.mName} remove too older brother.");
            }
            else
            {
                Console.WriteLine($"{animal.mName} is not in the zoo.");
            }
        }
    }
}

// See https://aka.ms/new-console-template for more information
using ZooPrjt;

Console.WriteLine("Hello, World!");

List<Animal> animals = new List<Animal>();
AnimalManager animalManager = new AnimalManager();
Zoo zoo = new Zoo(animals);
zoo.AddAnimal(new Pig("Porky", "pepapig", 3));
zoo.AddAnimal(new Chicken("Leo", "KFC", 5));

Animal oldAnimal = animalManager.AnimalComparer(zoo.GetAnimal(0), zoo.GetAnimal(1));
zoo.killAnimal(oldAnimal);

zoo.ShowAnimals();

using System.Text.Json;
using System.Xml.Linq;

Console.WriteLine("Rentre le json à convertir en xml : ");
string json = Console.ReadLine();
JsonDocument jsonDocument = JsonDocument.Parse(json);

XDocument xmlDocument = new XDocument(); // Crée l'instance du doc xml
XElement rootElement = new XElement("root"); // crée l'element racine 
convertJsonToXml(jsonDocument.RootElement, rootElement, "root"); // appel la fonction récurcive
xmlDocument.Add(rootElement);
xmlDocument.Save("converted.xml");

/**
 * JsonElement : Ensembles des élément parse du json via JsonDocument.Parse(json)
 * parentElement: Element xml parent 
 * parentName: Nom de l'element parent (utilie pour avoir le nom quand on referme la balise )
 */
void convertJsonToXml(JsonElement jsonElement, XElement parentElement, string parentName)
{
    if (jsonElement.ValueKind == JsonValueKind.Object)
    {
        foreach (var property in jsonElement.EnumerateObject()) // énumérer les propriétés de l'objet 
        {
            XElement parentElementInObjt = new XElement(property.Name); 
            parentElement.Add(parentElementInObjt);
            convertJsonToXml(property.Value, parentElementInObjt, property.Name);
        }
    }
    else if(jsonElement.ValueKind == JsonValueKind.Array)
    {
        foreach (var item in jsonElement.EnumerateArray()) // énumérer les éléments du tableau 
        {
            XElement parentElementInArray = new XElement(parentName);
            parentElement.Add(parentElementInArray);
            convertJsonToXml(item, parentElementInArray, parentName);
        }
    }
    else
    {
        // Ajoute le contenue de l'element 
        parentElement.SetValue(jsonElement.ToString());
    }
}


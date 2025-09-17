using System.Collections.Generic;
using UnityEngine;

public class World:MonoBehaviour
{
    //private GameObject _worldInstance;
    private InstanceDescr _worldDescription;
    private Dictionary<int, GameObject> _worldLocations;

    public void Initialize(InstanceDescr data)
    {
       // _worldInstance = new GameObject(data.name) { transform = { parent = WorldGeneratorEngine.Instance.gameObject.transform } };
      //  _worldInstance.AddComponent<World>();
       // _worldInstance.GetComponent<World>()._worldInstance = _worldInstance;
      //  _worldInstance.GetComponent<World>()._worldDescription = data;
      //  _worldInstance.GetComponent<World>()._worldLocations = new Dictionary<int, GameObject>();
        _worldDescription = data;
        _worldLocations = new Dictionary<int, GameObject>();

        //сохран€ю в бд мир
        //  DataBase.instance.AddWorldElement(data.name, "Worlds");
    }

    public void LoadLocations()
    {
     /*   Location.LocationData[] locData = DataBase.instance.GetLocationData(new[] { "id", "name", "size" }, "Locations", new[] { "world_id" }, new[] { this._worldDescription.id.ToString() });
        foreach (var location in locData)
        {
            Debug.Log($"ID: {location.id}, Name: {location.name}, Size: {location.size}");
        }*/
    }

    public void CreateLocation(ILocationType locType, int size)
    {
        int locationNumber = GetWorldLastLocationNumber() == 0 ? 0 : GetWorldLastLocationNumber() + 1;
        Location location = locType.GenerateLocation(locationNumber, size, gameObject);

        // Location location = locType.GenerateLocation(locationNumber, size, gameObject);
        // _worldLocations.Add(locationNumber, location.GetLocationInstance());
    }

    public int GetWorldLastLocationNumber()
    {
        //  return DataBase.instance.GetLastID("Locations");
        return 0;
    }

    public GameObject GetWorldInstance()
    {

        // return _worldInstance;
        return gameObject;
    }
}

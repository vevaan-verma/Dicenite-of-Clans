using System;
using System.Collections.Generic;
using UnityEditor.Rendering.Universal.ShaderGraph;
using UnityEngine;

[CreateAssetMenu]
public class PlaceableObjectDatabase : ScriptableObject {

    [Header("Object Settings")]
    public List<ObjectData> objectData;

}

[Serializable]
public class ObjectData {

    [field: SerializeField]
    public string name {

        get; private set;

    }

    public int ID {

        get; private set;

    }

    [field: SerializeField]
    public Sprite icon {

        get; private set;

    }

    [field: SerializeField]
    public Vector2Int size {

        get; private set;

    } = Vector2Int.one;

    [field: SerializeField]
    public GameObject prefab {

        get; private set;

    }

    [field: SerializeField]
    public bool stackable {

        get; private set;

    }

    [field: SerializeField]
    public int price {

        get; private set;

    }

    [field: SerializeField]
    public GameManager.MaterialType materialType {

        get; private set;

    }

    [field: SerializeField]
    public float spawnProbability {

        get; private set;

    }

    public void SetID(int ID) {

        this.ID = ID;

    }
}

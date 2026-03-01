using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Item Combination Recipe Database")]
public class CombinationRecipeDb_SO : ScriptableObject
{
    [Serializable]
    public class Recipe
    {
        [Tooltip("Item A id")]
        public string itemAId;

        [Tooltip("Item B id")]
        public string itemBId;

        [Tooltip("Result item")]
        public UniqueItem_SO result;
    }

    [SerializeField] private List<Recipe> recipes = new();

    public bool TryGetResult(string itemAId, string itemBId, out UniqueItem_SO result)
    {
        result = null;

        if (string.IsNullOrEmpty(itemAId) || string.IsNullOrEmpty(itemBId))
            return false;

        for (int i = 0; i < recipes.Count; i++)
        {
            var r = recipes[i];
            if (r == null || r.result == null) continue;

            bool matchDirect = r.itemAId == itemAId && r.itemBId == itemBId;
            bool matchSwapped = r.itemAId == itemBId && r.itemBId == itemAId;

            if (matchDirect || matchSwapped)
            {
                result = r.result;
                return true;
            }
        }

        return false;
    }
}

﻿// Saves the item info in a ScriptableObject that can be used ingame by
// referencing it from a MonoBehaviour. It only stores an item's static data.
//
// We also add each one to a dictionary automatically, so that all of them can
// be found by name without having to put them all in a database. Note that we
// have to put them all into the Resources folder and use Resources.LoadAll to
// load them. This is important because some items may not be referenced by any
// entity ingame (e.g. when a special event item isn't dropped anymore after the
// event). But all items should still be loadable from the database, even if
// they are not referenced by anyone anymore. So we have to use Resources.Load.
// (before we added them to the dict in OnEnable, but that's only called for
//  those that are referenced in the game. All others will be ignored be Unity.)
//
// An Item can be created by right clicking the Resources folder and selecting
// Create->uSurvival Item. Existing items can be found in the Resources folder.
//
// Note: this class is not abstract so we can create 'useless' items like recipe
// ingredients, etc.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[CreateAssetMenu(menuName="uSurvival Item/General", order=999)]
public class ScriptableItem : ScriptableObject
{
    [Header("Base Stats")]
    public int maxStack = 1; // set default already
    public int maxDurability = 1000; // all items should have durability
    public bool destroyable;
    [TextArea(1, 30)] public string toolTip;
    public Sprite image;

    [Header("3D Representation")]
    public ItemDrop drop; // spawned when players die
    public GameObject modelPrefab; // shown when equipped/in hands/etc.

    // tooltip /////////////////////////////////////////////////////////////////
    // fill in all variables into the tooltip
    // this saves us lots of ugly string concatenation code. we can't do it in
    // ScriptableItem because some variables can only be replaced here, hence we
    // would end up with some variables not replaced in the string when calling
    // Tooltip() from the template.
    // -> note: each tooltip can have any variables, or none if needed
    // -> example usage:
    /*
    <b>{NAME}</b>
    Description here...

    Destroyable: {DESTROYABLE}
    Amount: {AMOUNT}
    */
    public virtual string ToolTip()
    {
        // we use a StringBuilder so that addons can modify tooltips later too
        // ('string' itself can't be passed as a mutable object)
        StringBuilder tip = new StringBuilder(toolTip);
        tip.Replace("{NAME}", name);
        tip.Replace("{DESTROYABLE}", (destroyable ? "Yes" : "No"));
        return tip.ToString();
    }

    // caching /////////////////////////////////////////////////////////////////
    // we can only use Resources.Load in the main thread. we can't use it when
    // declaring static variables. so we have to use it as soon as 'dict' is
    // accessed for the first time from the main thread.
    // -> we save the hash so the dynamic item part doesn't have to contain and
    //    sync the whole name over the network
    static Dictionary<int, ScriptableItem> cache;
    public static Dictionary<int, ScriptableItem> dict
    {
        get
        {
            // load if not loaded yet
            return cache ?? (cache = Resources.LoadAll<ScriptableItem>("").ToDictionary(
                item => item.name.GetStableHashCode(), item => item)
            );
        }
    }
}
﻿using System;
using LiteNetLib.Utils;
using SimpleJSON;
using UnityEngine;
using Random = System.Random;

public class BeatmapBookmark : BeatmapObject
{
    private static readonly Random rand = new Random();

    public string Name = "Invalid Bookmark";
    public Color Color;

    public BeatmapBookmark(JSONNode node)
    {
        Time = RetrieveRequiredNode(node, "_time").AsFloat;
        Name = RetrieveRequiredNode(node, "_name");
        if (node.HasKey("_color")) Color = RetrieveRequiredNode(node, "_color");
        else Color = Color.HSVToRGB((float)rand.NextDouble(), 0.75f, 1);
    }


    public BeatmapBookmark(float time, string name)
    {
        Time = time;
        Name = name;
        Color = Color.HSVToRGB((float)rand.NextDouble(), 0.75f, 1);
    }

    public override ObjectType BeatmapType { get; set; } = ObjectType.BpmChange;

    public override JSONNode ConvertToJson()
    {
        JSONNode node = new JSONObject();
        node["_time"] = Math.Round(Time, DecimalPrecision);
        node["_name"] = Name;
        node["_color"] = Color;
        return node;
    }
    public override void Serialize(NetDataWriter writer)
    {
        writer.Put(Time);
        writer.Put(Name);
        writer.Put(ColorUtility.ToHtmlStringRGBA(Color));
        writer.Put(CustomData?.ToString() ?? "null");
    }

    public override void Deserialize(NetDataReader reader)
    {
        Time = reader.GetFloat();
        Name = reader.GetString();
        ColorUtility.TryParseHtmlString(reader.GetString(), out Color);
        if (reader.TryGetString(out var customData))
        {
            CustomData = JSON.Parse(customData);
        }
    }

    protected override bool IsConflictingWithObjectAtSameTime(BeatmapObject other, bool deletion) => true;

    public override void Apply(BeatmapObject originalData)
    {
        base.Apply(originalData);

        if (originalData is BeatmapBookmark bm) Name = bm.Name;
    }
}

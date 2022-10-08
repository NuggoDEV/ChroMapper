using System;
using Beatmap.Base;
using SimpleJSON;

namespace Beatmap.V2
{
    public class V2Waypoint : IWaypoint
    {
        public V2Waypoint()
        {
        }

        public V2Waypoint(IWaypoint other) : base(other)
        {
        }

        public V2Waypoint(JSONNode node)
        {
            Time = RetrieveRequiredNode(node, "_time").AsFloat;
            PosX = RetrieveRequiredNode(node, "_lineIndex").AsInt;
            PosY = RetrieveRequiredNode(node, "_lineLayer").AsInt;
            OffsetDirection = RetrieveRequiredNode(node, "_offsetDirection").AsInt;
            CustomData = node["_customData"];
        }

        public V2Waypoint(float time, int posX, int posY, int offsetDirection, JSONNode customData = null) : base(time,
            posX, posY, offsetDirection, customData)
        {
        }

        public override string CustomKeyTrack { get; } = "_track";

        public override string CustomKeyColor { get; } = "_color";

        public override string CustomKeyCoordinate { get; } = "_position";

        public override string CustomKeyWorldRotation { get; } = "_rotation";

        public override string CustomKeyLocalRotation { get; } = "_localRotation";

        public override JSONNode ToJson()
        {
            JSONNode node = new JSONObject();
            node["_time"] = Math.Round(Time, DecimalPrecision);
            node["_lineIndex"] = PosX;
            node["_lineLayer"] = PosY;
            node["_offsetDirection"] = OffsetDirection;
            if (CustomData != null) node["_customData"] = CustomData;
            return node;
        }

        public override IItem Clone() => new V2Waypoint(Time, PosX, PosY, OffsetDirection, CustomData?.Clone());
    }
}

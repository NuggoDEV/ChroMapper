using System;
using System.Linq;
using Beatmap.Base;
using SimpleJSON;
using UnityEngine;

namespace Beatmap.V3
{
    public class V3LightTranslationBase : BaseLightTranslationBase
    {
        public V3LightTranslationBase()
        {
        }

        public V3LightTranslationBase(JSONNode node)
        {
            Time = RetrieveRequiredNode(node, "b").AsFloat;
            UsePrevious = RetrieveRequiredNode(node, "p").AsInt;
            EaseType = RetrieveRequiredNode(node, "e").AsInt;
            Translation = RetrieveRequiredNode(node, "t").AsFloat;
            CustomData = node["customData"];
        }

        public V3LightTranslationBase(float time, float translation, int easeType, int usePrevious,
            JSONNode customData = null) : base(time, translation, easeType, usePrevious, customData)
        {
        }

        public override Color? CustomColor
        {
            get => null;
            set { }
        }

        public override string CustomKeyTrack { get; } = "track";
        public override string CustomKeyColor { get; } = "color";

        public override JSONNode ToJson()
        {
            JSONNode node = new JSONObject();
            node["b"] = Math.Round(Time, DecimalPrecision);
            node["p"] = UsePrevious;
            node["e"] = EaseType;
            node["t"] = Translation;
            CustomData = SaveCustom();
            if (!CustomData.Children.Any()) return node;
            node["customData"] = CustomData;
            return node;
        }

        public override BaseItem Clone() => new V3LightTranslationBase(Time, Translation, EaseType,
            UsePrevious, SaveCustom().Clone());
    }
}
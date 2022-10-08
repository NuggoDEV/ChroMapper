using Beatmap.Enums;
using SimpleJSON;

namespace Beatmap.Base.Customs
{
    public abstract class ICustomEvent : IObject
    {
        protected ICustomEvent()
        {
        }

        protected ICustomEvent(JSONNode node) => InstantiateHelper(ref node);

        protected ICustomEvent(float time, string type, JSONNode node = null) : base(time)
        {
            Type = type;
            Data = node is JSONObject ? node : new JSONObject();
        }

        public override ObjectType ObjectType { get; set; } = ObjectType.BpmChange;

        public override bool IsConflictingWithObjectAtSameTime(IObject other, bool deletion = false) => false;

        public override JSONNode ToJson() =>
            new JSONObject
            {
                [KeyTime] = Time,
                [KeyType] = Type,
                [KeyData] = Data
            };

        public string Type { get; set; }
        public JSONNode Data { get; set; }
        public abstract string KeyTime { get; }
        public abstract string KeyType { get; }
        public abstract string KeyData { get; }

        protected void InstantiateHelper(ref JSONNode node)
        {
            Time = RetrieveRequiredNode(node, KeyTime).AsFloat;
            Type = RetrieveRequiredNode(node, KeyType).Value;
            Data = RetrieveRequiredNode(node, KeyData);
        }
    }
}

using ProtoBuf;

namespace Stories.API.Models
{
    [ProtoContract]
    public class Story
    {
        [ProtoMember(1)]
        public int Id { get; set; }

        [ProtoMember(2)] 
        public string By { get; set; } = default!;

        [ProtoMember(3)]
        public int Descendants { get; set; }

        [ProtoMember(4)]
        public List<int> Kids { get; set; } = new();

        [ProtoMember(5)]
        public int Score { get; set; }

        [ProtoMember(6)]
        public int Time { get; set; }

        [ProtoMember(7)]
        public string Title { get; set; } = default!;

        [ProtoMember(8)]
        public string Type { get; set; } = default!;

        [ProtoMember(9)]
        public string Url { get; set; } = default!;
    }
}

using System;

namespace Battlelog.BattlelogApi.Models
{
    public class CommunityMission
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Reward { get; set; }
        public DateTimeOffset Start { get; set; }
        public DateTimeOffset End { get; set; }
        public BattlelogGame Game { get; set; }

        public TimeSpan TimeSpanBeforeEnding
        {
            get
            {
                if (Ended)
                    return TimeSpan.Zero;
                return (End - DateTimeOffset.UtcNow);
            }
        }

        public bool Ended => DateTimeOffset.UtcNow > End;

        protected bool Equals(CommunityMission other)
        {
            return string.Equals(Name, other.Name) && Start.Equals(other.Start) && End.Equals(other.End) &&
                   Game == other.Game;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((CommunityMission) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Name.GetHashCode();
                hashCode = (hashCode*397) ^ Start.GetHashCode();
                hashCode = (hashCode*397) ^ End.GetHashCode();
                hashCode = (hashCode*397) ^ (int) Game;
                return hashCode;
            }
        }

        public bool OnlyEndDiffer(CommunityMission other)
        {
            return string.Equals(Name, other.Name) && Start.Equals(other.Start) && Game == other.Game &&
                   !End.Equals(other.End);
        }

        public override string ToString()
        {
            var s = "";

            s += $"The mission \"{Name}\" for the game {Game}";

            if (Ended)
            {
                s += $" has ended on {End}.\r\n";
            }
            else
            {
                s += $" will ended on {End} in {TimeSpanBeforeEnding}.\r\n";
            }
            s += $"Description : {Description}\r\n";
            s += $"Reward : {Reward}\r\n";
            return s;
        }
    }
}
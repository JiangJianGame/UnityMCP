#if !UNITY_2021_2_OR_NEWER
using System;
using UnityEditor;

namespace UnityEditor.Build
{
    public struct NamedBuildTarget : IEquatable<NamedBuildTarget>
    {
        public BuildTargetGroup TargetGroup { get; }

        public NamedBuildTarget(BuildTargetGroup group)
        {
            TargetGroup = group;
        }

        public static NamedBuildTarget FromBuildTargetGroup(BuildTargetGroup group)
        {
            return new NamedBuildTarget(group);
        }

        public static implicit operator BuildTargetGroup(NamedBuildTarget target)
        {
            return target.TargetGroup;
        }

        public static implicit operator NamedBuildTarget(BuildTargetGroup group)
        {
            return new NamedBuildTarget(group);
        }

        public bool Equals(NamedBuildTarget other) => TargetGroup == other.TargetGroup;
        public override bool Equals(object obj) => obj is NamedBuildTarget other && Equals(other);
        public override int GetHashCode() => (int)TargetGroup;
        public static bool operator ==(NamedBuildTarget left, NamedBuildTarget right) => left.Equals(right);
        public static bool operator !=(NamedBuildTarget left, NamedBuildTarget right) => !left.Equals(right);
    }
}
#endif

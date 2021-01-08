using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace TestContainers.Utility
{
    public abstract class Versioning
    {
        public static readonly Versioning ANY = new AnyVersion(); 
        public abstract bool IsValid();
        public abstract string Separator { get; }
        
    }

    public class AnyVersion : Versioning
    {
        public override string Separator => ":";

        public override bool IsValid() => true;

        public override string ToString() => "latest";
    }

    public class TagVersioning : Versioning
    {
        public static readonly Regex TagRegex = new Regex("[\\w][\\w.\\-]{0,127}");
        private readonly string _tag;

        public TagVersioning(string tag)
        {
            _tag = tag;
        }
        public override string Separator => ":";

        public override bool IsValid() => TagRegex.IsMatch(_tag);

        public override string ToString() => _tag;
    }

    public class Sha256Versioning : Versioning
    {
        public static readonly Regex HashRegex = new Regex("[0-9a-fA-F]{32,}");
        private readonly string _hash;

        public Sha256Versioning(string hash)
        {
            _hash = hash;
        }
        public override string Separator => "@";

        public override bool IsValid() => HashRegex.IsMatch(_hash);

        public override string ToString() => "sha256:" + _hash;
    }

}

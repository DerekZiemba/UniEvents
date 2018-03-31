using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using UniEvents.Core;
using UniEvents.Core.Managers;
using ZMBA;

namespace UniEvents.Models {


   [JsonObject(MemberSerialization = MemberSerialization.OptIn, ItemNullValueHandling =NullValueHandling.Ignore)]
   public abstract class LocationNode : ZMBA.Interfaces.IKeyName, IEqualityComparer, IEqualityComparer<LocationNode>, IComparable<LocationNode> {
      private static StringComparer Cmp = StringComparer.OrdinalIgnoreCase;

      string ZMBA.Interfaces.IKeyName.Key => Name;

      public LocationNode Parent;
      public Dictionary<String, LocationNode> Children;

      public abstract CountryRegionNode CountryNode { get; }
      public abstract AdminDistrictNode StateNode { get; }
      public abstract LocalityNode CityNode { get; }
      public abstract PostalCodeNode ZipNode { get; }
      public abstract StreetAddressNode StreetNode { get; }

      [JsonProperty(PropertyName = "Name", Order = 1)] public string Name { get; internal set; }     
      [JsonProperty(PropertyName = "City", Order = 3)] public string CityName => CityNode?.Name;
      [JsonProperty(PropertyName = "State", Order = 4)] public string StateName => StateNode?.PreferredName ?? StateNode?.Name;
      [JsonProperty(PropertyName = "Zip", Order = 5)] public string ZipCode => ZipNode?.Name;
      [JsonProperty(PropertyName = "Country", Order = 6)] public string CountryName => CountryNode?.PreferredName ?? CountryNode?.Name;


      public override bool Equals(object x) => this.Equals((LocationNode)x);
      public bool Equals(LocationNode other) => Equals(this, other);
      public new bool Equals(object x, object y) => Equals((LocationNode)x, (LocationNode)y);
      public virtual bool Equals(LocationNode x, LocationNode y) {
         if (x == null || y == null) { return false; }
         if (!Cmp.Equals(x.Name, y.Name)) { return false; }
         if (!Cmp.Equals(x.CityName, y.CityName)) { return false; }
         if (!Cmp.Equals(x.StateName, y.StateName)) { return false; }
         if (!Cmp.Equals(x.ZipCode, y.ZipCode)) { return false; }
         if (!Cmp.Equals(x.CountryName, y.CountryName)) { return false; }
         return x.GetType() == y.GetType();
      }


      public override int GetHashCode() => GetHashCode(this);
      public int GetHashCode(object obj) => GetHashCode((LocationNode)obj);
      public virtual int GetHashCode(LocationNode obj) => StringComparer.OrdinalIgnoreCase.GetHashCode(Name);

      public virtual int CompareTo(LocationNode other) {
         if (ReferenceEquals(this, other)) return 0;
         if (ReferenceEquals(null, other)) return 1;
         return string.Compare(Name, other.Name, StringComparison.OrdinalIgnoreCase);
      }


      public abstract class LocationNodeWithAbbreviations : LocationNode {
         public string[] Variations { get; internal set; }
         public string PreferredName { get; internal set; }
         protected LocationNodeWithAbbreviations() { Children = new Dictionary<String, LocationNode>(Cmp); }

         public override AdminDistrictNode StateNode => null;
         public override LocalityNode CityNode => null;
         public override PostalCodeNode ZipNode => null;
         public override StreetAddressNode StreetNode => null;

         public override bool Equals(LocationNode x, LocationNode y) {
            if(base.Equals(x, y)) {
               LocationNodeWithAbbreviations _x = (LocationNodeWithAbbreviations)x;
               LocationNodeWithAbbreviations _y = (LocationNodeWithAbbreviations)y;
               return _x.Variations.Length == _y.Variations.Length && StringComparer.OrdinalIgnoreCase.Equals(_x.PreferredName, _y.PreferredName) && _x.Variations.SequenceEqual(_y.Variations);
            }
            return false;
         }
      }

      public class CountryRegionNode : LocationNodeWithAbbreviations {
         public override string ToString() => Name;
         public override CountryRegionNode CountryNode => this;

      }

      public class AdminDistrictNode : LocationNodeWithAbbreviations {
         public override string ToString() => Helpers.FormatAddress(null, null, null, StateName, null, CountryName);
         public override CountryRegionNode CountryNode => (CountryRegionNode)this.Parent;
         public override AdminDistrictNode StateNode => this;
      }


      public class LocalityNode : LocationNode {
         public LocalityNode() { Children = new Dictionary<String, LocationNode>(Cmp); }

         public override string ToString() => Helpers.FormatAddress(null, null, CityName, StateName, null, CountryName);

         public override CountryRegionNode CountryNode => (CountryRegionNode)StateNode.Parent;
         public override AdminDistrictNode StateNode => (AdminDistrictNode)CityNode.Parent;
         public override LocalityNode CityNode => this;
         public override PostalCodeNode ZipNode => null;
         public override StreetAddressNode StreetNode => null;
      }

      public abstract class SubdivisionNode : LocationNode {
         [JsonProperty(PropertyName = "Latitude", Order = 7)] public double Latitude { get; internal set; }
         [JsonProperty(PropertyName = "Longitude", Order = 8)] public double Longitude { get; internal set; }

         public override CountryRegionNode CountryNode => (CountryRegionNode)StateNode.Parent;
         public override AdminDistrictNode StateNode => (AdminDistrictNode)CityNode.Parent;
         public override LocalityNode CityNode => (LocalityNode)(ZipNode != null ? ZipNode.Parent : StreetNode.Parent);
      }

      public class PostalCodeNode : SubdivisionNode {
         public override string ToString() => Helpers.FormatAddress(null, null, CityName, StateName, ZipCode, CountryName);
         public override PostalCodeNode ZipNode => this;
         public override StreetAddressNode StreetNode => null;
      }

      public class StreetAddressNode : SubdivisionNode {
         [JsonProperty(PropertyName = "Address", Order = 2)] public string AddressLine { get; internal set; }

         public override string ToString() => Helpers.FormatAddress(Name, AddressLine, CityName, StateName, ZipCode, CountryName);

         public override PostalCodeNode ZipNode => (PostalCodeNode)(StreetNode.Parent is PostalCodeNode ? StreetNode.Parent : null);
         public override StreetAddressNode StreetNode => this;

         public override int GetHashCode() { 
            unchecked {
               var hashCode = base.GetHashCode();
               return (hashCode * 397) ^ (AddressLine != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(AddressLine) : 0);
            }
         }
         public override bool Equals(LocationNode x, LocationNode y) {
            if(base.Equals(x, y)) {
               var _x = x as StreetAddressNode;
               var _y = y as StreetAddressNode;
               return StringComparer.OrdinalIgnoreCase.Equals(_x.AddressLine, _y.AddressLine);
            }
            return false;
         }
         public override int CompareTo(LocationNode other) {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return string.Compare(Name + AddressLine, other.Name + (other as StreetAddressNode)?.AddressLine, StringComparison.OrdinalIgnoreCase);
         }
      }




   }


}

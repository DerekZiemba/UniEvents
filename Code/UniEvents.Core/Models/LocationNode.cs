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
using ZMBA.Interfaces;

namespace UniEvents.Models {


   [JsonObject(MemberSerialization = MemberSerialization.OptIn, ItemNullValueHandling =NullValueHandling.Ignore)]
   public abstract class LocationNode : IKey<string>, IValue<string>   {
      private static StringComparer Cmp = StringComparer.OrdinalIgnoreCase;
      protected int _hashcode;

      public LocationNode Parent;
      public Dictionary<String, LocationNode> Children;

      public string Key { get; internal set; }
      public long ID { get; set; }

      string IValue<string>.Value => Formatted;
      
      
      [JsonProperty(PropertyName = "Address", Order = 2)] public string Address => StreetNode?.Key;
      [JsonProperty(PropertyName = "City", Order = 3)] public string CityName => CityNode?.Key;
      [JsonProperty(PropertyName = "State", Order = 4)] public string StateName => StateNode?.Key;
      [JsonProperty(PropertyName = "Zip", Order = 5)] public string ZipCode => ZipNode?.Key;
      [JsonProperty(PropertyName = "Country", Order = 6)] public string CountryName => CountryNode?.Abbreviation ?? CountryNode?.Key;
      [JsonProperty(PropertyName = "Formatted", Order = 9)] public string Formatted { get; protected set; }


      public abstract CountryRegionNode CountryNode { get; }
      public abstract AdminDistrictNode StateNode { get; }
      public abstract LocalityNode CityNode { get; }
      public abstract PostalCodeNode ZipNode { get; }
      public abstract StreetAddressNode StreetNode { get; }


      public override string ToString() => Formatted;

      public override bool Equals(object x) => this.Equals((LocationNode)x);
      public bool Equals(LocationNode other) {
         if (other == null) { return false; }
         if(this._hashcode != other._hashcode) { return false; }
         return Cmp.Equals(this.Formatted, other.Formatted);
      }

      public override int GetHashCode() => this._hashcode;


      public abstract class LocationNodeWithAbbreviations : LocationNode {
         public string[] Variations { get; internal set; }
         public string Abbreviation { get; }

         public override AdminDistrictNode StateNode => null;
         public override LocalityNode CityNode => null;
         public override PostalCodeNode ZipNode => null;
         public override StreetAddressNode StreetNode => null;


         protected LocationNodeWithAbbreviations(string fullName, string[] variations) {
            Children = new Dictionary<String, LocationNode>(Cmp);
            Key = fullName;
            Variations = variations;
            for (var i = 0; i < Variations.Length; i++) {
               if (Variations[i][0] == '$') {
                  Variations[i] = Variations[i].Substring(1);
                  Abbreviation = Variations[i];
               }
            }
            if (Abbreviation == null) {
               Abbreviation = Variations[0];
            }
         }
      }

      public class CountryRegionNode : LocationNodeWithAbbreviations {
         public override CountryRegionNode CountryNode => this;

         public CountryRegionNode(string fullName, string[] variations): base(fullName, variations) {
            Formatted = fullName;
            unchecked { _hashcode = (typeof(CountryRegionNode).GetHashCode() * 397) ^ Cmp.GetHashCode(fullName); }
         }
      }

      public class AdminDistrictNode : LocationNodeWithAbbreviations {
         public override CountryRegionNode CountryNode => (CountryRegionNode)this.Parent;
         public override AdminDistrictNode StateNode => this;
         public AdminDistrictNode(string fullName, string[] variations, CountryRegionNode parent) : base(fullName, variations) {
            this.Parent = parent;
            Formatted = Helpers.FormatAddress(null, null, null, StateNode?.Abbreviation ?? StateNode?.Key, null, CountryName);
            unchecked { _hashcode = (typeof(AdminDistrictNode).GetHashCode() * 397) ^ Cmp.GetHashCode(Formatted); }
         }
      }


      public class LocalityNode : LocationNode {
         public override CountryRegionNode CountryNode => (CountryRegionNode)StateNode.Parent;
         public override AdminDistrictNode StateNode => (AdminDistrictNode)CityNode.Parent;
         public override LocalityNode CityNode => this;
         public override PostalCodeNode ZipNode => null;
         public override StreetAddressNode StreetNode => null;

         public LocalityNode(string name, AdminDistrictNode parent) {
            this.Children = new Dictionary<String, LocationNode>(Cmp);
            this.Parent = parent;
            this.Key = name;
            this.Formatted = Helpers.FormatAddress(null, null, CityName, StateNode?.Abbreviation ?? StateNode?.Key, null, CountryName);
            unchecked { _hashcode = (typeof(LocalityNode).GetHashCode() * 397) ^ Cmp.GetHashCode(Formatted); }
         }
      }

      public abstract class SubdivisionNode : LocationNode {      
         [JsonProperty(PropertyName = "Latitude", Order = 7)] public double Latitude { get; internal set; }
         [JsonProperty(PropertyName = "Longitude", Order = 8)] public double Longitude { get; internal set; }

         public override CountryRegionNode CountryNode => (CountryRegionNode)StateNode.Parent;
         public override AdminDistrictNode StateNode => (AdminDistrictNode)CityNode.Parent;
         public override LocalityNode CityNode => (LocalityNode)(ZipNode != null ? ZipNode.Parent : StreetNode.Parent);
       
      }

      public class PostalCodeNode : SubdivisionNode {
         public override PostalCodeNode ZipNode => this;
         public override StreetAddressNode StreetNode => null;

         public PostalCodeNode(string name, LocalityNode parent) {
            this.Parent = parent;
            this.Key = name;
            this.Formatted = Helpers.FormatAddress(null, null, CityName, StateNode?.Abbreviation ?? StateNode?.Key, Key, CountryName);
            unchecked { _hashcode = (typeof(PostalCodeNode).GetHashCode() * 397) ^ Cmp.GetHashCode(Formatted); }
         }
      }

      public class StreetAddressNode : SubdivisionNode {
         [JsonProperty(PropertyName = "Name", Order = 1)] public string Name { get; }

         public override PostalCodeNode ZipNode => (PostalCodeNode)(StreetNode.Parent is PostalCodeNode ? StreetNode.Parent : null);
         public override StreetAddressNode StreetNode => this;

         public StreetAddressNode(string name, string addressline, LocationNode parent) {
            this.Parent = parent;
            this.Key = addressline;
            this.Name = name;
            this.Formatted = Helpers.FormatAddress(Name, Address, CityName, StateNode?.Abbreviation ?? StateNode?.Key, Key, CountryName);
            unchecked { _hashcode = (typeof(StreetAddressNode).GetHashCode() * 397) ^ Cmp.GetHashCode(Formatted); }
         }
      }



      
   }


}

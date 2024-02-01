﻿using System;
using System.Collections.Generic;
using System.Data.Entity.Design.PluralizationServices;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

using Microsoft.VisualStudio.Modeling.Diagrams;
using Microsoft.VisualStudio.Modeling.Validation;

using Sawczyn.EFDesigner.EFModel.Annotations;
using Sawczyn.EFDesigner.EFModel.Extensions;

namespace Sawczyn.EFDesigner.EFModel
{
   [ValidationState(ValidationState.Enabled)]
   public partial class ModelRoot : IHasStore
   {
      /// <summary>
      ///    Provides pluralization for names (currently English only)
      /// </summary>
      public static readonly PluralizationService PluralizationService;

      internal static bool BatchUpdating = false;

      /// <summary>
      /// If true, property editor will only show known legal property type choices
      /// </summary>
      public static bool LimitModelAttributeTypeChoices { get; set; }

      /// <summary>
      /// Represents an array of C# reserved words.
      /// </summary>
      public readonly string[] ReservedWords =
      {
         "abstract",
         "as",
         "base",
         "bool",
         "break",
         "byte",
         "case",
         "catch",
         "char",
         "checked",
         "class",
         "const",
         "continue",
         "decimal",
         "default",
         "delegate",
         "do",
         "double",
         "else",
         "enum",
         "event",
         "explicit",
         "extern",
         "false",
         "finally",
         "fixed",
         "float",
         "for",
         "foreach",
         "goto",
         "if",
         "implicit",
         "in",
         "int",
         "interface",
         "internal",
         "is",
         "lock",
         "long",
         "namespace",
         "new",
         "null",
         "object",
         "operator",
         "out",
         "override",
         "params",
         "private",
         "protected",
         "public",
         "readonly",
         "ref",
         "return",
         "sbyte",
         "sealed",
         "short",
         "sizeof",
         "stackalloc",
         "static",
         "string",
         "struct",
         "switch",
         "this",
         "throw",
         "true",
         "try",
         "typeof",
         "uint",
         "ulong",
         "unchecked",
         "unsafe",
         "ushort",
         "using",
         "virtual",
         "void",
         "volatile",
         "while"
      };

      static ModelRoot()
      {
         try
         {
            PluralizationService = PluralizationService.CreateService(CultureInfo.CurrentCulture);
            InstallationDirectory = Path.GetDirectoryName(Assembly.GetAssembly(typeof(ModelRoot)).Location);
         }
         catch (NotImplementedException)
         {
            PluralizationService = null;
         }
      }

      internal static string InstallationDirectory { get; set; }

      /// <summary>
      ///    Current method that validates the model
      /// </summary>
      public static Action ExecuteValidator { get; set; }

      /// <summary>
      ///    Method to finds the diagram that currently has focus, if any
      /// </summary>
      public static Func<Diagram> GetCurrentDiagram { get; set; }

      /// <summary>
      ///    Method to output a diagram as a zip file
      /// </summary>
      public static Func<bool> WriteDiagramAsBinary { get; set; } = () => false;

      /// <summary>
      ///    FQN of the DbContext-derived class
      /// </summary>

      // ReSharper disable once UnusedMember.Global
      public string FullName
      {
         get
         {
            return string.IsNullOrWhiteSpace(Namespace)
                      ? $"global::{EntityContainerName}"
                      : $"global::{Namespace}.{EntityContainerName}";
         }
      }

      /// <summary>
      ///    True if the model is EFCore and the Entity Framework version is >= 5
      /// </summary>
      public bool IsEFCore5Plus
      {
         get
         {
            return EntityFrameworkVersion == EFVersion.EFCore && (EntityFrameworkPackageVersion == "Latest" || GetEntityFrameworkPackageVersionNum() >= 5);
         }
      }

      /// <summary>
      ///    True if the model is EFCore and the Entity Framework version is >= 6
      /// </summary>
      public bool IsEFCore6Plus
      {
         get
         {
            return EntityFrameworkVersion == EFVersion.EFCore && (EntityFrameworkPackageVersion == "Latest" || GetEntityFrameworkPackageVersionNum() >= 6);
         }
      }

      /// <summary>
      ///    True if the model is EFCore and the Entity Framework version is >= 7
      /// </summary>
      public bool IsEFCore7Plus
      {
         get
         {
            return EntityFrameworkVersion == EFVersion.EFCore && (EntityFrameworkPackageVersion == "Latest" || GetEntityFrameworkPackageVersionNum() >= 7);
         }
      }

      /// <summary>
      ///    Finds all diagrams associated to this model
      /// </summary>
      public EFModelDiagram[] GetDiagrams()
      {
         return Store
               .DefaultPartitionForClass(EFModelDiagram.DomainClassId)
               .GetAll<EFModelDiagram>()
               .ToArray();
      }

      #region Filename

      private string filename;

      /// <summary>
      ///    Sets the filename for saving this model
      /// </summary>
      public void SetFileName(string fileName)
      {
         filename = fileName;
      }

      /// <summary>
      ///    Gets the filename of this model
      /// </summary>
      public string GetFileName()
      {
         return filename;
      }

      #endregion

      #region OutputLocations

      private OutputLocations outputLocationsStorage;

      private OutputLocations GetOutputLocationsValue()
      {
         return outputLocationsStorage ?? (outputLocationsStorage = new OutputLocations(this));
      }

      private void SetOutputLocationsValue(OutputLocations value)
      {
         outputLocationsStorage = value;
      }

      #endregion OutputLocations

      #region Namespaces

      private Namespaces namespacesStorage;

      private Namespaces GetNamespacesValue()
      {
         return namespacesStorage ?? (namespacesStorage = new Namespaces(this));
      }

      private void SetNamespacesValue(Namespaces value)
      {
         namespacesStorage = value;
      }

      #endregion Namespaces

      #region Valid types based on EF version

      /// <summary>
      ///    List of spatial types, depending on EF version selected
      /// </summary>
      public string[] SpatialTypes
      {
         get
         {
            return EntityFrameworkVersion == EFVersion.EF6
                      ? new[]
                        {
                           "Geography",
                           "GeographyCollection",
                           "GeographyLineString",
                           "GeographyMultiLineString",
                           "GeographyMultiPoint",
                           "GeographyMultiPolygon",
                           "GeographyPoint",
                           "GeographyPolygon",
                           "Geometry",
                           "GeometryCollection",
                           "GeometryLineString",
                           "GeometryMultiLineString",
                           "GeometryMultiPoint",
                           "GeometryMultiPolygon",
                           "GeometryPoint",
                           "GeometryPolygon"
                        }
                      : new[] { "Geometry", "GeometryCollection", "LineString", "MultiLineString", "MultiPoint", "MultiPolygon", "Point", "Polygon" };
         }
      }

      /// <summary>
      ///    Class/struct types that can be used in the model
      /// </summary>
      public string[] ValidTypes
      {
         get
         {
            List<string> validTypes = new List<string>(new[]
                                                       {
                                                          "Binary",
                                                          "Boolean",
                                                          "byte",
                                                          "Byte",
                                                          "DateTime",
                                                          "DateTimeOffset",
                                                          "Decimal",
                                                          "Double",
                                                          "Guid",
                                                          "HierarchyId",
                                                          "Int16",
                                                          "Int32",
                                                          "Int64",
                                                          "Single",
                                                          "String",
                                                       });

            if (IsEFCore6Plus)
            {
               validTypes.Add("DateOnly");
               validTypes.Add("TimeOnly");
            }

            if (IsEFCore5Plus)
            {
               validTypes.Add("System.Net.IPAddress");
               validTypes.Add("System.Net.NetworkInformation.PhysicalAddress");
            }

            return validTypes.Union(SpatialTypes).ToArray();
         }
      }

      /// <summary>
      ///    CLR Types that can be used in the model
      /// </summary>
      public string[] ValidCLRTypes
      {
         get
         {
            List<string> validClrTypes = new List<string>(new[]
                                                          {
                                                             "Binary",
                                                             "bool", "bool?", "Nullable<bool>",
                                                             "Boolean", "Boolean?", "Nullable<Boolean>",
                                                             "Byte", "Byte?", "Nullable<Byte>",
                                                             "byte", "byte?", "Nullable<byte>",
                                                             "byte[]",
                                                             "DateTime", "DateTime?", "Nullable<DateTime>",
                                                             "DateTimeOffset", "DateTimeOffset?", "Nullable<DateTimeOffset>",
                                                             "DbGeography",
                                                             "DbGeometry",
                                                             "Decimal", "Decimal?", "Nullable<Decimal>",
                                                             "decimal", "decimal?", "Nullable<decimal>",
                                                             "Double", "Double?", "Nullable<Double>",
                                                             "double", "double?", "Nullable<double>",
                                                             "Guid", "Guid?", "Nullable<Guid>",
                                                             "int", "int?", "Nullable<int>",
                                                             "Int16", "Int16?", "Nullable<Int16>",
                                                             "Int32", "Int32?", "Nullable<Int32>",
                                                             "Int64", "Int64?", "Nullable<Int64>",
                                                             "long", "long?", "Nullable<long>",
                                                             "short", "short?", "Nullable<short>",
                                                             "Single", "Single?", "Nullable<Single>",
                                                             "String", "string?", "Nullable<String",
                                                             "TimeSpan", "TimeSpan?", "Nullable<TimeSpan>"
                                                          });

            if (IsEFCore5Plus)
               validClrTypes.Add("System.Net.IPAddress");

            if (IsEFCore6Plus)
            {
               validClrTypes.AddRange(new[] { "DateOnly", "DateOnly?", "Nullable<DateOnly>" });
               validClrTypes.AddRange(new[] { "TimeOnly", "TimeOnly?", "Nullable<TimeOnly>" });
            }

            return validClrTypes.Union(SpatialTypes).OrderBy(x => x).ToArray(); 
         }
      }

      /// <summary>
      ///    Validates that the type in question can be used as an identity.
      ///    EF6 is constrained as to identity types, as is EFCore before v5.
      ///    EFCore v5+ has no constraints, other than what's put on by the database type
      /// </summary>
      /// <param name="typename">Name of type to check for use as identity</param>
      /// <returns>True if valid, false otherwise</returns>
      public bool IsValidIdentityAttributeType(string typename)
      {
         return IsEFCore5Plus || ValidIdentityAttributeTypes.Contains(typename);
      }

      /// <summary>
      ///    Collection of type names valid for identity attribute types
      /// </summary>
      public string[] ValidIdentityAttributeTypes
      {
         get
         {
            List<string> baseResult = ValidIdentityTypeAttributesBaseList;

            baseResult.AddRange(Store.GetAll<ModelEnum>()
                                     .Where(e => baseResult.Contains(e.ValueType.ToString()))
                                     .Select(e => e.Name)
                                     .OrderBy(n => n));

            return baseResult.OrderBy(x => x).ToArray();
         }
      }

      private static List<string> ValidIdentityTypeAttributesBaseList
      {
         get
         {
            return new List<string>
                   {
                      "Byte",
                      "Guid",
                      "Int16",
                      "Int32",
                      "Int64",
                      "String"
                   };
         }
      }

      /// <summary>
      ///    Determines if a type name string is a valid C# CLR type for model usage.
      /// </summary>
      public bool IsValidCLRType(string type)
      {
         return ValidCLRTypes.Contains(type);
      }

      #endregion

      #region Nuget

      /// <summary>
      ///    Transforms the selected EntityFrameworkPackageVersion into a decimal number, only taking the first two segments into account. If a "Latest" version is chosen, looks up the appropriate real version.
      /// </summary>
      public double GetEntityFrameworkPackageVersionNum()
      {
         string packageVersion = EntityFrameworkPackageVersion;
         string[] parts = packageVersion.Split('.');

         if (packageVersion.EndsWith("Latest"))
         {
            (int A, int B, int C) actualVersion;

            switch (parts.Length)
            {
               case 1: // just "Latest"
                  actualVersion = NugetVersions.Last();

                  break;

               case 2: // x.Latest
                  actualVersion = NugetVersions.Last(v => v.A == int.Parse(parts[0], CultureInfo.InvariantCulture));

                  break;

               default: // x.y.Latest
                  actualVersion = NugetVersions.Last(v => (v.A == int.Parse(parts[0], CultureInfo.InvariantCulture)) && (v.B == int.Parse(parts[1], CultureInfo.InvariantCulture)));

                  break;
            }

            packageVersion = $"{actualVersion.A}.{actualVersion.B}";
         }
         else
            packageVersion = $"{parts[0]}.{parts[1]}";

         return double.Parse(packageVersion, CultureInfo.InvariantCulture);
      }

      private List<(int A, int B, int C)> nugetVersions;

      private List<(int A, int B, int C)> NugetVersions
      {
         get
         {
            return nugetVersions
                ?? (nugetVersions = NuGetHelper.EFPackageVersions[EntityFrameworkVersion]
                                               .Where(x => x.Count(c => c == '.') == 2)
                                               .Select(v => (int.TryParse(v.Substring(0, v.IndexOf('.')), out int x1)
                                                                ? x1
                                                                : 0
                                                           , int.TryParse(v.Substring(v.IndexOf('.') + 1, v.IndexOf('.', v.IndexOf('.') + 1) - v.IndexOf('.') - 1), out int x2)
                                                                ? x2
                                                                : 0
                                                           , int.TryParse(v.Substring(v.IndexOf('.', v.IndexOf('.') + 1) + 1), out int x3)
                                                                ? x3
                                                                : 0))
                                               .OrderBy<(int A, int B, int C), int>(v => v.A).ThenBy(v => v.B).ThenBy(v => v.C)
                                               .Distinct()
                                               .ToList());
         }
      }

      #endregion Nuget

      #region Validation methods

      [ValidationMethod(ValidationCategories.Save | ValidationCategories.Menu)]
      [UsedImplicitly]
      [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called by validation")]
      private void ConnectionStringMustExist(ValidationContext context)
      {
         if (!Classes.Any() && !Enums.Any())
            return;

         if (string.IsNullOrEmpty(ConnectionString) && string.IsNullOrEmpty(ConnectionStringName))
            context.LogWarning("Model: Default connection string missing", "MRWConnectionString", this);

         if (string.IsNullOrEmpty(EntityContainerName))
            context.LogError("Model: Entity container needs a name", "MREContainerNameEmpty", this);
      }

      [ValidationMethod(ValidationCategories.Open | ValidationCategories.Save | ValidationCategories.Menu)]
      [UsedImplicitly]
      [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called by validation")]
      private void SummaryDescriptionIsEmpty(ValidationContext context)
      {
         if (string.IsNullOrWhiteSpace(Summary) && WarnOnMissingDocumentation)
            context.LogWarning("Model: Summary documentation missing", "AWMissingSummary", this);
      }

      #endregion Validation methods

      #region DatabaseSchema tracking property

      /// <summary>
      /// Raises the DatabaseSchemaChanged event.
      /// </summary>
      /// <param name="oldValue">The old value of the database schema.</param>
      /// <param name="newValue">The new value of the database schema.</param>
      protected virtual void OnDatabaseSchemaChanged(string oldValue, string newValue)
      {
         TrackingHelper.UpdateTrackingCollectionProperty(Store,
                                                         Classes,
                                                         ModelClass.DatabaseSchemaDomainPropertyId,
                                                         ModelClass.IsDatabaseSchemaTrackingDomainPropertyId);
      }

      internal sealed partial class DatabaseSchemaPropertyHandler
      {
         protected override void OnValueChanged(ModelRoot element, string oldValue, string newValue)
         {
            base.OnValueChanged(element, oldValue, newValue);

            if (!element.Store.InUndoRedoOrRollback)
               element.OnDatabaseSchemaChanged(oldValue, newValue);
         }
      }

      #endregion DatabaseSchema tracking property

      #region DatabaseCollationDefault tracking property

      /// <summary>
      /// Raises the DatabaseCollationDefaultChanged event when the default collation of a database is changed.
      /// </summary>
      /// <param name="oldValue">The previous collation value.</param>
      /// <param name="newValue">The new collation value.</param>
      protected virtual void OnDatabaseCollationDefaultChanged(string oldValue, string newValue)
      {
         TrackingHelper.UpdateTrackingCollectionProperty(Store,
                                                         Classes,
                                                         ModelAttribute.DatabaseCollationDomainPropertyId,
                                                         ModelAttribute.IsDatabaseCollationTrackingDomainPropertyId);
      }

      internal sealed partial class DatabaseCollationDefaultPropertyHandler
      {
         protected override void OnValueChanged(ModelRoot element, string oldValue, string newValue)
         {
            base.OnValueChanged(element, oldValue, newValue);

            if (!element.Store.InUndoRedoOrRollback)
               element.OnDatabaseCollationDefaultChanged(oldValue, newValue);
         }
      }

      #endregion DatabaseCollationDefault tracking property

      #region DefaultCollectionClass tracking property

      /// <summary>
      /// Raises the CollectionClassChanged event.
      /// </summary>
      /// <param name="oldValue">The old value of the collection class.</param>
      /// <param name="newValue">The new value of the collection class.</param>
      protected virtual void OnCollectionClassChanged(string oldValue, string newValue)
      {
         TrackingHelper.UpdateTrackingCollectionProperty(Store,
                                                         Store.GetAll<Association>().ToList(),
                                                         Association.CollectionClassDomainPropertyId,
                                                         Association.IsCollectionClassTrackingDomainPropertyId);
      }

      internal sealed partial class DefaultCollectionClassPropertyHandler
      {
         protected override void OnValueChanged(ModelRoot element, string oldValue, string newValue)
         {
            base.OnValueChanged(element, oldValue, newValue);

            if (!element.Store.InUndoRedoOrRollback)
               element.OnCollectionClassChanged(oldValue, newValue);
         }
      }

      #endregion DefaultCollectionClass tracking property

      #region Entity Output Directory tracking property

      /// <summary>
      ///  Raises the EntityOutputDirectoryChanged event when the Entity Output Directory is changed.
      /// </summary>
      /// <param name="oldValue">The old value of the Entity Output Directory.</param>
      /// <param name="newValue">The new value of the Entity Output Directory.</param>
      protected virtual void OnEntityOutputDirectoryChanged(string oldValue, string newValue)
      {
         TrackingHelper.UpdateTrackingCollectionProperty(Store, Classes, ModelClass.OutputDirectoryDomainPropertyId, ModelClass.IsOutputDirectoryTrackingDomainPropertyId);
      }

      internal sealed partial class EntityOutputDirectoryPropertyHandler
      {
         protected override void OnValueChanged(ModelRoot element, string oldValue, string newValue)
         {
            base.OnValueChanged(element, oldValue, newValue);

            if (!element.Store.InUndoRedoOrRollback)
               element.OnEntityOutputDirectoryChanged(oldValue, newValue);
         }
      }

      #endregion

      #region Enum Output Directory tracking property

      /// <summary>
      /// Raises the event when the output directory for an enumeration has changed.
      /// </summary>
      /// <param name="oldValue">The old output directory.</param>
      /// <param name="newValue">The new output directory.</param>
      protected virtual void OnEnumOutputDirectoryChanged(string oldValue, string newValue)
      {
         TrackingHelper.UpdateTrackingCollectionProperty(Store, Classes, ModelEnum.OutputDirectoryDomainPropertyId, ModelEnum.IsOutputDirectoryTrackingDomainPropertyId);
      }

      internal sealed partial class EnumOutputDirectoryPropertyHandler
      {
         protected override void OnValueChanged(ModelRoot element, string oldValue, string newValue)
         {
            base.OnValueChanged(element, oldValue, newValue);

            if (!element.Store.InUndoRedoOrRollback)
               element.OnEnumOutputDirectoryChanged(oldValue, newValue);
         }
      }

      #endregion

      #region Namespace tracking property

      internal sealed partial class NamespacePropertyHandler
      {
         protected override void OnValueChanged(ModelRoot element, string oldValue, string newValue)
         {
            base.OnValueChanged(element, oldValue, newValue);

            if (!element.Store.InUndoRedoOrRollback)
            {
               if (string.IsNullOrWhiteSpace(element.EntityNamespace))
               {
                  TrackingHelper.UpdateTrackingCollectionProperty(element.Store,
                                                                  element.Classes.Where(c => !c.IsDependentType),
                                                                  ModelClass.NamespaceDomainPropertyId,
                                                                  ModelClass.IsNamespaceTrackingDomainPropertyId);
               }

               if (string.IsNullOrWhiteSpace(element.StructNamespace))
               {
                  TrackingHelper.UpdateTrackingCollectionProperty(element.Store,
                                                                  element.Classes.Where(c => c.IsDependentType),
                                                                  ModelClass.NamespaceDomainPropertyId,
                                                                  ModelClass.IsNamespaceTrackingDomainPropertyId);
               }

               if (string.IsNullOrWhiteSpace(element.EnumNamespace))
                  TrackingHelper.UpdateTrackingCollectionProperty(element.Store, element.Enums, ModelEnum.NamespaceDomainPropertyId, ModelEnum.IsNamespaceTrackingDomainPropertyId);
            }
         }
      }

      internal sealed partial class EntityNamespacePropertyHandler
      {
         protected override void OnValueChanged(ModelRoot element, string oldValue, string newValue)
         {
            base.OnValueChanged(element, oldValue, newValue);

            if (!element.Store.InUndoRedoOrRollback)
            {
               TrackingHelper.UpdateTrackingCollectionProperty(element.Store,
                                                               element.Classes.Where(c => !c.IsDependentType),
                                                               ModelClass.NamespaceDomainPropertyId,
                                                               ModelClass.IsNamespaceTrackingDomainPropertyId);
            }
         }
      }

      internal sealed partial class EnumNamespacePropertyHandler
      {
         protected override void OnValueChanged(ModelRoot element, string oldValue, string newValue)
         {
            base.OnValueChanged(element, oldValue, newValue);

            if (!element.Store.InUndoRedoOrRollback)
               TrackingHelper.UpdateTrackingCollectionProperty(element.Store, element.Enums, ModelEnum.NamespaceDomainPropertyId, ModelEnum.IsNamespaceTrackingDomainPropertyId);
         }
      }

      internal sealed partial class StructNamespacePropertyHandler
      {
         protected override void OnValueChanged(ModelRoot element, string oldValue, string newValue)
         {
            base.OnValueChanged(element, oldValue, newValue);

            if (!element.Store.InUndoRedoOrRollback)
            {
               TrackingHelper.UpdateTrackingCollectionProperty(element.Store,
                                                               element.Classes.Where(c => c.IsDependentType),
                                                               ModelClass.NamespaceDomainPropertyId,
                                                               ModelClass.IsNamespaceTrackingDomainPropertyId);
            }
         }
      }

      #endregion Namespace tracking property

      #region AutoPropertyDefault tracking property

      /// <summary>
      ///    Updates tracking properties when the IsImplementNotify value changes
      /// </summary>
      /// <param name="oldValue">Prior value</param>
      /// <param name="newValue">Current value</param>
      protected virtual void OnAutoPropertyDefaultChanged(bool oldValue, bool newValue)
      {
         TrackingHelper.UpdateTrackingCollectionProperty(Store,
                                                         Classes,
                                                         ModelClass.AutoPropertyDefaultDomainPropertyId,
                                                         ModelClass.IsAutoPropertyDefaultTrackingDomainPropertyId);
      }

      internal sealed partial class AutoPropertyDefaultPropertyHandler
      {
         protected override void OnValueChanged(ModelRoot element, bool oldValue, bool newValue)
         {
            base.OnValueChanged(element, oldValue, newValue);

            if (!element.Store.InUndoRedoOrRollback)
               element.OnAutoPropertyDefaultChanged(oldValue, newValue);
         }
      }

      #endregion AutoPropertyDefault tracking property
   }
}
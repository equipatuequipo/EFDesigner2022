using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Sawczyn.EFDesigner.EFModel.Annotations;

// ReSharper disable RedundantNameQualifier

namespace Sawczyn.EFDesigner.EFModel.EditingOnly
{
   [UsedImplicitly]
   public partial class GeneratedTextTransformation
   {
      #region Template

      // EFDesigner v4.3.3.1
      // Copyright (c) 2017-2023 Michael Sawczyn
      // https://github.com/msawczyn/EFDesigner

      /// <summary>
      /// A class that serves as a generator of EF Core 5 models based on the EF Core 3 generator.
      /// </summary>
      public class EFCore5ModelGenerator : EFCore3ModelGenerator
      {
         /// <summary>
         /// Initializes a new instance of the EFCore5ModelGenerator class with the specified host object.
         /// </summary>
         /// <param name="host">The host object.</param>
         public EFCore5ModelGenerator(GeneratedTextTransformation host) : base(host) { }

         /// <summary>
         /// Configures bidirectional associations for a given ModelClass object.
         /// </summary>
         /// <param name="modelClass">The ModelClass object for which bidirectional associations need to be configured.</param>
         /// <param name="visited">A list of visited Associations to avoid infinite recursion.</param>
         /// <param name="foreignKeyColumns">A list of foreign key columns.</param>
         /// <param name="declaredShadowProperties">A list of shadow properties declared for the current ModelClass object.</param>
         [SuppressMessage("ReSharper", "RedundantNameQualifier")]
         protected override void ConfigureBidirectionalAssociations(ModelClass modelClass, List<Association> visited, List<string> foreignKeyColumns, List<string> declaredShadowProperties)
         {
            WriteBidirectionalNonDependentAssociations(modelClass, visited, foreignKeyColumns);
            WriteBidirectionalDependentAssociations(modelClass, $"modelBuilder.Entity<{modelClass.FullName}>()", visited);
         }

         /// <summary>
         /// Creates the code segments configuring a model class.
         /// </summary>
         /// <param name="segments">Container holding the new code segments.</param>
         /// <param name="classesWithTables">The array of classes with tables.</param>
         /// <param name="foreignKeyColumns">The list of foreign key columns.</param>
         /// <param name="visited">The list of already-visited associations.</param>
         /// <param name="modelClass">The model class to be configured.</param>
         protected override void ConfigureModelClass(List<string> segments, ModelClass[] classesWithTables, List<string> foreignKeyColumns, List<Association> visited, ModelClass modelClass)
         {
            segments.Clear();
            foreignKeyColumns.Clear();
            NL();

            if (modelClass.IsDependentType)
            {
               segments.Add($"modelBuilder.Owned<{modelClass.FullName}>()");
               Output(segments);

               return;
            }

            segments.Add($"modelBuilder.Entity<{modelClass.FullName}>()");

            ConfigureTransientProperties(segments, modelClass);

            //if (modelRoot.InheritanceStrategy == CodeStrategy.TablePerConcreteType && modelClass.Superclass != null)
            //   segments.Add("Map(x => x.MapInheritedProperties())");

            if (classesWithTables.Contains(modelClass))
            {
               if (modelClass.IsQueryType)
               {
                  Output($"// There is no storage defined for {modelClass.Name} because its IsQueryType value is");
                  Output($"// set to 'true'. Please provide the {modelRoot.FullName}.Get{modelClass.Name}SqlQuery() method in the partial class.");
                  Output("// ");
                  Output($"// private string Get{modelClass.Name}SqlQuery()");
                  Output("// {");
                  Output($"//    return the defining SQL query that pulls all the properties for {modelClass.FullName}");
                  Output("// }");

                  segments.Add($"ToSqlQuery(Get{modelClass.Name}SqlQuery())");
               }
               else
                  ConfigureTable(segments, modelClass);
            }

            if (segments.Count > 1 || modelClass.IsDependentType)
               Output(segments);

            // attribute level
            ConfigureModelAttributes(segments, modelClass);

            bool hasDefinedConcurrencyToken = modelClass.AllAttributes.Any(x => x.IsConcurrencyToken);

            if (!hasDefinedConcurrencyToken && modelClass.EffectiveConcurrency == ConcurrencyOverride.Optimistic)
               Output($@"modelBuilder.Entity<{modelClass.FullName}>().Property<byte[]>(""Timestamp"").IsConcurrencyToken();");

            // Navigation endpoints are distingished as Source and Target. They are also distinguished as Principal
            // and Dependent. So how do these map to each other? Short answer: they don't - they're orthogonal concepts.
            // Source and Target are accidents of where the user started drawing the association, and help define where the
            // properties are in unidirectional associations. Principal and Dependent define where the foreign keys go in 
            // the persistence mechanism.

            // What matters to code generation is the Principal and Dependent classifications, so we focus on those. 
            // In the case of 1-1 or 0/1-0/1, it's situational, so the user has to tell us.
            // In all other cases, we can tell by the cardinalities of the associations.

            // navigation properties
            List<string> declaredShadowProperties = new List<string>();

            if (!modelClass.IsDependentType)
            {
               ConfigureUnidirectionalAssociations(modelClass, visited, foreignKeyColumns, declaredShadowProperties);
               ConfigureBidirectionalAssociations(modelClass, visited, foreignKeyColumns, declaredShadowProperties);
            }
         }

         /// <summary>
         /// Creates the code segments configuring a collection of model classes.
         /// </summary>
         /// <param name="segments">Container holding the new code segments.</param>
         /// <param name="classesWithTables">The array of model classes with tables.</param>
         /// <param name="foreignKeyColumns">The list of foreign key columns.</param>
         /// <param name="visited">The list of already-visited associations.</param>
         protected override void ConfigureModelClasses(List<string> segments, ModelClass[] classesWithTables, List<string> foreignKeyColumns, List<Association> visited)
         {
            foreach (ModelClass modelClass in modelRoot.Classes.Where(x => !x.IsAssociationClass && x.Persistent).OrderBy(x => x.Name))
               ConfigureModelClass(segments, classesWithTables, foreignKeyColumns, visited, modelClass);
         }

         /// <summary>
         /// Creates the code segments configuring the database table backing a model class.
         /// </summary>
         /// <param name="segments">Container holding the new code segments.</param>
         /// <param name="modelClass">The model class to base the table configuration on.</param>
         protected override void ConfigureTable(List<string> segments, ModelClass modelClass)
         {
            string tableName = string.IsNullOrEmpty(modelClass.TableName)
                                  ? modelClass.Name
                                  : modelClass.TableName;

            string viewName = string.IsNullOrEmpty(modelClass.ViewName)
                                 ? modelClass.Name
                                 : modelClass.ViewName;

            string schema = string.IsNullOrEmpty(modelClass.DatabaseSchema) || modelClass.DatabaseSchema == modelClass.ModelRoot.DatabaseSchema
                               ? string.Empty
                               : $", \"{modelClass.DatabaseSchema}\"";

            List<string> modifiers = new List<string>();

            if (modelClass.ExcludeFromMigrations)
               modifiers.Add("t.ExcludeFromMigrations();");

            if (modelRoot.IsEFCore6Plus 
             && modelClass.UseTemporalTables
             && !modelClass.IsDatabaseView
             && (!modelClass.Subclasses.Any() || modelClass.ModelRoot.InheritanceStrategy == CodeStrategy.TablePerHierarchy)
             && modelClass.Superclass == null)
               modifiers.Add("t.IsTemporal();");

            if (modelRoot.IsEFCore7Plus)
            {
               bool hasTrigger = modelClass.AllNavigationProperties()
                                           .Any(n =>
                                                   (n.AssociationObject.Source == modelClass && n.AssociationObject.Target == modelClass && (n.AssociationObject.SourceMultiplicity == Sawczyn.EFDesigner.EFModel.Multiplicity.One || n.AssociationObject.TargetMultiplicity == Sawczyn.EFDesigner.EFModel.Multiplicity.One))
                                                || (n.AssociationObject.Source == modelClass && (n.AssociationObject.TargetMultiplicity == Sawczyn.EFDesigner.EFModel.Multiplicity.One || n.AssociationObject.TargetDeleteAction == DeleteAction.Cascade))
                                                || (n.AssociationObject.Target == modelClass && (n.AssociationObject.SourceMultiplicity == Sawczyn.EFDesigner.EFModel.Multiplicity.One || n.AssociationObject.SourceDeleteAction == DeleteAction.Cascade)));

               // for later - EFCore7, SqlServer and triggers don't work well. We need to tell it that the triggers exist.
               // but how to know what they are at this stage of the game?
            }

            string buildActions = modifiers.Any()
                                     ? $", t => {{ {string.Join(" ", modifiers)} }}"
                                     : string.Empty;

            segments.Add(modelClass.IsDatabaseView
                            ? $"ToView(\"{viewName}\"{schema}{buildActions})"
                            : $"ToTable(\"{tableName}\"{schema}{buildActions})");

            if (modelClass.Superclass != null)
            {
               segments.Add($"HasBaseType<{modelClass.Superclass.FullName}>()");
            }
            else  // if there's a base class, we can't have new identifiers
            {
               // primary key code segments must be output last, since HasKey returns a different type
               List<ModelAttribute> allIdentityAttributes = modelClass.AllIdentityAttributes.ToList();

               if (allIdentityAttributes.Count == 1)
                  segments.Add($"HasKey(t => t.{allIdentityAttributes[0].Name})");
               else if (allIdentityAttributes.Count > 1)
                  segments.Add($"HasKey(t => new {{ t.{string.Join(", t.", allIdentityAttributes.Select(ia => ia.Name))} }})");
               else
                  segments.Add("HasNoKey()");
            }         }

         /// <summary>
         /// Configures and writes the unidirectional associations of a model class.
         /// </summary>
         /// <param name="modelClass">The model class to configure.</param>
         /// <param name="visited">A list of already visited associations.</param>
         /// <param name="foreignKeyColumns">A list of foreign key columns.</param>
         /// <param name="declaredShadowProperties">A list of declared shadow properties.</param>
         [SuppressMessage("ReSharper", "RedundantNameQualifier")]
         protected override void ConfigureUnidirectionalAssociations(ModelClass modelClass, List<Association> visited, List<string> foreignKeyColumns, List<string> declaredShadowProperties)
         {
            WriteUnidirectionalNonDependentAssociations(modelClass, visited, foreignKeyColumns);
            WriteUnidirectionalDependentAssociations(modelClass, $"modelBuilder.Entity<{modelClass.FullName}>()", visited);
         }

         /// <summary>
         /// Creates the code segments of a model attribute.
         /// </summary>
         /// <param name="modelAttribute">The model attribute.</param>
         /// <returns>A list of strings representing the segments of the model attribute.</returns>
         protected override List<string> GatherModelAttributeSegments(ModelAttribute modelAttribute)
         {
            List<string> segments = base.GatherModelAttributeSegments(modelAttribute);

            if (!string.IsNullOrEmpty(modelAttribute.InitialValue) && string.IsNullOrWhiteSpace(modelAttribute.DatabaseDefaultValue))
            {
               switch (modelAttribute.Type)
               {
                  case "String":
                     segments.Add($"HasDefaultValue(\"{modelAttribute.InitialValue.Trim(' ', '"')}\")");

                     break;

                  case "Char":
                     segments.Add($"HasDefaultValue('{modelAttribute.InitialValue.Trim(' ', '\'')}')");

                     break;

                  case "DateTime":
                     if (modelAttribute.InitialValue == "DateTime.UtcNow" || modelAttribute.InitialValue == "DateTime.Now")
                        segments.Add("HasDefaultValueSql(\"CURRENT_TIMESTAMP\")");

                     break;

                  case "DateTimeOffset":
                     if (modelAttribute.InitialValue == "DateTimeOffset.UtcNow" || modelAttribute.InitialValue == "DateTimeOffset.Now")
                        segments.Add("HasDefaultValueSql(\"CURRENT_TIMESTAMP\")");

                     break;

                  default:
                     string enumName = modelAttribute.InitialValue.Split('.').First();
                     ModelEnum enumObject = modelAttribute.ModelClass.ModelRoot.Enums.FirstOrDefault(e => e.Name == enumName);

                     // because we may use object properties as a initial value (like DateTime.Now below) we first need to check if it is an enum
                     if (modelAttribute.InitialValue.Contains(".") && enumObject != null) // enum
                     {
                        string enumValue = modelAttribute.InitialValue.Split('.').Last();
                        string enumFQN = enumObject.FullName ?? enumName;
                        segments.Add($"HasDefaultValue({enumFQN.Trim()}.{enumValue.Trim()})");
                     }
                     else
                        segments.Add($"HasDefaultValue({modelAttribute.InitialValue})");

                     break;
               }
            }
            else if (!string.IsNullOrWhiteSpace(modelAttribute.DatabaseDefaultValue))
            {
               segments.Add($"HasDefaultValueSql(\"{modelAttribute.DatabaseDefaultValue}\")");
            }

            if (!string.IsNullOrEmpty(modelAttribute.DatabaseCollation)
             && modelAttribute.DatabaseCollation != modelRoot.DatabaseCollationDefault
             && modelAttribute.Type == "String")
               segments.Add($"UseCollation(\"{modelAttribute.DatabaseCollation.Trim('"')}\")");

            if (modelRoot.IsEFCore6Plus
             && (modelAttribute.Type == "decimal" || modelAttribute.Type == "Decimal"))
            {
               string precision = string.IsNullOrEmpty(modelAttribute.TypePrecision)
                                     ? "18"
                                     : modelAttribute.TypePrecision;

               string scale = string.IsNullOrEmpty(modelAttribute.TypeScale)
                                 ? "2"
                                 : modelAttribute.TypeScale;

               segments.Add($"HasPrecision({precision}, {scale})");
            }

            int index = segments.IndexOf("IsRequired()");

            if (index >= 0)
            {
               segments.RemoveAt(index);
               segments.Add("IsRequired()");
            }

            return segments;
         }

         private void WriteBidirectionalAssociationWithAssociationClass(ModelClass modelClass, ModelClass associationClass, BidirectionalAssociation association)
         {
            string indent = associationClass.ModelRoot.UseTabs
                               ? "\t"
                               : "   ";

            BidirectionalAssociation associationToSource =
               (BidirectionalAssociation)associationClass.AllNavigationProperties().First(n => n.AssociationObject.Source == association.Source).AssociationObject;

            BidirectionalAssociation associationToTarget =
               (BidirectionalAssociation)associationClass.AllNavigationProperties().First(n => n.AssociationObject.Source == association.Target).AssociationObject;

            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (modelClass.ModelRoot.ChopMethodChains)
               PushIndent("            ");
            else
               PushIndent(indent);

            Output($".UsingEntity<{associationClass.FullName}>(");
            PushIndent(indent);
            Output("j => j");
            PushIndent(indent);
            Output($".HasOne(x => x.{associationToTarget.SourcePropertyName})");
            Output($".WithMany(x => x.{associationToTarget.TargetPropertyName})");
            Output($".HasForeignKey(x => x.{associationClass.Attributes.First(a => a.IsForeignKeyFor == associationToTarget.Id).Name}),");
            PopIndent();
            Output("j => j");
            PushIndent(indent);
            Output($".HasOne(x => x.{associationToSource.SourcePropertyName})");
            Output($".WithMany(x => x.{associationToSource.TargetPropertyName})");
            Output($".HasForeignKey(x => x.{associationClass.Attributes.First(a => a.IsForeignKeyFor == associationToSource.Id).Name}),");
            PopIndent();
            Output("j =>");
            Output("{");

            #region transient properties

            foreach (ModelAttribute transient in associationClass.Attributes.Where(x => !x.Persistent))
               Output($"j.Ignore(t => t.{transient.Name});");

            #endregion

            #region table definition

            string tableName = string.IsNullOrEmpty(associationClass.TableName)
                                  ? associationClass.Name
                                  : associationClass.TableName;

            string schema = string.IsNullOrEmpty(associationClass.DatabaseSchema) || associationClass.DatabaseSchema == associationClass.ModelRoot.DatabaseSchema
                               ? string.Empty
                               : $", \"{associationClass.DatabaseSchema}\"";

            List<string> modifiers = new List<string>();

            if (associationClass.UseTemporalTables)
               modifiers.Add(" t.IsTemporal();");

            string buildActions = modifiers.Any()
                                     ? $", t => {{ {string.Join(" ", modifiers)} }}"
                                     : string.Empty;

            Output($"j.ToTable(\"{tableName}\"{schema}{buildActions});");

            List<ModelAttribute> identityAttributes = associationClass.IdentityAttributes.ToList();

            if (identityAttributes.Count == 1)
               Output($"j.HasKey(t => t.{identityAttributes[0].Name});");
            else if (identityAttributes.Count > 1)
               Output($"j.HasKey(t => new {{ t.{string.Join(", t.", identityAttributes.Select(ia => ia.Name))} }});");

            #endregion

            #region model attributes

            foreach (ModelAttribute modelAttribute in associationClass.Attributes.Where(x => x.Persistent && !x.IsIdentity))
            {
               List<string> buffer = new List<string>();
               buffer.AddRange(GatherModelAttributeSegments(modelAttribute));

               if (buffer.Any())
                  Output($"j.Property(t => t.{modelAttribute.Name}).{string.Join(".", buffer)};");

               if (modelAttribute.Indexed && !modelClass.IsDatabaseView)
               {
                  buffer.Clear();
                  buffer.Add(!string.IsNullOrEmpty(modelAttribute.IndexName)
                                ? $"HasIndex(t => t.{modelAttribute.Name}, {modelAttribute.IndexName})"
                                : $"HasIndex(t => t.{modelAttribute.Name})");

                  if (modelAttribute.IndexedUnique)
                     buffer.Add("IsUnique()");

                  Output($"j.{string.Join(".", buffer)};");
               }
            }

            #endregion

            PopIndent();
            Output("});");
            PopIndent();
            PopIndent();
         }

         /// <summary>
         /// Writes bidirectional dependent associations
         /// </summary>
         /// <param name="sourceInstance">The source instance</param>
         /// <param name="baseSegment">The base segment</param>
         /// <param name="visited">The visited associations list</param>
         protected override void WriteBidirectionalDependentAssociations(ModelClass sourceInstance, string baseSegment, List<Association> visited)
         {
            // ReSharper disable once LoopCanBePartlyConvertedToQuery
            foreach (BidirectionalAssociation association in Association.GetLinksToTargets(sourceInstance)
                                                                        .OfType<BidirectionalAssociation>()
                                                                        .Where(x => x.Persistent && x.Target.IsDependentType))
            {
               if (visited.Contains(association))
                  continue;

               visited.Add(association);

               List<string> segments = new List<string>();

               string separator = sourceInstance.ModelRoot.ShadowKeyNamePattern == ShadowKeyPattern.TableColumn
                                     ? string.Empty
                                     : "_";

               switch (association.TargetMultiplicity) // realized by property on source
               {
                  case Sawczyn.EFDesigner.EFModel.Multiplicity.ZeroMany:
                     {
                        segments.Add(baseSegment);
                        segments.Add($"OwnsMany(p => p.{association.TargetPropertyName})");
                        segments.Add($"ToTable(\"{(string.IsNullOrEmpty(association.Target.TableName) ? association.Target.Name : association.Target.TableName)}\")");
                        Output(segments);

                        segments.Add(baseSegment);
                        segments.Add($"OwnsMany(p => p.{association.TargetPropertyName})");
                        segments.Add($"WithOwner(\"{association.SourcePropertyName}\")");
                        segments.Add($"HasForeignKey(\"{association.SourcePropertyName}{separator}Id\")");

                        Output(segments);

                        segments.Add(baseSegment);
                        segments.Add($"OwnsMany(p => p.{association.TargetPropertyName})");
                        segments.Add($"Property<{modelRoot.DefaultIdentityType}>(\"Id\")");

                        Output(segments);

                        segments.Add(baseSegment);
                        segments.Add($"OwnsMany(p => p.{association.TargetPropertyName})");
                        segments.Add("HasKey(\"Id\")");

                        Output(segments);

                        WriteBidirectionalDependentAssociations(association.Target, $"{baseSegment}.OwnsMany(p => p.{association.TargetPropertyName})", visited);

                        break;
                     }

                  case Sawczyn.EFDesigner.EFModel.Multiplicity.One:
                     {
                        segments.Add(baseSegment);
                        segments.Add($"OwnsOne(p => p.{association.TargetPropertyName})");
                        segments.Add($"WithOwner(p => p.{association.SourcePropertyName})");
                        Output(segments);

                        if (!string.IsNullOrEmpty(association.Target.TableName))
                        {
                           segments.Add(baseSegment);
                           segments.Add($"OwnsOne(p => p.{association.TargetPropertyName})");
                           segments.Add($"ToTable(\"{association.Target.TableName}\")");
                           Output(segments);
                        }

                        foreach (ModelAttribute modelAttribute in association.Target.AllAttributes)
                        {
                           segments.Add($"{baseSegment}.OwnsOne(p => p.{association.TargetPropertyName}).Property(p => p.{modelAttribute.Name})");

                           if (modelAttribute.ColumnName != modelAttribute.Name && !string.IsNullOrEmpty(modelAttribute.ColumnName))
                              segments.Add($"HasColumnName(\"{modelAttribute.ColumnName}\")");

                           if (modelAttribute.Required)
                              segments.Add("IsRequired()");

                           if (segments.Count > 1)
                              Output(segments);

                           segments.Clear();
                        }

                        segments.Add(baseSegment);
                        segments.Add($"Navigation(p => p.{association.TargetPropertyName}).IsRequired()");
                        Output(segments);

                        WriteBidirectionalDependentAssociations(association.Target, $"{baseSegment}.OwnsOne(p => p.{association.TargetPropertyName})", visited);

                        break;
                     }

                  case Sawczyn.EFDesigner.EFModel.Multiplicity.ZeroOne:
                     {
                        segments.Add(baseSegment);
                        segments.Add($"OwnsOne(p => p.{association.TargetPropertyName})");
                        segments.Add($"WithOwner(p => p.{association.SourcePropertyName})");
                        Output(segments);

                        if (!string.IsNullOrEmpty(association.Target.TableName))
                        {
                           segments.Add(baseSegment);
                           segments.Add($"OwnsOne(p => p.{association.TargetPropertyName})");
                           segments.Add($"ToTable(\"{association.Target.TableName}\")");
                           Output(segments);
                        }

                        foreach (ModelAttribute modelAttribute in association.Target.AllAttributes)
                        {
                           segments.Add($"{baseSegment}.OwnsOne(p => p.{association.TargetPropertyName}).Property(p => p.{modelAttribute.Name})");

                           if (modelAttribute.ColumnName != modelAttribute.Name && !string.IsNullOrEmpty(modelAttribute.ColumnName))
                              segments.Add($"HasColumnName(\"{modelAttribute.ColumnName}\")");

                           if (modelAttribute.Required)
                              segments.Add("IsRequired()");

                           if (segments.Count > 1)
                              Output(segments);

                           segments.Clear();
                        }

                        WriteBidirectionalDependentAssociations(association.Target, $"{baseSegment}.OwnsOne(p => p.{association.TargetPropertyName})", visited);

                        break;
                     }
               }
            }
         }

         /// <summary>
         /// Writes bidirectional non-dependent association elements for the given model class.
         /// </summary>
         /// <param name="modelClass">The model class for which to write the associations.</param>
         /// <param name="visited">A list of associations that have already been visited to avoid infinite recursion.</param>
         /// <param name="foreignKeyColumns">A list of foreign key columns.</param>
         protected override void WriteBidirectionalNonDependentAssociations(ModelClass modelClass, List<Association> visited, List<string> foreignKeyColumns)
         {
            // ReSharper disable once LoopCanBePartlyConvertedToQuery
            foreach (BidirectionalAssociation association in Association.GetLinksToTargets(modelClass)
                                                                        .OfType<BidirectionalAssociation>()
                                                                        .Where(x => x.Persistent && !x.Target.IsDependentType))
            {
               if (visited.Contains(association))
                  continue;

               visited.Add(association);

               List<string> segments = new List<string>();
               bool required = false;

               segments.Add($"modelBuilder.Entity<{modelClass.FullName}>()");

               switch (association.TargetMultiplicity) // realized by property on source
               {
                  case Sawczyn.EFDesigner.EFModel.Multiplicity.ZeroMany:
                     {
                        segments.Add($"HasMany<{association.Target.FullName}>(p => p.{association.TargetPropertyName})");
                        required = association.SourceMultiplicity == Sawczyn.EFDesigner.EFModel.Multiplicity.One;

                        break;
                     }

                  case Sawczyn.EFDesigner.EFModel.Multiplicity.One:
                  case Sawczyn.EFDesigner.EFModel.Multiplicity.ZeroOne:
                     {
                        segments.Add($"HasOne<{association.Target.FullName}>(p => p.{association.TargetPropertyName})");

                        break;
                     }
               }

               switch (association.SourceMultiplicity) // realized by property on target, but no property on target
               {
                  case Sawczyn.EFDesigner.EFModel.Multiplicity.ZeroMany:
                     segments.Add($"WithMany(p => p.{association.SourcePropertyName})");

                     if (association.TargetMultiplicity == Sawczyn.EFDesigner.EFModel.Multiplicity.ZeroMany)
                     {
                        ModelClass associationClass = modelClass.Store.ElementDirectory.AllElements
                                                                .OfType<ModelClass>()
                                                                .FirstOrDefault(m => m.DescribedAssociationElementId == association.Id);

                        if (associationClass == null)
                           segments.AddRange(WriteStandardBidirectionalAssociation(association, foreignKeyColumns, required));
                        else
                        {
                           OutputNoTerminator(segments);
                           WriteBidirectionalAssociationWithAssociationClass(modelClass, associationClass, association);
                        }
                     }

                     break;

                  case Sawczyn.EFDesigner.EFModel.Multiplicity.One:
                  case Sawczyn.EFDesigner.EFModel.Multiplicity.ZeroOne:
                     segments.Add($"WithOne(p => p.{association.SourcePropertyName})");
                     string foreignKeySegment = CreateForeignKeySegment(association, foreignKeyColumns);

                     if (!string.IsNullOrEmpty(foreignKeySegment))
                        segments.Add(foreignKeySegment);

                     WriteSourceDeleteBehavior(association, segments);

                     if (required
                      && (association.SourceMultiplicity != Sawczyn.EFDesigner.EFModel.Multiplicity.One
                       || association.TargetMultiplicity != Sawczyn.EFDesigner.EFModel.Multiplicity.One))
                        segments.Add("IsRequired()");

                     break;
               }

               if (segments.Any())
                  Output(segments);

               if (association.TargetAutoInclude)
                  Output($"modelBuilder.Entity<{association.Source.FullName}>().Navigation(e => e.{association.TargetPropertyName}).AutoInclude();");
               else if (association.SourceAutoInclude)
                  Output($"modelBuilder.Entity<{association.Target.FullName}>().Navigation(e => e.{association.SourcePropertyName}).AutoInclude();");

               if (!association.TargetAutoProperty)
               {
                  segments.Add($"modelBuilder.Entity<{association.Source.FullName}>().Navigation(e => e.{association.TargetPropertyName})");
                  segments.Add($"HasField(\"{association.TargetBackingFieldName}\")");

                  segments.Add(modelClass.ModelRoot.IsEFCore6Plus
                                  ? $"UsePropertyAccessMode(PropertyAccessMode.{association.TargetPropertyAccessMode})"
                                  : $"Metadata.SetPropertyAccessMode(PropertyAccessMode.{association.TargetPropertyAccessMode})");

                  Output(segments);
               }

               if (!association.SourceAutoProperty)
               {
                  segments.Add($"modelBuilder.Entity<{association.Target.FullName}>().Navigation(e => e.{association.SourcePropertyName})");
                  segments.Add($"HasField(\"{association.SourceBackingFieldName}\")");

                  segments.Add(modelClass.ModelRoot.IsEFCore6Plus
                                  ? $"UsePropertyAccessMode(PropertyAccessMode.{association.SourcePropertyAccessMode})"
                                  : $"Metadata.SetPropertyAccessMode(PropertyAccessMode.{association.SourcePropertyAccessMode})");

                  Output(segments);
               }
            }
         }

         /// <summary>
         /// Writes the necessary code for initializing the entity framework model on creation.
         /// </summary>
         /// <param name="segments">The list of code segments to append to.</param>
         /// <param name="classesWithTables">The array of classes with associated database tables.</param>
         protected override void WriteOnModelCreate(List<string> segments, ModelClass[] classesWithTables)
         {
            Output("partial void OnModelCreatingImpl(ModelBuilder modelBuilder);");
            Output("partial void OnModelCreatedImpl(ModelBuilder modelBuilder);");
            NL();

            Output("/// <summary>");
            Output("///     Override this method to further configure the model that was discovered by convention from the entity types");
            Output("///     exposed in <see cref=\"T:Microsoft.EntityFrameworkCore.DbSet`1\" /> properties on your derived context. The resulting model may be cached");
            Output("///     and re-used for subsequent instances of your derived context.");
            Output("/// </summary>");
            Output("/// <remarks>");
            Output("///     If a model is explicitly set on the options for this context (via <see cref=\"M:Microsoft.EntityFrameworkCore.DbContextOptionsBuilder.UseModel(Microsoft.EntityFrameworkCore.Metadata.IModel)\" />)");
            Output("///     then this method will not be run.");
            Output("/// </remarks>");
            Output("/// <param name=\"modelBuilder\">");
            Output("///     The builder being used to construct the model for this context. Databases (and other extensions) typically");
            Output("///     define extension methods on this object that allow you to configure aspects of the model that are specific");
            Output("///     to a given database.");
            Output("/// </param>");
            Output("protected override void OnModelCreating(ModelBuilder modelBuilder)");
            Output("{");
            Output("base.OnModelCreating(modelBuilder);");
            Output("OnModelCreatingImpl(modelBuilder);");
            NL();

            if (!string.IsNullOrEmpty(modelRoot.DatabaseSchema))
               Output($"modelBuilder.HasDefaultSchema(\"{modelRoot.DatabaseSchema.Trim('"')}\");");

            if (modelRoot.DatabaseCollationDefault.ToLowerInvariant() != "default")
               Output($"modelBuilder.UseCollation(\"{modelRoot.DatabaseCollationDefault.Trim('"')}\");");

            List<Association> visited = new List<Association>();
            List<string> foreignKeyColumns = new List<string>();

            ConfigureModelClasses(segments, classesWithTables, foreignKeyColumns, visited);

            NL();

            Output("OnModelCreatedImpl(modelBuilder);");
            Output("}");
         }

         private IEnumerable<string> WriteStandardBidirectionalAssociation(BidirectionalAssociation association, List<string> foreignKeyColumns, bool required)
         {
            List<string> segments = new List<string>();

            string tableMap = string.IsNullOrEmpty(association.JoinTableName)
                                 ? $"{association.Target.Name}_{association.SourcePropertyName}_x_{association.Source.Name}_{association.TargetPropertyName}"
                                 : association.JoinTableName;

            if (association.SourceMultiplicity == Sawczyn.EFDesigner.EFModel.Multiplicity.ZeroMany && association.TargetMultiplicity == Sawczyn.EFDesigner.EFModel.Multiplicity.ZeroMany)
            {
               string targetFKs = string.Join(", ", !string.IsNullOrWhiteSpace(association.TargetFKColumnName) 
                  ? association.TargetFKColumnName.Split(',').Select(a => $"\"{a}\"")
                  : association.Target.IdentityAttributes.Select(a => $"\"{association.Target.Name}_{a.Name}\""));
               string sourceFKs = string.Join(", ", !string.IsNullOrWhiteSpace(association.SourceFKColumnName) 
                  ? association.SourceFKColumnName.Split(',').Select(a => $"\"{a}\"")
                  : association.Source.IdentityAttributes.Select(a => $"\"{association.Source.Name}_{a.Name}\""));
               string joinTable = string.IsNullOrEmpty(association.JoinTableName) ? $"{association.Target.Name}_x_{association.Source.Name}" : association.JoinTableName;

               string segment =
                  "UsingEntity<Dictionary<string, object>>("
                + $"right => right.HasOne<{association.Target.FullName}>().WithMany().HasForeignKey({targetFKs}).OnDelete({PresentationHelper.GetDeleteActionSqlString(association.TargetDeleteAction)}),"
                + $"left => left.HasOne<{association.Source.FullName}>().WithMany().HasForeignKey({sourceFKs}).OnDelete({PresentationHelper.GetDeleteActionSqlString(association.SourceDeleteAction)}),"
                + $"join => join.ToTable(\"{joinTable}\"))";

               segments.Add(segment);
            }
            else
            {
               segments.Add($"UsingEntity(x => x.ToTable(\"{tableMap}\"))");

               string foreignKeySegment = CreateForeignKeySegment(association, foreignKeyColumns);

               if (!string.IsNullOrEmpty(foreignKeySegment))
                  segments.Add(foreignKeySegment);

               WriteSourceDeleteBehavior(association, segments);
               WriteTargetDeleteBehavior(association, segments);

               if (required
                && (association.SourceMultiplicity != Sawczyn.EFDesigner.EFModel.Multiplicity.One
                 || association.TargetMultiplicity != Sawczyn.EFDesigner.EFModel.Multiplicity.One))
                  segments.Add("IsRequired()");
            }

            return segments;
         }

         /// <summary>
         /// Writes the unidirectional dependent associations of a given ModelClass instance.
         /// </summary>
         /// <param name="sourceInstance">The ModelClass instance to search the unidirectional dependent associations.</param>
         /// <param name="baseSegment">The base segment to prepend the association's property segment.</param>
         /// <param name="visited">List of visited associations to avoid circular references.</param>
         protected override void WriteUnidirectionalDependentAssociations(ModelClass sourceInstance, string baseSegment, List<Association> visited)
         {
            // ReSharper disable once LoopCanBePartlyConvertedToQuery
            foreach (UnidirectionalAssociation association in Association.GetLinksToTargets(sourceInstance)
                                                                         .OfType<UnidirectionalAssociation>()
                                                                         .Where(x => x.Persistent && x.Target.IsDependentType))
            {
               if (visited.Contains(association))
                  continue;

               visited.Add(association);

               List<string> segments = new List<string>();

               string separator = sourceInstance.ModelRoot.ShadowKeyNamePattern == ShadowKeyPattern.TableColumn
                                     ? string.Empty
                                     : "_";

               switch (association.TargetMultiplicity) // realized by property on source
               {
                  case Sawczyn.EFDesigner.EFModel.Multiplicity.ZeroMany:
                     {
                        segments.Add(baseSegment);
                        segments.Add($"OwnsMany(p => p.{association.TargetPropertyName})");
                        segments.Add($"WithOwner(\"{association.Source.Name}_{association.TargetPropertyName}\")");
                        segments.Add($"HasForeignKey(\"{association.Source.Name}_{association.TargetPropertyName}{separator}Id\")");

                        Output(segments);

                        segments.Add(baseSegment);
                        segments.Add($"OwnsMany(p => p.{association.TargetPropertyName})");
                        segments.Add($"Property<{modelRoot.DefaultIdentityType}>(\"Id\")");

                        Output(segments);

                        segments.Add(baseSegment);
                        segments.Add($"OwnsMany(p => p.{association.TargetPropertyName})");
                        segments.Add("HasKey(\"Id\")");

                        Output(segments);

                        WriteUnidirectionalDependentAssociations(association.Target, $"{baseSegment}.OwnsMany(p => p.{association.TargetPropertyName})", visited);

                        break;
                     }

                  case Sawczyn.EFDesigner.EFModel.Multiplicity.One:
                     {
                        foreach (ModelAttribute modelAttribute in association.Target.AllAttributes)
                        {
                           segments.Add($"{baseSegment}.OwnsOne(p => p.{association.TargetPropertyName}).Property(p => p.{modelAttribute.Name})");

                           if (modelAttribute.ColumnName != modelAttribute.Name && !string.IsNullOrEmpty(modelAttribute.ColumnName))
                              segments.Add($"HasColumnName(\"{modelAttribute.ColumnName}\")");

                           if (modelAttribute.Required)
                              segments.Add("IsRequired()");

                           Output(segments);
                        }

                        segments.Add(baseSegment);
                        segments.Add($"Navigation(p => p.{association.TargetPropertyName}).IsRequired()");
                        Output(segments);

                        WriteUnidirectionalDependentAssociations(association.Target, $"{baseSegment}.OwnsOne(p => p.{association.TargetPropertyName})", visited);

                        break;
                     }

                  case Sawczyn.EFDesigner.EFModel.Multiplicity.ZeroOne:
                     {
                        foreach (ModelAttribute modelAttribute in association.Target.AllAttributes)
                        {
                           segments.Add($"{baseSegment}.OwnsOne(p => p.{association.TargetPropertyName}).Property(p => p.{modelAttribute.Name})");

                           if (modelAttribute.ColumnName != modelAttribute.Name && !string.IsNullOrEmpty(modelAttribute.ColumnName))
                              segments.Add($"HasColumnName(\"{modelAttribute.ColumnName}\")");

                           if (modelAttribute.Required)
                              segments.Add("IsRequired()");

                           Output(segments);
                        }

                        WriteUnidirectionalDependentAssociations(association.Target, $"{baseSegment}.OwnsOne(p => p.{association.TargetPropertyName})", visited);

                        break;
                     }
               }
            }
         }

         /// <summary>
         /// Writes unidirectional non-dependent associations
         /// </summary>
         /// <param name="modelClass">The model class</param>
         /// <param name="visited">The list of already-visited association</param>
         /// <param name="foreignKeyColumns">The list of foreign key columns</param>
         protected override void WriteUnidirectionalNonDependentAssociations(ModelClass modelClass, List<Association> visited, List<string> foreignKeyColumns)
         {
            // ReSharper disable once LoopCanBePartlyConvertedToQuery
            foreach (UnidirectionalAssociation association in Association.GetLinksToTargets(modelClass)
                                                                         .OfType<UnidirectionalAssociation>()
                                                                         .Where(x => x.Persistent && !x.Target.IsDependentType))
            {
               if (visited.Contains(association))
                  continue;

               visited.Add(association);

               List<string> segments = new List<string>();
               bool required = false;

               segments.Add($"modelBuilder.Entity<{modelClass.FullName}>()");

               switch (association.TargetMultiplicity) // realized by property on source
               {
                  case Sawczyn.EFDesigner.EFModel.Multiplicity.ZeroMany:
                     segments.Add($"HasMany<{association.Target.FullName}>(p => p.{association.TargetPropertyName})");
                     required = association.SourceMultiplicity == Sawczyn.EFDesigner.EFModel.Multiplicity.One;

                     break;

                  case Sawczyn.EFDesigner.EFModel.Multiplicity.One:
                  case Sawczyn.EFDesigner.EFModel.Multiplicity.ZeroOne:
                     segments.Add($"HasOne<{association.Target.FullName}>(p => p.{association.TargetPropertyName})");

                     break;
               }

               switch (association.SourceMultiplicity) // realized by property on target, but no property on target
               {
                  case Sawczyn.EFDesigner.EFModel.Multiplicity.ZeroMany:
                     segments.Add("WithMany()");
                     required = association.TargetMultiplicity == Sawczyn.EFDesigner.EFModel.Multiplicity.One;

                     if (association.TargetMultiplicity == Sawczyn.EFDesigner.EFModel.Multiplicity.ZeroMany)
                     {
                        string tableMap = string.IsNullOrEmpty(association.JoinTableName)
                                             ? $"{association.Target.Name}_x_{association.Source.Name}_{association.TargetPropertyName}"
                                             : association.JoinTableName;

                        segments.Add($"UsingEntity(x => x.ToTable(\"{tableMap}\"))");
                     }

                     break;

                  case Sawczyn.EFDesigner.EFModel.Multiplicity.One:
                  case Sawczyn.EFDesigner.EFModel.Multiplicity.ZeroOne:
                     segments.Add("WithOne()");

                     break;
               }

               string foreignKeySegment = CreateForeignKeySegment(association, foreignKeyColumns);

               if (!string.IsNullOrEmpty(foreignKeySegment))
                  segments.Add(foreignKeySegment);

               if (association.Dependent == association.Target)
               {
                  if (association.SourceDeleteAction == DeleteAction.None)
                     segments.Add("OnDelete(DeleteBehavior.NoAction)");
                  else if (association.SourceDeleteAction == DeleteAction.Cascade)
                     segments.Add("OnDelete(DeleteBehavior.Cascade)");
               }
               else if (association.Dependent == association.Source)
               {
                  if (association.TargetDeleteAction == DeleteAction.None)
                     segments.Add("OnDelete(DeleteBehavior.NoAction)");
                  else if (association.TargetDeleteAction == DeleteAction.Cascade)
                     segments.Add("OnDelete(DeleteBehavior.Cascade)");
               }

               if (required)
                  segments.Add("IsRequired()");

               Output(segments);

               if (association.TargetAutoInclude)
                  Output($"modelBuilder.Entity<{association.Source.FullName}>().Navigation(e => e.{association.TargetPropertyName}).AutoInclude();");

               if (!association.TargetAutoProperty)
               {
                  segments.Add($"modelBuilder.Entity<{association.Source.FullName}>().Navigation(e => e.{association.TargetPropertyName})");

                  segments.Add(modelClass.ModelRoot.IsEFCore6Plus
                                  ? $"UsePropertyAccessMode(PropertyAccessMode.{association.TargetPropertyAccessMode})"
                                  : $"Metadata.SetPropertyAccessMode(PropertyAccessMode.{association.TargetPropertyAccessMode})");

                  Output(segments);
               }
            }
         }
      }

      #endregion Template
   }
}
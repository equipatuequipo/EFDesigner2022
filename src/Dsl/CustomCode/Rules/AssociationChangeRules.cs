﻿using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Microsoft.VisualStudio.Modeling;

using Sawczyn.EFDesigner.EFModel.Extensions;

using stdole;

namespace Sawczyn.EFDesigner.EFModel
{
   [RuleOn(typeof(Association), FireTime = TimeToFire.TopLevelCommit)]
   internal class AssociationChangedRules : ChangeRule
   {
      // matches [Display(Name="*")] and [System.ComponentModel.DataAnnotations.Display(Name="*")]
      private static readonly Regex DisplayAttributeRegex = new Regex("^(.*)\\[(System\\.ComponentModel\\.DataAnnotations\\.)?Display\\(Name=\"([^\"]+)\"\\)\\](.*)$", RegexOptions.Compiled);

      private static void CheckSourceForDisplayText(BidirectionalAssociation bidirectionalAssociation)
      {
         Match match = DisplayAttributeRegex.Match(bidirectionalAssociation.SourceCustomAttributes);

         // is there a custom attribute for [Display]?
         if (match != Match.Empty)
         {
            // if SourceDisplayText is empty, move the Name down to SourceDisplayText
            if (string.IsNullOrWhiteSpace(bidirectionalAssociation.SourceDisplayText))
               bidirectionalAssociation.SourceDisplayText = match.Groups[3].Value;

            // if custom attribute's Name matches SourceDisplayText, remove that attribute, leaving other custom attributes if present
            if (match.Groups[3].Value == bidirectionalAssociation.SourceDisplayText)
               bidirectionalAssociation.SourceCustomAttributes = match.Groups[1].Value + match.Groups[4].Value;
         }
      }

      private static void CheckTargetForDisplayText(Association association)
      {
         Match match = DisplayAttributeRegex.Match(association.TargetCustomAttributes);

         // is there a custom attribute for [Display]?
         if (match != Match.Empty)
         {
            // if TargetDisplayText is empty, move the Name down to TargetDisplayText
            if (string.IsNullOrWhiteSpace(association.TargetDisplayText))
               association.TargetDisplayText = match.Groups[3].Value;

            // if custom attribute's Name matches TargetDisplayText, remove that attribute, leaving other custom attributes if present
            if (match.Groups[3].Value == association.TargetDisplayText)
               association.TargetCustomAttributes = match.Groups[1].Value + match.Groups[4].Value;
         }
      }

      /// <inheritdoc />
      public override void ElementPropertyChanged(ElementPropertyChangedEventArgs e)
      {
         base.ElementPropertyChanged(e);

         Association element = (Association)e.ModelElement;

         if (element.IsDeleted)
            return;

         Store store = element.Store;
         ModelRoot modelRoot = store.ModelRoot();
         Transaction current = store.TransactionManager.CurrentTransaction;

         if (current.IsSerializing || ModelRoot.BatchUpdating)
            return;

         if (Equals(e.NewValue, e.OldValue))
            return;

         List<string> errorMessages = EFCoreValidator.GetErrors(element).ToList();
         BidirectionalAssociation bidirectionalAssociation = element as BidirectionalAssociation;

         using (Transaction inner = store.TransactionManager.BeginTransaction("Association ElementPropertyChanged"))
         {
            bool doForeignKeyFixup = false;

            switch (e.DomainProperty.Name)
            {
               case "FKPropertyName":
                  {
                     if ((store.ModelRoot().EntityFrameworkVersion == EFVersion.EF6)
                      && (element.SourceMultiplicity != Multiplicity.ZeroMany)
                      && (element.TargetMultiplicity != Multiplicity.ZeroMany))
                     {
                        element.FKPropertyName = null;
                        doForeignKeyFixup = true;
                     }
                     else
                        ValidateForeignKeyNames(element, errorMessages);

                     if (!errorMessages.Any())
                        doForeignKeyFixup = true;

                     break;
                  }

               case "Persistent":
                  {
                     if (element.Persistent)
                     {
                        if (!element.Source.Persistent)
                           errorMessages.Add($"Can't make {element.GetDisplayText()} persistent since {element.Source.Name} is transient.");
                        if (!element.Target.Persistent)
                           errorMessages.Add($"Can't make {element.GetDisplayText()} persistent since {element.Target.Name} is transient.");
                     }
                     else
                     {
                        element.Source.Attributes.Where(a => a.IsForeignKeyFor == element.Id).ToList().ForEach(a => a.IsForeignKeyFor = Guid.Empty);
                        element.Target.Attributes.Where(a => a.IsForeignKeyFor == element.Id).ToList().ForEach(a => a.IsForeignKeyFor = Guid.Empty);
                     }

                     break;
                  }

               case "SourceCustomAttributes":
                  {
                     if ((bidirectionalAssociation != null) && !string.IsNullOrWhiteSpace(bidirectionalAssociation.SourceCustomAttributes))
                     {
                        bidirectionalAssociation.SourceCustomAttributes = $"[{bidirectionalAssociation.SourceCustomAttributes.Trim('[', ']')}]";
                        CheckSourceForDisplayText(bidirectionalAssociation);
                     }

                     break;
                  }

               case "SourceDisplayText":
                  {
                     if (bidirectionalAssociation != null)
                        CheckSourceForDisplayText(bidirectionalAssociation);

                     break;
                  }

               case "SourceFKColumnName":
                  {
                     if (element is BidirectionalAssociation bidiElement && !string.IsNullOrEmpty(bidiElement.SourceFKColumnName))
                     {
                        ValidateForeignKeyColumnNames(bidiElement.SourceFKColumnName, bidiElement.Source.AllIdentityAttributes, errorMessages);
                     }
                     break;
                  }

               case "SourceMultiplicity":
                  {
                     if (!ValidateMultiplicity(element, modelRoot, errorMessages, ref doForeignKeyFixup))
                        break;

                     Multiplicity priorSourceMultiplicity = (Multiplicity)e.OldValue;

                     if ((((priorSourceMultiplicity == Multiplicity.ZeroOne) || (priorSourceMultiplicity == Multiplicity.ZeroMany)) && (element.SourceMultiplicity == Multiplicity.One))
                      || (((element.SourceMultiplicity == Multiplicity.ZeroOne) || (element.SourceMultiplicity == Multiplicity.ZeroMany)) && (priorSourceMultiplicity == Multiplicity.One)))
                        doForeignKeyFixup = true;

                     string defaultSourcePropertyName =
                        (priorSourceMultiplicity == Multiplicity.ZeroMany) && (ModelRoot.PluralizationService?.IsSingular(element.Source.Name) == true)
                           ? ModelRoot.PluralizationService.Pluralize(element.Source.Name)
                           : element.Source.Name;

                     if (element is BidirectionalAssociation bidirectional && (bidirectional.SourcePropertyName == defaultSourcePropertyName))
                     {
                        bidirectional.SourcePropertyName = (element.SourceMultiplicity == Multiplicity.ZeroMany) && (ModelRoot.PluralizationService?.IsSingular(element.Source.Name) == true)
                                                              ? ModelRoot.PluralizationService.Pluralize(element.Source.Name)
                                                              : element.Source.Name;
                     }

                     break;
                  }

               case "SourcePropertyName":
                  {
                     string sourcePropertyNameErrorMessage = ValidateAssociationIdentifier(element, element.Target, (string)e.NewValue);

                     if (EFModelDiagram.IsDroppingExternal && (sourcePropertyNameErrorMessage != null))
                        element.Delete();
                     else
                        errorMessages.Add(sourcePropertyNameErrorMessage);

                     break;
                  }

               case "SourceRole":
                  {
                     if (element.SourceRole == EndpointRole.NotApplicable)
                        element.SourceRole = EndpointRole.NotSet;

                     //if (element.Source.IsDependentType)
                     //{
                     //   element.SourceRole = EndpointRole.Dependent;
                     //   element.TargetRole = EndpointRole.Principal;
                     //}
                     //else 
                     if (!SetEndpointRoles(element))
                     {
                        if ((element.SourceRole == EndpointRole.Dependent) && (element.TargetRole != EndpointRole.Principal))
                           element.TargetRole = EndpointRole.Principal;
                        else if ((element.SourceRole == EndpointRole.Principal) && (element.TargetRole != EndpointRole.Dependent))
                           element.TargetRole = EndpointRole.Dependent;
                     }

                     doForeignKeyFixup = true;

                     break;
                  }

               case "TargetCustomAttributes":
                  {
                     if (!string.IsNullOrWhiteSpace(element.TargetCustomAttributes))
                     {
                        element.TargetCustomAttributes = $"[{element.TargetCustomAttributes.Trim('[', ']')}]";
                        CheckTargetForDisplayText(element);
                     }

                     break;
                  }

               case "TargetDisplayText":
                  {
                     CheckTargetForDisplayText(element);

                     break;
                  }

               case "TargetMultiplicity":
                  {
                     if (!ValidateMultiplicity(element, modelRoot, errorMessages, ref doForeignKeyFixup))
                        break;

                     Multiplicity priorTargetMultiplicity = (Multiplicity)e.OldValue;

                     if ((((priorTargetMultiplicity == Multiplicity.ZeroOne) || (priorTargetMultiplicity == Multiplicity.ZeroMany)) && (element.TargetMultiplicity == Multiplicity.One))
                      || (((element.TargetMultiplicity == Multiplicity.ZeroOne) || (element.TargetMultiplicity == Multiplicity.ZeroMany)) && (priorTargetMultiplicity == Multiplicity.One)))
                        doForeignKeyFixup = true;

                     string defaultTargetPropertyName =
                        (priorTargetMultiplicity == Multiplicity.ZeroMany) && (ModelRoot.PluralizationService?.IsSingular(element.Target.Name) == true)
                           ? ModelRoot.PluralizationService.Pluralize(element.Target.Name)
                           : element.Target.Name;

                     if (element.TargetPropertyName == defaultTargetPropertyName)
                     {
                        element.TargetPropertyName = (element.TargetMultiplicity == Multiplicity.ZeroMany) && (ModelRoot.PluralizationService?.IsSingular(element.Target.Name) == true)
                                                        ? ModelRoot.PluralizationService.Pluralize(element.Target.Name)
                                                        : element.Target.Name;
                     }

                     break;
                  }

               case "TargetPropertyName":
                  {
                     // if we're creating an association via drag/drop, it's possible the existing property name
                     // is the same as the default property name. The default doesn't get created until the transaction is 
                     // committed, so the drop's action will cause a name clash. Remove the clashing property, but
                     // only if drag/drop.

                     string targetPropertyNameErrorMessage = ValidateAssociationIdentifier(element, element.Source, (string)e.NewValue);

                     if (EFModelDiagram.IsDroppingExternal && (targetPropertyNameErrorMessage != null))
                        element.Delete();
                     else
                        errorMessages.Add(targetPropertyNameErrorMessage);

                     break;
                  }

               case "TargetRole":
                  {
                     if (element.TargetRole == EndpointRole.NotApplicable)
                        element.TargetRole = EndpointRole.NotSet;

                     //if (element.Target.IsDependentType && (element.SourceRole != EndpointRole.Principal || element.TargetRole != EndpointRole.Dependent))
                     //{
                     //   element.SourceRole = EndpointRole.Principal;
                     //   element.TargetRole = EndpointRole.Dependent;
                     //   doForeignKeyFixup = true;
                     //}
                     //else 
                     if (!SetEndpointRoles(element))
                     {
                        if ((element.TargetRole == EndpointRole.Dependent) && (element.SourceRole != EndpointRole.Principal))
                        {
                           element.SourceRole = EndpointRole.Principal;
                           doForeignKeyFixup = true;
                        }
                        else if ((element.TargetRole == EndpointRole.Principal) && (element.SourceRole != EndpointRole.Dependent))
                        {
                           element.SourceRole = EndpointRole.Dependent;
                           doForeignKeyFixup = true;
                        }
                     }

                     break;
                  }

               case "TargetFKColumnName":
                  {
                     if (element is BidirectionalAssociation bidiElement && !string.IsNullOrEmpty(bidiElement.TargetFKColumnName))
                     {
                        ValidateForeignKeyColumnNames(bidiElement.TargetFKColumnName, bidiElement.Target.AllIdentityAttributes, errorMessages);
                     }
                     break;
                  }
            }

            if (doForeignKeyFixup)
               FixupForeignKeys(element);

            inner.Commit();
            element.RedrawItem();
         }

         errorMessages = errorMessages.Where(m => m != null).ToList();

         if (errorMessages.Any())
         {
            current.Rollback();
            ErrorDisplay.Show(store, string.Join("\n", errorMessages));
         }
      }

      public static void FixupForeignKeys(Association element)
      {
         List<ModelAttribute> fkProperties = element.Source.Attributes.Where(x => x.IsForeignKeyFor == element.Id)
                                                    .Union(element.Target.Attributes.Where(x => x.IsForeignKeyFor == element.Id))
                                                    .ToList();

         // EF6 can't have declared foreign keys for 1..1 / 0-1..1 / 1..0-1 / 0-1..0-1 relationships
         if (!string.IsNullOrEmpty(element.FKPropertyName)
          && (element.Source.ModelRoot.EntityFrameworkVersion == EFVersion.EF6)
          && (element.SourceMultiplicity != Multiplicity.ZeroMany)
          && (element.TargetMultiplicity != Multiplicity.ZeroMany))
            element.FKPropertyName = null;

         // if no FKs, remove all the attributes for this element
         if (string.IsNullOrEmpty(element.FKPropertyName) || (element.Dependent == null))
         {
            List<ModelAttribute> unnecessaryProperties = fkProperties.Where(x => !x.IsIdentity).ToList();

            if (unnecessaryProperties.Any())
               WarningDisplay.Show($"{element.GetDisplayText()} doesn't specify defined foreign keys. Removing foreign key attribute(s) {string.Join(", ", unnecessaryProperties.Select(x => x.GetDisplayText()))}");

            foreach (ModelAttribute attribute in unnecessaryProperties)
            {
               attribute.ClearFKMods(string.Empty);
               attribute.Delete();
            }

            return;
         }

         // synchronize what's there to what should be there
         string[] currentForeignKeyPropertyNames = element.GetForeignKeyPropertyNames();

         (IEnumerable<string> add, IEnumerable<ModelAttribute> remove) = fkProperties.Synchronize(currentForeignKeyPropertyNames, (attribute, name) => attribute.Name == name);

         List<ModelAttribute> removeList = remove.ToList();
         fkProperties = fkProperties.Except(removeList).ToList();

         // remove extras
         if (removeList.Any())
            WarningDisplay.Show($"{element.GetDisplayText()} has extra foreign keys. Removing unnecessary foreign key attribute(s) {string.Join(", ", removeList.Select(x => x.GetDisplayText()))}");

         for (int index = 0; index < removeList.Count; index++)
         {
            ModelAttribute attribute = removeList[index];
            attribute.ClearFKMods(string.Empty);
            attribute.Delete();
            removeList.RemoveAt(index--);
         }

         // reparent existing properties if needed
         foreach (ModelAttribute existing in fkProperties.Where(x => x.ModelClass != element.Dependent))
         {
            existing.ClearFKMods();
            existing.ModelClass.MoveAttribute(existing, element.Dependent);
            existing.SetFKMods(element);
         }

         // create new properties if they don't already exist
         foreach (string propertyName in add.Where(n => element.Dependent.Attributes.All(a => a.Name != n)))
            element.Dependent.Attributes.Add(new ModelAttribute(element.Store, new PropertyAssignment(ModelAttribute.NameDomainPropertyId, propertyName)));

         // make a pass through and fixup the types, summaries, etc. based on the principal's identity attributes
         ModelAttribute[] principalIdentityAttributes = element.Principal.AllIdentityAttributes.ToArray();
         string summaryBoilerplate = element.GetSummaryBoilerplate();

         for (int index = 0; index < currentForeignKeyPropertyNames.Length; index++)
         {
            ModelAttribute fkProperty = element.Dependent.Attributes.First(x => x.Name == currentForeignKeyPropertyNames[index]);
            ModelAttribute idProperty = principalIdentityAttributes[index];

            bool required = element.Dependent == element.Source
                               ? element.TargetMultiplicity == Multiplicity.One
                               : element.SourceMultiplicity == Multiplicity.One;

            fkProperty.SetFKMods(element, summaryBoilerplate, required, idProperty.Type);
         }
      }

      internal static bool SetEndpointRoles(Association element)
      {
         // Note that we're checking 'if (x != y) x = y' to ensure that unnecessary rules don't fire off

         switch (element.TargetMultiplicity)
         {
            case Multiplicity.ZeroMany:

               switch (element.SourceMultiplicity)
               {
                  case Multiplicity.ZeroMany:
                     if (element.SourceRole != EndpointRole.NotSet)
                        element.SourceRole = EndpointRole.NotSet;

                     if (element.TargetRole != EndpointRole.NotSet)
                        element.TargetRole = EndpointRole.NotSet;

                     return true;

                  case Multiplicity.One:
                     if (element.SourceRole != EndpointRole.Principal)
                        element.SourceRole = EndpointRole.Principal;

                     if (element.TargetRole != EndpointRole.Dependent)
                        element.TargetRole = EndpointRole.Dependent;

                     return true;

                  case Multiplicity.ZeroOne:
                     if (element.SourceRole != EndpointRole.Principal)
                        element.SourceRole = EndpointRole.Principal;

                     if (element.TargetRole != EndpointRole.Dependent)
                        element.TargetRole = EndpointRole.Dependent;

                     return true;
               }

               break;

            case Multiplicity.One:

               switch (element.SourceMultiplicity)
               {
                  case Multiplicity.ZeroMany:
                     if (element.SourceRole != EndpointRole.Dependent)
                        element.SourceRole = EndpointRole.Dependent;

                     if (element.TargetRole != EndpointRole.Principal)
                        element.TargetRole = EndpointRole.Principal;

                     return true;

                  case Multiplicity.One:

                     return false;

                  case Multiplicity.ZeroOne:
                     if (element.SourceRole != EndpointRole.Dependent)
                        element.SourceRole = EndpointRole.Dependent;

                     if (element.TargetRole != EndpointRole.Principal)
                        element.TargetRole = EndpointRole.Principal;

                     return true;
               }

               break;

            case Multiplicity.ZeroOne:

               switch (element.SourceMultiplicity)
               {
                  case Multiplicity.ZeroMany:
                     if (element.SourceRole != EndpointRole.Dependent)
                        element.SourceRole = EndpointRole.Dependent;

                     if (element.TargetRole != EndpointRole.Principal)
                        element.TargetRole = EndpointRole.Principal;

                     return true;

                  case Multiplicity.One:
                     if (element.SourceRole != EndpointRole.Principal)
                        element.SourceRole = EndpointRole.Principal;

                     if (element.TargetRole != EndpointRole.Dependent)
                        element.TargetRole = EndpointRole.Dependent;

                     return true;

                  case Multiplicity.ZeroOne:

                     return false;
               }

               break;
         }

         return false;
      }

      private static string ValidateAssociationIdentifier(Association association, ModelClass targetedClass, string identifier)
      {
         if (string.IsNullOrWhiteSpace(identifier) || !CodeGenerator.IsValidLanguageIndependentIdentifier(identifier))
            return $"{identifier} isn't a valid .NET identifier";

         ModelClass offendingModelClass = targetedClass.AllAttributes.FirstOrDefault(x => x.Name == identifier)?.ModelClass
                                       ?? targetedClass.AllNavigationProperties(association).FirstOrDefault(x => x.PropertyName == identifier)?.ClassType;

         return offendingModelClass != null
                   ? $"Duplicate symbol {identifier} in {offendingModelClass.Name}"
                   : null;
      }

      private static void ValidateForeignKeyNames(Association element, List<string> errorMessages)
      {
         if (!string.IsNullOrWhiteSpace(element.FKPropertyName))
         {
            string[] foreignKeyPropertyNames = element.GetForeignKeyPropertyNames();
            string tag = $"({element.Source.Name}:{element.Target.Name})";

            if (element.Dependent == null)
            {
               errorMessages.Add($"{tag} can't have foreign keys defined; no dependent role found");

               return;
            }

            int propertyCount = foreignKeyPropertyNames.Length;
            int identityCount = element.Principal.AllIdentityAttributes.Count();

            if (propertyCount != identityCount)
            {
               errorMessages.Add($"{tag} foreign key must have zero or {identityCount} {(identityCount == 1 ? "property" : "properties")} defined, "
                               + $"since {element.Principal.Name} has {identityCount} identity properties. Found {propertyCount} instead");
            }

            // validate names
            foreach (string propertyName in foreignKeyPropertyNames)
            {
               if (!CodeGenerator.IsValidLanguageIndependentIdentifier(propertyName))
                  errorMessages.Add($"{tag} FK property name '{propertyName}' isn't a valid .NET identifier");

               if (element.Dependent.AllAttributes.Except(element.Dependent.Attributes).Any(a => a.Name == propertyName))
                  errorMessages.Add($"{tag} FK property name '{propertyName}' is used in a base class of {element.Dependent.Name}");
            }

            // ensure no FKs are autogenerated identities
            errorMessages.AddRange(element.GetFKAutoIdentityErrors()
                                          .Select(attribute => $"{attribute.Name} in {element.Dependent.FullName} is an auto-generated identity. Migration will fail."));
         }
      }

      private static void ValidateForeignKeyColumnNames(string columnNames, IEnumerable<ModelAttribute> attributes, List<string> errorMessages)
      {
         string[] columnNamesParts = columnNames.Split(',');

         if (columnNamesParts.Length != attributes.Count()) 
            errorMessages.Add($"{attributes.Count()} (comma separated) column name(s) are required because of the number of entity identities");
      }

      private bool ValidateMultiplicity(Association element, ModelRoot modelRoot, List<string> errorMessages, ref bool doForeignKeyFixup)
      {
         if (!element.AllCardinalitiesAreValid(out string errorMessage))
         {
            errorMessages.Add(errorMessage);

            return false;
         }

         if (!element.Source.IsDependentType && !element.Target.IsDependentType)
         {
            if (element.Is(Multiplicity.One, Multiplicity.One) || element.Is(Multiplicity.ZeroOne, Multiplicity.ZeroOne))
            {
               if (element.SourceRole != EndpointRole.NotSet)
                  element.SourceRole = EndpointRole.NotSet;

               if (element.TargetRole != EndpointRole.NotSet)
                  element.TargetRole = EndpointRole.NotSet;
            }
            else
               SetEndpointRoles(element);
         }

         // cascade delete behavior could now be illegal. Reset to default
         element.SourceDeleteAction = DeleteAction.Default;
         element.TargetDeleteAction = DeleteAction.Default;

         if (element.Dependent == null)
         {
            element.FKPropertyName = null;
            doForeignKeyFixup = true;
         }

         if ((modelRoot.EntityFrameworkVersion == EFVersion.EF6 || modelRoot.IsEFCore5Plus)
          && (element.SourceMultiplicity != Multiplicity.ZeroMany)
          && (element.TargetMultiplicity != Multiplicity.ZeroMany))
         {
            element.FKPropertyName = null;
            doForeignKeyFixup = true;
         }

         return true;
      }
   }
}
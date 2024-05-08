﻿#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1570 // XML comment has badly formed XML
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using DslModeling = global::Microsoft.VisualStudio.Modeling;
using DslDesign = global::Microsoft.VisualStudio.Modeling.Design;
using System;
using System.Diagnostics;
using System.Drawing.Design;
using System.Windows.Forms;
using DslDiagrams = global::Microsoft.VisualStudio.Modeling.Diagrams;

namespace Sawczyn.EFDesigner.EFModel
{
   /// <summary>
   /// Helper class used to create and initialize toolbox items for this DSL.
   /// </summary>
   /// <remarks>
   /// Double-derived class to allow easier code customization.
   /// </remarks>
   public partial class EFModelToolboxHelper : EFModelToolboxHelperBase 
   {
      /// <summary>
      /// Constructs a new EFModelToolboxHelper.
      /// </summary>
      public EFModelToolboxHelper(global::System.IServiceProvider serviceProvider)
         : base(serviceProvider) { }
   }
   
   /// <summary>
   /// Helper class used to create and initialize toolbox items for this DSL.
   /// </summary>
   
   [global::System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "The store is disposed on domain shut down")]
   public abstract class EFModelToolboxHelperBase
   {
      /// <summary>
      /// Toolbox item filter string used to identify EFModel toolbox items.  
      /// </summary>
      /// <remarks>
      /// See the MSDN documentation for the ToolboxItemFilterAttribute class for more information on toolbox
      /// item filters.
      /// </remarks>
      public const string ToolboxFilterString = "EFModel.4.4";
      /// <summary>
      /// Toolbox item filter string used to identify ModelClass element tool.
      /// </summary>
      public const string ModelClassFilterString = "ModelClass.4.4";
      /// <summary>
      /// Toolbox item filter string used to identify UnidirectionalAssociation connector tool.
      /// </summary>
      public const string UnidirectionalAssociationFilterString = "UnidirectionalAssociation.4.4";
      /// <summary>
      /// Toolbox item filter string used to identify BidirectionalAssociation connector tool.
      /// </summary>
      public const string BidirectionalAssociationFilterString = "BidirectionalAssociation.4.4";
      /// <summary>
      /// Toolbox item filter string used to identify Generalization connector tool.
      /// </summary>
      public const string GeneralizationFilterString = "Generalization.4.4";
      /// <summary>
      /// Toolbox item filter string used to identify Comment element tool.
      /// </summary>
      public const string CommentFilterString = "Comment.4.4";
      /// <summary>
      /// Toolbox item filter string used to identify CommentLink connector tool.
      /// </summary>
      public const string CommentLinkFilterString = "CommentLink.4.4";
      /// <summary>
      /// Toolbox item filter string used to identify Enumeration element tool.
      /// </summary>
      public const string EnumerationFilterString = "Enumeration.4.4";

   
      private global::System.Collections.Generic.Dictionary<string, DslDesign::ModelingToolboxItem> toolboxItemCache = new global::System.Collections.Generic.Dictionary<string, DslDesign::ModelingToolboxItem>();
      private DslModeling::Store toolboxStore;
      
      private global::System.IServiceProvider sp;
      
      /// <summary>
      /// Constructs a new EFModelToolboxHelperBase.
      /// </summary>
      protected EFModelToolboxHelperBase(global::System.IServiceProvider serviceProvider)
      {
         if(serviceProvider == null) throw new global::System.ArgumentNullException("serviceProvider");
         
         this.sp = serviceProvider;
      }
      
      /// <summary>
      /// Serivce provider used to access services from the hosting environment.
      /// </summary>
      protected global::System.IServiceProvider ServiceProvider
      {
         get
         {
            return this.sp;
         }
      }

      /// <summary>
      /// Returns the display name of the tab that should be opened by default when the editor is opened.
      /// </summary>
      public static string DefaultToolboxTabName
      {
         get
         {
            return global::Sawczyn.EFDesigner.EFModel.EFModelDomainModel.SingletonResourceManager.GetString("EF Model DiagramsToolboxTab", global::System.Globalization.CultureInfo.CurrentUICulture);
         }
      }
      
      
      /// <summary>
      /// Returns the toolbox items count in the default tool box tab.
      /// </summary>
      public static int DefaultToolboxTabToolboxItemsCount
      {
         get
         {
            return 7;
         }
      }
      

      /// <summary>
      /// Returns a list of custom toolbox items to be added dynamically
      /// </summary>
      public virtual global::System.Collections.Generic.IList<DslDesign::ModelingToolboxItem> CreateToolboxItems()
      {
         global::System.Collections.Generic.List<DslDesign::ModelingToolboxItem> toolboxItems = new global::System.Collections.Generic.List<DslDesign::ModelingToolboxItem>();
         return toolboxItems;
      }
      
      /// <summary>
      /// Creates an ElementGroupPrototype for the element tool corresponding to the given domain class id.
      /// Default behavior is to create a prototype containing an instance of the domain class.
      /// Derived classes may override this to add additional information to the prototype.
      /// </summary>
      protected virtual DslModeling::ElementGroupPrototype CreateElementToolPrototype(DslModeling::Store store, global::System.Guid domainClassId)
      {
         DslModeling::ModelElement element = store.ElementFactory.CreateElement(domainClassId);
         DslModeling::ElementGroup elementGroup = new DslModeling::ElementGroup(store.DefaultPartition);
         elementGroup.AddGraph(element, true);
         return elementGroup.CreatePrototype();
      }

      /// <summary>
      /// Returns instance of ModelingToolboxItem based on specified name.
      /// This method must be called from within a Transaction. Failure to do so will result in an exception
      /// </summary>
      /// <param name="itemId">unique name of desired ToolboxItem</param>
      /// <param name="store">Store to perform the operation against</param>
      /// <returns>An instance of ModelingToolboxItem if the itemId can be resolved, null otherwise</returns>
      public virtual DslDesign::ModelingToolboxItem GetToolboxItem(string itemId, DslModeling::Store store)
      {
         DslDesign::ModelingToolboxItem result = null;
         if (string.IsNullOrEmpty(itemId))
         {
            return null;
         }
         if (store == null)
         {
            return null;
         }
         global::System.Resources.ResourceManager resourceManager = global::Sawczyn.EFDesigner.EFModel.EFModelDomainModel.SingletonResourceManager;
         global::System.Globalization.CultureInfo resourceCulture = global::System.Globalization.CultureInfo.CurrentUICulture;
         switch(itemId)
         {
            case "Sawczyn.EFDesigner.EFModel.ModelClassToolboxItem":
               // Add ModelClass shape tool.
               result = new DslDesign::ModelingToolboxItem(
                  "Sawczyn.EFDesigner.EFModel.ModelClassToolboxItem", // Unique identifier (non-localized) for the toolbox item.
                  1, // Position relative to other items in the same toolbox tab.
                  resourceManager.GetString("ModelClassToolboxItem", resourceCulture), // Localized display name for the item.
                  (global::System.Drawing.Bitmap)DslDiagrams::ImageHelper.GetImage(resourceManager.GetObject("ModelClassToolboxBitmap", resourceCulture)), // Image displayed next to the toolbox item.
                  "Sawczyn.EFDesigner.EFModel.EF Model DiagramsToolboxTab", // Unique identifier (non-localized) for the toolbox item tab.
                  resourceManager.GetString("EF Model DiagramsToolboxTab", resourceCulture), // Localized display name for the toolbox tab.
                  "ModelClassF1Keyword", // F1 help keyword for the toolbox item.
                  resourceManager.GetString("ModelClassToolboxTooltip", resourceCulture), // Localized tooltip text for the toolbox item.
                  CreateElementToolPrototype(store, global::Sawczyn.EFDesigner.EFModel.ModelClass.DomainClassId), // ElementGroupPrototype (data object) representing model element on the toolbox.
                  new global::System.ComponentModel.ToolboxItemFilterAttribute[] { // Collection of ToolboxItemFilterAttribute objects that determine visibility of the toolbox item.
                        new global::System.ComponentModel.ToolboxItemFilterAttribute("ModelClass"), // HACK : MEXEDGE
                  new global::System.ComponentModel.ToolboxItemFilterAttribute(ToolboxFilterString, global::System.ComponentModel.ToolboxItemFilterType.Require),
                  new global::System.ComponentModel.ToolboxItemFilterAttribute(ModelClassFilterString)
                  });
               break;
            case "Sawczyn.EFDesigner.EFModel.UnidirectionalAssociationToolboxItem":

               // Add UnidirectionalAssociation connector tool.
               result = new DslDesign::ModelingToolboxItem(
                  "Sawczyn.EFDesigner.EFModel.UnidirectionalAssociationToolboxItem", // Unique identifier (non-localized) for the toolbox item.
                  2, // Position relative to other items in the same toolbox tab.
                  resourceManager.GetString("UnidirectionalAssociationToolboxItem", resourceCulture), // Localized display name for the item.
                  (global::System.Drawing.Bitmap)DslDiagrams::ImageHelper.GetImage(resourceManager.GetObject("UnidirectionalAssociationToolboxBitmap", resourceCulture)), // Image displayed next to the toolbox item.
                  "Sawczyn.EFDesigner.EFModel.EF Model DiagramsToolboxTab", // Unique identifier (non-localized) for the toolbox item tab.
                  resourceManager.GetString("EF Model DiagramsToolboxTab", resourceCulture), // Localized display name for the toolbox tab.
                  "ConnectUnidirectionalAssociationF1Keyword", // F1 help keyword for the toolbox item.
                  resourceManager.GetString("UnidirectionalAssociationToolboxTooltip", resourceCulture), // Localized tooltip text for the toolbox item.
                  null, // Connector toolbox items do not have an underlying data object.
                  new global::System.ComponentModel.ToolboxItemFilterAttribute[] { // Collection of ToolboxItemFilterAttribute objects that determine visibility of the toolbox item.
                     new global::System.ComponentModel.ToolboxItemFilterAttribute(ToolboxFilterString, global::System.ComponentModel.ToolboxItemFilterType.Require), 
                     new global::System.ComponentModel.ToolboxItemFilterAttribute(UnidirectionalAssociationFilterString),
                            new global::System.ComponentModel.ToolboxItemFilterAttribute("UnidirectionalAssociation"), // HACK : MEXEDGE
                  });
               break;
            case "Sawczyn.EFDesigner.EFModel.BidirectionalAssociationToolboxItem":

               // Add BidirectionalAssociation connector tool.
               result = new DslDesign::ModelingToolboxItem(
                  "Sawczyn.EFDesigner.EFModel.BidirectionalAssociationToolboxItem", // Unique identifier (non-localized) for the toolbox item.
                  3, // Position relative to other items in the same toolbox tab.
                  resourceManager.GetString("BidirectionalAssociationToolboxItem", resourceCulture), // Localized display name for the item.
                  (global::System.Drawing.Bitmap)DslDiagrams::ImageHelper.GetImage(resourceManager.GetObject("BidirectionalAssociationToolboxBitmap", resourceCulture)), // Image displayed next to the toolbox item.
                  "Sawczyn.EFDesigner.EFModel.EF Model DiagramsToolboxTab", // Unique identifier (non-localized) for the toolbox item tab.
                  resourceManager.GetString("EF Model DiagramsToolboxTab", resourceCulture), // Localized display name for the toolbox tab.
                  "ConnectBidirectionalAssociationF1Keyword", // F1 help keyword for the toolbox item.
                  resourceManager.GetString("BidirectionalAssociationToolboxTooltip", resourceCulture), // Localized tooltip text for the toolbox item.
                  null, // Connector toolbox items do not have an underlying data object.
                  new global::System.ComponentModel.ToolboxItemFilterAttribute[] { // Collection of ToolboxItemFilterAttribute objects that determine visibility of the toolbox item.
                     new global::System.ComponentModel.ToolboxItemFilterAttribute(ToolboxFilterString, global::System.ComponentModel.ToolboxItemFilterType.Require), 
                     new global::System.ComponentModel.ToolboxItemFilterAttribute(BidirectionalAssociationFilterString),
                            new global::System.ComponentModel.ToolboxItemFilterAttribute("BidirectionalAssociation"), // HACK : MEXEDGE
                  });
               break;
            case "Sawczyn.EFDesigner.EFModel.GeneralizationToolboxItem":

               // Add Generalization connector tool.
               result = new DslDesign::ModelingToolboxItem(
                  "Sawczyn.EFDesigner.EFModel.GeneralizationToolboxItem", // Unique identifier (non-localized) for the toolbox item.
                  4, // Position relative to other items in the same toolbox tab.
                  resourceManager.GetString("GeneralizationToolboxItem", resourceCulture), // Localized display name for the item.
                  (global::System.Drawing.Bitmap)DslDiagrams::ImageHelper.GetImage(resourceManager.GetObject("GeneralizationToolboxBitmap", resourceCulture)), // Image displayed next to the toolbox item.
                  "Sawczyn.EFDesigner.EFModel.EF Model DiagramsToolboxTab", // Unique identifier (non-localized) for the toolbox item tab.
                  resourceManager.GetString("EF Model DiagramsToolboxTab", resourceCulture), // Localized display name for the toolbox tab.
                  "GeneralizationF1Keyword", // F1 help keyword for the toolbox item.
                  resourceManager.GetString("GeneralizationToolboxTooltip", resourceCulture), // Localized tooltip text for the toolbox item.
                  null, // Connector toolbox items do not have an underlying data object.
                  new global::System.ComponentModel.ToolboxItemFilterAttribute[] { // Collection of ToolboxItemFilterAttribute objects that determine visibility of the toolbox item.
                     new global::System.ComponentModel.ToolboxItemFilterAttribute(ToolboxFilterString, global::System.ComponentModel.ToolboxItemFilterType.Require), 
                     new global::System.ComponentModel.ToolboxItemFilterAttribute(GeneralizationFilterString),
                            new global::System.ComponentModel.ToolboxItemFilterAttribute("Generalization"), // HACK : MEXEDGE
                  });
               break;
            case "Sawczyn.EFDesigner.EFModel.CommentToolboxItem":
               // Add Comment shape tool.
               result = new DslDesign::ModelingToolboxItem(
                  "Sawczyn.EFDesigner.EFModel.CommentToolboxItem", // Unique identifier (non-localized) for the toolbox item.
                  5, // Position relative to other items in the same toolbox tab.
                  resourceManager.GetString("CommentToolboxItem", resourceCulture), // Localized display name for the item.
                  (global::System.Drawing.Bitmap)DslDiagrams::ImageHelper.GetImage(resourceManager.GetObject("CommentToolboxBitmap", resourceCulture)), // Image displayed next to the toolbox item.
                  "Sawczyn.EFDesigner.EFModel.EF Model DiagramsToolboxTab", // Unique identifier (non-localized) for the toolbox item tab.
                  resourceManager.GetString("EF Model DiagramsToolboxTab", resourceCulture), // Localized display name for the toolbox tab.
                  "CommentF1Keyword", // F1 help keyword for the toolbox item.
                  resourceManager.GetString("CommentToolboxTooltip", resourceCulture), // Localized tooltip text for the toolbox item.
                  CreateElementToolPrototype(store, global::Sawczyn.EFDesigner.EFModel.Comment.DomainClassId), // ElementGroupPrototype (data object) representing model element on the toolbox.
                  new global::System.ComponentModel.ToolboxItemFilterAttribute[] { // Collection of ToolboxItemFilterAttribute objects that determine visibility of the toolbox item.
                        new global::System.ComponentModel.ToolboxItemFilterAttribute("Comment"), // HACK : MEXEDGE
                  new global::System.ComponentModel.ToolboxItemFilterAttribute(ToolboxFilterString, global::System.ComponentModel.ToolboxItemFilterType.Require),
                  new global::System.ComponentModel.ToolboxItemFilterAttribute(CommentFilterString)
                  });
               break;
            case "Sawczyn.EFDesigner.EFModel.CommentLinkToolboxItem":

               // Add CommentLink connector tool.
               result = new DslDesign::ModelingToolboxItem(
                  "Sawczyn.EFDesigner.EFModel.CommentLinkToolboxItem", // Unique identifier (non-localized) for the toolbox item.
                  6, // Position relative to other items in the same toolbox tab.
                  resourceManager.GetString("CommentLinkToolboxItem", resourceCulture), // Localized display name for the item.
                  (global::System.Drawing.Bitmap)DslDiagrams::ImageHelper.GetImage(resourceManager.GetObject("CommentLinkToolboxBitmap", resourceCulture)), // Image displayed next to the toolbox item.
                  "Sawczyn.EFDesigner.EFModel.EF Model DiagramsToolboxTab", // Unique identifier (non-localized) for the toolbox item tab.
                  resourceManager.GetString("EF Model DiagramsToolboxTab", resourceCulture), // Localized display name for the toolbox tab.
                  "CommentsReferenceDesignElementsF1Keyword", // F1 help keyword for the toolbox item.
                  resourceManager.GetString("CommentLinkToolboxTooltip", resourceCulture), // Localized tooltip text for the toolbox item.
                  null, // Connector toolbox items do not have an underlying data object.
                  new global::System.ComponentModel.ToolboxItemFilterAttribute[] { // Collection of ToolboxItemFilterAttribute objects that determine visibility of the toolbox item.
                     new global::System.ComponentModel.ToolboxItemFilterAttribute(ToolboxFilterString, global::System.ComponentModel.ToolboxItemFilterType.Require), 
                     new global::System.ComponentModel.ToolboxItemFilterAttribute(CommentLinkFilterString),
                            new global::System.ComponentModel.ToolboxItemFilterAttribute("CommentLink"), // HACK : MEXEDGE
                  });
               break;
            case "Sawczyn.EFDesigner.EFModel.EnumerationToolboxItem":
               // Add Enumeration shape tool.
               result = new DslDesign::ModelingToolboxItem(
                  "Sawczyn.EFDesigner.EFModel.EnumerationToolboxItem", // Unique identifier (non-localized) for the toolbox item.
                  7, // Position relative to other items in the same toolbox tab.
                  resourceManager.GetString("EnumerationToolboxItem", resourceCulture), // Localized display name for the item.
                  (global::System.Drawing.Bitmap)DslDiagrams::ImageHelper.GetImage(resourceManager.GetObject("EnumerationToolboxBitmap", resourceCulture)), // Image displayed next to the toolbox item.
                  "Sawczyn.EFDesigner.EFModel.EF Model DiagramsToolboxTab", // Unique identifier (non-localized) for the toolbox item tab.
                  resourceManager.GetString("EF Model DiagramsToolboxTab", resourceCulture), // Localized display name for the toolbox tab.
                  "ModelEnumF1Keyword", // F1 help keyword for the toolbox item.
                  resourceManager.GetString("EnumerationToolboxTooltip", resourceCulture), // Localized tooltip text for the toolbox item.
                  CreateElementToolPrototype(store, global::Sawczyn.EFDesigner.EFModel.ModelEnum.DomainClassId), // ElementGroupPrototype (data object) representing model element on the toolbox.
                  new global::System.ComponentModel.ToolboxItemFilterAttribute[] { // Collection of ToolboxItemFilterAttribute objects that determine visibility of the toolbox item.
                        new global::System.ComponentModel.ToolboxItemFilterAttribute("Enumeration"), // HACK : MEXEDGE
                  new global::System.ComponentModel.ToolboxItemFilterAttribute(ToolboxFilterString, global::System.ComponentModel.ToolboxItemFilterType.Require),
                  new global::System.ComponentModel.ToolboxItemFilterAttribute(EnumerationFilterString)
                  });
               break;
            default:
               break;
         } // end switch
         
         return result;
      }
      

      /// <summary>
      /// The store toe be used for all the toolbox item creation
      /// </summary>
      protected DslModeling::Store ToolboxStore
      {
         get
         { 
            if (toolboxStore==null)
            {
               toolboxStore = new DslModeling::Store(this.ServiceProvider);
               EventHandler StoreCleanUp = (o, e) =>
               {
                  //Since Store implements IDisposable, we need to dispose it when we're finished
                  if (this.toolboxStore != null)
                  {
                     this.toolboxStore.Dispose();
                  }
                  this.toolboxStore = null;
               };
               //There is no DomainUnload event for the default AppDomain, so we listen for both ProcessExit and DomainUnload
               AppDomain.CurrentDomain.ProcessExit += new EventHandler(StoreCleanUp);
               AppDomain.CurrentDomain.DomainUnload += new EventHandler(StoreCleanUp);
               
               //load the domain model
               toolboxStore.LoadDomainModels(typeof(global::Sawczyn.EFDesigner.EFModel.EFModelDomainModel));
               
            }
            return toolboxStore;
         }
      }
      
      /// <summary>
      /// Given a toolbox item "unique ID" returns the the toolbox item using cached dictionary
      /// </summary>
      /// <param name="itemId">The unique ToolboxItem to retrieve</param>
      private DslDesign::ModelingToolboxItem GetToolboxItem(string itemId)
      {
         DslDesign::ModelingToolboxItem item = null;

         if (!this.toolboxItemCache.TryGetValue(itemId, out item))
         {
            DslModeling::Store store = this.ToolboxStore;
            
            // Open transaction so we can create model elements corresponding to toolbox items.
            using (DslModeling::Transaction t = store.TransactionManager.BeginTransaction("CreateToolboxItems"))
            {
               // Retrieve the specified ToolboxItem from the DSL
               this.toolboxItemCache[itemId] = item = this.GetToolboxItem(itemId, store);
            }
         }

         return item;
      }
      
      /// <summary>
      /// Given a toolbox item "unique ID" and a data format identifier, returns the content of
      /// the data format. 
      /// </summary>
      /// <param name="itemId">The unique ToolboxItem to retrieve data for</param>
      /// <param name="format">The desired format of the resulting data</param>
      public virtual object GetToolboxItemData(string itemId, DataFormats.Format format)
      {
         DslDesign::ModelingToolboxItem item = null;

         global::System.Resources.ResourceManager resourceManager = global::Sawczyn.EFDesigner.EFModel.EFModelDomainModel.SingletonResourceManager;
         global::System.Globalization.CultureInfo resourceCulture = global::System.Globalization.CultureInfo.CurrentUICulture;

         System.Windows.Forms.IDataObject tbxDataObj;

         //get the toolbox item
         item = GetToolboxItem(itemId);

         if (item != null)
         {
            ToolboxItemContainer container = new ToolboxItemContainer(item);
            tbxDataObj = container.ToolboxData;

            if (tbxDataObj.GetDataPresent(format.Name))
            {
               return tbxDataObj.GetData(format.Name);
            }
            else 
            {
               string invalidFormatString = resourceManager.GetString("UnsupportedToolboxFormat", resourceCulture);
               throw new InvalidOperationException(string.Format(resourceCulture, invalidFormatString, format.Name));
            }
         }

         string errorFormatString = resourceManager.GetString("UnresolvedToolboxItem", resourceCulture);
         throw new InvalidOperationException(string.Format(resourceCulture, errorFormatString, itemId));
      }

        internal static string[] GetToolboxNames()
        {
            return new string[]
            {
                  "ModelClass", "UnidirectionalAssociation", "BidirectionalAssociation", "Generalization", "Comment", "CommentLink", "Enumeration", 
            };
        }
    }
}
